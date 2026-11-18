using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BoyonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal BoyonBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Boyon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BoyonEntity(this, place, common);

    /// <summary>Carries out one cue of Boyon's animations.</summary>
    private BoyonSegment? ApplyBoyonCue(RoomEnemyPlace place, BoyonEnemyState state, BoyonCue cue)
    {
        switch (cue.Kind)
        {
            case BoyonCueKind.StartBounce:
                StartBoyonBounce(state);
                break;
            case BoyonCueKind.ProcessOffScreen:
                place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
                break;
            case BoyonCueKind.StopProcessingOffScreen:
                place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoyonCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Initializes the Boyon.</summary>
    internal static void InitializeBoyon(RoomEnemyPlace place)
    {
        _ = place.SpawnSettings<BoyonSpawnSettings>();

        // The Boyon shows nothing until its idle animation shows its first frame on its first update; the room's
        // enemy load leaves it showing nothing too.
        place.Frame = EnemyFrameId.Empty;

        var state = new BoyonEnemyState
        {
            BounceMovement = BoyonBounceMovement.Rising,
            CurvePlace = BoyonCurvePlace.Top,
            BaselineCurvePlace = BoyonCurvePlace.Top,
            Phase = BoyonPhase.DerivingArc,
        };
        place.SetSpeciesState(state);
        SetBoyonAnimation(place, state, BoyonSegment.Idle);
    }

    /// <summary>Runs the Boyon's main AI.</summary>
    internal static void RunBoyonMain(RoomEnemyPlace place, BoyonEnemyState state, SamusState samus)
    {
        // The game spends Boyon's first active AI frame deriving the arc's baseline and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (state.Phase == BoyonPhase.DerivingArc)
        {
            BoyonSpawnSettings settings = place.SpawnSettings<BoyonSpawnSettings>();
            DeriveBoyonArcBaseline(state, settings.SpeedMultiplier, SignedSixteenSixteen.FromWholePixels(settings.JumpHeight));
            return;
        }

        if (IsBoyonSamusWithinHorizontalRange(place, samus))
        {
            // Proximity permits another bounce: a resting Boyon starts bouncing and a landed one bounces again, its
            // bouncing animation restarting; an arc already under way plays on.
            if (state.Phase != BoyonPhase.Bouncing)
            {
                state.Phase = BoyonPhase.Bouncing;
                SetBoyonAnimation(place, state, BoyonSegment.Bouncing);
            }
        }
        else if (state.Phase != BoyonPhase.Bouncing)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (state.Phase == BoyonPhase.Landed)
            {
                state.Phase = BoyonPhase.Resting;
                SetBoyonAnimation(place, state, BoyonSegment.Idle);
            }
            return;
        }

        switch (state.BounceMovement)
        {
            case BoyonBounceMovement.Falling:
                MoveBoyonFalling(place, state);
                break;
            case BoyonBounceMovement.Rising:
                MoveBoyonRising(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Boyon has no behaviour for bounce movement {state.BounceMovement}.");
        }
    }

    /// <summary>
    /// Derives the arc's baseline: from the place it stands, the Boyon moves down its curve one entry at a time, adding
    /// each entry's scaled sample to the arc's distance, until the distance reaches the jump height, compared plainly
    /// (#2282); the place it then stands at is where every fall ends, and it rests there. Each entry past the top adds at
    /// least one 256th of a pixel, so the derivation ends; the state holds no multiplier below one.
    /// </summary>
    /// <param name="state">The Boyon.</param>
    /// <param name="multiplier">Its spawn settings' curve multiplier, one or more.</param>
    /// <param name="jumpHeight">
    /// Its spawn settings' arc height, 48 to 96 pixels for Boyons, compared plainly with the arc's distance (#2282).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">The arc is lower than nothing.</exception>
    internal static void DeriveBoyonArcBaseline(BoyonEnemyState state, int multiplier, SignedSixteenSixteen jumpHeight)
    {
        if (jumpHeight.IsNegative)
            throw new ArgumentOutOfRangeException(nameof(jumpHeight), jumpHeight, "A Boyon's arc is no lower than nothing.");
        SignedSixteenSixteen distance = SignedSixteenSixteen.Zero;
        do
        {
            state.Speed = MultiplyBoyonCurveEntry(state, multiplier);
            distance += state.Speed.AsDisplacement();
            state.FallOneEntry();
        }
        while (distance.InTwoFiftySixths < jumpHeight.InTwoFiftySixths);

        state.BaselineCurvePlace = state.CurvePlace;
        state.Phase = BoyonPhase.Resting;
    }

    /// <summary>Runs falling movement.</summary>
    private static void MoveBoyonFalling(RoomEnemyPlace place, BoyonEnemyState state)
    {
        state.FallOneEntry();
        state.Speed = MultiplyBoyonCurveEntry(state, place.SpawnSettings<BoyonSpawnSettings>().SpeedMultiplier);

        // Taking the product's whole pixels discards its fraction. Boyon keeps no fraction of a pixel
        // here: each frame moves by exactly the product's whole pixels.
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + state.Speed.Whole);
        if (state.CurvePlace < state.BaselineCurvePlace)
            return;

        state.BounceMovement = BoyonBounceMovement.Rising;
        state.Phase = BoyonPhase.Landed;
    }

    /// <summary>Runs rising movement.</summary>
    private static void MoveBoyonRising(RoomEnemyPlace place, BoyonEnemyState state)
    {
        state.Speed = MultiplyBoyonCurveEntry(state, place.SpawnSettings<BoyonSpawnSettings>().SpeedMultiplier);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - state.Speed.Whole);
        if (state.RiseOneEntry())
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// The Boyon's speed at its place: the curve sample there scaled by its spawn settings' <paramref name="multiplier"/>,
    /// in 256ths of a pixel an update. Entries after the stored curve use the explicit saturated sample rather than
    /// reading past it.
    /// </summary>
    internal static UnsignedSixteenSixteen MultiplyBoyonCurveEntry(BoyonEnemyState state, int multiplier) =>
        UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(BoyonBounceCurve.Multiply(state.CurvePlace, multiplier));

    /// <summary>Whether Samus lies within Boyon's authored horizontal reach, in pixel columns, of the enemy.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(RoomEnemyPlace place, SamusState samus)
    {
        int distance = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole);
        return distance < place.SpawnSettings<BoyonSpawnSettings>().TriggerDistance;
    }

    /// <summary>Starts the idle or bounce animation and restarts its frame and auxiliary timers.</summary>
    private static void SetBoyonAnimation(RoomEnemyPlace place, BoyonEnemyState state, BoyonSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, BoyonAnimations.StartingWith(segment));
    }

    /// <summary>
    /// The bounce cue: request the bounce sound. The bouncing animation that raises it starts only as the Boyon starts
    /// bouncing, so the arc it permits is already under way (#2432).
    /// </summary>
    /// <exception cref="InvalidOperationException">The Boyon is not bouncing.</exception>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        if (state.Phase != BoyonPhase.Bouncing)
            throw new InvalidOperationException($"A Boyon's bounce cue ran in its {state.Phase} phase.");
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoyonBounceSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Runs Boyon's idle and bouncing loops (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, BoyonAnimations.Segments,
            static (BoyonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyBoyonCue(place, state, cue) is { } target
                ? new AnimationContinuation<BoyonSegment>(BoyonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
}
