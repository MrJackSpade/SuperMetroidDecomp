using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room services resident PLMs use after load (#633). Samus and the area's boss flags are
/// required. Every other service is an explicit optional capability: the PLM family that uses it
/// checks for it where it previously checked its delegate, and a family whose program needs a missing
/// service fails loudly.
/// </summary>
[RoomLifetime]
public sealed record RoomPlmServices
{
    /// <summary>Samus as PLMs see her.</summary>
    public required IRoomSamusAccess Samus { get; init; }

    /// <summary>The area's boss, mini-boss and Torizo defeat bits.</summary>
    public required IAreaBossFlagReader Bosses { get; init; }

    /// <summary>Global event bits PLMs read and set.</summary>
    public IGlobalEventFlags? Events { get; init; }

    /// <summary>Tourian entrance statue progress for grey-door conditions.</summary>
    public ITourianStatueProgress? TourianStatue { get; init; }

    /// <summary>Layer-3 FX the Speed Booster escape and N00b tube drive.</summary>
    public IRoomPlmFx? RoomFx { get; init; }

    /// <summary>Room-shake writes.</summary>
    public IRoomEarthquakeWriter? Earthquake { get; init; }

    /// <summary>Enemy-side effects.</summary>
    public IRoomPlmEnemyEffects? Enemies { get; init; }
}
