using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite head's idle, submerge and emerge animations, its body's pillar stacks and its overlay's throw and cap animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="MagdolliteCue"/> with the values it acts on.
/// </summary>
internal static class MagdolliteAnimations
{
    /// <summary>The still pictures: each pillar stack by its height and the pillar's cap, each segment with the picture it holds.</summary>
    private static readonly KeyValuePair<MagdolliteSegment, EnemyFrameId>[] StillPictures =
    [
        new(P.OneSegmentPillar, EnemyFrameId.MagdolliteOneSegmentPillar),
        new(P.TwoSegmentPillar, EnemyFrameId.MagdolliteTwoSegmentPillar),
        new(P.ThreeSegmentPillar, EnemyFrameId.MagdolliteThreeSegmentPillar),
        new(P.FourSegmentPillar, EnemyFrameId.MagdolliteFourSegmentPillar),
        new(P.FiveSegmentPillar, EnemyFrameId.MagdolliteFiveSegmentPillar),
        new(P.SixSegmentPillar, EnemyFrameId.MagdolliteSixSegmentPillar),
        new(P.SevenSegmentPillar, EnemyFrameId.MagdolliteSevenSegmentPillar),
        new(P.EightSegmentPillar, EnemyFrameId.MagdolliteEightSegmentPillar),
        new(P.PillarCap, EnemyFrameId.MagdollitePillarCap),
    ];

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteSegment, EnemyFrameId, MagdolliteCue> Segments { get; } = new(
        new Dictionary<MagdolliteSegment, AnimationBeat<EnemyFrameId, MagdolliteCue>[]>(
            StillPictures.ToDictionary(still => still.Key, still => new[] { Hold(still.Value) }))
    {
        [P.LeftIdle] = Idle(EnemyFrameId.MagdolliteHeadFacingLeftHigh, EnemyFrameId.MagdolliteHeadFacingLeftMid, EnemyFrameId.MagdolliteHeadFacingLeftLow),
        [P.LeftThrow] = Throw(EnemyFrameId.MagdolliteRearingFacingLeft, EnemyFrameId.MagdolliteLeaningFacingLeft, -8),
        [P.LeftLavaFling] = LavaFling(EnemyFrameId.MagdolliteReachingFacingLeft),
        [P.LeftThrowFinish] = ThrowFinish(-8, -4, EnemyFrameId.MagdolliteThrowingFacingLeft),
        [P.LeftSubmerge] = Submerge(EnemyFrameId.MagdolliteHeadFacingLeftMid, EnemyFrameId.MagdolliteSinkingFacingLeft, EnemyFrameId.MagdolliteSunkFacingLeft),
        [P.LeftRaisePillar] = RaisePillar(EnemyFrameId.MagdolliteMoundFacingLeft),
        [P.LeftPillarRaised] = PillarRaised,
        [P.LeftEmerge] = EmergeStart,
        [P.LeftLowerPillar] = LowerPillar(EnemyFrameId.MagdolliteMoundFacingLeft),
        [P.LeftSurfacing] = Surfacing(EnemyFrameId.MagdolliteSunkFacingLeft, EnemyFrameId.MagdolliteSinkingFacingLeft, EnemyFrameId.MagdolliteHeadFacingLeftMid),
        [P.RightIdle] = Idle(EnemyFrameId.MagdolliteHeadFacingRightHigh, EnemyFrameId.MagdolliteHeadFacingRightMid, EnemyFrameId.MagdolliteHeadFacingRightLow),
        [P.RightThrow] = Throw(EnemyFrameId.MagdolliteRearingFacingRight, EnemyFrameId.MagdolliteLeaningFacingRight, 8),
        [P.RightLavaFling] = LavaFling(EnemyFrameId.MagdolliteReachingFacingRight),
        [P.RightThrowFinish] = ThrowFinish(8, -8, EnemyFrameId.MagdolliteThrowingFacingRight),
        [P.RightSubmerge] = Submerge(EnemyFrameId.MagdolliteHeadFacingRightMid, EnemyFrameId.MagdolliteSinkingFacingRight, EnemyFrameId.MagdolliteSunkFacingRight),
        [P.RightRaisePillar] = RaisePillar(EnemyFrameId.MagdolliteMoundFacingRight),
        [P.RightPillarRaised] = PillarRaised,
        [P.RightEmerge] = EmergeStart,
        [P.RightLowerPillar] = LowerPillar(EnemyFrameId.MagdolliteMoundFacingRight),
        [P.RightSurfacing] = Surfacing(EnemyFrameId.MagdolliteSunkFacingRight, EnemyFrameId.MagdolliteSinkingFacingRight, EnemyFrameId.MagdolliteHeadFacingRightMid),
    });

    /// <summary>The pixels the pillar rises or lowers in a submerge or emerge, one a play of its mound's step.</summary>
    private const int PillarTravel = 24;

    /// <summary>The gobs of lava a throw flings, one a play of its reaching hand's fling (#2439).</summary>
    private const int LavaFlings = 3;

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2337): a throw rears, flings
    /// <see cref="LavaFlings"/> gobs of lava and finishes (#2439), a submerge sinks the head and then raises the pillar a
    /// pixel at a time, an emerge lowers it a pixel at a time and then surfaces the head, and the pillar stacks and cap are
    /// still pictures. A throw, a submerge and an emerge each end on their finishing cue, which moves their part on.
    /// </summary>
    private static Dictionary<MagdolliteSegment, Animation<MagdolliteSegment>> StartingAt { get; } = Segments.StartsEnding(
        new[]
        {
            FacingStarts(new()
            {
                Idle = P.LeftIdle, Throw = P.LeftThrow, LavaFling = P.LeftLavaFling, ThrowFinish = P.LeftThrowFinish, Submerge = P.LeftSubmerge,
                RaisePillar = P.LeftRaisePillar, PillarRaised = P.LeftPillarRaised, Emerge = P.LeftEmerge, LowerPillar = P.LeftLowerPillar,
                Surfacing = P.LeftSurfacing,
            }),
            FacingStarts(new()
            {
                Idle = P.RightIdle, Throw = P.RightThrow, LavaFling = P.RightLavaFling, ThrowFinish = P.RightThrowFinish, Submerge = P.RightSubmerge,
                RaisePillar = P.RightRaisePillar, PillarRaised = P.RightPillarRaised, Emerge = P.RightEmerge, LowerPillar = P.RightLowerPillar,
                Surfacing = P.RightSurfacing,
            }),
        }.SelectMany(starts => starts).ToDictionary(),
        AnimationEnd.Holds, [.. StillPictures.Select(still => still.Key)]);

    /// <summary>
    /// The animations one facing's segments start: its idle loops, its throw rears, flings <see cref="LavaFlings"/> gobs of
    /// lava and finishes, its submerge sinks the head and raises the pillar <see cref="PillarTravel"/> pixels, and its emerge
    /// lowers the pillar as far and surfaces the head.
    /// </summary>
    private static KeyValuePair<MagdolliteSegment, Animation<MagdolliteSegment>>[] FacingStarts(MagdolliteFacingSegments facing) =>
    [
        new(facing.Idle, Animation<MagdolliteSegment>.Looping([Segments.Whole(facing.Idle)])),
        new(facing.Throw, new([Segments.Whole(facing.Throw), Segments.Repeated(facing.LavaFling, LavaFlings), Segments.Whole(facing.ThrowFinish)], AnimationEnd.Holds)),
        new(facing.Submerge, new([Segments.Whole(facing.Submerge), Segments.Repeated(facing.RaisePillar, PillarTravel), Segments.Whole(facing.PillarRaised)], AnimationEnd.Holds)),
        new(facing.Emerge, new([Segments.Whole(facing.Emerge), Segments.Repeated(facing.LowerPillar, PillarTravel), Segments.Whole(facing.Surfacing)], AnimationEnd.Holds)),
    ];

    /// <summary>The pillar raised, the submerge finishing: the same in either facing.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] PillarRaised => [Cue(new MagdolliteCue.Signal(K.FinishSubmerge))];

    /// <summary>An emerge's start, the parts' origins restored: the same in either facing.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] EmergeStart => [Cue(new MagdolliteCue.Signal(K.RestoreOrigins))];

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteSegment> StartingWith(MagdolliteSegment segment) => StartingAt[segment];

    /// <summary>The head bobbing over a facing's poses, <paramref name="high"/>, <paramref name="mid"/>, <paramref name="low"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] Idle(EnemyFrameId high, EnemyFrameId mid, EnemyFrameId low) =>
    [
        Show(high, 13),
        Show(mid, 13),
        Show(low, 13),
        Show(mid, 13),
    ];

    /// <summary>A throw's wind-up over a facing's poses, <paramref name="rearing"/>, <paramref name="leaning"/>, <paramref name="forward"/>, its origin shifted forward the way it faces.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] Throw(EnemyFrameId rearing, EnemyFrameId leaning, int forward) =>
    [
        Cue(new MagdolliteCue.PlaySoundIfOnScreen(MagdolliteSoundDefinitions.Throw)),
        Cue(new MagdolliteCue.Signal(K.StartCooldown)),
        Show(rearing, 26),
        Show(leaning, 8),
        Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(forward, -4))),
    ];

    /// <summary>One fling of lava over a facing's reaching pose, <paramref name="reaching"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] LavaFling(EnemyFrameId reaching) =>
    [
        Show(reaching, 5),
        Cue(new MagdolliteCue.Signal(K.SpawnLava)),
    ];

    /// <summary>A throw's finish over a facing's throwing pose, <paramref name="forward"/>, <paramref name="up"/>, <paramref name="throwing"/>, its origin shifted as that facing's throw leaves it.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] ThrowFinish(int forward, int up, EnemyFrameId throwing) =>
    [
        Cue(new MagdolliteCue.ShiftFromThrowOrigin(new(forward, up))),
        Show(throwing, 5),
        Cue(new MagdolliteCue.Signal(K.FinishThrow)),
    ];

    /// <summary>The head sinking over a facing's poses, <paramref name="headMid"/>, <paramref name="sinking"/>, <paramref name="sunk"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] Submerge(EnemyFrameId headMid, EnemyFrameId sinking, EnemyFrameId sunk) =>
    [
        Show(headMid, 5),
        Cue(new MagdolliteCue.Move(new(0, 2))),
        Show(headMid, 5),
        Show(sinking, 5),
        Show(sunk, 5),
        Cue(new MagdolliteCue.Move(new(0, 2))),
        Show(sunk, 5),
        Cue(new MagdolliteCue.Signal(K.SinkAndShowParts)),
    ];

    /// <summary>One pixel of the pillar's rise over a facing's mound, <paramref name="mound"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] RaisePillar(EnemyFrameId mound) =>
    [
        Cue(new MagdolliteCue.MoveWithPillar(new(0, -1))),
        Show(mound, 1),
    ];

    /// <summary>One pixel of the pillar's fall over a facing's mound, <paramref name="mound"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] LowerPillar(EnemyFrameId mound) =>
    [
        Cue(new MagdolliteCue.MoveWithPillar(new(0, 1))),
        Show(mound, 1),
    ];

    /// <summary>The head surfacing over a facing's poses, <paramref name="sunk"/>, <paramref name="sinking"/>, <paramref name="headMid"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue>[] Surfacing(EnemyFrameId sunk, EnemyFrameId sinking, EnemyFrameId headMid) =>
    [
        Cue(new MagdolliteCue.Signal(K.SurfaceAndHideParts)),
        Show(sunk, 5),
        Cue(new MagdolliteCue.Move(new(0, -2))),
        Show(sunk, 5),
        Show(sinking, 5),
        Show(headMid, 5),
        Cue(new MagdolliteCue.Move(new(0, -2))),
        Show(headMid, 5),
        Cue(new MagdolliteCue.Signal(K.FinishEmerge)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, MagdolliteCue> Cue(MagdolliteCue cue) => new AnimationBeat<EnemyFrameId, MagdolliteCue>.Event(cue);
}
