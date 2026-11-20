using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The Zoa species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ZoaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Zoa], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new ZoaEntity(place, common));

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

        // The setup starts the left-facing shooting animation directly, leaving the frame timer as the spawn set it,
        // so asking for it again at rest starts nothing.
        state.Playback = new(ZoaAnimations.StartingWith(ZoaSegment.FacingLeftShooting), AnimationPlace.StartOfAnimation);
        place.Presence = place.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>What <c>RunZoaMain</c> does for each <see cref="ZoaAction"/> member, each member's own arm.</summary>
    private static IdentityTable<ZoaAction, Action<RoomEnemyPlace, ZoaEnemyState, SamusState, Position16<RoomPixels>>> RunZoaMainForZoaAction => field ??= new(new Dictionary<ZoaAction, Action<RoomEnemyPlace, ZoaEnemyState, SamusState, Position16<RoomPixels>>>
    {
        [ZoaAction.WaitForSamus] = static (place, state, samus, camera) => RunZoaWait(place, state, samus),
        [ZoaAction.Rising] = static (place, state, samus, camera) => RunZoaRising(place, state, samus),
        [ZoaAction.Shooting] = static (place, state, samus, camera) => RunZoaShooting(place, state, camera),
    });

    /// <summary>Runs a Zoa for one update: its proximity wait, rise or horizontal shot, from the room state.</summary>
    private static void RunZoaMain(
        RoomEnemyPlace place,
        ZoaEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        RunZoaMainForZoaAction[state.Action](place, state, samus, camera);
    }

    /// <summary>Wakes when Samus comes within its activation distance across and rises facing her.</summary>
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
            // The Zoa rises by its rising speed each update.
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

        // The shot travels the way the Zoa faces. The shooting animation's cues set the speed as it starts and at its later
        // beats.
        SignedSixteenSixteen xDisplacement = speed.Toward(state.Facing);
        place.Position = place.Position.AdvanceX(xDisplacement);
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(place, camera))
        {
            SetZoaAnimation(place, state, ZoaAnimationChoices.Shooting(state.Facing));
            return;
        }

        // Once its center leaves the camera's inclusive view, Zoa teleports to the
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
