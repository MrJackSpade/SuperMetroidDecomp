using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>What a room's setup reaches once the room's enemies are loaded (#2287).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed partial record RoomSetupEnemiesLoaded
{
    /// <summary>The room's enemies, whose earthquake and projectiles the setups start.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>The session's progress.</summary>
    public required SharedGameState System { get; init; }

    /// <summary>The escape's diagonal quake carried from the previous room's main, which two escape setups clear.</summary>
    public required EscapeQuakeCarriedAcrossRooms EscapeQuake { get; init; }
}
