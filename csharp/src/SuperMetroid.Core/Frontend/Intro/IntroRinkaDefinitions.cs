namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed cinematic-object and physical launch definitions for the intro Rinkas.</summary>
internal static class IntroRinkaDefinitions
{

    /// <summary>The intro Rinka's initialization, empty frame behaviour, and initial animation.</summary>
    /// <remarks>
    /// Issues #625 and #990: its animation matches the authored data.
    /// Three ten-call frames select three authored pictures,
    /// followed by the start-moving step. The four-frame loop
    /// then selects the second, first, second and third for ten calls
    /// each before its jump returns to the loop's start. The generic animation
    /// loads the first frame on relative call 0, the frame behaviour and first
    /// loop frame on call 30, and subsequent loop frames on calls 40,
    /// 50, and 60. Because the frame behaviour runs before the animation advances,
    /// movement begins on the following actor call. The frame order is
    /// authored animation policy; retain the bounded two-phase stream.
    ///
    /// Issues #625 and #991: the three selected frames,
    /// f=0..2, each have four parts and tile
    /// 406+f, matching the authored data.
    /// Part j=0..3 uses X=0 for even j or 504 for odd j, Y=0 for j&lt;2
    /// or 248 otherwise, the Rinka's colours in front of the scene, tile 406+f, plus the vertical flip
    /// for j&lt;2 and the horizontal flip for even j. This is an exact bounded
    /// four-quadrant construction; the frame animation selects no other
    /// frames or adjacent visual data.
    /// </remarks>
    public static CinematicActorDefinition RinkaActor =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.IntroRinka);

    /// <summary>The Rinka spawner's empty behaviours and initial animation.</summary>
    /// <remarks>
    /// Issues #625 and #989: its animation contains seven steps:
    /// wait 74, empty frame, spawn the first pair, wait 128,
    /// empty frame, spawn the second pair, then delete.
    /// All seven match the authored data. The generic
    /// animation loads the first wait on zero-based call 0, spawns the
    /// first pair on call 74, then the second pair and deletes on call 202.
    /// IntroRinkaSystem follows this animation and verifies both waves.
    /// The unequal waits and two distinct behaviours are authored timing
    /// policy; retain the explicit bounded schedule rather than fit a
    /// recurrence or infer a third wave from adjacent data.
    /// </remarks>
    public static CinematicActorDefinition SpawnerActor =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.IntroRinkaSpawner);

    /// <summary>The values <see cref="IntroRinkaLaunches.Rinka"/> selects, in index order.</summary>
    internal static readonly IntroRinkaPhysicalDefinition[] Rinkas =
    [
        new(new(112, 72), 0),
        new(new(192, 56), -1),
        new(new(128, 48), 0),
        new(new(232, 80), -1),
    ];

    /// <summary>The Rinkas' shared half-pixel X/Y velocity.</summary>
    public static SignedSixteenSixteen HalfPixel { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
}
