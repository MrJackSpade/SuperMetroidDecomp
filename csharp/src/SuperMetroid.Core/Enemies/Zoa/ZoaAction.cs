namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The behaviours Zoa's AI runs.
/// Its values are arbitrary.
/// </summary>
public enum ZoaAction
{
    /// <summary>Remain hidden until Samus is closer across than <see cref="ZoaSpeedDefinitions.ActivationDistance"/>, then select the facing rise animation.</summary>
    WaitForSamus,
    /// <summary>Become visible and move upward at <see cref="ZoaSpeedDefinitions.RisingSpeed"/> while Samus is above; reaching her height selects horizontal launch.</summary>
    Rising,
    /// <summary>Launch the enemy itself horizontally using animation-controlled speeds; once its center leaves the camera square, hide and restore its spawn whole coordinates.</summary>
    Shooting,
}
