public enum LevelState
{
    InProgress,
    Failed,
    Completed
}
public struct ShotData
{
    public bool IsGhost;
    public float PositionX;
    public float PositionY;
    public float PositionZ;
    public float DirectionX;
    public float DirectionY;
    public float VelocityX;
    public float VelocityY;

    public ShotData(bool isGhost, float positionX, float positionY, float positionZ, float directionX, float directionY, float velocityX, float velocityY)
    {
        IsGhost = isGhost;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        DirectionX = directionX;
        DirectionY = directionY;
        VelocityX = velocityX;
        VelocityY = velocityY;
    }
}

// This struct is used to enable or disable features(Mechanics) in the game
public class FeatureFlags
{
    public bool coreMechanic;
    public bool projectile;
    
    public FeatureFlags()
    {
        coreMechanic = false;
        projectile = false;
    }
}
