using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The Ceres door species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CeresDoorBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly RoomEarthquake _earthquake;
    private readonly EnemyAudioQueue _audio;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyPresentation _presentation;
    private readonly EnemyProjectileEngine _projectiles;
    private readonly RidleyEncounter _ridleyEncounter;

    internal CeresDoorBehavior(EnemyRoomBinding room, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemySlotTable slotTable, EnemyPresentation presentation, EnemyProjectileEngine projectiles, RidleyEncounter ridleyEncounter)
    {
        _room = room;
        _earthquake = earthquake;
        _audio = audio;
        _slotTable = slotTable;
        _presentation = presentation;
        _projectiles = projectiles;
        _ridleyEncounter = ridleyEncounter;
    }

    /// <summary>Executes the Ceres door's private animation instructions.</summary>
    private bool TryProcessCeresDoorInstruction(RoomEnemySlot slot, SamusState? samus, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.CeresDoor ||
            !Enum.IsDefined((CeresDoorInstruction)word))
            return false;

        switch ((CeresDoorInstruction)word)
        {
            case CeresDoorInstruction.GotoYIfSamusIsDistant:
            {
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres door proximity instruction requires the active Samus actor.");
                }

                // `$A6:F63E` subtracts the two unsigned position words, interprets the
                // wrapped result as signed, then takes its absolute value independently
                // on each axis. If either distance is at least $30, the operand is a
                // same-bank loop target; otherwise execution skips that operand.
                int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
                int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
                cursor = xDistance >= 0x30 || yDistance >= 0x30
                    ? EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)))
                    : unchecked((ushort)(cursor + 4));
                return true;
            }
            case CeresDoorInstruction.GotoYIfAreaBossIsAlive:
                // `$A6:F66A-$F676` samples bit zero of the current area's SRAM-mirror
                // boss byte. Ceres begins with that bit clear, so the facing-right door
                // loops as a tangible actor during Ridley's fight. `$A6:C117` publishes
                // the boss bit together with status two; the next instruction pass must
                // then skip the branch operand and reach `$F68B`'s intangible setup.
                // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                // X=$0008 and clipped Samus at X=$001D after the getaway cutscene.
                cursor = _room.Services.Bosses.IsDefeated(BossBits.AreaBoss)
                    ? unchecked((ushort)(cursor + 4))
                    : EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                return true;
            case CeresDoorInstruction.GotoYIfCeresRidleyHasNotEscaped:
                cursor = _ridleyEncounter.CeresStatus != 0
                    ? unchecked((ushort)(cursor + 4))
                    : EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                return true;
            case CeresDoorInstruction.SetAsIntangible:
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.SetAsTangible:
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.SetDrawnByRidleyFlag:
                slot.VariableB = 1;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.SetAsInvisible:
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.SetAsVisibleClearDrawnByRidleyFlag:
                slot.VariableB = 0;
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.SetAsVisible:
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case CeresDoorInstruction.QueueOpeningSFX:
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, CeresDoorInstructionSounds.Opening), maximumQueued: 6);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Ceres door does not own instruction ${word:X4}.");
        }
    }

    private const ushort CeresDoorRumbleDuration = 0x0030;
    private const ushort CeresDoorRumbleInterval = 4;
    private const ushort CeresDoorRumbleSoundEffect = 0x0025;
    private const ushort CeresDoorSmokeAnimation = 3;
    private const ushort CeresDoorExplosionAnimation = 0x000c;
    private const ushort CeresDoorEscapedStatus = 0x8000;


    /// <summary>
    /// Most recent library-two sound queued by Ceres-door destruction during this enemy frame.
    /// The view keeps its exact cadence observable to ROM-backed audits; active playback
    /// consumes the lossless <see cref="EnemyAudioQueue.SoundRequests"/> list.
    /// </summary>
    public ushort? LastCeresDoorSoundEffectLibrary2 { get; private set; }

    private CeresDoorVisualCatalog SelectedCeresDoorVisual =>
        _room.Artwork?.CeresDoorVisual ?? throw new InvalidOperationException(
            "Ceres door requires installed visual artwork.");

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for every retail population variant.</summary>
    internal void InitializeCeresDoor(RoomEnemySlot slot)
    {
        // Native Enemy.init0 ($0FB4) is populated from the record's parameter-one word and
        // remains mutable after load. RoomEnemySlot.Parameter1 is that exact WRAM field.
        // The earlier population "init" word seeds Enemy.instList before initialization;
        // it is not the variant despite its host-side historical name.
        ushort variant = slot.Parameter1;

        CeresDoorInitializationDefinition initialization =
            CeresDoorInitializationDefinitions.For(variant);

        slot.SpritemapPointer = CeresDoorInstructionProgramDefinitions.InitialSpritemap;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.VramTilesIndex = 0;
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        slot.VariableA = (ushort)initialization.MainFunction;
        slot.CurrentInstruction = initialization.InstructionList;
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (variant == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            SelectedCeresDoorVisual.LoadTiles(_presentation._vram!);
        }

        if (_ridleyEncounter.CeresStatus == 0 && variant == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            SelectedCeresDoorVisual.LoadNormalColors(_presentation._cgram!, CeresDoorVisualRomData.NormalTargetColor);
            return;
        }

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (_ridleyEncounter.CeresStatus != 0)
            SelectedCeresDoorVisual.LoadEscapeColors(_presentation._cgram!, CeresDoorVisualRomData.ActiveTargetColor);
        else
            SelectedCeresDoorVisual.LoadNormalColors(_presentation._cgram!, CeresDoorVisualRomData.ActiveTargetColor);
    }

    /// <summary>Dispatches the function word stored in Ceres-door variable A ($0FA8).</summary>
    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        CeresDoorFunction function =
            ClosedNativeWords.Decode<CeresDoorFunction>(slot.VariableA, "Ceres door main function");
        switch (function)
        {
            case CeresDoorFunction.HandleEarthquakeDuringEscape:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x0014);
                return;
            case CeresDoorFunction.HandleEarthquakeDuringEscapeInRidleysRoom:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x001d);
                return;

            case CeresDoorFunction.RidleyEscapeMode7Wall:
                // Ridley's room overlay begins hidden. Odd status values reveal it and swap
                // to enemy palette seven; the actor remains present so later Mode-7 drawing
                // can retain the native priority relationship with Samus and Ridley.
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if ((_ridleyEncounter.CeresStatus & 1) != 0)
                {
                    slot.PaletteIndex = EnemyPaletteBits.Palette7;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case CeresDoorFunction.RotatingElevatorRoomDefault:
                RunCeresDoorPaletteAnimation();
                if (_ridleyEncounter.CeresStatus >= 2)
                {
                    // $A6:F7BD installs the destruction function and initializes all three
                    // actor-owned counters, but deliberately does not execute $F7DC until
                    // the following enemy frame.
                    slot.VariableA = (ushort)CeresDoorFunction.RotatingElevatorRumble;
                    slot.VariableD = CeresDoorRumbleDuration;
                    slot.VariableE = 0;
                    slot.VariableF = 0;
                }
                return;

            case CeresDoorFunction.RotatingElevatorRumble:
                RunCeresDoorRumbleAndExplosions(slot);
                return;

            case CeresDoorFunction.ElevatorAnimation:
                RunCeresDoorPaletteAnimation();
                return;

            default:
                throw new InvalidOperationException($"Undefined CeresDoorFunction {function}.");
        }
    }

    /// <summary>Ports the two entry points at $A6:F76B/$F770 without advancing the RNG.</summary>
    private void RunCeresDoorEarthquake(ushort baseEarthquakeType)
    {
        // The door cannot start a second quake while the shared timer is active. A separate
        // engine stage decrements that timer; this actor only chooses the next type/duration.
        if (_ridleyEncounter.CeresStatus < 2 || _earthquake.EarthquakeTimer != 0)
            return;

        Func<ushort> readRandomNumber = _room.Random.Read;
        ushort lowTwelveBits = unchecked((ushort)(readRandomNumber() & 0x0fff));
        if (lowTwelveBits < 0x0080)
        {
            // Rarely select the stronger sibling quake six entries after the base type.
            _earthquake.EarthquakeTimer = 4;
            _earthquake.EarthquakeType = unchecked((ushort)(baseEarthquakeType + 6));
            return;
        }

        _earthquake.EarthquakeTimer = 2;
        _earthquake.EarthquakeType = baseEarthquakeType;
    }

    /// <summary>Ports the 49-call destruction state at $A6:F7DC exactly.</summary>
    internal void RunCeresDoorRumbleAndExplosions(RoomEnemySlot slot)
    {
        slot.VariableD = unchecked((ushort)(slot.VariableD - 1));
        if ((short)slot.VariableD < 0)
        {
            // The 49th call underflows $0000 to $FFFF. Native code hides the door, returns
            // it to the perpetual elevator-animation function, and publishes $8000 so the
            // room's Mode-7 controller begins the rotation after Ridley's escape.
            slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
            slot.VariableA = (ushort)CeresDoorFunction.ElevatorAnimation;
            if (_ridleyEncounter.CeresStatus != 0)
                _ridleyEncounter.CeresStatus = CeresDoorEscapedStatus;
            return;
        }

        slot.VariableE = unchecked((ushort)(slot.VariableE - 1));
        if ((short)slot.VariableE >= 0)
            return;

        slot.VariableE = CeresDoorRumbleInterval;
        slot.VariableF = unchecked((ushort)(slot.VariableF - 1));
        if ((short)slot.VariableF < 0)
            slot.VariableF = unchecked((ushort)(CeresDoorRumbleGeometryDefinitions.Count - 1));

        (short xOffset, short yOffset) = CeresDoorRumbleGeometryDefinitions.Offset(slot.VariableF);
        ushort explosionX = unchecked((ushort)(slot.Position.X.Whole + xOffset));
        ushort explosionY = unchecked((ushort)(slot.Position.Y.Whole + yOffset));

        // GenerateRandomNumber is called exactly once per explosion. Values below $4000
        // select animation $0C; all other values select animation three (smoke).
        ushort randomNumber = _room.Random.Generate();
        ushort animation = randomNumber < 0x4000
            ? CeresDoorExplosionAnimation
            : CeresDoorSmokeAnimation;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(explosionX, explosionY), animation);

        // QueueSound_Lib2_Max6 runs even if the finite enemy-projectile pool was full and
        // could not allocate the visual effect, so publish the request unconditionally.
        LastCeresDoorSoundEffectLibrary2 = CeresDoorRumbleSoundEffect;
        // New audio consumers use the lossless list; retain LastCeresDoorSoundEffectLibrary2
        // as a debugger/test compatibility view until the older per-enemy seams are migrated.
        _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, CeresDoorRumbleSoundEffect), maximumQueued: 6);
    }

    internal void RunCeresDoorPaletteAnimation()
    {
        // $A6:F850 selects six colors by NMI counter bits 3..5. Enemy FrameCounter advances
        // at the same accepted-frame cadence in this runtime, so slot zero is the shared
        // timebase for the room-owned palette cycle.
        ushort frame = _slotTable._slots[0].FrameCounter;

        // AnimateCeresElevatorPlatform at $A6:F8F1 does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating these four Mode-7 tilemap bytes forever. Omitting this
        // queue made the moving OBJ pad flash correctly, then left the landed tile platform
        // frozen on whichever frame happened to be present at deletion.
        SelectedCeresDoorVisual.LoadMode7DoorFrame(_presentation._vram!, (frame & 2) >> 1);

        int colorRow = (frame & 0x0038) >> 3;
        SelectedCeresDoorVisual.LoadAnimationColors(_presentation._cgram!, colorRow);
    }

    internal void RegisterCeresDoorInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_CeresDoor, null, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeCeresDoor(slot);
        });
    }

    internal void RegisterCeresDoorMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_CeresDoor, null, (slot, frame, nmiFrameCounter8) =>
        {
            RunCeresDoorMain(slot);
        });
    }

    internal void RegisterCeresDoorOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessCeresDoorInstruction(slot, context.Samus, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastCeresDoorSoundEffectLibrary2 = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCeresDoorSoundEffectLibrary2 = null;
    }
}
