using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>One Owtch behaviour case: a first behaviour, the walk animation checked and the direction its cue sets.</summary>
/// <param name="Initial">The first behaviour the Owtch is placed in.</param>
/// <param name="Program">The walk animation, whose cue sets the direction before its shell loop (#2166).</param>
/// <param name="Crawl">The crawl the animation loops after its cue (#2276).</param>
/// <param name="Walking">The walking behaviour the animation's cue sets.</param>
internal readonly record struct VerifyOwtchBehaviorCase(OwtchBehaviorState Initial, OwtchSegment Program, OwtchSegment Crawl, OwtchBehaviorState Walking);
