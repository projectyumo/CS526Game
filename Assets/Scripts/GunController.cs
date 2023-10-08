using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{

    public GameObject bulletObj;
    private float _bulletSpeed = 50f;
    private Queue<ShotDetails> _previousShots = new Queue<ShotDetails>();
    private AnalyticsManager _analyticsManager;
    
    void Start()
    {
        _analyticsManager = FindObjectOfType<AnalyticsManager>();
    }

    void Update()
    {
        // Rotation logic
        Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        if (Input.GetMouseButtonDown(0) && (GameObject.Find("Bullet(Clone)") == null) )
        {
            _analyticsManager.shotsTaken++;
            _analyticsManager.LogAnalytics();
            Shoot();


        }
    }

    void Shoot()
    {
        // If queue is not empty, reshoot the previous shot
        if (_previousShots.Count > 0)
        {
            var shot = _previousShots.Dequeue();
            GameObject ghostBullet = Instantiate(bulletObj, shot.position, Quaternion.identity);
            Rigidbody2D ghostBulletRb = ghostBullet.GetComponent<Rigidbody2D>();
            ghostBulletRb.velocity = shot.direction * _bulletSpeed;
        }

        // Instantiate bullet and set its direction
        GameObject bullet = Instantiate(bulletObj, transform.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

        // The direction from the weapon to the mouse
        Vector2 shootDirection = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

        bulletRb.velocity = shootDirection * _bulletSpeed;

        // Save this shot
        _previousShots.Enqueue(new ShotDetails { position = transform.position, direction = shootDirection });

    }

    private class ShotDetails
    {
        public Vector3 position;
        public Vector2 direction;
    }
}
