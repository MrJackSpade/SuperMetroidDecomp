using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One room-object population entry: its block and its family's typed setup (#2383), which names the kind it places
/// (#2557).
/// </summary>
/// <param name="Block">The block it is placed on.</param>
/// <param name="Setup">The setup its family runs, with what the placement gives it.</param>
/// <param name="Site">Whether the block is one of the room's grid or of the terrain beyond the room.</param>
internal readonly record struct RoomObjectPlacementDefinition(
    Position16<RoomBlocks> Block, RoomObjectPlacementSetup Setup, RoomObjectSite Site = RoomObjectSite.RoomGrid)
{
    /// <summary>
    /// A scroll trigger placed once with the reach its extensions give it (#2557): the trigger, then each block of each run
    /// in turn, each run continuing from the block before it in its extension's direction.
    /// </summary>
    /// <param name="block">The trigger's block.</param>
    /// <param name="change">The scroll change the trigger applies.</param>
    /// <param name="reach">The runs of extension blocks the trigger reaches through, in order from the trigger.</param>
    internal static RoomObjectPlacementDefinition[] ScrollTrigger(
        Position16<RoomBlocks> block, RoomScrollChange change, params ReadOnlySpan<ScrollReachRun> reach)
    {
        var placements = new List<RoomObjectPlacementDefinition> { new(block, new ScrollTriggerSetup(change)) };
        Position16<RoomBlocks> next = block;
        foreach (ScrollReachRun run in reach)
        {
            var setup = new ScrollExtensionSetup(run.Extension);
            for (int extended = 0; extended < run.Blocks; extended++)
            {
                next = run.Extension.Beyond(next);
                placements.Add(new(next, setup));
            }
        }
        return [.. placements];
    }
}
