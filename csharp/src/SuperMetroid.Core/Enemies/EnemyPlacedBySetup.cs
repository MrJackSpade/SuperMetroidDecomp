namespace SuperMetroid.Core.Enemies;

/// <summary>An entry whose enemy its species' setup places: the spawn leaves the place where it stands for the setup.</summary>
public sealed partial record EnemyPlacedBySetup : EnemyPlacing
{
    /// <summary>The place stays where it stands; the species' setup places the enemy.</summary>
    /// <param name="current">Where the place stands before the spawn.</param>
    internal override FixedPosition<RoomPixels> Place(FixedPosition<RoomPixels> current) => current;
}
