namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The behaviour a wall Space Pirate's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum WallSpacePirateAction
{
    /// <summary>Watch Samus while continuing the climbing animation on its wall (<see cref="WallSpacePirateEnemyState.Wall"/>).</summary>
    Climbing,

    /// <summary>Holds on its wall while its attack animation fires and readies the jump to the other wall.</summary>
    Attacking,

    /// <summary>Follow the game's sine/cosine arc from one wall to the other, its <see cref="WallSpacePirateEnemyState.Wall"/>.</summary>
    WallJumping,
}
