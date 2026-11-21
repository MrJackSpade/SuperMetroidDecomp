using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The Kzan species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KzanBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    /// <summary>A Kzan's top, from its settings, or the bottom that follows it (#2146).</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<KzanSpawnSettings>([EnemyDefinitionId.KzanTop], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new KzanTopEntity(this, occupant, place, common, settings)));
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.KzanBottom], (occupant, _, place, common) =>
            EnemyOccupant.Of(new KzanBottomEntity(this, occupant, place, common)));
    }

    /// <summary>The room's enemy places, where each bottom finds its top when it is placed.</summary>
    private readonly EnemyPlaces _places;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Couples each Kzan bottom to the top its room lists before it.</summary>
    internal KzanBehavior(EnemyPlaces places, EnemyAudioQueue audio)
    {
        _audio = audio;
        _places = places;
    }

    /// <summary>Initializes the top's idle animation, fall speed, fall distance and wait timer from its spawn settings.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static KzanEnemyState InitializeKzanTop(KzanSpawnSettings kzanSpawnSettings, RoomEnemyPlace place)
    {
        var state = new KzanEnemyState(KzanAction.WaitingToFall);

        state.Playback = EnemyAnimationPlayback.Start(place, KzanAnimations.StartingWith(KzanSegment.Idle));

        // The spawn settings name the speed the fall starts at. Placements include the fastest the fall
        // reaches.
        state.FallingSpeed = kzanSpawnSettings.FallSpeed;

        // The spawn settings name the fall distance in pixels and the top's wait. A wait of zero does not run out at
        // once: it runs the full countdown (WrappingCountdown).
        int spawnY = place.Position.Y.Whole;
        state.FallingTargetY = spawnY + kzanSpawnSettings.FallDistance;
        state.RisingTargetY = spawnY;
        state.FallWaitTimer = WrappingCountdown.Start(kzanSpawnSettings.FallWait);
        return state;
    }

    /// <summary>The place of the top <paramref name="bottom"/> follows, the enemy its room lists directly before it.</summary>
    internal RoomEnemyPlace TopOf(RoomEnemyPlace bottom) =>
        _places.Member(bottom, KzanPartListingDefinitions.Listing, EnemyDefinitionId.KzanBottom, EnemyDefinitionId.KzanTop);

    /// <summary>
    /// Keeps the lower actor below <paramref name="top"/> (<see cref="KzanMotionDefinitions.BottomBelowTop"/>), the top it
    /// found when it was placed, whatever that place holds now: as it is set up and on every enemy update.
    /// </summary>
    internal static void FollowKzanTop(RoomEnemyPlace bottom, RoomEnemyPlace top)
    {
        bottom.Position = bottom.Position.WithWhole(new(top.Position.X.Whole, top.Position.Y.Whole + KzanMotionDefinitions.BottomBelowTop));
    }

    /// <summary>What <see cref="RunKzanTopMain"/> does for each <see cref="KzanAction"/> member, each member's own arm.</summary>
    private IdentityTable<KzanAction, Action<KzanSpawnSettings, RoomEnemyPlace, KzanEnemyState, SamusState>> RunKzanTopMainForKzanAction => field ??= new(new Dictionary<KzanAction, Action<KzanSpawnSettings, RoomEnemyPlace, KzanEnemyState, SamusState>>
    {
        [KzanAction.WaitingToFall] = (settings, place, state, samus) => RunKzanWaitingToFall(settings, state),
        [KzanAction.Falling] = (_, place, state, samus) => RunKzanFalling(place, state, samus),
        [KzanAction.WaitingToRise] = (_, place, state, samus) => RunKzanWaitingToRise(state),
        [KzanAction.Rising] = (_, place, state, samus) => RunKzanRising(place, state, samus),
    });

    /// <summary>Runs the top actor through fall wait, accelerated descent, rise wait, and slow ascent.</summary>
    private void RunKzanTopMain(
        KzanSpawnSettings kzanSpawnSettings, RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        RunKzanTopMainForKzanAction[state.Action](kzanSpawnSettings, place, state, samus);
    }

    /// <summary>Decrements the wrapping top wait and resets the fall speed when descent begins.</summary>
    private static void RunKzanWaitingToFall(KzanSpawnSettings kzanSpawnSettings, KzanEnemyState state)
    {
        state.FallWaitTimer = state.FallWaitTimer.Decrement().Remaining;
        if (!state.FallWaitTimer.IsZero)
            return;

        state.FallWaitTimer = WrappingCountdown.Start(kzanSpawnSettings.FallWait);
        state.FallingSpeed = kzanSpawnSettings.FallSpeed;
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

    /// <summary>Rises by <see cref="KzanMotionDefinitions.RisingStep"/> each update, stops at the spawn height, and carries Samus by the whole-pixel step.</summary>
    private static void RunKzanRising(
        RoomEnemyPlace place,
        KzanEnemyState state,
        SamusState samus)
    {
        int previousY = place.Position.Y.Whole;

        // The Kzan rises by exactly its rising step each update.
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
    internal static void AdvanceKzanTypedAnimation(RoomEnemyPlace place, KzanEnemyState kzanEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, kzanEnemyState, KzanAnimations.Segments,
            static (KzanEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs KzanTop's main AI.</summary>
    internal void RunKzanTopMainAi(KzanSpawnSettings kzanSpawnSettings, RoomEnemyPlace place, KzanEnemyState kzanEnemyState, IEnemyAiFrame frame)
    {
        RunKzanTopMain(kzanSpawnSettings, place, kzanEnemyState, frame.Samus);
    }
}
