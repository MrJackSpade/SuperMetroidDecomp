using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The Zoa species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ZoaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zoa];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new ZoaEntity(place, common));

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

    /// <summary>Initializes the Zoa, saving the population coordinate and hiding the idle actor.</summary>
    internal static void InitializeZoa(RoomEnemyPlace place)
    {
        var state = new ZoaEnemyState(ZoaAction.WaitForSamus)
        {
            Facing = HorizontalDirection.Left,
            LaunchSpeed = SignedSixteenSixteen.Zero,
            SpawnPosition = place.Position.Whole,
        };
        place.SetSpeciesState(state);

        // A resting Zoa holds the left-facing shooting animation it starts with, so asking for it again at rest starts
        // nothing.
        state.Playback = new(ZoaAnimations.StartingWith(ZoaSegment.FacingLeftShooting), AnimationPlace.StartOfAnimation);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs a Zoa for one update: its proximity wait, rise or horizontal shot, from the room state.</summary>
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

    /// <summary>Wakes when Samus enters the 128-pixel column and rises facing her.</summary>
    private static void RunZoaWait(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus)
    {
        if (Math.Abs(samus.Position.X.Whole - place.Position.X.Whole) >= ZoaSpeedDefinitions.ActivationDistance)
            return;

        // A waiting Zoa stands at its spawn, inside the room, and rises facing Samus.
        state.Facing = samus.Position.X.Whole < place.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        SetZoaAnimation(place, state, ZoaAnimationChoices.Rising(state.Facing));
        state.Action = ZoaAction.Rising;
    }

    /// <summary>Reveals Zoa and rises until it reaches Samus's height, then starts its shooting animation.</summary>
    private static void RunZoaRising(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus)
    {
        place.Presence = place.Presence.Without(EnemyPresence.Hidden);
        if (samus.Position.Y.Whole < place.Position.Y.Whole)
        {
            // The Zoa rises half a pixel each update.
            place.Position = place.Position.AdvanceY(-ZoaSpeedDefinitions.RisingSpeed);
            return;
        }

        SetZoaAnimation(place, state, ZoaAnimationChoices.Shooting(state.Facing));
        state.Action = ZoaAction.Shooting;
    }

    /// <summary>Travels in the selected facing direction and resets whole-pixel position when its center exits the camera square.</summary>
    private static void RunZoaShooting(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        Position16<RoomPixels> camera)
    {
        SignedSixteenSixteen speed = state.LaunchSpeed;

        // The shot travels the way the Zoa faces. The shooting animation's cues set the speed as it starts and after 64
        // and 72 updates.
        SignedSixteenSixteen xDisplacement = speed.Toward(state.Facing);
        place.Position = place.Position.AdvanceX(xDisplacement);
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(place, camera))
        {
            SetZoaAnimation(place, state, ZoaAnimationChoices.Shooting(state.Facing));
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
        place.Position = place.Position.WithWhole(state.SpawnPosition);
        state.Facing = HorizontalDirection.Left;
        SetZoaAnimation(place, state, ZoaAnimationChoices.Shooting(state.Facing));
        state.Action = ZoaAction.WaitForSamus;
    }

    /// <summary>Starts <paramref name="segment"/>'s animation, restarting its timers, only when it differs from the one playing (#2145).</summary>
    private static void SetZoaAnimation(RoomEnemyPlace place, ZoaEnemyState state, ZoaSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, ZoaAnimations.StartingWith(segment));

    /// <summary>Runs Zoa's main AI.</summary>
    internal static void RunZoaMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunZoaMain(place, place.RequireSpeciesState<ZoaEnemyState>(), frame.Samus, frame.Camera);
    }
}
