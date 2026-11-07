namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The pre-instructions a coloured cap installs (#1296). The values are arbitrary (#1414).</summary>
internal enum ColoredDoorFrameBehavior
{
    /// <summary>No pre-instruction.</summary>
    None = 1,
    /// <summary>The yellow cap wakes when a Power Bomb hits it.</summary>
    WakeOnPowerBomb = 2,
    /// <summary>The red cap wakes when a missile hits it; a Super Missile seeds the counter.</summary>
    WakeOnMissile = 3,
    /// <summary>The green cap wakes when a Super Missile hits it.</summary>
    WakeOnSuperMissile = 4,
}
