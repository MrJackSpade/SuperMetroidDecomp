using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The reveal of one row of the mural behind Mother Brain's tank, spawned at the row's own block (#2258).</summary>
/// <param name="Row">The row revealed.</param>
public sealed partial record MotherBrainMuralRowReveal(MotherBrainMuralRow Row) : MotherBrainRoomChangeRequest(Row.Block)
{
    /// <inheritdoc/>
    internal override TerrainChange Change => Row.Reveal;
}
