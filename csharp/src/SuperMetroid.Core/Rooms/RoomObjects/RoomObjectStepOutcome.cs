namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What one room-object step leaves for the update that ran it to act on (#2514): the items collected, the stations'
/// actions and the enemy projectiles the room objects threw, each in the order the step made them. Every step builds its
/// own outcome and returns it; nothing in it outlives the update that acts on it.
/// </summary>
public sealed class RoomObjectStepOutcome
{
    /// <summary>The items the step collected, in the order their places updated.</summary>
    private readonly List<CollectiblePickupEvent> _pickups = [];

    /// <summary>The stations' actions the step made, in the order their places updated.</summary>
    private readonly List<StationActivationEvent> _stationActivations = [];

    /// <summary>The enemy projectiles the step's room objects threw, in the order they threw them.</summary>
    private readonly List<RoomObjectProjectileRequest> _thrownProjectiles = [];

    /// <summary>The items the step collected, whose messages the update opens, in the order their places updated.</summary>
    public IReadOnlyList<CollectiblePickupEvent> Pickups => _pickups;

    /// <summary>The stations' actions the step made, whose messages the update opens, in the order their places updated.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivations => _stationActivations;

    /// <summary>The enemy projectiles the step's room objects threw, which the update spawns in the order they were thrown.</summary>
    public IReadOnlyList<RoomObjectProjectileRequest> ThrownProjectiles => _thrownProjectiles;

    /// <summary>An item was collected in the step, opening its message.</summary>
    internal void Collected(CollectiblePickupEvent pickup) => _pickups.Add(pickup);

    /// <summary>A station acted in the step: it asks to save, saved, or completed its refill or map download.</summary>
    internal void StationActed(StationActivationEvent activation) => _stationActivations.Add(Ensure.NotNull(activation));

    /// <summary>A room object threw an enemy projectile in the step.</summary>
    internal void Threw(RoomObjectProjectileRequest projectile) => _thrownProjectiles.Add(Ensure.NotNull(projectile));
}
