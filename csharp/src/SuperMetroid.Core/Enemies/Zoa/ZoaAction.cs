namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The behaviours Zoa's AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum ZoaAction
{
    /// <summary>Remain hidden until Samus is fewer than 128 horizontal pixels away, then select the facing rise animation.</summary>
    WaitForSamus = 1,
    /// <summary>Become visible and move upward 0.5 pixel per AI update while Samus is above; reaching her height selects horizontal launch.</summary>
    Rising,
    /// <summary>Launch the enemy itself horizontally using animation-controlled speeds; once its center leaves the camera square, hide and restore its spawn whole coordinates.</summary>
    Shooting,
}
