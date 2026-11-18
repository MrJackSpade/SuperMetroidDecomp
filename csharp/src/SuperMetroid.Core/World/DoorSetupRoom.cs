using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// The destination room a door's setup changes (#2382): the parts its load has built when the setup runs, after the
/// room's objects and before its enemies, and Samus, whom the elevatube's setups place and release.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct DoorSetupRoom
{
    /// <summary>The destination's level, which the setup's room objects edit.</summary>
    internal required RoomLevelData Level { get; init; }

    /// <summary>The destination's scroll grid, whose screens the setup changes.</summary>
    internal required RoomScrollGrid Scrolls { get; init; }

    /// <summary>The destination's area, whose boss the Wrecked Ship treadmill reads.</summary>
    internal required AreaId Area { get; init; }

    /// <summary>The destination's room objects, which the setup spawns among.</summary>
    internal required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The destination's block animations, which the treadmill setups start.</summary>
    internal required RoomBlockAnimations BlockAnimations { get; init; }

    /// <summary>The elevatube's ride, or null outside the elevatube (#2287).</summary>
    internal required MaridiaElevatubeRoomMainState? MaridiaElevatube { get; init; }

    /// <summary>Samus, whom the elevatube's setups place and release.</summary>
    internal required SamusState Samus { get; init; }
}
