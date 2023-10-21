using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    public GameObject bulletObj;
    public GameObject ghostBulletObj;
    public GameObject playerObj;
    private Queue<ShotDetails> _previousShots = new Queue<ShotDetails>();
    private AnalyticsManager _analyticsManager;
    private SpriteRenderer spriteRenderer;
    public LevelManager levelManager;
    private int destroyTime = 5;
    public Color ghostPlayerColor = new(0.572549f, 0.7764707f, 0.7764707f, 0.7f);
    // Queue of active ghost players. Used to keep track of the ghost players that are currently active.
    // So that they can be referenced from here when they have to be destroyed.
    public Queue<GameObject> activeGhostPlayers = new Queue<GameObject>();
    // Queue of idle bullets. Used to keep track of the bullets that are currently idle.
    public Queue<GameObject> idleGhostBullets = new Queue<GameObject>();
    // Tracks if the current bullet shot is the last bullet among remaining shots.
    private bool isLastBullet = false;

    private float bulletSpeed = 0f;
    private float minBulletSpeed = 5f;
    private float maxBulletSpeed = 20f;
    private float chargeRate = 50f;
    private float maxCharge = 100f;
    private float currentCharge = 0f;
    private string spritePath = "Assets/Icons/aim_pointer.png";

    void Start()
    {
        _analyticsManager = FindObjectOfType<AnalyticsManager>();
        levelManager = FindObjectOfType<LevelManager>();
        Transform gun = this.transform.Find("Gun");
        spriteRenderer = gun.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Rotation logic
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        // DEBUG STATEMENT
        // if (currentCharge > 0f){ Debug.Log("Current Charge" + currentCharge); }
        
        if (levelManager.bulletCount == 1)
        {
            isLastBullet = true;
        }

        // While mouse is being held down, shot will charge
        if (Input.GetMouseButton(0) && (GameObject.Find("Bullet(Clone)") == null) && (GameObject.Find("activeGhost") == null) )
        {
            currentCharge += chargeRate * Time.deltaTime;
            currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);
            if (currentCharge <= 14){
              spritePath = "Sprites/aim_pointer";
            } else if (currentCharge <= 28){
              spritePath = "Sprites/aim_pointer_charge_1";
            } else if (currentCharge <= 42){
              spritePath = "Sprites/aim_pointer_charge_2";
            } else if (currentCharge <= 56){
              spritePath = "Sprites/aim_pointer_charge_3";
            } else if (currentCharge <= 70){
              spritePath = "Sprites/aim_pointer_charge_4";
            } else if (currentCharge <= 84){
              spritePath = "Sprites/aim_pointer_charge_5";
            } else {
              spritePath = "Sprites/aim_pointer_charge_6";
            }
            spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);

        // When mouse is released, and current charge
        } else if (Input.GetMouseButtonUp(0) && currentCharge > 0f){
            _analyticsManager.ld.shotsTaken++;
            _analyticsManager.LogAnalytics();

            // Don't want players to waste shot because they didn't know they needed to charge it
            // Let bullets shoot at slightly above the destroy speed to make sure blanks aren't shot.
            bulletSpeed = maxBulletSpeed*currentCharge/maxCharge;
            bulletSpeed = Mathf.Max(bulletSpeed, minBulletSpeed);
            currentCharge = 0f;
            spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/aim_pointer");
            // Shoot and create Echo.
            Shoot();
            // Create echo only if it is not the last bullet.
            if (!isLastBullet)
            {
                CreateEcho();
            }
        }
    }

    void CreateEcho()
    {
            GameObject ghostBullet = Instantiate(ghostBulletObj, transform.position, Quaternion.identity);
            ghostBullet.name = "idleGhost";

            // 3: Player, changing layer=3 will give the ghostBullet the same collision properties
            // as the player. Idle ghostBullets should not collide with any balls.
            ghostBullet.layer = 3;
            
            // TODO: Need to enqueue as many times as the number of idle ghost bullets instantiated.
            idleGhostBullets.Enqueue(ghostBullet);

            // Remove any existing ghost players
            while (activeGhostPlayers.Count > 0)
            {
                GameObject existingGhostPlayer = activeGhostPlayers.Dequeue();
                Destroy(existingGhostPlayer);
            }
            
            var ghostPlayerName = "ghostPlayer";
            GameObject ghostPlayer = Instantiate(playerObj, transform.position, Quaternion.identity);
            ghostPlayer.name = ghostPlayerName;
            ghostPlayer.transform.Find("AimPointer").transform.Find("Gun").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(spritePath);

            //  Need to remove the script from ghost player or else it will just follow the user controls.
            PlayerController playerScript = ghostPlayer.GetComponent<PlayerController>();
            GunController gunScript = ghostPlayer.transform.Find("AimPointer").GetComponent<GunController>();
            Destroy(playerScript);
            Destroy(gunScript);

            // Change the color of the ghost player
            ghostPlayer.GetComponent<SpriteRenderer>().color = ghostPlayerColor;
            ghostPlayer.GetComponent<Renderer>().sortingOrder = 5;
            
            // Track ghostPlayer objects
            // TODO: Need to enqueue as many times as the number of ghost players instantiated.
            activeGhostPlayers.Enqueue(ghostPlayer);
    }

    void Shoot()
    {
        // If queue is not empty, reshoot the previous shot
        if (_previousShots.Count > 0)
        {
            var shot = _previousShots.Dequeue();

            GameObject ghostBullet = GameObject.Find("idleGhost");
            // Make idle ghost bullet active
            if (ghostBullet)
            {
              Rigidbody2D ghostBulletRb = ghostBullet.GetComponent<Rigidbody2D>();
              ghostBulletRb.velocity = shot.Velocity;
              ghostBullet.name = "activeGhost";
              Destroy(ghostBullet, destroyTime);
              // 8: ghostBullet, activates the collision properties of the ball
              ghostBullet.layer = 8;
              
              // TODO: Need to dequeue as many times as idle ghost bullets are activated
              idleGhostBullets.Dequeue();
            }
        }

        // Instantiate bullet and set its direction
        GameObject bullet = Instantiate(bulletObj, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        // The direction from the weapon to the mouse
        Vector2 shootDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        shootDirection.Normalize();

        bulletRb.velocity = shootDirection * bulletSpeed;
        Destroy(bullet, destroyTime);

        // Save this shot
        _previousShots.Enqueue(new ShotDetails { Position = transform.position, Direction = shootDirection, Velocity = shootDirection * bulletSpeed});
        
        // Decrement remaining shots
        levelManager.BulletCountDown();
    }

    private class ShotDetails
    {
        public Vector3 Position;
        public Vector2 Direction;
        public Vector2 Velocity;
    }
}
