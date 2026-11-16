using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room services resident room objects use after load (#633). Every room has each of them; a room without liquid
/// still has its room FX, whose liquid motion reports the absence (<see cref="IRoomFxMotion.BaseY"/> and
/// <see cref="IRoomFxMotion.TargetSurface"/>).
/// </summary>
[RoomLifetime]
public sealed record RoomObjectServices
{
    /// <summary>Samus as room objects see her.</summary>
    public required IRoomSamusAccess Samus { get; init; }

    /// <summary>The area's boss, mini-boss and Torizo defeat bits.</summary>
    public required IAreaBossFlagReader Bosses { get; init; }

    /// <summary>The story events that have happened, which room objects read and mark.</summary>
    public required IGlobalEventFlags Events { get; init; }

    /// <summary>Tourian entrance statue progress for grey-door conditions.</summary>
    public required ITourianStatueProgress TourianStatue { get; init; }

    /// <summary>The room FX the Speed Booster escape, the n00b tube, collectibles and Tourian's acid trigger drive.</summary>
    public required IRoomObjectFx RoomFx { get; init; }

    /// <summary>Room-shake writes.</summary>
    public required IRoomEarthquakeWriter Earthquake { get; init; }

    /// <summary>Enemy-side effects.</summary>
    public required IRoomObjectEnemyEffects Enemies { get; init; }
}
