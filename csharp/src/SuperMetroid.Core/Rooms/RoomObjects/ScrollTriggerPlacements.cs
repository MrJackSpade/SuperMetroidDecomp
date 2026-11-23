namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The placements of a scroll trigger and the extensions that give it its reach (#2557).</summary>
internal static partial class ScrollTriggerPlacements
{
    /// <summary>
    /// A scroll trigger placed once with the reach its extensions give it: the trigger, then each block of each run in
    /// turn, each run continuing from the block before it in its extension's direction.
    /// </summary>
    /// <param name="block">The trigger's block.</param>
    /// <param name="change">The scroll change the trigger applies.</param>
    /// <param name="reach">The runs of extension blocks the trigger reaches through, in order from the trigger.</param>
    internal static RoomObjectPlacementDefinition[] Reaching(
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
