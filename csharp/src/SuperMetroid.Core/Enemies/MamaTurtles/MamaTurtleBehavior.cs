using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.MamaTurtles.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MamaTurtleBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. MamaTurtlePartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MamaTurtleEntity(this, slot, common);

    /// <summary>Moves parent and children against room blocks during hover, fall, and crawling.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Records the wall-impact shake from Mama Turtle's hover collision.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Links the four physically adjacent babies to their parent slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies touch damage for Mama's expanded body or exposed shell.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' end-of-update sound requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the tatori family to room terrain, collision, slots, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, EnemySlotTable slotTable, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
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
                StartMamaTurtleEnteringShell(slot.RequireSpeciesState<MamaTurtleEnemyState>());
                return null;
            case MamaTurtleCueKind.RiseToHoverRightwards:
                StartMamaTurtleRisingToHover(slot.RequireSpeciesState<MamaTurtleEnemyState>(), direction: HorizontalDirection.Right);
                return MamaTurtleProgram.MamaSpinning;
            case MamaTurtleCueKind.RiseToHoverLeftwards:
                StartMamaTurtleRisingToHover(slot.RequireSpeciesState<MamaTurtleEnemyState>(), direction: HorizontalDirection.Left);
                return MamaTurtleProgram.MamaSpinning;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.RequestSoundAtUpdateEnd(EnemySoundSource.MamaTurtle, EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.Crawl:
            case MamaTurtleCueKind.LeaveShell:
            case MamaTurtleCueKind.LeftShell:
            case MamaTurtleCueKind.LoopOrTurnAround:
            case MamaTurtleCueKind.SetSpinningStoppable:
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
                ProcessBabyTurtleCrawlInstruction(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus, level);
                return null;
            case MamaTurtleCueKind.LoopOrTurnAround:
                return SelectBabyTurtleCrawlLoop(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>());
            case MamaTurtleCueKind.LeaveShell:
                return SelectBabyTurtleLeaveShell(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus);
            case MamaTurtleCueKind.LeftShell:
                return FinishBabyTurtleLeavingShell(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus);
            case MamaTurtleCueKind.SetSpinningStoppable:
                slot.RequireSpeciesState<BabyTurtleEnemyState>().Function = BabyTurtleAiFunction.SpinningStoppable;
                return null;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.RequestSoundAtUpdateEnd(EnemySoundSource.MamaTurtle, EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.EnterShell:
            case MamaTurtleCueKind.RiseToHoverLeftwards:
            case MamaTurtleCueKind.RiseToHoverRightwards:
            default:
                throw new InvalidOperationException($"A baby turtle's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Maximum crawl distance from a baby's spawn before its list reverses direction.</summary>
    private const int BabyTurtleTravelDistance = 48;
    /// <summary>Upper room Y where Mama pauses after a rider-powered ascent.</summary>
    private const int MamaTurtlePeakYPosition = 488;
    /// <summary>Whole pixels climbed per tick while Samus rides Mama toward the peak.</summary>
    private const int MamaTurtleRisingSpeed = 7;
    /// <summary>Ticks held at the ascent peak before Mama begins falling.</summary>
    private const int MamaTurtlePeakPauseFrames = 30;
    /// <summary>Whole-speed cap consulted by the native fall routine's slot-zero read.</summary>
    private const int MamaTurtleMaximumFallingSpeed = 4;
    /// <summary>Whole-speed cap for horizontal hover acceleration.</summary>
    private const int MamaTurtleMaximumHoveringSpeed = 3;

    /// <summary>Ports Mama Turtle initialization AI <c>$A2:8D6C</c>.</summary>
    internal static void InitializeMamaTurtle(RoomEnemySlot slot)
    {
        var state = new MamaTurtleEnemyState
        {
            Function = MamaTurtleAiFunction.Initial,
            AsleepFlag = WrappingCountdown.Start(1),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The population already supplies $A800, but the initializer explicitly ORs $2000.
        // Spell that write out because instruction processing is essential to leaving shell.
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        slot.Radius = slot.Radius.WithY(0);
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(MamaTurtleProgram.MamaAsleep);
    }

    /// <summary>Ports Baby Turtle initialization AI <c>$A2:8D9D</c>.</summary>
    internal static void InitializeBabyTurtle(RoomEnemySlot slot)
    {
        var state = new BabyTurtleEnemyState
        {
            SpawnTopCenter = new Position16<RoomPixels>(
                slot.Position.X.Whole, CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y)),
            Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus,
            Velocity = new Offset16<RoomPixels>(slot.SpawnSettings<TurtleSpawnSettings>().Speed, 0),
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(slot.SpawnSettings<TurtleSpawnSettings>().Speed < 0
            ? MamaTurtleProgram.BabyCrawlingLeft
            : MamaTurtleProgram.BabyCrawlingRight);
    }

    /// <summary>Ports Mama Turtle main AI <c>$A2:8DD2</c> and its indirect functions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemySlot slot,
        MamaTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        int frameCounterByte)
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
                    slot, state, samus, level, frameCounterByte);
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
                    $"Mama Turtle main-AI function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports the first-frame parent/child setup at <c>$A2:8DD8</c>.</summary>
    private void LinkMamaTurtleChildren(RoomEnemySlot mama, MamaTurtleEnemyState state)
    {
        if (mama.SlotIndex + 4 >= EnemySlotOrder.Capacity)
            throw new InvalidDataException("Mama Turtle does not have four following physical slots.");

        for (int childOffset = 1; childOffset <= 4; childOffset++)
        {
            RoomEnemySlot child = _slotTable.Slots[mama.SlotIndex + childOffset];
            if (child.DefinitionId != EnemyDefinitionId.BabyTurtle)
            {
                throw new InvalidDataException(
                    $"Mama Turtle slot {mama.SlotIndex} expected Baby Turtle in following " +
                    $"slot {child.SlotIndex}, found {child.DefinitionId}.");
            }

            BabyTurtleEnemyState childState = child.RequireSpeciesState<BabyTurtleEnemyState>();
            child.Colouring = mama.Colouring;
            child.TileIndexBase = mama.TileIndexBase;
            childState.ParentSlotIndex = mama.SlotIndex;
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
        int signedDifference = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        if (distance >= 24)
            return;

        // The first 24 contour words describe Samus to the left; the second 24 describe
        // Samus to the right. This asymmetry is visible around the sleeping shell's lip.
        int contourOffset = MamaTurtleShell.GetOffset(signedDifference);
        mama.Radius = mama.Radius.WithY(CoordinateAxis.Wrap(-contourOffset));
        mama.Properties = mama.Properties.With(EnemyProperties.SolidToSamus);

        if (!PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
            return;

        int shellTop = CoordinateAxis.Wrap(mama.Position.Y.Whole - mama.Radius.Y);
        int overlap = CoordinateAxis.Wrap(samus.Kinematics.BottomBoundary - shellTop);
        if (WordWrap.Signed(overlap) < 0)
            return;

        samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, WordWrap.Signed(overlap));
    }

    /// <summary>Ports the deliberately jittery shell-opening routine at <c>$A2:8EE0</c>.</summary>
    private void RunMamaTurtleLeavingShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int frameCounterByte)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if ((frameCounterByte & 1) != 0)
            return;

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(1, 0);
        }

        mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - 1);
        mama.Radius = mama.Radius.WithY(16);
        mama.Position = mama.Position.WithWholeX((mama.Position.Y.Whole & 1) == 0
            ? CoordinateAxis.Wrap(mama.Position.X.Whole + 1)
            : CoordinateAxis.Wrap(mama.Position.X.Whole - 1));

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
            CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole) >= 0
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

        // The two adjacent acceleration records at $8D56 accelerate toward Samus's side by a sixteenth of a pixel per update.
        state.XAcceleration = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole) < 0
            ? SignedSixteenSixteen.FromRaw(SixteenSixteen.FromWhole(1) / 16)
            : SignedSixteenSixteen.FromRaw(-SixteenSixteen.FromWhole(1) / 16);
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.Zero);
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
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(16);
            _audio.RequestSoundAtUpdateEnd(EnemySoundSource.MamaTurtle, EnemySoundEffectDefinitions.MamaTurtleWallSound, priority: SoundRequestPriority.Normal);
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
        SignedSixteenSixteen result = samus.Kinematics.ExtraDisplacement.X - state.Velocity.X;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(result.WholePixels < -16 ? result.WithWholePixels(-16) : result);
    }

    /// <summary>Adds signed 16.16 hover acceleration with carry and caps whole speed using the old velocity's sign.</summary>
    internal static void AddAndClampMamaTurtleHorizontalAcceleration(
        MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        // One 16.16 add: the fraction's carry reaches the whole pixels.
        SignedSixteenSixteen sum = velocity + state.XAcceleration;
        int oldWhole = velocity.WholeWord;
        int newWhole = sum.WholeWord;

        int magnitude = Math.Abs(CoordinateAxis.AsSigned(newWhole));
        int cappedWhole = magnitude < MamaTurtleMaximumHoveringSpeed
            ? newWhole
            // Native chooses the cap's sign from the old velocity word, before storing the
            // just-computed result. Preserve that one-frame zero-crossing behavior.
            : CoordinateAxis.AsSigned(oldWhole) < 0
                ? WordWrap.Unsigned(-MamaTurtleMaximumHoveringSpeed)
                : MamaTurtleMaximumHoveringSpeed;
        state.Velocity = state.Velocity.WithX(
            SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(cappedWhole), sum.Fraction));
    }

    /// <summary>
    /// Reverses the hovering turtle's horizontal velocity and acceleration at a wall ($903C-$906B). Each value's
    /// fraction is negated and its whole part inverted, which reverses a value exactly unless its fraction is zero,
    /// when the reversed value lands one whole pixel lower. A velocity whose whole part is -1 (moving left by less
    /// than a pixel) lowers the reversed acceleration's whole part by one more pixel.
    /// </summary>
    internal static void ReverseMamaTurtleAtWall(MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(
            ~velocity.WholePixels, WordWrap.Unsigned(-velocity.Fraction)));

        SignedSixteenSixteen acceleration = state.XAcceleration;
        int lowered = velocity.WholePixels == -1 ? 1 : 0;
        state.XAcceleration = SignedSixteenSixteen.FromParts(
            WordWrap.Signed(~acceleration.WholePixels - lowered), WordWrap.Unsigned(-acceleration.Fraction));
    }

    /// <summary>Ports the rider-powered seven-pixel ascent at <c>$A2:9083</c>.</summary>
    private void RunMamaTurtleRisingToPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (CoordinateAxis.Difference(mama.Position.Y.Whole, MamaTurtlePeakYPosition) < 0)
        {
            state.FunctionTimer = WrappingCountdown.Start(MamaTurtlePeakPauseFrames);
            state.Function = MamaTurtleAiFunction.HoveringAtPeak;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
            return;
        }

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - MamaTurtleRisingSpeed);
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, WordWrap.Signed(MamaTurtleRisingSpeed));
            return;
        }

        state.Function = MamaTurtleAiFunction.Falling;
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
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
        MamaTurtleEnemyState comparisonState = _slotTable.Slots[0].SpeciesState<MamaTurtleEnemyState>() ?? state;
        if (WordWrap.Signed(comparisonState.Velocity.Y.WholeWord - MamaTurtleMaximumFallingSpeed) < 0)
        {
            // The $2000 fraction step carries into the whole word; both are one 16.16 add.
            state.Velocity = state.Velocity.WithY(
                state.Velocity.Y + SignedSixteenSixteen.FromParts(0, 8192));
        }

        // Only the whole word is passed to MoveEnemyDown; the fraction influences later
        // whole-speed carries but is not itself part of this frame's displacement.
        if (!_terrain.MoveEnemyVertically(level, mama, SixteenSixteen.FromWhole(state.Velocity.Y.WholePixels)))
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

        // The prologue requires the child's parent to be a Mama Turtle. The cartridge also clears and republishes a
        // carried-height word on the parent here, which nothing reads.
        _ = RequireMamaTurtleParent(state);
        switch (state.Function)
        {
            case BabyTurtleAiFunction.CrawlingNotCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.Function = BabyTurtleAiFunction.HidingCarryingSamus;
                    state.NotCarryingSamusReactionTimer = WrappingCountdown.Start(4);
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
                    state.NotCarryingSamusReactionTimer = WrappingCountdown.Start(4);
                    return;
                }
                state.NotCarryingSamusReactionTimer = state.NotCarryingSamusReactionTimer.Decrement().Remaining;
                if (state.NotCarryingSamusReactionTimer.IsZero)
                {
                    state.Function = BabyTurtleAiFunction.HidingNotCarryingSamus;
                    state.FunctionTimer = WrappingCountdown.Start(60);
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
                    bool facingRight = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right;
                    state.Velocity = new Offset16<RoomPixels>(facingRight ? 3 : -3, 1);
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
                if (!PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                    state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Baby Turtle main-AI pointer {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Moves a spinning child against terrain, reversing its X velocity on wall collision.</summary>
    private void MoveSpinningBabyTurtle(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level, baby, SixteenSixteen.FromWhole(state.Velocity.X)))
        {
            state.Velocity = state.Velocity.WithX(WordWrap.Signed(-state.Velocity.X));
            return;
        }
        _terrain.MoveEnemyVertically(level, baby, SixteenSixteen.FromWhole(state.Velocity.Y));
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

        int previousY = baby.Position.Y.Whole;
        baby.Position = baby.Position.WithWholeY(state.SpawnTopCenter.Y);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SixteenSixteen.FromWhole(state.Velocity.X));

        RoomEnemySlot parentSlot = _slotTable.Slots[state.ParentSlotIndex];
        MamaTurtleEnemyState parent = parentSlot.RequireSpeciesState<MamaTurtleEnemyState>();
        if (parent.Function != MamaTurtleAiFunction.Asleep)
            return;

        int signedDifference = CoordinateAxis.Difference(parentSlot.Position.X.Whole, baby.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        int contourOffset = distance >= MamaTurtleShellContourDefinitions.HalfWidth
            ? 1
            : MamaTurtleShell.GetOffset(signedDifference);
        _terrain.MoveEnemyVertically(level, baby, SixteenSixteen.FromWhole(contourOffset));

        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, CoordinateAxis.Difference(baby.Position.Y.Whole, previousY));
        }
    }

    /// <summary>Instruction <c>$A2:9412</c>: turn around at 48 pixels from spawn.</summary>
    private static MamaTurtleProgram SelectBabyTurtleCrawlLoop(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state)
    {
        int signedDifference = CoordinateAxis.Difference(state.SpawnTopCenter.X, baby.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        if (distance >= BabyTurtleTravelDistance)
        {
            state.Velocity = state.Velocity.WithX(signedDifference < 0 ? -1 : 1);
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
        HorizontalDirection direction)
    {
        // Cartridge comments describe the visual facing, while the stored velocity is the
        // opposite sign: $9451 writes -1 and $946B writes +1.
        state.Function = MamaTurtleAiFunction.RisingToHover;
        state.Velocity = state.Velocity.WithWholeX(direction == HorizontalDirection.Right ? -1 : 1);
        state.FunctionTimer = WrappingCountdown.Start(16);
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
        if (mama.Properties.HasAny(EnemyProperties.SolidToSamus))
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

        baby.Position = baby.Position.WithWholeX(CoordinateAxis.Difference(baby.Position.X.Whole, samus.Position.X.Whole) >= 0
            ? CoordinateAxis.Wrap(samus.Position.X.Whole + samus.Kinematics.Radius.X + baby.Radius.X)
            : CoordinateAxis.Wrap(samus.Position.X.Whole - samus.Kinematics.Radius.X - baby.Radius.X));
        state.Function = BabyTurtleAiFunction.CrawlingNotCarryingSamus;
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SixteenSixteen.FromWhole(state.Velocity.X));
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
        RoomEnemySlot boundsSource = _slotTable.Slots[0];
        int left = CoordinateAxis.Wrap(
            boundsSource.Position.X.Whole - boundsSource.Radius.X - 8);
        int right = CoordinateAxis.Wrap(
            boundsSource.Position.X.Whole + boundsSource.Radius.X + 8);
        int top = CoordinateAxis.Wrap(
            boundsSource.Position.Y.Whole - boundsSource.Radius.Y + 4);
        int bottom = CoordinateAxis.Wrap(
            boundsSource.Position.Y.Whole + boundsSource.Radius.Y - 4);

        int samusLeftMinusOne = CoordinateAxis.Wrap(
            samus.Position.X.Whole - samus.Kinematics.Radius.X - 1);
        if (CoordinateAxis.Difference(samusLeftMinusOne, right) >= 0)
            return;
        int samusRight = CoordinateAxis.Wrap(
            samus.Position.X.Whole + samus.Kinematics.Radius.X);
        if (CoordinateAxis.Difference(samusRight, left) < 0)
            return;
        int samusTopPlusOne = CoordinateAxis.Wrap(
            samus.Position.Y.Whole - samus.Kinematics.Radius.Y + 1);
        if (CoordinateAxis.Difference(samusTopPlusOne, bottom) >= 0)
            return;
        int samusBottom = CoordinateAxis.Wrap(
            samus.Position.Y.Whole + samus.Kinematics.Radius.Y);
        if (CoordinateAxis.Difference(samusBottom, top) < 0 || !samus.InvincibilityTimer.IsZero)
            return;

        _collision.ResolveNormalEnemyTouch(mama, samus);
    }

    /// <summary>Resolves the child's native parent slot and rejects a non-Mama occupant.</summary>
    private MamaTurtleEnemyState RequireMamaTurtleParent(BabyTurtleEnemyState child)
    {
        RoomEnemySlot parent = _slotTable.Slots[child.ParentSlotIndex];
        if (parent.DefinitionId != EnemyDefinitionId.MamaTurtle)
        {
            throw new InvalidDataException(
                $"Baby Turtle parent slot {child.ParentSlotIndex} names enemy " +
                $"{parent.DefinitionId}, not Mama Turtle.");
        }
        return parent.RequireSpeciesState<MamaTurtleEnemyState>();
    }

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
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Starts a baby turtle animation with its instruction timer set to one.</summary>
    private static void InstallTurtleInstruction(RoomEnemySlot slot, BabyTurtleEnemyState state, MamaTurtleProgram program)
    {
        state.Program = AnimationProgramCursor<MamaTurtleProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Rejects turtle movement without active room collision data.</summary>
    private static void RequireTurtleLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Turtle movement requires active room collision data.");
    }

    /// <summary>Runs the baby wake callback after common bomb damage.</summary>
    internal void AddMamaTurtleBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BabyTurtleShot)
        {
            AfterCommon = c => ResolveBabyTurtleShotAfterCommon(c.Enemy.RequireSpeciesState<BabyTurtleEnemyState>()),
        });
    }

    /// <summary>Ports MamaTurtle's main AI.</summary>
    internal void RunMamaTurtleMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMamaTurtleMain(
            slot,
            slot.RequireSpeciesState<MamaTurtleEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Clock.ShortCounter);
    }

    /// <summary>Ports BabyTurtle's main AI.</summary>
    internal void RunBabyTurtleMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBabyTurtleMain(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances MamaTurtle's typed animation.</summary>
    internal void AdvanceMamaTurtleTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        MamaTurtleEnemyState state = slot.RequireSpeciesState<MamaTurtleEnemyState>();
        AnimationProgramCursor<MamaTurtleProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, MamaTurtleAnimations.Programs, ref cursor, MamaTurtleFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyMamaTurtleCue(slot, cue));
        state.Program = cursor;
    }

    /// <summary>Advances BabyTurtle's typed animation.</summary>
    internal void AdvanceBabyTurtleTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        BabyTurtleEnemyState state = slot.RequireSpeciesState<BabyTurtleEnemyState>();
        AnimationProgramCursor<MamaTurtleProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, MamaTurtleAnimations.Programs, ref cursor, MamaTurtleFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyBabyTurtleCue(slot, cue, context.Samus, context.Level));
        state.Program = cursor;
    }

    /// <summary>Runs the baby wake callback after common projectile damage.</summary>
    internal void AddMamaTurtleShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BabyTurtleShot)
        {
            AfterCommon = c => ResolveBabyTurtleShotAfterCommon(c.Enemy.RequireSpeciesState<BabyTurtleEnemyState>()),
        });
    }

    /// <summary>Routes Mama contact to shell/fall handling and baby contact to separation and wake behavior.</summary>
    internal void AddMamaTurtleTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.MamaTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MamaTurtleTouch,
            context => ResolveMamaTurtleTouch(context.Slot, context.Slot.RequireSpeciesState<MamaTurtleEnemyState>(), context.Samus));

        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BabyTurtleTouch,
            context => ResolveBabyTurtleTouch(context.Slot, context.Slot.RequireSpeciesState<BabyTurtleEnemyState>(), context.Samus, context.Level));
    }
}
