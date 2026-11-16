using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The Kzan species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KzanBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. KzanPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new KzanEntity(this, place, common);

    /// <summary>The room's enemy places, where each bottom finds its top when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The top each bottom found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _tops = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Couples each Kzan bottom to the top its room lists before it.</summary>
    internal KzanBehavior(EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _places = places;
    }

    /// <summary>The pause at the bottom before the top begins to rise.</summary>
    private const int KzanRiseWaitFrames = 64;
    /// <summary>The rise of each rising update: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen KzanRisingStep = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
    /// <summary>The speed each falling update adds: one sixteenth of a pixel per update.</summary>
    private static readonly SignedSixteenSixteen KzanFallAcceleration = SignedSixteenSixteen.FromSixteenthsOfPixel(1);
    /// <summary>The fastest the falling acceleration reaches: four pixels per update.</summary>
    private static readonly SignedSixteenSixteen KzanMaximumFallingSpeed = SignedSixteenSixteen.FromParts(4, 0);

    /// <summary>The enemy-library landing sound published when the falling top reaches its target.</summary>
    private static readonly SoundEffectId KzanLandingSound =
        EnemySoundEffectDefinitions.KzanLandingSoundEffect;

    /// <summary>Initializes the top's idle animation, fall speed, fall distance and wait timer from its spawn settings.</summary>
    internal static void InitializeKzanTop(RoomEnemyPlace place)
    {
        var state = new KzanEnemyState(KzanAction.WaitingToFall);
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, KzanAnimations.StartingWith(KzanSegment.Idle));

        // The spawn settings name the speed the fall starts at. Retail placements include four pixels per update,
        // the fastest the fall reaches.
        state.FallingSpeed = place.SpawnSettings<KzanSpawnSettings>().FallSpeed;

        // The spawn settings name the fall distance in pixels and the top wait countdown.
        // A zero countdown deliberately wraps to -1 on its first decrement rather than
        // triggering immediately.
        int spawnY = place.Position.Y.Whole;
        state.FallingTargetY = CoordinateAxis.Wrap(spawnY + place.SpawnSettings<KzanSpawnSettings>().FallDistance);
        state.RisingTargetY = spawnY;
        state.FallWaitTimerResetValue = place.SpawnSettings<KzanSpawnSettings>().FallWait;
        state.FallWaitTimer = WrappingCountdown.Start(state.FallWaitTimerResetValue);
    }

    /// <summary>Places the lower actor twelve pixels below the preceding top during setup.</summary>
    /// <exception cref="InvalidDataException">The enemy before the bottom is not a Kzan top.</exception>
    internal void InitializeKzanBottom(RoomEnemyPlace place)
    {
        // The bottom follows whatever enemy precedes it. Every retail Kzan population (the Wasteland and the
        // Wrecked Ship's main shaft) places each bottom directly after its top, so that enemy is its top; any
        // other placement is refused rather than followed.
        if (_places.ListedBeforeOrNone(place, 1) is not { DefinitionId: EnemyDefinitionId.KzanTop } top)
        {
            throw new InvalidDataException(
                $"Kzan bottom in place {place.Ordinal} does not directly follow a Kzan top, as every retail Kzan population places it.");
        }
        _tops.Place(place, top);
        FollowKzanTop(place);
    }

    /// <summary>Keeps the lower actor below the top on every enemy update.</summary>
    internal void RunKzanBottomMain(RoomEnemyPlace place) => FollowKzanTop(place);

    /// <summary>
    /// Follows the top the bottom found when it was placed, the enemy its room lists directly before it, whatever that
    /// place holds now.
    /// </summary>
    private void FollowKzanTop(RoomEnemyPlace bottom)
    {
        RoomEnemyPlace top = _tops.Of(bottom);
        bottom.Position = bottom.Position.WithWhole(new(top.Position.X.Whole, CoordinateAxis.Wrap(top.Position.Y.Whole + KzanMotionDefinitions.BottomBelowTop)));
    }

    /// <summary>Runs the top actor through fall wait, accelerated descent, rise wait, and half-pixel ascent.</summary>
    private void RunKzanTopMain(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case KzanAction.WaitingToFall:
                RunKzanWaitingToFall(place, state);
                return;

            case KzanAction.Falling:
                RunKzanFalling(place, state, samus);
                return;

            case KzanAction.WaitingToRise:
                RunKzanWaitingToRise(state);
                return;

            case KzanAction.Rising:
                RunKzanRising(place, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Kzan has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Decrements the wrapping top wait and resets the fall speed when descent begins.</summary>
    private static void RunKzanWaitingToFall(RoomEnemyPlace place, KzanEnemyState state)
    {
        state.FallWaitTimer = state.FallWaitTimer.Decrement().Remaining;
        if (!state.FallWaitTimer.IsZero)
            return;

        state.FallWaitTimer = WrappingCountdown.Start(state.FallWaitTimerResetValue);
        state.FallingSpeed = place.SpawnSettings<KzanSpawnSettings>().FallSpeed;
        state.Action = KzanAction.Falling;
    }

    /// <summary>Applies the downward speed, clamps to the target using signed wrapped comparison, carries Samus, and accelerates the fall.</summary>
    private void RunKzanFalling(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        state.PreviousY = place.Position.Y.Whole;
        place.Position = place.Position.AdvanceY(state.FallingSpeed);

        // The comparison checks only whether the wrapped difference is negative, which the spatial difference
        // keeps: coordinates near the room's edge retain the same game behavior.
        if (!CoordinateAxis.Precedes(place.Position.Y.Whole, state.FallingTargetY))
        {
            state.RiseWaitTimer = KzanRiseWaitFrames;
            state.Action = KzanAction.WaitingToRise;
            place.Position = place.Position.WithWholeY(state.FallingTargetY);
            _audio.QueueEnemySound(KzanLandingSound, priority: SoundRequestPriority.Normal);
        }

        CarrySamusWithKzanIfTouchingFromBelow(place, state, samus);

        state.FallingSpeed = SignedSixteenSixteen.Min(state.FallingSpeed + KzanFallAcceleration, KzanMaximumFallingSpeed);
    }

    /// <summary>Counts down the 64-frame landing hold before switching to ascent.</summary>
    private static void RunKzanWaitingToRise(KzanEnemyState state)
    {
        if (--state.RiseWaitTimer == 0)
            state.Action = KzanAction.Rising;
    }

    /// <summary>Subtracts one-half pixel per update, stops at the spawn height, and carries Samus by the whole-pixel step.</summary>
    private static void RunKzanRising(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        state.PreviousY = place.Position.Y.Whole;

        // The Japanese and North American release rises by exactly half a pixel per update.
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(KzanRisingStep));
        if (CoordinateAxis.Precedes(place.Position.Y.Whole, state.RisingTargetY))
        {
            state.Action = KzanAction.WaitingToFall;
            place.Position = place.Position.WithWholeY(state.RisingTargetY);
        }

        CarrySamusWithKzanIfTouchingFromBelow(place, state, samus);
    }

    /// <summary>
    /// Kzan's private copy of the check whether an enemy touches Samus from below. Its only
    /// difference from the common helper is the shifted Y sample (<see cref="KzanMotionDefinitions.TouchSampleDrop"/>). Horizontal
    /// overlap is strict; vertical tangency is accepted after the shifted center test.
    /// </summary>
    private static bool IsKzanTouchingSamusFromBelow(
        RoomEnemyPlace place,
        SamusState samus)
    {
        int xDistance = CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole);
        if (xDistance >= samus.Kinematics.Radius.X + place.Radius.X)
            return false;

        int shiftedYDelta = CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole) + KzanMotionDefinitions.TouchSampleDrop;
        if (shiftedYDelta >= 0)
            return false;

        int yDistance = Math.Abs(shiftedYDelta);
        if (yDistance < samus.Kinematics.Radius.Y)
            return true;

        return yDistance - samus.Kinematics.Radius.Y <= place.Radius.Y;
    }

    /// <summary>
    /// Adds only the accepted whole-pixel Kzan delta to Samus's extra Y displacement.
    /// Code deliberately leaves extra Y subdisplacement untouched.
    /// </summary>
    private static void CarrySamusWithKzanIfTouchingFromBelow(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        if (!IsKzanTouchingSamusFromBelow(place, samus))
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, CoordinateAxis.Difference(place.Position.Y.Whole, state.PreviousY));
    }

    /// <summary>Advances Kzan's typed animation.</summary>
    internal static void AdvanceKzanTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, KzanAnimations.Segments,
            static (KzanEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Kzan animation raised {cue}."));

    /// <summary>Runs KzanTop's main AI.</summary>
    internal void RunKzanTopMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunKzanTopMain(place, place.RequireSpeciesState<KzanEnemyState>(), frame.Samus);
    }
}
