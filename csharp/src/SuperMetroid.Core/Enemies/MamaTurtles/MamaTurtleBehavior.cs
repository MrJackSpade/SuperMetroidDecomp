using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MamaTurtleBehavior : IEnemySpecies
{
    /// <summary>Provides the live Samus address space for baby-turtle facing decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Moves parent and children against room blocks during hover, fall, and crawling.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Records the wall-impact shake from Mama Turtle's hover collision.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Links the four physically adjacent babies to their parent slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies touch damage for Mama's expanded body or exposed shell.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Connects the tatori family to room terrain, collision, slots, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemySlotTable slotTable, EnemyCollision collision)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Mama Turtle's animations; rising to hover continues at its spin.</summary>
    private MamaTurtleProgram? ApplyMamaTurtleCue(RoomEnemySlot slot, MamaTurtleCue cue)
    {
        switch (cue.Kind)
        {
            case MamaTurtleCueKind.EnterShell:
                StartMamaTurtleEnteringShell(RequireMamaTurtleState(slot));
                return null;
            case MamaTurtleCueKind.RiseToHoverRightwards:
                StartMamaTurtleRisingToHover(RequireMamaTurtleState(slot), rightward: true);
                return MamaTurtleProgram.MamaSpinning;
            case MamaTurtleCueKind.RiseToHoverLeftwards:
                StartMamaTurtleRisingToHover(RequireMamaTurtleState(slot), rightward: false);
                return MamaTurtleProgram.MamaSpinning;
            case MamaTurtleCueKind.PlaySpinningSound:
                LastMamaTurtleSoundEffect = EnemySoundEffectDefinitions.MamaTurtleSpinSound;
                return null;
            default:
                throw new InvalidOperationException($"The Mama Turtle's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>
    /// Carries out one cue of a baby turtle's animations. The crawl loop, leaving the shell while ridden and the end of
    /// leaving it continue at the animation their callbacks return; every other cue runs on.
    /// </summary>
    private MamaTurtleProgram? ApplyBabyTurtleCue(RoomEnemySlot slot, MamaTurtleCue cue, SamusState? samus, RoomLevelData? level)
    {
        switch (cue.Kind)
        {
            case MamaTurtleCueKind.Crawl:
                ProcessBabyTurtleCrawlInstruction(slot, RequireBabyTurtleState(slot), samus, level);
                return null;
            case MamaTurtleCueKind.LoopOrTurnAround:
                return SelectBabyTurtleCrawlLoop(slot, RequireBabyTurtleState(slot));
            case MamaTurtleCueKind.LeaveShell:
                return SelectBabyTurtleLeaveShell(slot, RequireBabyTurtleState(slot), samus);
            case MamaTurtleCueKind.LeftShell:
                return FinishBabyTurtleLeavingShell(slot, RequireBabyTurtleState(slot), samus);
            case MamaTurtleCueKind.SetSpinningStoppable:
                RequireBabyTurtleState(slot).Function = BabyTurtleAiFunction.SpinningStoppable;
                return null;
            case MamaTurtleCueKind.PlaySpinningSound:
                LastMamaTurtleSoundEffect = EnemySoundEffectDefinitions.MamaTurtleSpinSound;
                return null;
            default:
                throw new InvalidOperationException($"A baby turtle's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Maximum crawl distance from a baby's spawn before its list reverses direction.</summary>
    private const ushort BabyTurtleTravelDistance = 48;
    /// <summary>Upper room Y where Mama pauses after a rider-powered ascent.</summary>
    private const ushort MamaTurtlePeakYPosition = 488;
    /// <summary>Whole pixels climbed per tick while Samus rides Mama toward the peak.</summary>
    private const ushort MamaTurtleRisingSpeed = 7;
    /// <summary>Ticks held at the ascent peak before Mama begins falling.</summary>
    private const ushort MamaTurtlePeakPauseFrames = 30;
    /// <summary>Whole-speed cap consulted by the native fall routine's slot-zero read.</summary>
    private const ushort MamaTurtleMaximumFallingSpeed = 4;
    /// <summary>Whole-speed cap for horizontal hover acceleration.</summary>
    private const ushort MamaTurtleMaximumHoveringSpeed = 3;

    /// <summary>Maps enemy slots to Mama Turtle's shell, hover, and ascent state.</summary>
    internal readonly MamaTurtleEnemyState?[] _mamaTurtleStates =
        new MamaTurtleEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Maps following enemy slots to their baby crawl, carry, and spinning state.</summary>
    internal readonly BabyTurtleEnemyState?[] _babyTurtleStates =
        new BabyTurtleEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two sound requested by this family during the enemy frame.</summary>
    public ushort? LastMamaTurtleSoundEffect { get; private set; }

    /// <summary>Ports Mama Turtle initialization AI <c>$A2:8D6C</c>.</summary>
    private void InitializeMamaTurtle(RoomEnemySlot slot)
    {
        var state = new MamaTurtleEnemyState(slot)
        {
            Function = MamaTurtleAiFunction.Initial,
            AsleepFlag = NativeCountdown.Start(1),
        };
        _mamaTurtleStates[slot.SlotIndex] = state;

        // The population already supplies $A800, but the initializer explicitly ORs $2000.
        // Spell that write out because instruction processing is essential to leaving shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.Radius = slot.Radius.WithY(0);
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(MamaTurtleProgram.MamaAsleep);
    }

    /// <summary>Ports Baby Turtle initialization AI <c>$A2:8D9D</c>.</summary>
    private void InitializeBabyTurtle(RoomEnemySlot slot)
    {
        var state = new BabyTurtleEnemyState(slot)
        {
            SpawnXPosition = slot.Position.X.Whole,
            SpawnTopBoundary = unchecked((ushort)(slot.Position.Y.Whole - slot.Radius.Y)),
            Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus,
            Velocity = new Offset16<RoomPixels>(unchecked((short)slot.Parameter1), 0),
        };
        _babyTurtleStates[slot.SlotIndex] = state;

        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(unchecked((short)slot.Parameter1) < 0
            ? MamaTurtleProgram.BabyCrawlingLeft
            : MamaTurtleProgram.BabyCrawlingRight);
    }

    /// <summary>Ports Mama Turtle main AI <c>$A2:8DD2</c> and its indirect functions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemySlot slot,
        MamaTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        byte nmiFrameCounter8)
    {
        if (samus is null)
            throw new InvalidOperationException("Mama Turtle AI requires the active Samus actor.");

        switch (state.Function)
        {
            case MamaTurtleAiFunction.Initial:
                LinkMamaTurtleChildren(slot, state);
                return;
            case MamaTurtleAiFunction.Idle:
                return;
            case MamaTurtleAiFunction.Asleep:
                RunMamaTurtleAsleep(slot, state, samus);
                return;
            case MamaTurtleAiFunction.LeavingShell:
                RequireTurtleLevel(level);
                RunMamaTurtleLeavingShell(
                    slot, state, samus, level, nmiFrameCounter8);
                return;
            case MamaTurtleAiFunction.EnteringShell:
                RunMamaTurtleEnteringShell(slot, state, samus);
                return;
            case MamaTurtleAiFunction.RisingToHover:
                RequireTurtleLevel(level);
                RunMamaTurtleRisingToHover(slot, state, samus, level);
                return;
            case MamaTurtleAiFunction.Hovering:
                RequireTurtleLevel(level);
                RunMamaTurtleHovering(slot, state, samus, level);
                return;
            case MamaTurtleAiFunction.RisingToPeak:
                RunMamaTurtleRisingToPeak(slot, state, samus);
                return;
            case MamaTurtleAiFunction.HoveringAtPeak:
                RunMamaTurtleHoveringAtPeak(slot, state, samus);
                return;
            case MamaTurtleAiFunction.Falling:
                RequireTurtleLevel(level);
                RunMamaTurtleFalling(slot, state, samus, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Mama Turtle main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the first-frame parent/child setup at <c>$A2:8DD8</c>.</summary>
    private void LinkMamaTurtleChildren(RoomEnemySlot mama, MamaTurtleEnemyState state)
    {
        if (mama.SlotIndex + 4 >= RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException("Mama Turtle does not have four following physical slots.");

        for (int childOffset = 1; childOffset <= 4; childOffset++)
        {
            RoomEnemySlot child = _slotTable._slots[mama.SlotIndex + childOffset];
            if (child.EnemyDefinitionPointer != EnemyDefinitionId.BabyTurtle)
            {
                throw new InvalidDataException(
                    $"Mama Turtle slot {mama.SlotIndex} expected Baby Turtle in following " +
                    $"slot {child.SlotIndex}, found ${(int)child.EnemyDefinitionPointer:X4}.");
            }

            BabyTurtleEnemyState childState = RequireBabyTurtleState(child);
            child.PaletteIndex = mama.PaletteIndex;
            child.VramTilesIndex = mama.VramTilesIndex;
            childState.ParentNativeIndex = mama.NativeIndex;
        }

        state.Function = MamaTurtleAiFunction.Asleep;
    }

    /// <summary>Ports the sleeping shell height/carry routine at <c>$A2:8E0A</c>.</summary>
    internal static void RunMamaTurtleAsleep(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (state.AsleepFlag.IsZero)
        {
            state.Function = MamaTurtleAiFunction.LeavingShell;
            mama.Properties = mama.Properties.Without(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        mama.Radius = mama.Radius.WithY(0);
        short signedDifference = unchecked((short)(mama.Position.X.Whole - samus.Position.X.Whole));
        int distance = Math.Abs((int)signedDifference);
        if (distance >= 24)
            return;

        // The first 24 contour words describe Samus to the left; the second 24 describe
        // Samus to the right. This asymmetry is visible around the sleeping shell's lip.
        short contourOffset = MamaTurtleShellContourDefinitions.GetOffset(signedDifference);
        mama.Radius = mama.Radius.WithY(unchecked((ushort)-contourOffset));
        mama.Properties = unchecked((ushort)(mama.Properties | (ushort)EnemyProperties.SolidToSamus));

        if (!PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
            return;

        ushort shellTop = unchecked((ushort)(mama.Position.Y.Whole - mama.Radius.Y));
        ushort overlap = unchecked((ushort)(samus.Kinematics.BottomBoundary - shellTop));
        if (unchecked((short)overlap) < 0)
            return;

        samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, unchecked((short)(overlap)));
    }

    /// <summary>Ports the deliberately jittery shell-opening routine at <c>$A2:8EE0</c>.</summary>
    private void RunMamaTurtleLeavingShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        byte nmiFrameCounter8)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if ((nmiFrameCounter8 & 1) != 0)
            return;

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(1, 0);
        }

        mama.Position = mama.Position.WithWholeY(unchecked((ushort)(mama.Position.Y.Whole - 1)));
        mama.Radius = mama.Radius.WithY(16);
        mama.Position = mama.Position.WithWholeX((mama.Position.Y.Whole & 1) == 0
            ? unchecked((ushort)(mama.Position.X.Whole + 1))
            : unchecked((ushort)(mama.Position.X.Whole - 1)));

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, 1 << 16))
            return;

        InstallTurtleInstruction(
            mama,
            state,
            MamaTurtleProgram.MamaLeaveShellLeft);

        // $8F35 is `STA $0006,x`, not `STA Enemy+$0006,x`. It corrupts low WRAM with $20
        // in retail and does not alter the enemy. Reproducing unrelated low-WRAM corruption
        // would be harmful; importantly, we do not invent an enemy-field meaning for it.
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports the facing-dependent return-to-shell selector at <c>$A2:8F3F</c>.</summary>
    private static void RunMamaTurtleEnteringShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        InstallTurtleInstruction(
            mama,
            state,
            unchecked((short)(mama.Position.X.Whole - samus.Position.X.Whole)) >= 0
                ? MamaTurtleProgram.MamaEnterShellLeft
                : MamaTurtleProgram.MamaEnterShellRight);
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports the 16-frame upward transition at <c>$A2:8F8D</c>.</summary>
    private void RunMamaTurtleRisingToHover(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (_terrain.MoveEnemyVertically(level, mama, -1 << 16))
            return;

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, 1);
        }

        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsZero)
            return;

        // The two adjacent {sub,whole} pairs at $8D56 accelerate toward Samus's side.
        if (unchecked((short)(mama.Position.X.Whole - samus.Position.X.Whole)) < 0)
        {
            state.XSubAcceleration = 4096;
            state.XAcceleration = 0;
        }
        else
        {
            state.XSubAcceleration = 61440;
            state.XAcceleration = unchecked((ushort)-1);
        }
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.Zero);
        state.Function = MamaTurtleAiFunction.Hovering;
    }

    /// <summary>Ports accelerating horizontal hover and its original wall bug at $A2:8FEB.</summary>
    private void RunMamaTurtleHovering(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, state.Velocity.X.RawValue))
        {
            ReverseMamaTurtleAtWall(state);
            _earthquake.EarthquakeType = 0;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(16);
            LastMamaTurtleSoundEffect = EnemySoundEffectDefinitions.MamaTurtleWallSound;
            return;
        }

        HandleSamusLandingOnMamaTurtle(mama, state, samus);
        AddAndClampMamaTurtleHorizontalAcceleration(state);
    }

    /// <summary>Ports <c>HandleSamusLandingOnHoveringTatori</c> at $A2:8F5F.</summary>
    private static void HandleSamusLandingOnMamaTurtle(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (!PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
            return;

        state.Function = MamaTurtleAiFunction.RisingToPeak;
        SnesSignedSixteenSixteen result = samus.Kinematics.ExtraDisplacement.X - state.Velocity.X;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(result.WholePixels < -16 ? result.WithWholePixels(-16) : result);
    }

    /// <summary>Adds signed 16.16 hover acceleration with carry and caps whole speed using the old velocity's sign.</summary>
    private static void AddAndClampMamaTurtleHorizontalAcceleration(
        MamaTurtleEnemyState state)
    {
        SnesSignedSixteenSixteen velocity = state.Velocity.X;
        uint subSum = (uint)velocity.Fraction + state.XSubAcceleration;
        ushort oldWhole = velocity.WholeWord;
        ushort newWhole = unchecked((ushort)(
            oldWhole + state.XAcceleration + (SnesCarry.OutOfWord(subSum))));

        int magnitude = Math.Abs((int)unchecked((short)newWhole));
        ushort cappedWhole = magnitude < MamaTurtleMaximumHoveringSpeed
            ? newWhole
            // Native chooses the cap's sign from the old velocity word, before storing the
            // just-computed result. Preserve that one-frame zero-crossing behavior.
            : unchecked((short)oldWhole) < 0
                ? unchecked((ushort)-MamaTurtleMaximumHoveringSpeed)
                : MamaTurtleMaximumHoveringSpeed;
        state.Velocity = state.Velocity.WithX(
            SnesSignedSixteenSixteen.FromParts(unchecked((short)cappedWhole), unchecked((ushort)subSum)));
    }

    /// <summary>
    /// Reproduces $903C-$906B, including carry propagation across two otherwise separate
    /// negations and the missing final INC on the whole acceleration word.
    /// </summary>
    private static void ReverseMamaTurtleAtWall(MamaTurtleEnemyState state)
    {
        SnesSignedSixteenSixteen velocity = state.Velocity.X;
        ushort negatedFraction = unchecked((ushort)(~velocity.Fraction + 1));

        uint xVelocitySum = (uint)velocity.WholeWord + 1; // BCS supplied carry-in one.
        bool carry = xVelocitySum > ushort.MaxValue;
        ushort negatedWhole = unchecked((ushort)(~(ushort)xVelocitySum + 1));
        state.Velocity = state.Velocity.WithX(
            SnesSignedSixteenSixteen.FromParts(unchecked((short)negatedWhole), negatedFraction));

        state.XSubAcceleration = unchecked((ushort)(~state.XSubAcceleration + 1));
        uint accelerationSum = (uint)state.XAcceleration + (carry ? 1u : 0u);
        state.XAcceleration = unchecked((ushort)~(ushort)accelerationSum); // No INC in ROM.
    }

    /// <summary>Ports the rider-powered seven-pixel ascent at <c>$A2:9083</c>.</summary>
    private void RunMamaTurtleRisingToPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (unchecked((short)(mama.Position.Y.Whole - MamaTurtlePeakYPosition)) < 0)
        {
            state.FunctionTimer = NativeCountdown.Start(MamaTurtlePeakPauseFrames);
            state.Function = MamaTurtleAiFunction.HoveringAtPeak;
            state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.Zero);
            return;
        }

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            mama.Position = mama.Position.WithWholeY(unchecked((ushort)(mama.Position.Y.Whole - MamaTurtleRisingSpeed)));
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, unchecked((short)(MamaTurtleRisingSpeed)));
            return;
        }

        state.Function = MamaTurtleAiFunction.Falling;
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.Zero);
    }

    /// <summary>Ports the 30-frame peak delay at <c>$A2:90CC</c>.</summary>
    private void RunMamaTurtleHoveringAtPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsZero)
            state.Function = MamaTurtleAiFunction.Falling;
    }

    /// <summary>Ports gravity, collision, and landing-list selection at <c>$A2:90E1</c>.</summary>
    private void RunMamaTurtleFalling(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);

        // The original cap comparison accidentally omits X and reads slot-zero's extension.
        // The retail parent is slot zero, but retaining the explicit source documents the
        // behavior for debugger-created populations as well.
        MamaTurtleEnemyState comparisonState = _mamaTurtleStates[0] ?? state;
        if (unchecked((short)(comparisonState.Velocity.Y.WholeWord - MamaTurtleMaximumFallingSpeed)) < 0)
        {
            // The $2000 fraction step carries into the whole word; both are one 16.16 add.
            state.Velocity = state.Velocity.WithY(
                state.Velocity.Y + SnesSignedSixteenSixteen.FromParts(0, 8192));
        }

        // Only the whole word is passed to MoveEnemyDown; the fraction influences later
        // whole-speed carries but is not itself part of this frame's displacement.
        if (!_terrain.MoveEnemyVertically(level, mama, SnesSixteenSixteen.FromWhole(state.Velocity.Y.WholePixels)))
            return;

        InstallTurtleInstruction(
            mama,
            state,
            state.Velocity.X.WholePixels < 0
                ? MamaTurtleProgram.MamaLeaveShellLeft
                : MamaTurtleProgram.MamaLeaveShellRight);
        state.Function = MamaTurtleAiFunction.Idle;
    }

    /// <summary>Ports Baby Turtle main AI <c>$A2:912E</c> and its six live functions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle AI requires the active Samus actor.");

        MamaTurtleEnemyState parent = RequireMamaTurtleParent(state);
        parent.CarryingChildHeight = 0;
        switch (state.Function)
        {
            case BabyTurtleAiFunction.CrawlingNotCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.Function = BabyTurtleAiFunction.HidingCarryingSamus;
                    state.NotCarryingSamusReactionTimer = NativeCountdown.Start(4);
                    InstallTurtleInstruction(
                        baby,
                        state,
                        state.Velocity.X < 0
                            ? MamaTurtleProgram.BabyHidingLeft
                            : MamaTurtleProgram.BabyHidingRight);
                }
                return;

            case BabyTurtleAiFunction.HidingCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.NotCarryingSamusReactionTimer = NativeCountdown.Start(4);
                    return;
                }
                state.NotCarryingSamusReactionTimer = state.NotCarryingSamusReactionTimer.Decrement().Remaining;
                if (state.NotCarryingSamusReactionTimer.IsZero)
                {
                    state.Function = BabyTurtleAiFunction.HidingNotCarryingSamus;
                    state.FunctionTimer = NativeCountdown.Start(60);
                }
                return;

            case BabyTurtleAiFunction.HidingNotCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.Function = BabyTurtleAiFunction.SpinningUnstoppable;
                    InstallTurtleInstruction(
                        baby,
                        state,
                        MamaTurtleProgram.BabySpinning);
                    bool facingRight = (samus.PoseMetadata.ReadPoseXDirection(_room.LoadedBus) & 0x0f) == (byte)SamusFacingDirection.Right; // magic-number-audit: allow(PoseOrMovement) - the native low-nibble mask isolating the pose X-direction byte.
                    state.Velocity = new Offset16<RoomPixels>(facingRight ? (short)3 : (short)-3, 1);
                    return;
                }
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    InstallBabyTurtleCrawlingInstruction(baby, state);
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                }
                return;

            case BabyTurtleAiFunction.SpinningUnstoppable:
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAiFunction.SpinningStoppable:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    InstallBabyTurtleCrawlingInstruction(baby, state);
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                    return;
                }
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAiFunction.CrawlingCarryingSamus:
                parent.CarryingChildHeight = baby.Radius.Y;
                if (!PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Baby Turtle main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Moves a spinning child against terrain, reversing its X velocity on wall collision.</summary>
    private void MoveSpinningBabyTurtle(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level, baby, SnesSixteenSixteen.FromWhole(state.Velocity.X)))
        {
            state.Velocity = state.Velocity.WithX(unchecked((short)-state.Velocity.X));
            return;
        }
        _terrain.MoveEnemyVertically(level, baby, SnesSixteenSixteen.FromWhole(state.Velocity.Y));
    }

    /// <summary>Instruction <c>$A2:9381</c>: crawl across the sleeping parent contour.</summary>
    private void ProcessBabyTurtleCrawlInstruction(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null || level is null)
        {
            throw new InvalidOperationException(
                "Baby Turtle crawl bytecode requires Samus and active room collision data.");
        }

        bool carryingSamus = PlatformsBehavior.IsSamusRidingPlatform(baby, samus);
        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(state.Velocity.X, 0);
        }

        ushort previousY = baby.Position.Y.Whole;
        baby.Position = baby.Position.WithWholeY(state.SpawnTopBoundary);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SnesSixteenSixteen.FromWhole(state.Velocity.X));

        RoomEnemySlot parentSlot = _slotTable.SlotFromNativeIndex(state.ParentNativeIndex);
        MamaTurtleEnemyState parent = RequireMamaTurtleState(parentSlot);
        if (parent.Function != MamaTurtleAiFunction.Asleep)
            return;

        short signedDifference = unchecked((short)(parentSlot.Position.X.Whole - baby.Position.X.Whole));
        int distance = Math.Abs((int)signedDifference);
        short contourOffset = distance >= MamaTurtleShellContourDefinitions.HalfWidth
            ? (short)1
            : MamaTurtleShellContourDefinitions.GetOffset(signedDifference);
        _terrain.MoveEnemyVertically(level, baby, SnesSixteenSixteen.FromWhole(contourOffset));

        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, unchecked((short)(baby.Position.Y.Whole - previousY)));
        }
    }

    /// <summary>Instruction <c>$A2:9412</c>: turn around at 48 pixels from spawn.</summary>
    private static MamaTurtleProgram SelectBabyTurtleCrawlLoop(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state)
    {
        short signedDifference = unchecked((short)(state.SpawnXPosition - baby.Position.X.Whole));
        int distance = Math.Abs((int)signedDifference);
        if (distance >= BabyTurtleTravelDistance)
        {
            state.Velocity = state.Velocity.WithX(signedDifference < 0 ? (short)-1 : (short)1);
        }
        return state.Velocity.X < 0
            ? MamaTurtleProgram.BabyCrawlingLeft
            : MamaTurtleProgram.BabyCrawlingRight;
    }

    /// <summary>Instruction <c>$A2:9447</c>: begin Mama's shell-entry behavior.</summary>
    private static void StartMamaTurtleEnteringShell(MamaTurtleEnemyState state) =>
        state.Function = MamaTurtleAiFunction.EnteringShell;

    /// <summary>Instructions <c>$9451/$946B</c>: begin rising with selected X direction.</summary>
    private static void StartMamaTurtleRisingToHover(
        MamaTurtleEnemyState state,
        bool rightward)
    {
        // Cartridge comments describe the visual facing, while the stored velocity is the
        // opposite sign: $9451 writes -1 and $946B writes +1.
        state.Function = MamaTurtleAiFunction.RisingToHover;
        state.Velocity = state.Velocity.WithWholeX(rightward ? (short)-1 : (short)1);
        state.FunctionTimer = NativeCountdown.Start(16);
    }

    /// <summary>Instruction <c>$A2:9485</c>: branch out of hiding only while ridden; otherwise the hiding animation runs on.</summary>
    private static MamaTurtleProgram? SelectBabyTurtleLeaveShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        if (!PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
            return null;
        return state.Velocity.X < 0
            ? MamaTurtleProgram.BabyLeaveShellLeft
            : MamaTurtleProgram.BabyLeaveShellRight;
    }

    /// <summary>Instruction <c>$A2:94A1</c>: restore crawling after leaving the shell.</summary>
    private static MamaTurtleProgram FinishBabyTurtleLeavingShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        state.Function = PlatformsBehavior.IsSamusRidingPlatform(baby, samus)
            ? BabyTurtleAiFunction.CrawlingCarryingSamus
            : BabyTurtleAiFunction.CrawlingNotCarryingSamus;
        return state.Velocity.X < 0
            ? MamaTurtleProgram.BabyCrawlingLeft
            : MamaTurtleProgram.BabyCrawlingRight;
    }

    /// <summary>Ports Mama's custom ordinary-touch callback at <c>$A2:9281</c>.</summary>
    private void ResolveMamaTurtleTouch(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if ((mama.Properties & (ushort)EnemyProperties.SolidToSamus) != 0)
            return;
        _collision.ResolveNormalEnemyTouch(mama, samus);
        state.Function = MamaTurtleAiFunction.Falling;
        state.Velocity = state.Velocity.WithWholeY(2);
    }

    /// <summary>Ports Baby's direction flip, separation, and wake callback at $A2:929F.</summary>
    private void ResolveBabyTurtleTouch(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData? level)
    {
        if (state.Function == BabyTurtleAiFunction.CrawlingCarryingSamus)
            return;
        RequireTurtleLevel(level);

        if (state.Velocity.X < 0)
        {
            InstallTurtleInstruction(
                baby,
                state,
                MamaTurtleProgram.BabyCrawlingRight);
            state.Velocity = state.Velocity.WithX(1);
        }
        else
        {
            InstallTurtleInstruction(
                baby,
                state,
                MamaTurtleProgram.BabyCrawlingLeft);
            state.Velocity = state.Velocity.WithX(-1);
        }

        baby.Position = baby.Position.WithWholeX(unchecked((short)(baby.Position.X.Whole - samus.Position.X.Whole)) >= 0
            ? unchecked((ushort)(samus.Position.X.Whole + samus.Kinematics.Radius.X + baby.Radius.X))
            : unchecked((ushort)(samus.Position.X.Whole - samus.Kinematics.Radius.X - baby.Radius.X)));
        state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SnesSixteenSixteen.FromWhole(state.Velocity.X));
        WakeMamaTurtle(state);
    }

    /// <summary>Private tail of Baby shot AI <c>$A2:930F</c>.</summary>
    private void ResolveBabyTurtleShotAfterCommon(BabyTurtleEnemyState state) =>
        WakeMamaTurtle(state);

    /// <summary>Decrements the parent's asleep flag after a baby touch or shot reaction.</summary>
    private void WakeMamaTurtle(BabyTurtleEnemyState child)
    {
        MamaTurtleEnemyState parent = RequireMamaTurtleParent(child);
        parent.AsleepFlag = parent.AsleepFlag.DecrementToZero();
    }

    /// <summary>Ports Mama's expanded private damage box at <c>$A2:9315</c>.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemySlot mama,
        SamusState samus)
    {
        // Four bounds deliberately come from physical slot zero because all four native
        // loads omit `,x`. The damage callback still runs for the current Mama slot.
        RoomEnemySlot boundsSource = _slotTable._slots[0];
        ushort left = unchecked((ushort)(
            boundsSource.Position.X.Whole - boundsSource.Radius.X - 8));
        ushort right = unchecked((ushort)(
            boundsSource.Position.X.Whole + boundsSource.Radius.X + 8));
        ushort top = unchecked((ushort)(
            boundsSource.Position.Y.Whole - boundsSource.Radius.Y + 4));
        ushort bottom = unchecked((ushort)(
            boundsSource.Position.Y.Whole + boundsSource.Radius.Y - 4));

        ushort samusLeftMinusOne = unchecked((ushort)(
            samus.Position.X.Whole - samus.Kinematics.Radius.X - 1));
        if (unchecked((short)(samusLeftMinusOne - right)) >= 0)
            return;
        ushort samusRight = unchecked((ushort)(
            samus.Position.X.Whole + samus.Kinematics.Radius.X));
        if (unchecked((short)(samusRight - left)) < 0)
            return;
        ushort samusTopPlusOne = unchecked((ushort)(
            samus.Position.Y.Whole - samus.Kinematics.Radius.Y + 1));
        if (unchecked((short)(samusTopPlusOne - bottom)) >= 0)
            return;
        ushort samusBottom = unchecked((ushort)(
            samus.Position.Y.Whole + samus.Kinematics.Radius.Y));
        if (unchecked((short)(samusBottom - top)) < 0 || !samus.InvincibilityTimer.IsZero)
            return;

        _collision.ResolveNormalEnemyTouch(mama, samus);
    }

    /// <summary>Resolves the child's native parent slot and rejects a non-Mama occupant.</summary>
    private MamaTurtleEnemyState RequireMamaTurtleParent(BabyTurtleEnemyState child)
    {
        RoomEnemySlot parent = _slotTable.SlotFromNativeIndex(child.ParentNativeIndex);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.MamaTurtle)
        {
            throw new InvalidDataException(
                $"Baby Turtle parent index ${child.ParentNativeIndex:X4} names enemy " +
                $"${(int)parent.EnemyDefinitionPointer:X4}, not Mama Turtle.");
        }
        return RequireMamaTurtleState(parent);
    }

    /// <summary>Requires initialized parent state for a Mama Turtle slot.</summary>
    private MamaTurtleEnemyState RequireMamaTurtleState(RoomEnemySlot slot) =>
        _mamaTurtleStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Mama Turtle state.");

    /// <summary>Requires initialized child state for a Baby Turtle slot.</summary>
    private BabyTurtleEnemyState RequireBabyTurtleState(RoomEnemySlot slot) =>
        _babyTurtleStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Baby Turtle state.");

    /// <summary>Selects the left or right crawl list from the child's signed X velocity.</summary>
    private static void InstallBabyTurtleCrawlingInstruction(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state) =>
        InstallTurtleInstruction(
            baby,
            state,
            state.Velocity.X < 0
                ? MamaTurtleProgram.BabyCrawlingLeft
                : MamaTurtleProgram.BabyCrawlingRight);

    /// <summary>Starts a Mama Turtle animation with its instruction timer set to one.</summary>
    private static void InstallTurtleInstruction(RoomEnemySlot slot, MamaTurtleEnemyState state, MamaTurtleProgram program)
    {
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Starts a baby turtle animation with its instruction timer set to one.</summary>
    private static void InstallTurtleInstruction(RoomEnemySlot slot, BabyTurtleEnemyState state, MamaTurtleProgram program)
    {
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Rejects turtle movement without active room collision data.</summary>
    private static void RequireTurtleLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Turtle movement requires active room collision data.");
    }

    /// <summary>Runs the baby wake callback after common bomb damage.</summary>
    internal void RegisterMamaTurtleBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BabyTurtleShot)
        {
            AfterCommon = c => ResolveBabyTurtleShotAfterCommon(RequireBabyTurtleState(c.Enemy)),
        });
    }

    /// <summary>Registers parent and baby setup routines for their respective native definitions.</summary>
    internal void RegisterMamaTurtleInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_MamaTurtle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMamaTurtle(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_BabyTurtle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBabyTurtle(slot);
        });
    }

    /// <summary>Dispatches parent and baby lifecycle functions with the active Samus and terrain context.</summary>
    internal void RegisterMamaTurtleMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_MamaTurtle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle, (slot, frame, nmiFrameCounter8) =>
        {
            RunMamaTurtleMain(
                slot,
                RequireMamaTurtleState(slot),
                frame.Samus,
                frame.Level,
                nmiFrameCounter8);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_BabyTurtle, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle, (slot, frame, nmiFrameCounter8) =>
        {
            RunBabyTurtleMain(slot, RequireBabyTurtleState(slot), frame.Samus, frame.Level);
        });
    }

    /// <summary>Runs the Mama Turtle's and baby turtles' animations as typed programs (#641).</summary>
    internal void RegisterMamaTurtleTypedAnimations(EnemyAiTable table)
    {
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            MamaTurtleEnemyState state = RequireMamaTurtleState(slot);
            AnimationProgramCursor<MamaTurtleProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, MamaTurtleAnimations.Programs, ref cursor, MamaTurtleFrameDefinitions.Spritemap,
                (cue, _) => ApplyMamaTurtleCue(slot, cue));
            state.Program = cursor;
        });
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle, (slot, context) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            BabyTurtleEnemyState state = RequireBabyTurtleState(slot);
            AnimationProgramCursor<MamaTurtleProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, MamaTurtleAnimations.Programs, ref cursor, MamaTurtleFrameDefinitions.Spritemap,
                (cue, _) => ApplyBabyTurtleCue(slot, cue, context.Samus, context.Level));
            state.Program = cursor;
        });
    }

    /// <summary>Runs the baby wake callback after common projectile damage.</summary>
    internal void RegisterMamaTurtleShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BabyTurtleShot)
        {
            AfterCommon = (c, _) => ResolveBabyTurtleShotAfterCommon(RequireBabyTurtleState(c.Enemy)),
        });
    }

    /// <summary>Routes Mama contact to shell/fall handling and baby contact to separation and wake behavior.</summary>
    internal void RegisterMamaTurtleTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MamaTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MamaTurtleTouch,
            context => ResolveMamaTurtleTouch(context.Slot, RequireMamaTurtleState(context.Slot), context.Samus));

        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BabyTurtleTouch,
            context => ResolveBabyTurtleTouch(context.Slot, RequireBabyTurtleState(context.Slot), context.Samus, context.Level));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastMamaTurtleSoundEffect = null;
        Array.Clear(_mamaTurtleStates);
        Array.Clear(_babyTurtleStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMamaTurtleSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastMamaTurtleSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
