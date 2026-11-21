using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class BoyonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the species owner on the room's enemy audio queue.</summary>
    internal BoyonBehavior(EnemyAudioQueue audio) => _audio = audio;

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Boyon], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new BoyonEntity(this, occupant, place, common));

    /// <summary>What <see cref="ApplyBoyonCue"/> does for each <see cref="BoyonCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<BoyonCueKind, Action<RoomEnemyPlace, BoyonEnemyState>> ApplyBoyonCueForBoyonCueKind => field ??= new(new Dictionary<BoyonCueKind, Action<RoomEnemyPlace, BoyonEnemyState>>
    {
        [BoyonCueKind.PlayBounceSound] = (place, state) => PlayBoyonBounceSound(),
        [BoyonCueKind.ProcessOffScreen] = (place, state) => place.Processing = place.Processing.With(EnemyProcessing.OffScreen),
        [BoyonCueKind.StopProcessingOffScreen] = (place, state) => place.Processing = place.Processing.Without(EnemyProcessing.OffScreen),
    });

    /// <summary>Carries out one cue of Boyon's animations.</summary>
    private void ApplyBoyonCue(RoomEnemyPlace place, BoyonEnemyState state, BoyonCue cue)
    {
        ApplyBoyonCueForBoyonCueKind[cue.Kind](place, state);
    }

    /// <summary>Initializes the Boyon.</summary>
    internal static void InitializeBoyon(RoomEnemyPlace place)
    {
        // The Boyon shows nothing until its idle animation shows its first frame on its first update; the room's
        // enemy load leaves it showing nothing too.
        place.Frame = EnemyFrameId.PointAtOrigin;

        var state = new BoyonEnemyState
        {
            CurvePlace = BoyonCurvePlace.Top,
            BaselineCurvePlace = BoyonCurvePlace.Top,
            Phase = BoyonPhase.DerivingArc,
        };
        place.SetSpeciesState(state);
        SetBoyonAnimation(place, state, BoyonSegment.Idle);
    }

    /// <summary>Runs the Boyon's main AI.</summary>
    internal static void RunBoyonMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, BoyonEnemyState state, SamusState samus)
    {
        // The game spends Boyon's first active AI frame deriving the arc's baseline and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (state.Phase == BoyonPhase.DerivingArc)
        {
            BoyonSpawnSettings settings = occupant.SpawnSettings<BoyonSpawnSettings>();
            DeriveBoyonArcBaseline(state, settings.SpeedMultiplier, SignedSixteenSixteen.FromWholePixels(settings.JumpHeight));
            return;
        }

        // An arc already under way plays on, wherever Samus stands.
        if (BounceMoveOf(state.Phase) is not { } bounce)
        {
            if (!IsBoyonSamusWithinHorizontalRange(occupant, place, samus))
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
            // Proximity permits another bounce: a resting Boyon starts bouncing and a landed one bounces again, its
            // bouncing animation restarting. Every bounce starts with its rise, which moves it on the same update.
            state.Phase = BoyonPhase.Rising;
            SetBoyonAnimation(place, state, BoyonSegment.Bouncing);
            bounce = MoveBoyonRising;
        }
        bounce(occupant, place, state);
    }

    /// <summary>
    /// How a bouncing Boyon moves in <paramref name="phase"/> (#2553): its rise or its fall; none while it derives its arc,
    /// rests or has landed, the phases that are not bouncing.
    /// </summary>
    private static Action<RoomEnemyOccupant, RoomEnemyPlace, BoyonEnemyState>? BounceMoveOf(BoyonPhase phase) => phase switch
    {
        BoyonPhase.Rising => MoveBoyonRising,
        BoyonPhase.Falling => MoveBoyonFalling,
        BoyonPhase.DerivingArc or BoyonPhase.Resting or BoyonPhase.Landed => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Undefined {nameof(BoyonPhase)}."),
    };

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
            distance += MultiplyBoyonCurveEntry(state, multiplier).AsDisplacement();
            state.FallOneEntry();
        }
        while (distance < jumpHeight);

        state.BaselineCurvePlace = state.CurvePlace;
        state.Phase = BoyonPhase.Resting;
    }

    /// <summary>Runs falling movement.</summary>
    private static void MoveBoyonFalling(RoomEnemyOccupant occupant, RoomEnemyPlace place, BoyonEnemyState state)
    {
        state.FallOneEntry();
        UnsignedSixteenSixteen speed = MultiplyBoyonCurveEntry(state, occupant.SpawnSettings<BoyonSpawnSettings>().SpeedMultiplier);

        // Taking the product's whole pixels discards its fraction. Boyon keeps no fraction of a pixel
        // here: each frame moves by exactly the product's whole pixels.
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + speed.Whole);
        if (state.CurvePlace < state.BaselineCurvePlace)
            return;

        state.Phase = BoyonPhase.Landed;
    }

    /// <summary>Runs rising movement.</summary>
    private static void MoveBoyonRising(RoomEnemyOccupant occupant, RoomEnemyPlace place, BoyonEnemyState state)
    {
        UnsignedSixteenSixteen speed = MultiplyBoyonCurveEntry(state, occupant.SpawnSettings<BoyonSpawnSettings>().SpeedMultiplier);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole - speed.Whole);
        if (state.RiseOneEntry())
            state.Phase = BoyonPhase.Falling;
    }

    /// <summary>
    /// The Boyon's speed at its place: the curve sample there scaled by its spawn settings'
    /// <paramref name="multiplier"/>; past the curve's end it keeps the curve's last speed.
    /// </summary>
    internal static UnsignedSixteenSixteen MultiplyBoyonCurveEntry(BoyonEnemyState state, int multiplier) =>
        BoyonBounceCurve.Multiply(state.CurvePlace, multiplier);

    /// <summary>Whether Samus lies within Boyon's authored horizontal reach, in pixel columns, of the enemy.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(RoomEnemyOccupant occupant, RoomEnemyPlace place, SamusState samus)
    {
        int distance = Math.Abs(samus.Position.X.Whole - place.Position.X.Whole);
        return distance < occupant.SpawnSettings<BoyonSpawnSettings>().TriggerDistance;
    }

    /// <summary>Starts the idle or bouncing animation from its start.</summary>
    private static void SetBoyonAnimation(RoomEnemyPlace place, BoyonEnemyState state, BoyonSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, BoyonAnimations.StartingWith(segment));
    }

    /// <summary>
    /// The bounce cue: request the bounce sound, all the cue does. The bouncing animation that raises it starts only
    /// as the Boyon starts bouncing, so the arc it permits is already under way (#2432).
    /// </summary>
    private void PlayBoyonBounceSound() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.BoyonBounceSound, priority: SoundRequestPriority.Normal);

    /// <summary>Runs Boyon's idle and bouncing loops (#641).</summary>
    internal void AdvanceBoyonAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, BoyonAnimations.Segments,
            static (BoyonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (BoyonEnemyState state, BoyonCue cue) => ApplyBoyonCue(place, state, cue));
}
