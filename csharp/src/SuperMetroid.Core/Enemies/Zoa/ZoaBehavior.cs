using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The Zoa species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ZoaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zoa];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ZoaEntity(place, common);

    /// <summary>Advances Zoa's typed animation (#641) in place of the shared enemy animation.</summary>
    internal static void AdvanceZoaAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        ZoaEnemyState state = place.RequireSpeciesState<ZoaEnemyState>();
        AnimationPlayback<ZoaSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, ZoaAnimations.Segments, ref playback,
            cue =>
            {
                ApplyZoaCue(state, cue);
                return null;
            });
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of Zoa's animation: it selects the launch's speed.</summary>
    private static void ApplyZoaCue(ZoaEnemyState state, ZoaCue cue) => state.LaunchSpeed = cue.LaunchSpeed;

    /// <summary>Samus must be within 128 horizontal pixels before a hidden Zoa rises.</summary>
    private const int ZoaActivationColumnDistance = 128;
    /// <summary>Upward travel of half a pixel per gameplay frame.</summary>
    private static readonly SignedSixteenSixteen ZoaRisingSpeed = SignedSixteenSixteen.FromParts(0, 32768);

    /// <summary>Initializes the Zoa, saving the population coordinate and hiding the idle actor.</summary>
    internal static void InitializeZoa(RoomEnemyPlace place)
    {
        var state = new ZoaEnemyState(ZoaAction.WaitForSamus)
        {
            RequestedAnimation = ZoaSegment.FacingLeftShooting,
            InstalledAnimation = ZoaSegment.FacingLeftShooting,
            LaunchSpeed = SignedSixteenSixteen.Zero,
            SpawnPosition = place.Position.Whole,
        };
        place.SetSpeciesState(state);

        // The initializer starts the left-facing shooting animation directly and records it as started, so asking for it
        // again at rest starts nothing.
        state.Playback = new(ZoaAnimations.StartingWith(ZoaSegment.FacingLeftShooting), AnimationPlace.StartOfAnimation);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs a Zoa's per-frame behaviour: its proximity wait, rise or horizontal shot, from the room state.</summary>
    private static void RunZoaMain(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Action)
        {
            case ZoaAction.WaitForSamus:
                RunZoaWait(place, state, samus);
                return;
            case ZoaAction.Rising:
                RunZoaRising(place, state, samus);
                return;
            case ZoaAction.Shooting:
                RunZoaShooting(place, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Zoa has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Wakes when Samus enters the 128-pixel column and selects a rising pose from the wrapped horizontal direction.</summary>
    internal static void RunZoaWait(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus)
    {
        if (CoordinateAxis.Distance(samus.Position.X.Whole, place.Position.X.Whole) >= ZoaActivationColumnDistance)
            return;

        // The Zoa rises facing Samus, judged by the same wrapped distance from the Zoa to her.
        state.RequestedAnimation = ZoaAnimationChoices.Rising(
            facingRight: !CoordinateAxis.Precedes(samus.Position.X.Whole, place.Position.X.Whole));
        SetZoaAnimation(place, state);
        state.Action = ZoaAction.Rising;
    }

    /// <summary>Reveals Zoa and rises until it reaches Samus's height, then starts its shooting animation.</summary>
    internal static void RunZoaRising(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus)
    {
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, place.Position.Y.Whole) < 0)
        {
            // The Japanese and North American release subtracts exactly 0.5 px/frame; the European release's step is 0.625.
            // This project targets the Japanese and North American retail release and reads its data.
            place.Position = place.Position.AdvanceY(-ZoaRisingSpeed);
            return;
        }

        state.RequestedAnimation = ZoaAnimationChoices.ShootingWithSameFacing(state.RequestedAnimation);
        SetZoaAnimation(place, state);
        state.Action = ZoaAction.Shooting;
    }

    /// <summary>Travels in the selected facing direction and resets whole-pixel position when its center exits the camera square.</summary>
    internal static void RunZoaShooting(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        Position16<RoomPixels> camera)
    {
        SignedSixteenSixteen speed = state.LaunchSpeed;

        // Shooting facing left travels left; any other animation travels right. The shooting animation's cues change the
        // speed at its 64-, 8- and 48-update marks.
        SignedSixteenSixteen xDisplacement = state.RequestedAnimation == ZoaSegment.FacingLeftShooting
            ? -speed
            : speed;
        place.Position = place.Position.AdvanceX(xDisplacement);
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(place, camera))
        {
            SetZoaAnimation(place, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        // The return stores only the whole pixels: the subpixel fractions left by the
        // rise and the shot carry into the next flight.
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        place.Position = place.Position.WithWhole(state.SpawnPosition);
        state.RequestedAnimation = ZoaSegment.FacingLeftShooting;
        SetZoaAnimation(place, state);
        state.Action = ZoaAction.WaitForSamus;
    }

    /// <summary>Starts the selected animation, restarting its timers, only when it differs from the previous one.</summary>
    internal static void SetZoaAnimation(RoomEnemyPlace place, ZoaEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = EnemyAnimationPlayback.Start(place, ZoaAnimations.StartingWith(state.RequestedAnimation));
    }

    /// <summary>Runs Zoa's main AI.</summary>
    internal static void RunZoaMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunZoaMain(place, place.RequireSpeciesState<ZoaEnemyState>(), frame.Samus, frame.Camera);
    }
}
