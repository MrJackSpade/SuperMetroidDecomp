namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One stage of Crocomire's approach onto his bridge (<see cref="CrocomireBridgeCollapseDefinitions.ApproachStages"/>,
/// #2182, #2540), run once as he first reaches it: its bridge blocks start to crumble, then its dust rises.
/// </summary>
/// <param name="CrumblingBlocks">The bridge blocks the stage starts crumbling, in order; none for a stage that only raises dust.</param>
/// <param name="Dust">Where the stage's big dust cloud rises.</param>
internal sealed record CrocomireBridgeApproachStage(IReadOnlyList<Position16<RoomBlocks>> CrumblingBlocks, Position16<RoomPixels> Dust);
