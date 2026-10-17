namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How far Ceres's escape has progressed. Each stage replaces the previous one; the elevator door's destruction
/// replaces the self-destruct stage, so a reader that asks whether Ridley has escaped sees every later stage as
/// escaped, and only <see cref="RidleyEscaped"/> itself reveals the Mode 7 wall overlay in Ridley's room.
/// </summary>
public enum CeresEscapeStatus
{
    /// <summary>Ridley has not escaped: the station's state on a fresh load and during the fight.</summary>
    RidleyNotEscaped,

    /// <summary>Ceres Ridley has retreated with the Baby and handed the room to the getaway.</summary>
    RidleyEscaped,

    /// <summary>The self-destruct warning has been published; the Ceres doors start their escape earthquakes.</summary>
    SelfDestructStarted,

    /// <summary>The elevator room's door has rumbled apart, releasing the elevator shaft's rotation.</summary>
    ElevatorDoorDestroyed,
}
