using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship: its top and bottom initialization, landing, save prompt, Samus boarding and exit, and the
/// post-Ceres and escape lift-offs, with the frame events and save answer it publishes.
/// </summary>
[RoomLifetime]
internal sealed class GunshipBehavior : IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. GunshipPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new GunshipEntity(this, slot, common);

    /// <summary>Queues engine and entrance-pad sounds at the native frame boundaries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Creates gunship dust during takeoff.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies room events and the bus used by Samus pose setup.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Holds the top, bottom, and entrance pad in consecutive physical slots.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds the frame, room, and slot owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemySlotTable slotTable, EnemyFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Room load: the placement scenario and the Samus the post-Ceres ship is placed against.</summary>
    internal void BeginRoomLoad(GunshipLoadScenario scenario, SamusState? samus)
    {
        _gunshipLoadScenario = scenario;
        _samusAtEnemyInitialization = samus;
    }
    /// <summary>Selects ordinary Landing Site placement or the high-altitude post-Ceres descent.</summary>
    private GunshipLoadScenario _gunshipLoadScenario;
    /// <summary>Samus actor captured for the post-Ceres ship's initial placement.</summary>
    private SamusState? _samusAtEnemyInitialization;
    /// <summary>Whether gunship restoration has requested message box $1C and is awaiting <see cref="GunshipBehavior.AnswerGunshipSavePrompt"/> before its exit animation can resume.</summary>
    public bool GunshipSavePromptPending { get; internal set; }
    /// <summary>Persistent Yes/No result of the latest gunship save answer, reset on room load; true requests outer-runtime SRAM persistence rather than performing a save itself.</summary>
    public bool GunshipSaveRequested { get; internal set; }

    /// <summary>
    /// Supplies the choice normally returned by message box $1C after restoration. SRAM
    /// persistence remains an outer-runtime seam; the actor publishes a Yes choice while
    /// continuing the cartridge's identical exit animation for either answer.
    /// </summary>
    /// <param name="save">True to request outer-runtime persistence, or false to resume the exit animation without requesting a save.</param>
    /// <exception cref="InvalidOperationException">The system is not loaded or no gunship save prompt is awaiting a response.</exception>
    public void AnswerGunshipSavePrompt(bool save)
    {
        _room.EnsureLoaded();
        if (!GunshipSavePromptPending)
            throw new InvalidOperationException("The gunship save prompt is not awaiting a response.");

        RoomEnemySlot top = _slotTable.Slots[0];
        RoomEnemySlot pad = _slotTable.Slots[2];
        GunshipSavePromptPending = false;
        GunshipSaveRequested = save;
        Part(top).Function = GunshipFunction.WaitForExitPadToOpen;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
        Part(top).FunctionTimer = 144;
    }

    /// <summary>
    /// Each hull and pad slot's typed program cursor. Initialization sets a slot's cursor before its program runs, so a
    /// slot's entry always belongs to its current piece.
    /// </summary>
    private readonly Dictionary<int, AnimationProgramCursor<GunshipProgram>> _shipPrograms = [];

    /// <summary>Starts <paramref name="program"/> on <paramref name="slot"/> from the next update.</summary>
    private void StartGunshipProgram(RoomEnemySlot slot, GunshipProgram program)
    {
        slot.InstructionTimer = WrappingCountdown.Start(1);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(program);
    }

    /// <summary>Runs a hull or pad piece's typed program where the enemy instruction interpreter would.</summary>
    internal void AdvanceGunshipAnimation(RoomEnemySlot slot)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<GunshipProgram> cursor = _shipPrograms[slot.SlotIndex];
        EnemyProgramAnimation.Run(slot, GunshipAnimations.Programs, ref cursor, GunshipFrameDefinitions.ShownFrames.Of,
            (cue, _) => throw new InvalidOperationException($"The gunship's programs publish no cue; got {cue}."));
        _shipPrograms[slot.SlotIndex] = cursor;
    }

    /// <summary>Initializes the top hull and selects ordinary idle or post-Ceres descent.</summary>
    internal void InitializeGunshipTop(RoomEnemySlot slot)
    {
        var part = new GunshipPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(GunshipProgram.TopHull);
        slot.PaletteRow = 7;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            SamusState samus = _samusAtEnemyInitialization
                ?? throw new InvalidOperationException(
                    "Post-Ceres gunship initialization requires the loader's Samus actor.");
            // `$A2:A669-$A678` attaches the top hull seventeen pixels above Samus and
            // enters function three, the high-altitude descent used only after Ceres.
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole - 17)));
            part.Function = GunshipFunction.DescendAfterCeres;
        }
        else
        {
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 25)));
            part.BaseY = slot.Position.Y.Whole;
            part.Function = GunshipFunction.Idle;
        }
        part.BobTimer = 1;
        part.BobPhase = 0;
    }

    /// <summary>Initializes bottom hull or pad from physical neighboring slot aliases.</summary>
    internal void InitializeGunshipBottom(RoomEnemySlot slot)
    {
        var part = new GunshipPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(slot.Spawn.Population.Parameter2 != 0
            ? GunshipProgram.BottomEntrancePad
            : GunshipProgram.BottomHull);

        // $A2:A6F1 reads enemy_drawing_queue[(cur_enemy_index >> 1) + 106].
        // For the two Landing Site bottom slots those WRAM addresses alias the preceding
        // slot's +$20 vram_tiles_index word. Express the alias semantically, but retain the
        // dependency on physical slot order.
        if (slot.SlotIndex == 0)
            throw new InvalidDataException("Gunship bottom cannot occupy enemy slot zero.");
        slot.TileIndexBase = _slotTable.Slots[slot.SlotIndex - 1].TileIndexBase;
        slot.PaletteRow = 7;

        if (slot.Spawn.Population.Parameter2 != 0)
        {
            // For slot two, the equally strange +61 indexed read aliases slot zero's +$06
            // Y-position word. The opening pad sits one pixel above the top hull origin.
            if (slot.SlotIndex < 2)
                throw new InvalidDataException("Gunship entrance pad requires two preceding enemy slots.");
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(_slotTable.Slots[slot.SlotIndex - 2].Position.Y.Whole - 1)));
        }
        else
        {
            if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
            {
                SamusState samus = _samusAtEnemyInitialization
                    ?? throw new InvalidOperationException(
                        "Post-Ceres gunship bottom initialization requires Samus.");
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + 23)));
            }
            else
            {
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 15)));
                part.EngineSoundTimer = 71;
            }
        }
        part.Function = GunshipFunction.NoOperation;
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ControllerButton newlyPressedControllerInput)
    {
        if (top.SlotIndex + 2 >= _slotTable.EnemyCount)
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];

        // $A2:A75C decrements the solid bottom's engine-sound timer and reloads 70 on
        // one/underflow. QueueSfx2_Max6($4D) occurs before the reload, exactly as it does in
        // GunshipTop_Main; this periodic producer continues while the ship is idling/bobbing.
        CountdownStep bottomTimer = WrappingCountdown.FromRaw(checked((ushort)Part(bottom).EngineSoundTimer)).Decrement();
        Part(bottom).EngineSoundTimer = bottomTimer.Value;
        if (bottomTimer.IsZeroOrNegative)
        {
            _audio.QueueEnemySound(Library2Sound.GunshipHover, maximumQueued: SoundQueueLimit.Max6);
            Part(bottom).EngineSoundTimer = 70;
        }

        // The landed ship continuously performs its four-phase bob before dispatching the function itself, in every
        // function from waiting for the landing entrance through loading the liftoff dust.
        GunshipFunction function = Part(top).Function;
        if (BobsWhileLanded(function))
            StepGunshipBob(top);

        switch (function)
        {
            case GunshipFunction.DescendAfterCeres:
                DescendPostCeresGunship(top, samus);
                return;
            case GunshipFunction.ApplyLandingBrakes:
                BouncePostCeresGunship(top, samus);
                return;
            case GunshipFunction.WaitForLandingEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.EjectSamus;
                return;
            case GunshipFunction.EjectSamus:
                RaisePostCeresSamus(top, samus);
                return;
            case GunshipFunction.FinishLanding:
                if (TickGunshipFunctionTimer(top))
                {
                    Part(top).Function = GunshipFunction.Idle;
                    if (samus is not null)
                        samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case GunshipFunction.Idle:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case GunshipFunction.WaitForEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.LowerSamus;
                return;
            case GunshipFunction.LowerSamus:
                LowerSamusIntoGunship(top, samus);
                return;
            case GunshipFunction.WaitForEntranceToClose:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipFunction.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(top, samus);
                return;
            case GunshipFunction.HandleSaveConfirmation:
                GunshipSavePromptPending = true;
                _frameResults.Gunship = GunshipFrameEvent.SavePromptRequested;
                return;
            case GunshipFunction.WaitForExitPadToOpen:
                if (TickGunshipFunctionTimer(top))
                    Part(top).Function = GunshipFunction.RaiseSamus;
                return;
            case GunshipFunction.RaiseSamus:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case GunshipFunction.FinishSamusExit:
                if (TickGunshipFunctionTimer(top))
                {
                    Part(top).Function = GunshipFunction.Idle;
                    if (samus is not null)
                        samus.Control.Unlock();
                    _frameResults.Gunship = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case GunshipFunction.LoadLiftoffDustTiles:
                ShowGunshipTakeoffPage(top);
                return;
            case GunshipFunction.FireUpEngines:
                FireUpGunshipEngines(top, samus);
                return;
            case GunshipFunction.SteadyLiftoff:
                LiftGunshipAtConstantSpeed(top, samus);
                return;
            case GunshipFunction.AcceleratingLiftoff:
                AccelerateEscapingGunship(top, samus);
                return;
            case GunshipFunction.MoveAccelerating:
                MoveEscapingGunship(top, samus);
                return;
            case GunshipFunction.NoOperation:
                throw new InvalidDataException(
                    $"Gunship function {Part(top).Function} is not translated.");
            case GunshipFunction.None:
            default:
                throw new InvalidOperationException($"Undefined GunshipFunction {function}.");
        }
    }

    /// <summary>Carries Samus and all ship parts downward, slowing below Y=$0300 before the landing clamp.</summary>
    private void DescendPostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship descent lost Samus.");

        // Function three carries all four actors as a rigid body. Above Y=$0300 it moves
        // $4.8000 pixels per call; below that threshold it slows to $2.8000 and clamps the
        // top hull to $045F before beginning the cartridge's seventeen-entry bounce table.
        // The clamp ($A2:A8B2) writes only whole positions; subpixels keep this call's sum.
        uint delta = top.Position.Y.Whole < GunshipMotionDefinitions.SlowDescentY ? GunshipMotionDefinitions.FastDescentSpeed : GunshipMotionDefinitions.SlowDescentSpeed;
        AddGunshipYFixed(samus, top, delta);
        if (top.Position.Y.Whole < GunshipMotionDefinitions.LandedTopY)
            return;

        RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        Part(top).Function = GunshipFunction.ApplyLandingBrakes;
        Part(top).LandingBrakeStep = 0;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship bounce lost Samus.");

        int yDelta = GunshipMotion.LandingBrakeYDelta(checked((ushort)Part(top).LandingBrakeStep));
        samus.Position += new Offset16<RoomPixels>(0, checked((short)yDelta));
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable.Slots[top.SlotIndex + component];
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + yDelta)));
        }

        Part(top).LandingBrakeStep = unchecked((ushort)(Part(top).LandingBrakeStep + 1));
        if (Part(top).LandingBrakeStep < 17)
            return;

        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        Part(top).Function = GunshipFunction.WaitForLandingEntranceToOpen;
        Part(top).BaseY = top.Position.Y.Whole;
        Part(top).BobTimer = 1;
        Part(top).BobPhase = 0;
        samus.Position = samus.Position.WithWholeX(unchecked((ushort)(top.Position.X.Whole + 1)));
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadOpened;
        _audio.QueueEnemySound(Library3Sound.GunshipPadOpening, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship exit lost Samus.");

        samus.Position -= new Offset16<RoomPixels>(0, 1);
        int targetY = unchecked((ushort)(Part(top).BaseY - 30));
        if (!EnemyMath.IsNegative16(samus.Position.Y.Whole - targetY))
            return;

        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        Part(top).Function = GunshipFunction.FinishLanding;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.LandingPadClosed;
    }

    /// <summary>Adds the same 16.16 Y displacement to Samus and all three ship components.</summary>
    private void AddGunshipYFixed(SamusState samus, RoomEnemySlot top, uint delta)
    {
        uint samusFixed = new FixedCoordinate(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction).RawValue;
        samusFixed = unchecked(samusFixed + delta);
        samus.Position = samus.Position.WithWholeY(FixedCoordinate.WholeOf(unchecked((int)samusFixed)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(unchecked((ushort)samusFixed));

        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable.Slots[top.SlotIndex + component];
            uint fixedPosition = slot.Position.Y.RawValue;
            fixedPosition = unchecked(fixedPosition + delta);
            slot.Position = slot.Position.WithY(FixedCoordinate.FromRaw(unchecked(fixedPosition)));
        }
    }

    /// <summary>
    /// Whether the landed ship bobs before running <paramref name="function"/>: every function from waiting for the
    /// landing entrance through loading the liftoff dust. The cartridge tests its function word against the range of
    /// those routines' addresses ($A942-$AC1A, which also contains Idle); firing up the engines is the first routine past it.
    /// </summary>
    private static bool BobsWhileLanded(GunshipFunction function) => function is
        GunshipFunction.WaitForLandingEntranceToOpen or GunshipFunction.EjectSamus or GunshipFunction.FinishLanding or
        GunshipFunction.Idle or GunshipFunction.WaitForEntranceToOpen or GunshipFunction.LowerSamus or
        GunshipFunction.WaitForEntranceToClose or GunshipFunction.BeginLiftoffOrRestoreSamus or
        GunshipFunction.HandleSaveConfirmation or GunshipFunction.WaitForExitPadToOpen or GunshipFunction.RaiseSamus or
        GunshipFunction.FinishSamusExit or GunshipFunction.LoadLiftoffDustTiles;

    /// <summary>Cycles the four native idle-bob entries while moving the hull and pad together.</summary>
    internal void StepGunshipBob(RoomEnemySlot top)
    {
        CountdownStep bobTimer = WrappingCountdown.FromRaw(checked((ushort)Part(top).BobTimer)).Decrement();
        Part(top).BobTimer = bobTimer.Value;
        if (!bobTimer.IsZeroOrNegative)
            return;

        GunshipIdleBobDefinition bob = GunshipMotion.IdleBob(
            unchecked((ushort)(Part(top).BobPhase & 3)));
        Part(top).BobTimer = bob.Timer;
        int yDelta = bob.YDelta;
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable.Slots[top.SlotIndex + component];
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + yDelta)));
        }
        Part(top).BobPhase = (Part(top).BobPhase + 1) & 3;
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState? samus,
        ControllerButton newlyPressedControllerInput)
    {
        ControllerButton newlyPressed = newlyPressedControllerInput;
        if (samus is null || !newlyPressed.HasAny(ControllerButton.Down))
            return;

        bool insideEntrance =
            (short)unchecked((ushort)(top.Position.X.Whole - 8 - samus.Position.X.Whole)) < 0 &&
            (short)unchecked((ushort)(top.Position.X.Whole + 8 - samus.Position.X.Whole)) >= 0 &&
            (short)unchecked((ushort)(top.Position.Y.Whole - 64 - samus.Position.Y.Whole)) < 0 &&
            (short)unchecked((ushort)(top.Position.Y.Whole - samus.Position.Y.Whole)) >= 0;
        if (insideEntrance && SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
            Part(top).Function = GunshipFunction.WaitForEntranceToOpen;
            // $A2:AA17-AA1D stores the ship's X in both Samus X words, so the camera sees
            // no horizontal motion from this alignment.
            if (samus.Position.X.Whole != GunshipMotionDefinitions.LandedX)
            {
                samus.Position = samus.Position.WithWholeX(top.Position.X.Whole);
                samus.WritePreviousXPosition(samus.Position.X.Whole);
            }
            SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
            samus.Control.EnterGunship();
            samus.Presentation.PrimeGraphics(samus);
            pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
            StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
            Part(top).FunctionTimer = 144;
            _frameResults.Gunship = GunshipFrameEvent.EntryStarted;
            _audio.QueueEnemySound(Library3Sound.GunshipPadOpening, maximumQueued: SoundQueueLimit.Max6);
        }
    }

    /// <summary>Decrements the native function timer, treating one and signed underflow as expiry.</summary>
    private static bool TickGunshipFunctionTimer(RoomEnemySlot top)
    {
        CountdownStep functionTimer = WrappingCountdown.FromRaw(checked((ushort)Part(top).FunctionTimer)).Decrement();
        Part(top).FunctionTimer = functionTimer.Value;
        return functionTimer.IsZeroOrNegative;
    }

    /// <summary>Lowers Samus two pixels per frame until the closing-pad threshold.</summary>
    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship entry lost its Samus actor.");
        samus.Position += new Offset16<RoomPixels>(0, 2);
        if (EnemyMath.IsNegative16(samus.Position.Y.Whole - unchecked((ushort)(Part(top).BaseY + 18))))
            return;

        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        Part(top).Function = GunshipFunction.WaitForEntranceToClose;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.EntryPadClosing;
    }

    /// <summary>Refills Samus in two-unit steps or skips directly to escape takeoff after Mother Brain.</summary>
    private void RestoreSamusInGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship restoration lost its Samus actor.");

        // Event $0E is set by Mother Brain's death sequence. Native bypasses every refill
        // and save-prompt branch here, installs the takeoff tile uploader, and keeps the
        // already hidden/input-locked Samus rigidly attached to the ship.
        if (_room.Services.Events.Has(EventNumber.ZebesTimebombSet))
        {
            RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];
            Part(top).Function = GunshipFunction.LoadLiftoffDustTiles;
            Part(top).NextLiftoffPage = 0;
            Part(bottom).EscapeVelocity = 0;
            Part(bottom).RumbleFrame = 0;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        SamusEnergyRestoration.Restore(samus, 2);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if (samus.ReserveEnergy < samus.MaxReserveEnergy ||
            samus.Health < samus.MaxHealth ||
            samus.Missiles < samus.MaxMissiles ||
            samus.SuperMissiles < samus.MaxSuperMissiles ||
            samus.PowerBombs < samus.MaxPowerBombs)
            return;

        // The prompt itself opens when $AB1F executes on the following frame.
        Part(top).Function = GunshipFunction.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus two pixels per frame, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.Position -= new Offset16<RoomPixels>(0, 2);
        if (!EnemyMath.IsNegative16(samus.Position.Y.Whole - unchecked((ushort)(Part(top).BaseY - 30))))
            return;

        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        Part(top).Function = GunshipFunction.FinishSamusExit;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        Part(top).FunctionTimer = 144;
        _frameResults.Gunship = GunshipFrameEvent.ExitPadClosing;
        _audio.QueueEnemySound(Library3Sound.GunshipPadClosing, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>
    /// Ports gunship function 17 at <c>$A2:ABC7</c> (#1406): shows the next of the five liftoff looks, one a step, from this
    /// update's frame on; after the fifth, the engines fire up.
    /// </summary>
    /// <exception cref="InvalidDataException">The page index left its five pages.</exception>
    internal void ShowGunshipTakeoffPage(RoomEnemySlot top)
    {
        int page = Part(top).NextLiftoffPage;
        if ((uint)page >= GunshipLiftoffTransferDefinitions.Pages.Count)
            throw new InvalidDataException("Gunship takeoff page escaped its five pages.");
        _room.Session.ShowLook(GunshipLiftoffTransferDefinitions.Pages[page]);

        Part(top).NextLiftoffPage = page + 1;
        if (Part(top).NextLiftoffPage >= GunshipLiftoffTransferDefinitions.Pages.Count)
        {
            Part(top).Function = GunshipFunction.FireUpEngines;
            Part(top).NextLiftoffPage = 0;
        }
    }

    /// <summary>Ports the 128-frame engine-rumble function at <c>$A2:AC1B</c>.</summary>
    private void FireUpGunshipEngines(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship takeoff lost Samus.");
        RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        int rumbleFrame = Part(bottom).RumbleFrame;
        int shake = (rumbleFrame & 1) != 0
            ? (rumbleFrame < 64 ? 1 : 2)
            : (rumbleFrame < 64 ? -1 : -2);
        samus.Position += new Offset16<RoomPixels>(0, unchecked((short)shake));
        top.Position = top.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole - 17)));
        pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
        bottom.Position = bottom.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + 23)));

        Part(bottom).RumbleFrame = unchecked((ushort)(Part(bottom).RumbleFrame + 1));
        if (Part(bottom).RumbleFrame == 64)
        {
            // The six calls at `$A2:AC9B-$ACCA` happen after the frame counter reaches
            // exactly 64. Descending projectile allocation means parameter A occupies the
            // highest surviving slot, matching the ordinary bank-$86 scheduler.
            for (int parameter = 0; parameter <= 10; parameter += 2)
                _projectiles.SpawnGunshipLiftoffDustCloud(parameter, samus);
        }
        if (Part(bottom).RumbleFrame >= 128)
        {
            Part(top).Function = GunshipFunction.SteadyLiftoff;
            Part(top).FunctionTimer = 0;
        }
    }

    /// <summary>Ports constant two-pixel liftoff at <c>$A2:ACD7</c>.</summary>
    private void LiftGunshipAtConstantSpeed(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship liftoff lost Samus.");
        MoveGunshipAndSamusToY(top, samus, unchecked((ushort)(samus.Position.Y.Whole - 2)));
        if (EnemyMath.IsNegative16(top.Position.Y.Whole - 896))
        {
            Part(top).Function = GunshipFunction.AcceleratingLiftoff;
            Part(_slotTable.Slots[top.SlotIndex + 1]).EscapeVelocity = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Ports the accelerating takeoff function at <c>$A2:AD0E</c>.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        MoveEscapingGunship(top, samus);
        if (EnemyMath.IsNegative16(top.Position.Y.Whole - 256))
        {
            Part(top).Function = GunshipFunction.MoveAccelerating;
            _frameResults.Gunship = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Ports the shared 8.8 velocity integrator at <c>$A2:AD2D</c>.</summary>
    private void MoveEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Escaping gunship lost Samus.");
        RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];
        GunshipPartState bottomPart = Part(bottom);
        bottomPart.EscapeVelocity = unchecked((ushort)(bottomPart.EscapeVelocity + GunshipMotionDefinitions.EscapeAcceleration));
        if (UnsignedEightEight.WithoutFraction(bottomPart.EscapeVelocity) >= GunshipMotionDefinitions.EscapeVelocityCapTrigger)
            bottomPart.EscapeVelocity = GunshipMotionDefinitions.EscapeVelocityCap;

        uint samusY = samus.Kinematics.Position.Y.RawValue;
        samusY = unchecked(samusY - SixteenSixteen.FromEightEight((uint)bottomPart.EscapeVelocity));
        samus.Kinematics.Position = samus.Kinematics.Position.WithY(FixedCoordinate.FromRaw(samusY));
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private void MoveGunshipAndSamusToY(
        RoomEnemySlot top,
        SamusState samus,
        int samusY)
    {
        samus.Position = samus.Position.WithWholeY(checked((ushort)samusY));
        RoomEnemySlot bottom = _slotTable.Slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable.Slots[top.SlotIndex + 2];
        top.Position = top.Position.WithWholeY(unchecked((ushort)(samusY - 17)));
        pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
        bottom.Position = bottom.Position.WithWholeY(unchecked((ushort)(samusY + 23)));
    }

    /// <summary>Restores a resource by two without exceeding its maximum.</summary>
    private static int RestoreTwo(int current, int maximum)
    {
        if (current >= maximum)
            return current;
        return Math.Min(current + 2, maximum);
    }

    /// <summary>Ports Gunship's main AI.</summary>
    internal void RunGunshipMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGunshipTopMain(
            slot,
            frame.Samus,
            frame.NewlyPressedControllerInput);
    }

    /// <summary>The state of a gunship hull or pad slot.</summary>
    private static GunshipPartState Part(RoomEnemySlot slot) => GunshipPartState.Of(slot);
}
