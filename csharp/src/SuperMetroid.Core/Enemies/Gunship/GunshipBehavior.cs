using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship: its top and bottom initialization, landing, save prompt, Samus boarding and exit, and the
/// post-Ceres and escape lift-offs, with the frame events and save answer it publishes.
/// </summary>
internal sealed class GunshipBehavior
{
    /// <summary>Queues engine and entrance-pad sounds at the native frame boundaries.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Creates gunship dust during takeoff.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies room events and the bus used by Samus pose setup.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Holds the top, bottom, and entrance pad in consecutive physical slots.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds the frame, room, and slot owners used by the ship's three-part actor.</summary>
    internal GunshipBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _audio = audio;
        _projectiles = projectiles;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Selects ordinary Landing Site placement or the high-altitude post-Ceres descent.</summary>
    internal GunshipLoadScenario _gunshipLoadScenario;
    /// <summary>Samus actor captured for the post-Ceres ship's initial placement.</summary>
    internal SamusState? _samusAtEnemyInitialization;
    /// <summary>Most recent gunship lifecycle event in the current enemy pass, cleared to None at the next pass; answering the save prompt also publishes an event immediately.</summary>
    public GunshipFrameEvent LastGunshipEvent { get; internal set; }
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

        RoomEnemySlot top = _slotTable._slots[0];
        RoomEnemySlot pad = _slotTable._slots[2];
        GunshipSavePromptPending = false;
        GunshipSaveRequested = save;
        top.VariableF = (ushort)GunshipFunction.WaitForExitPadToOpen;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.SavePromptAnswered;
    }

    /// <summary>
    /// Each hull and pad slot's typed program cursor. Initialization sets a slot's cursor before its program runs, so a
    /// slot's entry always belongs to its current piece.
    /// </summary>
    private readonly Dictionary<int, AnimationProgramCursor<GunshipProgram>> _shipPrograms = [];

    /// <summary>Starts <paramref name="program"/> on <paramref name="slot"/> from the next update.</summary>
    private void StartGunshipProgram(RoomEnemySlot slot, GunshipProgram program)
    {
        slot.InstructionTimer = NativeCountdown.Start(1);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(program);
    }

    /// <summary>Runs a hull or pad piece's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceGunshipAnimation(RoomEnemySlot slot)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<GunshipProgram> cursor = _shipPrograms[slot.SlotIndex];
        EnemyProgramAnimation.Run(slot, GunshipAnimations.Programs, ref cursor, GunshipFrameDefinitions.Spritemap,
            (cue, _) => throw new InvalidOperationException($"The gunship's programs publish no cue; got {cue}."));
        _shipPrograms[slot.SlotIndex] = cursor;
    }

    /// <summary>Runs the hull and entrance pad as typed programs (#641).</summary>
    internal void RegisterGunshipTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.ShipTop or EnemyDefinitionId.ShipBottomEntrance,
            (slot, _) => AdvanceGunshipAnimation(slot));

    /// <summary>Initializes the top hull and selects ordinary idle or post-Ceres descent.</summary>
    internal void InitializeGunshipTop(RoomEnemySlot slot)
    {
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(GunshipProgram.TopHull);
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (_gunshipLoadScenario == GunshipLoadScenario.EscapingCeres)
        {
            SamusState samus = _samusAtEnemyInitialization
                ?? throw new InvalidOperationException(
                    "Post-Ceres gunship initialization requires the loader's Samus actor.");
            // `$A2:A669-$A678` attaches the top hull seventeen pixels above Samus and
            // enters function three, the high-altitude descent used only after Ceres.
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole - 17)));
            slot.VariableF = (ushort)GunshipFunction.DescendAfterCeres;
        }
        else
        {
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 25)));
            slot.VariableE = slot.Position.Y.Whole;
            slot.VariableF = (ushort)GunshipFunction.Idle;
        }
        slot.VariableD = 1;
        slot.VariableC = 0;
    }

    /// <summary>Initializes bottom hull or pad from physical neighboring slot aliases.</summary>
    internal void InitializeGunshipBottom(RoomEnemySlot slot)
    {
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions | EnemyProperties.IgnoreSamusCollision);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        _shipPrograms[slot.SlotIndex] = AnimationProgramCursor<GunshipProgram>.Start(slot.Parameter2 != 0
            ? GunshipProgram.BottomEntrancePad
            : GunshipProgram.BottomHull);

        // $A2:A6F1 reads enemy_drawing_queue[(cur_enemy_index >> 1) + 106].
        // For the two Landing Site bottom slots those WRAM addresses alias the preceding
        // slot's +$20 vram_tiles_index word. Express the alias semantically, but retain the
        // dependency on physical slot order.
        if (slot.SlotIndex == 0)
            throw new InvalidDataException("Gunship bottom cannot occupy enemy slot zero.");
        slot.VramTilesIndex = _slotTable._slots[slot.SlotIndex - 1].VramTilesIndex;
        slot.PaletteIndex = EnemyPaletteBits.Palette7;

        if (slot.Parameter2 != 0)
        {
            // For slot two, the equally strange +61 indexed read aliases slot zero's +$06
            // Y-position word. The opening pad sits one pixel above the top hull origin.
            if (slot.SlotIndex < 2)
                throw new InvalidDataException("Gunship entrance pad requires two preceding enemy slots.");
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(_slotTable._slots[slot.SlotIndex - 2].Position.Y.Whole - 1)));
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
                slot.VariableD = 71;
            }
        }
        slot.VariableF = (ushort)GunshipFunction.NoOperation;
    }

    /// <summary>Runs the top hull's sound timer and landing, boarding, save, or takeoff state machine.</summary>
    internal void RunGunshipTopMain(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput,
        VramWriteQueue? vramWriteQueue)
    {
        if (top.SlotIndex + 2 >= _slotTable.EnemyCount)
            throw new InvalidDataException("Gunship top is missing its two following component slots.");

        RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];

        // $A2:A75C decrements the solid bottom's engine-sound timer and reloads 70 on
        // one/underflow. QueueSfx2_Max6($4D) occurs before the reload, exactly as it does in
        // GunshipTop_Main; this periodic producer continues while the ship is idling/bobbing.
        ushort oldBottomTimer = bottom.VariableD;
        bottom.VariableD = unchecked((ushort)(bottom.VariableD - 1));
        if (oldBottomTimer == 1 || (short)bottom.VariableD < 0)
        {
            _audio.QueueEnemySound(SoundEffectLibrary2Sounds.GunshipEngine, maximumQueued: 6);
            bottom.VariableD = 70;
        }

        // The native address-range test admits functions $A942-$AC1A (two signed sums, an exact range). Idle function $A9BD
        // is inside that interval, so the landed ship continuously performs its four-phase
        // bob before dispatching the function itself.
        if (top.VariableF is >= GunshipMotionDefinitions.BobbingFunctionsFirst and <= GunshipMotionDefinitions.BobbingFunctionsLast)
            StepGunshipBob(top);

        GunshipFunction function = ClosedNativeWords.Decode<GunshipFunction>(top.VariableF, "gunship function");
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
                    top.VariableF = (ushort)GunshipFunction.EjectSamus;
                return;
            case GunshipFunction.EjectSamus:
                RaisePostCeresSamus(top, samus);
                return;
            case GunshipFunction.FinishLanding:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = (ushort)GunshipFunction.Idle;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.LandingCompleted;
                }
                return;
            case GunshipFunction.Idle:
                HandleIdleGunshipEntrance(top, samus, newlyPressedControllerInput);
                return;
            case GunshipFunction.WaitForEntranceToOpen:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.LowerSamus;
                return;
            case GunshipFunction.LowerSamus:
                LowerSamusIntoGunship(top, samus);
                return;
            case GunshipFunction.WaitForEntranceToClose:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.BeginLiftoffOrRestoreSamus;
                return;
            case GunshipFunction.BeginLiftoffOrRestoreSamus:
                RestoreSamusInGunship(top, samus);
                return;
            case GunshipFunction.HandleSaveConfirmation:
                GunshipSavePromptPending = true;
                LastGunshipEvent = GunshipFrameEvent.SavePromptRequested;
                return;
            case GunshipFunction.WaitForExitPadToOpen:
                if (TickGunshipFunctionTimer(top))
                    top.VariableF = (ushort)GunshipFunction.RaiseSamus;
                return;
            case GunshipFunction.RaiseSamus:
                RaiseSamusOutOfGunship(top, samus);
                return;
            case GunshipFunction.FinishSamusExit:
                if (TickGunshipFunctionTimer(top))
                {
                    top.VariableF = (ushort)GunshipFunction.Idle;
                    if (samus is not null)
                        samus.InputLocked = false;
                    LastGunshipEvent = GunshipFrameEvent.ExitCompleted;
                }
                return;
            case GunshipFunction.LoadLiftoffDustTiles:
                QueueGunshipTakeoffTiles(top, vramWriteQueue);
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
                    $"Gunship function $A2:{top.VariableF:X4} is not translated.");
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

        RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.Position = top.Position.WithWholeY(GunshipMotionDefinitions.LandedTopY);
        bottom.Position = bottom.Position.WithWholeY(GunshipMotionDefinitions.LandedBottomY);
        pad.Position = pad.Position.WithWholeY(GunshipMotionDefinitions.LandedPadY);
        top.VariableF = (ushort)GunshipFunction.ApplyLandingBrakes;
        top.VariableE = 0;
    }

    /// <summary>Applies seventeen authored brake offsets, then opens the pad and aligns Samus.</summary>
    internal void BouncePostCeresGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship bounce lost Samus.");

        short yDelta = GunshipMotionDefinitions.LandingBrakeYDelta(top.VariableE);
        samus.Position += new Offset16<RoomPixels>(0, unchecked((short)yDelta));
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable._slots[top.SlotIndex + component];
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + yDelta)));
        }

        top.VariableE++;
        if (top.VariableE < 17)
            return;

        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.WaitForLandingEntranceToOpen;
        top.VariableE = top.Position.Y.Whole;
        top.VariableD = 1;
        top.VariableC = 0;
        samus.Position = samus.Position.WithWholeX(unchecked((ushort)(top.Position.X.Whole + 1)));
        samus.WritePreviousXPosition(samus.Position.X.Whole);
        StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.LandingPadOpened;
        _audio.QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
    }

    /// <summary>Raises Samus from the landed ship and closes its pad at the exit height.</summary>
    private void RaisePostCeresSamus(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Post-Ceres gunship exit lost Samus.");

        samus.Position -= new Offset16<RoomPixels>(0, 1);
        ushort targetY = unchecked((ushort)(top.VariableE - 30));
        if (!EnemyMath.IsNegative16(samus.Position.Y.Whole - targetY))
            return;

        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.FinishLanding;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.LandingPadClosed;
    }

    /// <summary>Adds the same 16.16 Y displacement to Samus and all three ship components.</summary>
    private void AddGunshipYFixed(SamusState samus, RoomEnemySlot top, uint delta)
    {
        uint samusFixed = new SnesFixedPosition(samus.Position.Y.Whole, samus.Kinematics.Position.Y.Fraction).RawValue;
        samusFixed = unchecked(samusFixed + delta);
        samus.Position = samus.Position.WithWholeY(SnesFixedPosition.WholeOf(unchecked((int)samusFixed)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(unchecked((ushort)samusFixed));

        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable._slots[top.SlotIndex + component];
            uint fixedPosition = slot.Position.Y.RawValue;
            fixedPosition = unchecked(fixedPosition + delta);
            slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)fixedPosition)));
        }
    }

    /// <summary>Cycles the four native idle-bob entries while moving the hull and pad together.</summary>
    internal void StepGunshipBob(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableD;
        top.VariableD = unchecked((ushort)(top.VariableD - 1));
        if (oldTimer != 1 && (short)top.VariableD >= 0)
            return;

        GunshipIdleBobDefinition bob = GunshipMotionDefinitions.IdleBob(
            unchecked((ushort)(top.VariableC & 3)));
        top.VariableD = bob.Timer;
        sbyte yDelta = bob.YDelta;
        for (int component = 0; component < 3; component++)
        {
            RoomEnemySlot slot = _slotTable._slots[top.SlotIndex + component];
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + yDelta)));
        }
        top.VariableC = unchecked((ushort)((top.VariableC + 1) & 3));
    }

    /// <summary>Accepts a new Down press inside the standing entrance box and opens the pad.</summary>
    private void HandleIdleGunshipEntrance(
        RoomEnemySlot top,
        SamusState? samus,
        ushort newlyPressedControllerInput)
    {
        SnesButton newlyPressed = SnesButtons.FromRaw(
            newlyPressedControllerInput,
            "gunship entrance AI");
        if (samus is null || !newlyPressed.HasAny(SnesButton.Down))
            return;

        bool insideEntrance =
            (short)unchecked((ushort)(top.Position.X.Whole - 8 - samus.Position.X.Whole)) < 0 &&
            (short)unchecked((ushort)(top.Position.X.Whole + 8 - samus.Position.X.Whole)) >= 0 &&
            (short)unchecked((ushort)(top.Position.Y.Whole - 64 - samus.Position.Y.Whole)) < 0 &&
            (short)unchecked((ushort)(top.Position.Y.Whole - samus.Position.Y.Whole)) >= 0;
        if (insideEntrance && samus.PoseMetadata.ReadMovementKind(_room.LoadedBus) == SamusMovementType.Standing)
        {
            RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
            top.VariableF = (ushort)GunshipFunction.WaitForEntranceToOpen;
            // $A2:AA17-AA1D stores the ship's X in both Samus X words, so the camera sees
            // no horizontal motion from this alignment.
            if (samus.Position.X.Whole != GunshipMotionDefinitions.LandedX)
            {
                samus.Position = samus.Position.WithWholeX(top.Position.X.Whole);
                samus.WritePreviousXPosition(samus.Position.X.Whole);
            }
            samus.PoseMetadata.ApplyForwardFacingPoseSetup(_room.LoadedBus);
            samus.InputLocked = true;
            samus.Presentation.PrimeGraphics(_room.LoadedBus);
            pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
            StartGunshipProgram(pad, GunshipProgram.EntrancePadOpening);
            top.VariableA = 144;
            LastGunshipEvent = GunshipFrameEvent.EntryStarted;
            _audio.QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePad, maximumQueued: 6);
        }
    }

    /// <summary>Decrements the native function timer, treating one and signed underflow as expiry.</summary>
    private static bool TickGunshipFunctionTimer(RoomEnemySlot top)
    {
        ushort oldTimer = top.VariableA;
        top.VariableA = unchecked((ushort)(top.VariableA - 1));
        return oldTimer == 1 || (short)top.VariableA < 0;
    }

    /// <summary>Lowers Samus two pixels per frame until the closing-pad threshold.</summary>
    private void LowerSamusIntoGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship entry lost its Samus actor.");
        samus.Position += new Offset16<RoomPixels>(0, 2);
        if (EnemyMath.IsNegative16(samus.Position.Y.Whole - unchecked((ushort)(top.VariableE + 18))))
            return;

        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.WaitForEntranceToClose;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.EntryPadClosing;
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
            RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];
            top.VariableF = (ushort)GunshipFunction.LoadLiftoffDustTiles;
            top.VariableB = 0;
            bottom.VariableF = 0;
            bottom.VariableE = 0;
            LastGunshipEvent = GunshipFrameEvent.EscapeTakeoffStarted;
            return;
        }

        SamusEnergyRestoration.Restore(samus, 2);
        samus.Missiles = RestoreTwo(samus.Missiles, samus.MaxMissiles);
        samus.SuperMissiles = RestoreTwo(samus.SuperMissiles, samus.MaxSuperMissiles);
        samus.PowerBombs = RestoreTwo(samus.PowerBombs, samus.MaxPowerBombs);
        if ((short)(samus.ReserveEnergy - samus.MaxReserveEnergy) < 0 ||
            (short)(samus.Health - samus.MaxHealth) < 0 ||
            (short)(samus.Missiles - samus.MaxMissiles) < 0 ||
            (short)(samus.SuperMissiles - samus.MaxSuperMissiles) < 0 ||
            (short)(samus.PowerBombs - samus.MaxPowerBombs) < 0)
            return;

        // The prompt itself opens when $AB1F executes on the following frame.
        top.VariableF = (ushort)GunshipFunction.HandleSaveConfirmation;
    }

    /// <summary>Raises Samus two pixels per frame, then closes the pad and emits the exit event.</summary>
    private void RaiseSamusOutOfGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship exit lost its Samus actor.");
        samus.Position -= new Offset16<RoomPixels>(0, 2);
        if (!EnemyMath.IsNegative16(samus.Position.Y.Whole - unchecked((ushort)(top.VariableE - 30))))
            return;

        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.VariableF = (ushort)GunshipFunction.FinishSamusExit;
        StartGunshipProgram(pad, GunshipProgram.EntrancePadClosing);
        top.VariableA = 144;
        LastGunshipEvent = GunshipFrameEvent.ExitPadClosing;
        _audio.QueueEnemySound(SoundEffectLibrary3Sounds.GunshipEntrancePadClosing, maximumQueued: 6);
    }

    /// <summary>Ports gunship function 17 at <c>$A2:ABC7</c>.</summary>
    [PpuLayoutBoundary("codec: gunship liftoff transfer records' native VRAM destinations ($A2:ABC7)")]
    internal void QueueGunshipTakeoffTiles(
        RoomEnemySlot top,
        VramWriteQueue? vramWriteQueue)
    {
        if (vramWriteQueue is null)
            throw new InvalidOperationException("Gunship takeoff requires the runtime VRAM queue.");

        int transferIndex = top.VariableB;
        if ((uint)transferIndex >= GunshipLiftoffTransferDefinitions.Frames.Length)
            throw new InvalidDataException("Gunship takeoff tile index escaped its five-entry table.");
        GunshipLiftoffTransferDefinition transfer =
            GunshipLiftoffTransferDefinitions.Frames[transferIndex];
        if (_room.Artwork?.GunshipLiftoff is not null)
            vramWriteQueue.EnqueueAsset(transfer.Asset,
                GunshipLiftoffTransferDefinitions.ByteCount, VramDmaDestination.FromNative(transfer.DestinationWord));
        else
            vramWriteQueue.Enqueue(GunshipLiftoffTransferDefinitions.ByteCount,
                transfer.SourceAddress, VramDmaDestination.FromNative(transfer.DestinationWord));

        top.VariableB++;
        if (top.VariableB >= GunshipLiftoffTransferDefinitions.Frames.Length)
        {
            top.VariableF = (ushort)GunshipFunction.FireUpEngines;
            top.VariableB = 0;
        }
    }

    /// <summary>Ports the 128-frame engine-rumble function at <c>$A2:AC1B</c>.</summary>
    private void FireUpGunshipEngines(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Gunship takeoff lost Samus.");
        RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        ushort rumbleFrame = bottom.VariableE;
        int shake = (rumbleFrame & 1) != 0
            ? (rumbleFrame < 64 ? 1 : 2)
            : (rumbleFrame < 64 ? -1 : -2);
        samus.Position += new Offset16<RoomPixels>(0, unchecked((short)shake));
        top.Position = top.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole - 17)));
        pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
        bottom.Position = bottom.Position.WithWholeY(unchecked((ushort)(samus.Position.Y.Whole + 23)));

        bottom.VariableE++;
        if (bottom.VariableE == 64)
        {
            // The six calls at `$A2:AC9B-$ACCA` happen after the frame counter reaches
            // exactly 64. Descending projectile allocation means parameter A occupies the
            // highest surviving slot, matching the ordinary bank-$86 scheduler.
            for (ushort parameter = 0; parameter <= 10; parameter += 2)
                _projectiles.SpawnGunshipLiftoffDustCloud(parameter, samus);
        }
        if (bottom.VariableE >= 128)
        {
            top.VariableF = (ushort)GunshipFunction.SteadyLiftoff;
            top.VariableA = 0;
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
            top.VariableF = (ushort)GunshipFunction.AcceleratingLiftoff;
            _slotTable._slots[top.SlotIndex + 1].VariableF = GunshipMotionDefinitions.EscapeStartVelocity;
        }
    }

    /// <summary>Ports the accelerating takeoff function at <c>$A2:AD0E</c>.</summary>
    private void AccelerateEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        MoveEscapingGunship(top, samus);
        if (EnemyMath.IsNegative16(top.Position.Y.Whole - 256))
        {
            top.VariableF = (ushort)GunshipFunction.MoveAccelerating;
            LastGunshipEvent = GunshipFrameEvent.EscapeTakeoffCompleted;
        }
    }

    /// <summary>Ports the shared 8.8 velocity integrator at <c>$A2:AD2D</c>.</summary>
    private void MoveEscapingGunship(RoomEnemySlot top, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Escaping gunship lost Samus.");
        RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];
        bottom.VariableF = unchecked((ushort)(bottom.VariableF + GunshipMotionDefinitions.EscapeAcceleration));
        if ((SnesLittleEndian.HighByteInPlace(bottom.VariableF)) >= GunshipMotionDefinitions.EscapeVelocityCapTrigger)
            bottom.VariableF = GunshipMotionDefinitions.EscapeVelocityCap;

        uint samusY = samus.Kinematics.Position.Y.RawValue;
        samusY = unchecked(samusY - SnesSixteenSixteen.FromEightEight((uint)bottom.VariableF));
        samus.Kinematics.Position = samus.Kinematics.Position.WithY(SnesFixedPosition.FromRaw(samusY));
        MoveGunshipAndSamusToY(top, samus, samus.Position.Y.Whole);
    }

    /// <summary>Positions hull and pad at fixed whole-pixel offsets from Samus during escape.</summary>
    private void MoveGunshipAndSamusToY(
        RoomEnemySlot top,
        SamusState samus,
        ushort samusY)
    {
        samus.Position = samus.Position.WithWholeY(samusY);
        RoomEnemySlot bottom = _slotTable._slots[top.SlotIndex + 1];
        RoomEnemySlot pad = _slotTable._slots[top.SlotIndex + 2];
        top.Position = top.Position.WithWholeY(unchecked((ushort)(samusY - 17)));
        pad.Position = pad.Position.WithWholeY(unchecked((ushort)(top.Position.Y.Whole - 1)));
        bottom.Position = bottom.Position.WithWholeY(unchecked((ushort)(samusY + 23)));
    }

    /// <summary>Restores a resource by two without exceeding its maximum.</summary>
    private static ushort RestoreTwo(ushort current, ushort maximum)
    {
        if ((short)(current - maximum) >= 0)
            return current;
        return unchecked((ushort)Math.Min(current + 2, maximum));
    }

    /// <summary>Routes top-hull initialization through the native ship entry point.</summary>
    internal void RegisterGunshipTopInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_ShipTop, null, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeGunshipTop(slot);
        });
    }

    /// <summary>Routes bottom hull and entrance-pad initialization through their shared entry point.</summary>
    internal void RegisterGunshipBottomInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_ShipBottomEntrance, null, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeGunshipBottom(slot);
        });
    }

    /// <summary>Runs the top hull's lifecycle once per enemy frame.</summary>
    internal void RegisterGunshipMainAi(EnemyAiTable table)
    {
        table.RegisterMain(EnemyAiRoutine.MainAI_ShipTop, null, (slot, frame, nmiFrameCounter8) =>
        {
            RunGunshipTopMain(
                slot,
                frame.Samus,
                frame.NewlyPressedControllerInput,
                frame.VramWriteQueue);
        });
    }
}
