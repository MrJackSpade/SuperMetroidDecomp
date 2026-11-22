using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Hoppers.HopperCueKind;
using P = SuperMetroid.Core.Enemies.Hoppers.HopperSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="HopperCue"/> with the values it acts on.
/// </summary>
internal static partial class HopperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HopperSegment, EnemyFrameId, HopperCue> Segments { get; } = new(
        new Dictionary<HopperSegment, AnimationBeat<EnemyFrameId, HopperCue>[]>
    {
        [P.SidehopperFloorJumping] = Jumping(EnemyFrameId.SidehopperLegsStraight, HopSound),
        [P.SidehopperFloorLanded] = Landed(EnemyFrameId.SidehopperLegsBent, EnemyFrameId.SidehopperLegsCrouched, LandSound),
        [P.SidehopperCeilingJumping] = Jumping(EnemyFrameId.SidehopperOnCeilingLegsStraight, HopSound),
        [P.SidehopperCeilingLanded] = Landed(EnemyFrameId.SidehopperOnCeilingLegsBent, EnemyFrameId.SidehopperOnCeilingLegsCrouched, LandSound),
        [P.DessgeegaFloorJumping] = Jumping(EnemyFrameId.DessgeegaLegsStraight),
        [P.DessgeegaFloorLanded] = Landed(EnemyFrameId.DessgeegaLegsBent, EnemyFrameId.DessgeegaLegsCrouched),
        [P.DessgeegaCeilingJumping] = Jumping(EnemyFrameId.DessgeegaOnCeilingLegsStraight),
        [P.DessgeegaCeilingLanded] = Landed(EnemyFrameId.DessgeegaOnCeilingLegsBent, EnemyFrameId.DessgeegaOnCeilingLegsCrouched),
        [P.LargeSidehopperFloorJumping] = Jumping(EnemyFrameId.LargeSidehopperLegsStraight, HopSound),
        [P.LargeSidehopperFloorLanded] = Landed(EnemyFrameId.LargeSidehopperLegsBent, EnemyFrameId.LargeSidehopperLegsCrouched, LandSound),
        [P.LargeSidehopperCeilingJumping] = Jumping(EnemyFrameId.LargeSidehopperOnCeilingLegsStraight, HopSound),
        [P.LargeSidehopperCeilingLanded] = Landed(EnemyFrameId.LargeSidehopperOnCeilingLegsBent, EnemyFrameId.LargeSidehopperOnCeilingLegsCrouched, LandSound),
        [P.LargeDessgeegaFloorJumping] = Jumping(EnemyFrameId.LargeDessgeegaLegsStraight),
        [P.LargeDessgeegaFloorLanded] = Landed(EnemyFrameId.LargeDessgeegaLegsBent, EnemyFrameId.LargeDessgeegaLegsCrouched),
        [P.LargeDessgeegaCeilingJumping] = Jumping(EnemyFrameId.LargeDessgeegaOnCeilingLegsStraight),
        [P.LargeDessgeegaCeilingLanded] = Landed(EnemyFrameId.LargeDessgeegaOnCeilingLegsBent, EnemyFrameId.LargeDessgeegaOnCeilingLegsCrouched),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<HopperSegment, Animation<HopperSegment>> StartingAt { get; } = new Dictionary<HopperSegment, Animation<HopperSegment>>
    {
        [P.SidehopperFloorJumping] = new([Segments.Whole(P.SidehopperFloorJumping)], AnimationEnd.Holds),
        [P.SidehopperFloorLanded] = new([Segments.Whole(P.SidehopperFloorLanded)], AnimationEnd.Holds),
        [P.SidehopperCeilingJumping] = new([Segments.Whole(P.SidehopperCeilingJumping)], AnimationEnd.Holds),
        [P.SidehopperCeilingLanded] = new([Segments.Whole(P.SidehopperCeilingLanded)], AnimationEnd.Holds),
        [P.DessgeegaFloorJumping] = new([Segments.Whole(P.DessgeegaFloorJumping)], AnimationEnd.Holds),
        [P.DessgeegaFloorLanded] = new([Segments.Whole(P.DessgeegaFloorLanded)], AnimationEnd.Holds),
        [P.DessgeegaCeilingJumping] = new([Segments.Whole(P.DessgeegaCeilingJumping)], AnimationEnd.Holds),
        [P.DessgeegaCeilingLanded] = new([Segments.Whole(P.DessgeegaCeilingLanded)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorJumping] = new([Segments.Whole(P.LargeSidehopperFloorJumping)], AnimationEnd.Holds),
        [P.LargeSidehopperFloorLanded] = new([Segments.Whole(P.LargeSidehopperFloorLanded)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingJumping] = new([Segments.Whole(P.LargeSidehopperCeilingJumping)], AnimationEnd.Holds),
        [P.LargeSidehopperCeilingLanded] = new([Segments.Whole(P.LargeSidehopperCeilingLanded)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorJumping] = new([Segments.Whole(P.LargeDessgeegaFloorJumping)], AnimationEnd.Holds),
        [P.LargeDessgeegaFloorLanded] = new([Segments.Whole(P.LargeDessgeegaFloorLanded)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingJumping] = new([Segments.Whole(P.LargeDessgeegaCeilingJumping)], AnimationEnd.Holds),
        [P.LargeDessgeegaCeilingLanded] = new([Segments.Whole(P.LargeDessgeegaCeilingLanded)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HopperSegment> StartingWith(HopperSegment segment) => StartingAt[segment];

    /// <summary>A Sidehopper's hop sound, which the Dessgeegas' hops do not make.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> HopSound => Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Hop));

    /// <summary>A Sidehopper's landing sound, which the Dessgeegas' landings do not make.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> LandSound => Cue(new HopperCue.PlaySound(HopperSoundDefinitions.Land));

    /// <summary>
    /// A hopper's hop on its surface: it runs off screen while airborne, makes its <paramref name="sound"/>, if any, and holds
    /// <paramref name="legsStraight"/>.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, HopperCue>[] Jumping(EnemyFrameId legsStraight, params AnimationBeat<EnemyFrameId, HopperCue>[] sound) =>
        [Cue(new HopperCue.Signal(K.ProcessOffScreen)), .. sound, Hold(legsStraight)];

    /// <summary>
    /// A hopper's landing on its surface: it stops running off screen, makes its <paramref name="sound"/>, if any, bends and
    /// crouches twice, <paramref name="legsBent"/> and <paramref name="legsCrouched"/>, and is ready to hop again.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, HopperCue>[] Landed(EnemyFrameId legsBent, EnemyFrameId legsCrouched,
        params AnimationBeat<EnemyFrameId, HopperCue>[] sound) =>
    [
        Cue(new HopperCue.Signal(K.StopProcessingOffScreen)),
        .. sound,
        Show(legsBent, 2),
        Show(legsCrouched, 5),
        Show(legsBent, 2),
        Show(legsCrouched, 3),
        Cue(new HopperCue.Signal(K.ReadyToHop)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HopperCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, HopperCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HopperCue> Cue(HopperCue cue) => new AnimationBeat<EnemyFrameId, HopperCue>.Event(cue);
}
