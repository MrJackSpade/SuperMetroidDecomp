namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's two authored bounce movements: falling down the speed curve and then rising.
/// </summary>
public enum BoyonBounceMovement
{
    /// <summary>Advances down the speed curve until the stored spawn-height baseline is reached.</summary>
    Falling,
    /// <summary>Retreats up the speed curve until it passes the top of the arc and selects falling.</summary>
    Rising,
}
