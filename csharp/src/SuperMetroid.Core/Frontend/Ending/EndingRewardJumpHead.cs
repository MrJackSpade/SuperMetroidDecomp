namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A suited reward jump's head: the actor and the screen places its animation readies and launches it at.</summary>
/// <param name="Actor">The head actor.</param>
/// <param name="Readied">Where the head stands once its animation readies it.</param>
/// <param name="Launched">Where the head stands once its animation launches it.</param>
internal sealed partial record EndingRewardJumpHead(EndingRewardActor Actor, Offset16<ScreenPixels> Readied, Offset16<ScreenPixels> Launched);
