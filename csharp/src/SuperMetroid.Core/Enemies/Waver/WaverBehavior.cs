using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The Waver species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WaverBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Waver];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new WaverEntity(this, place, common);

    /// <summary>Room collision service used to reflect horizontal motion and the sine wave on impact.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Connects Waver AI to room terrain collision.</summary>
    internal WaverBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out Waver's one animation cue.</summary>
    private static void ApplyWaverCue(WaverEnemyState state, WaverCue cue)
    {
        switch (cue.Kind)
        {
            case WaverCueKind.FinishSpin:
                // The four-frame spin hands its completion back to main AI rather than
                // going on to the steady animation itself.
                state.SpinFinished = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WaverCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Horizontal speed of one and a half pixels per update.</summary>
    private static readonly SignedSixteenSixteen WaverHorizontalSpeed = SignedSixteenSixteen.FromParts(1, 32768);
    /// <summary>Four-pixel amplitude used by Waver's negative-sine vertical motion.</summary>
    private const int WaverVerticalRadius = 4;

    /// <summary>Initializes the Waver.</summary>
    internal static void InitializeWaver(RoomEnemyPlace place)
    {
        HorizontalDirection facing = place.SpawnSettings<WaverSpawnSettings>().Facing;
        SignedSixteenSixteen signedSpeed = facing == HorizontalDirection.Right
            ? WaverHorizontalSpeed
            : -WaverHorizontalSpeed;
        var state = new WaverEnemyState
        {
            XVelocity = signedSpeed,
            InstalledAnimation = WaverSegment.SteadyFacingLeft,
            Angle = RotationAngle.Zero,
            SpinFinished = false,
            RequestedAnimation = WaverAnimationChoices.Steady(facing),
        };
        place.SetSpeciesState(state);

        // The initializer first installs the left steady animation and then asks for the spawn's facing. Only
        // right-facing spawns therefore change it; left-facing spawns ask for the installed one and keep it.
        state.Playback = new(WaverAnimations.StartingWith(WaverSegment.SteadyFacingLeft), AnimationPlace.StartOfAnimation);
        SetWaverAnimation(place, state);
    }

    /// <summary>Runs the Waver's main AI.</summary>
    private void RunWaverMain(
        RoomEnemyPlace place,
        WaverEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen horizontalDisplacement = state.XVelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                place,
                horizontalDisplacement))
        {
            // The wall reversal negates the velocity's whole and fraction parts and stores them back
            // together. For every initialized Waver this is exactly a signed 1/65536-pixel
            // negation; expressing that value directly preserves the game's ±1.5 px/frame.
            state.XVelocity = -horizontalDisplacement;
            // A wall turns Waver around and ends any spin.
            state.RequestedAnimation = WaverAnimationChoices.TurnedAround(state.RequestedAnimation);
            SetWaverAnimation(place, state);
        }
        else
        {
            int verticalPixels = EnemyMath.CircleNegativeSinePixels(
                state.Angle.ToTrigonometryAngle(),
                WaverVerticalRadius);
            if (_terrain.MoveEnemyVertically(level, place, SignedSixteenSixteen.FromWholePixels(verticalPixels)))
            {
                state.ReflectWave();
            }
            else
            {
                state.AdvanceWave();
            }
        }

        // Once per half-wave, the phase's position within its half-turn equals 56. Waver temporarily swaps to
        // the four-map spin without changing its facing.
        if (state.Angle.ToTrigonometryAngle().StepsIntoHalfTurn == 56)
        {
            state.RequestedAnimation = WaverAnimationChoices.Spinning(state.RequestedAnimation);
            SetWaverAnimation(place, state);
        }

        // The spin animation sets this flag after 32 animation frames. Main AI owns
        // the return to steady art, matching the cue/AI seam rather than timing it here.
        if (state.SpinFinished)
        {
            state.SpinFinished = false;
            state.RequestedAnimation = WaverAnimationChoices.SteadyWithSameFacing(state.RequestedAnimation);
            SetWaverAnimation(place, state);
        }
    }

    /// <summary>Starts a changed facing or spin animation and restarts its frame timer.</summary>
    internal static void SetWaverAnimation(RoomEnemyPlace place, WaverEnemyState state)
    {
        WaverSegment requested = state.RequestedAnimation;
        if (requested == state.InstalledAnimation)
            return;

        state.InstalledAnimation = requested;
        state.Playback = EnemyAnimationPlayback.Start(place, WaverAnimations.StartingWith(requested));
    }

    /// <summary>Runs Waver's main AI.</summary>
    internal void RunWaverMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunWaverMain(place, place.RequireSpeciesState<WaverEnemyState>(), frame.Level);
    }

    /// <summary>Advances Waver's typed animation.</summary>
    internal static void AdvanceWaverTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, WaverAnimations.Segments,
            static (WaverEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyWaverCue(state, cue);
                return null;
            });
}
