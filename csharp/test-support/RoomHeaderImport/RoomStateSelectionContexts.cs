using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.TestSupport.RoomHeaderImport;

/// <summary>Room-state selection contexts that checks select with, built as production builds them.</summary>
public static class RoomStateSelectionContexts
{
    /// <summary>
    /// The conditions of a game with no progress: no story event has happened, no boss is defeated and Samus carries
    /// neither the Morph Ball and missiles nor Power Bombs. Each read builds a new context with its own events.
    /// </summary>
    public static RoomStateSelectionContext NoProgress =>
        new(new PersistentProgress().Events, DefeatedBosses.None, HasMorphBallAndMissiles: false, HasPowerBombs: false);
}
