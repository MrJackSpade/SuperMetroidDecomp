using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The Kzan species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KzanBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.KzanBottom, EnemyDefinitionId.KzanTop];

    /// <summary>A Kzan's top, or the bottom that follows it (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no Kzan definition.</exception>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(occupant).DefinitionId;
        if (definition == EnemyDefinitionId.KzanTop)
            return new KzanTopEntity(this, occupant, place, common);
        if (definition == EnemyDefinitionId.KzanBottom)
            return new KzanBottomEntity(this, occupant, place, common);
        throw new InvalidOperationException($"The Kzan species creates no {occupant.DefinitionId}.");
    }

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

    /// <summary>Initializes the top's idle animation, fall speed, fall distance and wait timer from its spawn settings.</summary>
    internal static void InitializeKzanTop(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new KzanEnemyState(KzanAction.WaitingToFall);
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, KzanAnimations.StartingWith(KzanSegment.Idle));

        // The spawn settings name the speed the fall starts at. Placements include four pixels per update,
        // the fastest the fall reaches.
        state.FallingSpeed = occupant.SpawnSettings<KzanSpawnSettings>().FallSpeed;

        // The spawn settings name the fall distance in pixels and the top's wait. A wait of zero does not run out at
        // once: it runs the full countdown (WrappingCountdown).
        int spawnY = place.Position.Y.Whole;
        state.FallingTargetY = spawnY + occupant.SpawnSettings<KzanSpawnSettings>().FallDistance;
        state.RisingTargetY = spawnY;
        state.FallWaitTimer = WrappingCountdown.Start(occupant.SpawnSettings<KzanSpawnSettings>().FallWait);
    }

    /// <summary>Places the lower actor twelve pixels below the preceding top during setup.</summary>
    /// <exception cref="InvalidDataException">The enemy before the bottom is not a Kzan top.</exception>
    internal void InitializeKzanBottom(RoomEnemyPlace place)
    {
        // The bottom follows whatever enemy precedes it. Every room with Kzans (the Wasteland and the
        // Wrecked Ship's main shaft) places each bottom directly after its top, so that enemy is its top; any
        // other placement is refused rather than followed.
        if (_places.ListedBeforeOrNone(place, 1) is not { DefinitionId: EnemyDefinitionId.KzanTop } top)
        {
            throw new InvalidDataException(
                $"Kzan bottom in place {place.Ordinal} does not directly follow a Kzan top, as every room with Kzans places it.");
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
        bottom.Position = bottom.Position.WithWhole(new(top.Position.X.Whole, top.Position.Y.Whole + KzanMotionDefinitions.BottomBelowTop));
    }

    /// <summary>Runs the top actor through fall wait, accelerated descent, rise wait, and half-pixel ascent.</summary>
    private void RunKzanTopMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        switch (state.Action)
        {
            case KzanAction.WaitingToFall:
                RunKzanWaitingToFall(occupant, place, state);
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
    private static void RunKzanWaitingToFall(RoomEnemyOccupant occupant, RoomEnemyPlace place, KzanEnemyState state)
    {
        state.FallWaitTimer = state.FallWaitTimer.Decrement().Remaining;
        if (!state.FallWaitTimer.IsZero)
            return;

        state.FallWaitTimer = WrappingCountdown.Start(occupant.SpawnSettings<KzanSpawnSettings>().FallWait);
        state.FallingSpeed = occupant.SpawnSettings<KzanSpawnSettings>().FallSpeed;
        state.Action = KzanAction.Falling;
    }

    /// <summary>Applies the downward speed, stops at the target, carries Samus, and accelerates the fall.</summary>
    private void RunKzanFalling(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        int previousY = place.Position.Y.Whole;
        place.Position = place.Position.AdvanceY(state.FallingSpeed);

        // The top lands once it reaches its falling target.
        if (place.Position.Y.Whole >= state.FallingTargetY)
        {
            state.RiseWaitTimer = UpdateCountdown.For(KzanMotionDefinitions.RiseWait);
            state.Action = KzanAction.WaitingToRise;
            place.Position = place.Position.WithWholeY(state.FallingTargetY);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.KzanLandingSoundEffect, priority: SoundRequestPriority.Normal);
        }

        CarrySamusWithKzanIfTouchingFromBelow(place, previousY, samus);

        state.FallingSpeed = SignedSixteenSixteen.Min(state.FallingSpeed + KzanMotionDefinitions.FallAcceleration, KzanMotionDefinitions.FastestFall);
    }

    /// <summary>Counts down the landing hold (<see cref="KzanMotionDefinitions.RiseWait"/>) before switching to ascent.</summary>
    private static void RunKzanWaitingToRise(KzanEnemyState state)
    {
        if (state.RiseWaitTimer.StepEnds())
            state.Action = KzanAction.Rising;
    }

    /// <summary>Subtracts one-half pixel per update, stops at the spawn height, and carries Samus by the whole-pixel step.</summary>
    private static void RunKzanRising(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        int previousY = place.Position.Y.Whole;

        // The Kzan rises by exactly half a pixel per update.
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(KzanMotionDefinitions.RisingStep));
        if (place.Position.Y.Whole < state.RisingTargetY)
        {
            state.Action = KzanAction.WaitingToFall;
            place.Position = place.Position.WithWholeY(state.RisingTargetY);
        }

        CarrySamusWithKzanIfTouchingFromBelow(place, previousY, samus);
    }

    /// <summary>
    /// Carries Samus, standing on the Kzan, by the whole pixels it moved this update from <paramref name="previousY"/>,
    /// its room Y before the move (#2473); her fraction of a pixel is left as it is.
    /// </summary>
    private static void CarrySamusWithKzanIfTouchingFromBelow(
        RoomEnemyPlace place,
        int previousY,
        SamusState samus)
    {
        if (!EnemyCollision.IsSamusStandingOn(place, samus, KzanMotionDefinitions.SamusStandingBias))
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, place.Position.Y.Whole - previousY);
    }

    /// <summary>Advances Kzan's typed animation.</summary>
    internal static void AdvanceKzanTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, KzanAnimations.Segments,
            static (KzanEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Kzan animation raised {cue}."));

    /// <summary>Runs KzanTop's main AI.</summary>
    internal void RunKzanTopMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunKzanTopMain(occupant, place, place.RequireSpeciesState<KzanEnemyState>(), frame.Samus);
    }
}
