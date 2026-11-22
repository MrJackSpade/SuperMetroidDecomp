using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room the enemies' requested room effects land in (#2382): its level, scroll grid and area, the owners the effects
/// change, and Samus, whose equipment the colour effects they start read. The enemy pass hands the loaded room's; a room
/// load hands the parts it has built, before the room is adopted.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly partial record struct EnemyRoomEffectSite
{
    /// <summary>The room's level, which the boss room objects edit.</summary>
    internal required RoomLevelData Level { get; init; }

    /// <summary>The room's scroll grid, whose screens Botwoon's cleared wall opens.</summary>
    internal required RoomScrollGrid Scrolls { get; init; }

    /// <summary>The room's area, whose boss Mother Brain's defeat records.</summary>
    internal required AreaId Area { get; init; }

    /// <summary>The room's room objects, which the requests spawn among.</summary>
    internal required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The room's effect overlay, which Mother Brain's acid change moves.</summary>
    internal required RoomFxOverlayState FxOverlay { get; init; }

    /// <summary>The room's colour effects, which the enemies' glows and Mother Brain's escape start among.</summary>
    internal required ColourEffectSystem ColourEffects { get; init; }

    /// <summary>The room's enemies, whose Chozo statue and gate requests the room objects answer.</summary>
    internal required RoomEnemySystem Enemies { get; init; }

    /// <summary>Samus, whose equipment the started colour effects read and whose drained get-up Mother Brain's escape ends.</summary>
    internal required SamusState Samus { get; init; }

    /// <summary>The loaded room's site, with <paramref name="samus"/>.</summary>
    internal static EnemyRoomEffectSite Of(RoomScope room, SamusState samus) => new()
    {
        Level = room.LevelData,
        Scrolls = room.Camera.Scrolls,
        Area = room.Room.Area,
        RoomObjects = room.RoomObjects,
        FxOverlay = room.FxOverlay,
        ColourEffects = room.ColourEffects,
        Enemies = room.Enemies,
        Samus = Ensure.NotNull(samus),
    };
}
