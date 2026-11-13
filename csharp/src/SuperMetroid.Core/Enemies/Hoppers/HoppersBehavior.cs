using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>The Hoppers species owner: its per-room state, AI behaviours and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class HoppersBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dessgeega, EnemyDefinitionId.DessgeegaLarge, EnemyDefinitionId.Sidehopper, EnemyDefinitionId.SidehopperLarge, EnemyDefinitionId.SidehopperTourian];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new HoppersEntity(this, slot, common);

    /// <summary>Supplies the shared RNG seed consumed during Hopper initialization.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves vertical and horizontal collisions in both floor and ceiling hops.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds the five Hopper definitions to shared room RNG and terrain movement.</summary>
    internal HoppersBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the hoppers' animations.</summary>
    private HopperSegment? ApplyHopperCue(RoomEnemySlot slot, HopperEnemyState state, HopperCue cue)
    {
        switch (cue.Kind)
        {
            case HopperCueKind.ProcessOffScreen:
                slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
                break;
            case HopperCueKind.StopProcessingOffScreen:
                slot.Processing = slot.Processing.Without(EnemyProcessing.OffScreen);
                break;
            case HopperCueKind.PlaySound:
                _audio.QueueEnemySound(cue.Sound ?? throw new InvalidOperationException("A hopper sound cue has no sound."), priority: SoundRequestPriority.Normal);
                break;
            case HopperCueKind.ReadyToHop:
                state.ReadyToHop = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(HopperCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Global RNG seed written when a Hopper initializes.</summary>
    private const int HopperRandomSeed = 37;
    /// <summary>Terminal place along the quadratic speed table, row 64, that caps the falling half of a hop.</summary>
    private static readonly EnemyQuadraticSpeedIndex MaximumHopperYSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(64);

    /// <summary>Initializes the Hopper.</summary>
    internal void InitializeHopper(RoomEnemySlot slot)
    {
        HopperKind kind = HopperAnimationChoices.KindOf(slot.DefinitionId);
        // The initializer overwrites the global RNG seed and consumes one generated value
        // even though it never reads that value. This changes later enemy randomness and is
        // therefore observable whenever a hopper appears earlier in population order.
        _room.Random.Seed(HopperRandomSeed);
        _room.Random.Generate();

        var state = new HopperEnemyState
        {
            Falling = false,
            ReadyToHop = false,
            Kind = kind,
            UpsideDown = slot.SpawnSettings<HopperSpawnSettings>().Mounting == HopperMounting.Ceiling,
            Action = HopperAction.ChooseHopSize,
        };
        slot.SetSpeciesState(state);

        SetHopperAnimation(slot, state, ReadHopperAnimation(state, SurfaceOf(state), HopperPhase.Landed));

        // Every Hopper definition authors the same small and big hop heights and speed steps, so the definition
        // selects nothing here; the two hop sizes' starting speeds are still calculated independently.
        state.SmallHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 4096,
            tableIndexDelta: 3);
        state.BigHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 12288,
            tableIndexDelta: 4);
    }

    /// <summary>Runs the Hopper's main AI and every indirect target.</summary>
    private void RunHopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        HopperEnemyState state = slot.RequireSpeciesState<HopperEnemyState>();
        switch (state.Action)
        {
            case HopperAction.ChooseHopSize:
                // The game consumes an entire frame choosing one of two setup functions.
                state.Action = HopperDecisions.BigHop.IsSetIn(_room.Random.Generate())
                    ? HopperAction.PrepareBigHop
                    : HopperAction.PrepareSmallHop;
                return;
            case HopperAction.PrepareSmallHop:
                PrepareHopperHop(state, HopperHopSize.Small);
                return;
            case HopperAction.PrepareBigHop:
                PrepareHopperHop(state, HopperHopSize.Big);
                return;
            case HopperAction.ChooseDirectionUpsideUp:
                ChooseHopperDirection(slot, state, samus, HopperSurface.Floor);
                return;
            case HopperAction.ChooseDirectionUpsideDown:
                ChooseHopperDirection(slot, state, samus, HopperSurface.Ceiling);
                return;
            case HopperAction.StartBackwardHopUpsideUp:
                StartHopperJump(slot, state, HopperSurface.Floor, HopperHopDirection.Backward);
                return;
            case HopperAction.StartForwardHopUpsideUp:
                StartHopperJump(slot, state, HopperSurface.Floor, HopperHopDirection.Forward);
                return;
            case HopperAction.StartBackwardHopUpsideDown:
                StartHopperJump(slot, state, HopperSurface.Ceiling, HopperHopDirection.Backward);
                return;
            case HopperAction.StartForwardHopUpsideDown:
                StartHopperJump(slot, state, HopperSurface.Ceiling, HopperHopDirection.Forward);
                return;
            case HopperAction.Landed:
                LandHopper(slot, state);
                return;
            case HopperAction.JumpingUpsideUpBackward:
            case HopperAction.JumpingUpsideUpForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level, HopperSurface.Floor);
                return;
            case HopperAction.JumpingUpsideDownBackward:
            case HopperAction.JumpingUpsideDownForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level, HopperSurface.Ceiling);
                return;
            case HopperAction.WaitToHop:
                if (state.ReadyToHop)
                {
                    state.ReadyToHop = false;
                    state.Action = HopperAction.ChooseHopSize;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Hopper has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Rejects movement dispatch before a room's collision plane is available.</summary>
    private static void RequireHopperLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Hopper movement requires room level data.");
    }

    /// <summary>Selects big or small quadratic-speed parameters and routes to the facing choice for the next hop.</summary>
    private static void PrepareHopperHop(HopperEnemyState state, HopperHopSize size)
    {
        state.YSpeedTableIndexDelta = size == HopperHopSize.Big ? 4 : 3;
        state.XVelocity = 3;
        state.YSpeedTableIndex = size == HopperHopSize.Big
            ? state.BigHopInitialYSpeedTableIndex
            : state.SmallHopInitialYSpeedTableIndex;
        state.Action = state.UpsideDown
            ? HopperAction.ChooseDirectionUpsideDown
            : HopperAction.ChooseDirectionUpsideUp;
    }

    /// <summary>Chooses the floor or ceiling hop branch from Samus's signed horizontal position relative to the actor.</summary>
    private static void ChooseHopperDirection(
        RoomEnemySlot slot,
        HopperEnemyState state,
        SamusState? samus,
        HopperSurface surface)
    {
        if (samus is null)
            throw new InvalidOperationException("Hopper direction selection requires Samus.");

        HopperHopDirection direction = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
            ? HopperHopDirection.Backward
            : HopperHopDirection.Forward;
        state.Action = new HopperHop(surface, direction) switch
        {
            (HopperSurface.Floor, HopperHopDirection.Backward) => HopperAction.StartBackwardHopUpsideUp,
            (HopperSurface.Floor, HopperHopDirection.Forward) => HopperAction.StartForwardHopUpsideUp,
            (HopperSurface.Ceiling, HopperHopDirection.Backward) => HopperAction.StartBackwardHopUpsideDown,
            (HopperSurface.Ceiling, HopperHopDirection.Forward) => HopperAction.StartForwardHopUpsideDown,
            _ => throw new InvalidOperationException($"Undefined hopper hop {surface}/{direction}."),
        };
    }

    /// <summary>Installs the jumping animation and negates X velocity only for the leftward branch.</summary>
    internal static void StartHopperJump(
        RoomEnemySlot slot,
        HopperEnemyState state,
        HopperSurface surface,
        HopperHopDirection direction)
    {
        // “Backward” is the leftward branch selected when Samus is left of the actor. The
        // initial setup always writes +3, so only this branch negates the whole-pixel word.
        if (direction == HopperHopDirection.Backward)
            state.XVelocity = -state.XVelocity;

        SetHopperAnimation(slot, state, ReadHopperAnimation(state, surface, HopperPhase.Jumping));
        state.Action = new HopperHop(surface, direction) switch
        {
            (HopperSurface.Floor, HopperHopDirection.Backward) => HopperAction.JumpingUpsideUpBackward,
            (HopperSurface.Floor, HopperHopDirection.Forward) => HopperAction.JumpingUpsideUpForward,
            (HopperSurface.Ceiling, HopperHopDirection.Backward) => HopperAction.JumpingUpsideDownBackward,
            (HopperSurface.Ceiling, HopperHopDirection.Forward) => HopperAction.JumpingUpsideDownForward,
            _ => throw new InvalidOperationException($"Undefined hopper hop {surface}/{direction}."),
        };
    }

    /// <summary>Starts the landed animation and returns the actor to its wait-for-hop function.</summary>
    internal static void LandHopper(RoomEnemySlot slot, HopperEnemyState state)
    {
        SetHopperAnimation(slot, state, ReadHopperAnimation(state, SurfaceOf(state), HopperPhase.Landed));
        state.Action = HopperAction.WaitToHop;
    }

    /// <summary>Advances a floor or ceiling hop with vertical-first collision and phase-dependent horizontal reversal.</summary>
    private void RunHopperMovement(
        RoomEnemySlot slot,
        HopperEnemyState state,
        RoomLevelData level,
        HopperSurface surface)
    {
        bool movingAwayFromSurface = !state.Falling;
        // Has four movements: floor actors use the negative half while
        // jumping and the positive half while falling; ceiling actors do the inverse.
        // Inequality expresses that table exactly. Equality reverses both phases, making
        // each actor collide with its starting surface and then migrate to the opposite one.
        bool useNegativeSpeed = (surface == HopperSurface.Ceiling) != movingAwayFromSurface;
        SignedSixteenSixteen verticalDisplacement = state.YSpeedTableIndex.Row.Toward(useNegativeSpeed ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);

        // Every movement entry resolves Y first. A collision while rising reverses
        // X and changes to the falling half without performing horizontal motion that frame.
        // A collision while falling installs Landed and likewise ends the frame immediately.
        if (_terrain.MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            if (movingAwayFromSurface)
            {
                state.XVelocity = -state.XVelocity;
                state.Falling = true;
            }
            else
            {
                state.Falling = false;
                state.Action = HopperAction.Landed;
            }
            return;
        }

        SignedSixteenSixteen horizontalDisplacement = SignedSixteenSixteen.FromWholePixels(state.XVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            state.XVelocity = -state.XVelocity;
            if (movingAwayFromSurface)
            {
                // Rising code treats a wall exactly like a ceiling/floor interruption and
                // immediately enters the falling half. Falling code only reverses X.
                state.Falling = true;
                return;
            }
        }

        if (movingAwayFromSurface)
        {
            // The game stores the moved-back place and branches on its sign: a place before the first row is the apex,
            // replaced by the first row as the hop turns back toward the surface.
            EnemyQuadraticSpeedIndex next = state.YSpeedTableIndex - EnemyQuadraticSpeedIndex.FromRows(state.YSpeedTableIndexDelta);
            if (next.IsNegative)
            {
                state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
                state.Falling = true;
            }
            else
            {
                state.YSpeedTableIndex = next;
            }
            return;
        }

        // The game caps the returning record by the sign of its word difference from the cap; both stay far
        // below the word's sign bit, so that is the ordinary minimum.
        state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Min(
            state.YSpeedTableIndex + EnemyQuadraticSpeedIndex.FromRows(state.YSpeedTableIndexDelta),
            MaximumHopperYSpeedTableIndex);
    }

    /// <summary>Sums the quadratic table's coarse speeds until the authored jump height is reached.</summary>
    internal static EnemyQuadraticSpeedIndex CalculateInitialHopperYSpeedTableIndex(
        int jumpHeight,
        int tableIndexDelta)
    {
        EnemyQuadraticSpeedIndex tableIndex = EnemyQuadraticSpeedIndex.Start;
        int accumulatedHeight = 0;
        // Each pass moves the row on by the delta, so the height is reached within the table's rows or never: past them
        // the row lookup rejects the climb before the place could grow further, and a delta of zero sums the first row's
        // speed of zero only (#1627).
        for (int iteration = 0; iteration < EnemyQuadraticSpeedDefinitions.RowCount; iteration++)
        {
            tableIndex += EnemyQuadraticSpeedIndex.FromRows(tableIndexDelta);

            // The height sums each row's coarse speed, in 256ths of a pixel; summing full speeds changes every
            // initial jump row.
            int heightIncrement = tableIndex.Row.CoarseToward(EnemySpeedSign.Positive);
            accumulatedHeight = CoordinateAxis.Wrap(accumulatedHeight + heightIncrement);
            if (CoordinateAxis.Difference(accumulatedHeight, jumpHeight) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            "Hopper initial-speed calculation did not reach its authored jump height.");
    }

    /// <summary>Selects the animation for this kind's facing and jumping or landing half.</summary>
    private static HopperSegment ReadHopperAnimation(
        HopperEnemyState state,
        HopperSurface surface,
        HopperPhase phase) =>
        HopperAnimationChoices.Animation(state.Kind, surface, phase);

    /// <summary>The surface the population's first parameter placed this hopper on.</summary>
    private static HopperSurface SurfaceOf(HopperEnemyState state) =>
        state.UpsideDown ? HopperSurface.Ceiling : HopperSurface.Floor;

    /// <summary>Starts a new animation with a one-tick countdown.</summary>
    private static void SetHopperAnimation(
        RoomEnemySlot slot,
        HopperEnemyState state,
        HopperSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, HopperAnimations.StartingWith(segment));
    }

    /// <summary>Ports Hoppers's main AI.</summary>
    internal void RunHoppersMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunHopperMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Hoppers's typed animation.</summary>
    internal void AdvanceHoppersTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        HopperEnemyState state = slot.RequireSpeciesState<HopperEnemyState>();
        AnimationPlayback<HopperSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, HopperAnimations.Segments, ref playback,
            cue => ApplyHopperCue(slot, state, cue) is { } target
                ? new AnimationContinuation<HopperSegment>(HopperAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }
}
