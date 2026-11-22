using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room services resident room objects use after load (#633). Every room has each of them; a room without liquid
/// still has its room FX, whose liquid motion reports the absence (<see cref="IRoomLiquid.BaseY"/> and
/// <see cref="IRoomLiquid.TargetSurface"/>).
/// </summary>
[RoomLifetime]
public sealed partial record RoomObjectServices
{
    /// <summary>Samus as room objects see her.</summary>
    public required IRoomSamusAccess Samus { get; init; }

    /// <summary>Which of the area's bosses, mini-bosses and Torizos are defeated.</summary>
    public required IDefeatedBossesReader Bosses { get; init; }

    /// <summary>The story events that have happened, which room objects read and mark.</summary>
    public required IStoryEvents Events { get; init; }

    /// <summary>
    /// The statue room's statue, whose finished sinking its grey door waits for; null in every other room, which places no
    /// cap waiting for it (#2486).
    /// </summary>
    public ITourianStatueProgress? TourianStatue { get; init; }

    /// <summary>The room FX the Speed Booster escape, the n00b tube, collectibles and Tourian's acid trigger drive.</summary>
    public required IRoomObjectFx RoomFx { get; init; }

    /// <summary>Room-shake writes.</summary>
    public required IRoomEarthquakeWriter Earthquake { get; init; }

    /// <summary>Enemy-side effects.</summary>
    public required IRoomObjectEnemyEffects Enemies { get; init; }
}
