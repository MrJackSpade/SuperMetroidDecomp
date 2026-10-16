using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres door species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CeresDoorBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CeresDoor];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CeresDoorEntity(this, slot, common);

    /// <summary>Provides Ceres room artwork, boss status, and the gameplay RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the quake type and duration selected during the escape.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the door's opening and destruction sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides slot zero's frame counter for the shared palette cycle.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Owns VRAM and CGRAM destinations for Ceres door artwork.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Spawns smoke or explosion effects around the shaking door.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies Ridley's escape status and receives the completed-escape marker.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Captures the room systems used by the Ceres door's variants and escape sequence.</summary>
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

    /// <summary>Each door slot's typed program cursor in the loaded room.</summary>
    internal readonly Dictionary<int, AnimationProgramCursor<CeresDoorProgram>> _doorPrograms = [];

    /// <summary>Runs a door's typed program where the enemy instruction interpreter would.</summary>
    private void AdvanceCeresDoorAnimation(RoomEnemySlot slot, SamusState? samus)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        AnimationProgramCursor<CeresDoorProgram> cursor = _doorPrograms[slot.SlotIndex];
        EnemyProgramAnimation.Run(slot, CeresDoorAnimations.Programs, ref cursor, CeresDoorFrameDefinitions.Spritemap,
            (cue, _) => ApplyCeresDoorCue(slot, samus, cue));
        _doorPrograms[slot.SlotIndex] = cursor;
    }

    /// <summary>Carries out one cue of a door's program, returning the program a branch continues at, or null to run on.</summary>
    private CeresDoorProgram? ApplyCeresDoorCue(RoomEnemySlot slot, SamusState? samus, CeresDoorCue cue)
    {
        switch (cue.Kind)
        {
            case CeresDoorCueKind.GoToIfSamusDistant:
            {
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Ceres door proximity instruction requires the active Samus actor.");
                }

                // `$A6:F63E` subtracts the two unsigned position words, interprets the
                // wrapped result as signed, then takes its absolute value independently
                // on each axis. If either distance is at least 48 pixels, the door branches.
                int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
                int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
                return xDistance >= 48 || yDistance >= 48 ? cue.Target : null;
            }
            case CeresDoorCueKind.GoToIfAreaBossAlive:
                // `$A6:F66A-$F676` samples bit zero of the current area's SRAM-mirror
                // boss byte. Ceres begins with that bit clear, so the facing-right door
                // loops as a tangible actor during Ridley's fight. `$A6:C117` publishes
                // the boss bit together with status two; the next pass must then run on
                // past the branch to `$F68B`'s intangible setup.
                // Hardcoding the early-game branch stranded the invisible 8x32 actor at
                // X=$0008 and clipped Samus at X=$001D after the getaway cutscene.
                return _room.Services.Bosses.IsDefeated(BossBits.AreaBoss) ? null : cue.Target;
            case CeresDoorCueKind.GoToIfRidleyNotEscaped:
                return _ridleyEncounter.CeresStatus != 0 ? null : cue.Target;
            case CeresDoorCueKind.MakeIntangible:
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return null;
            case CeresDoorCueKind.MakeTangible:
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                return null;
            case CeresDoorCueKind.HandDrawingToRidley:
                slot.VariableB = 1;
                return null;
            case CeresDoorCueKind.MakeInvisible:
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.MakeVisibleAndTakeDrawingBack:
                slot.VariableB = 0;
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.MakeVisible:
                slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                return null;
            case CeresDoorCueKind.PlayOpeningSound:
                _audio.QueueEnemySound(SoundEffectLibrary3Sounds.CeresDoorOpening, maximumQueued: 6);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CeresDoorCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initial countdown whose underflow ends the 49-update destruction sequence.</summary>
    private const ushort CeresDoorRumbleDuration = 48;
    /// <summary>Reload value for the interval between destruction effects.</summary>
    private const ushort CeresDoorRumbleInterval = 4;
    /// <summary>Room graphics animation selected for the smoke outcome.</summary>
    private const MiscDustAnimation CeresDoorSmokeAnimation = MiscDustAnimation.SmallExplosion;
    /// <summary>Room graphics animation selected for the explosion outcome.</summary>
    private const MiscDustAnimation CeresDoorExplosionAnimation = MiscDustAnimation.Smoke;

    /// <summary>
    /// Most recent library-two sound queued by Ceres-door destruction during this enemy frame.
    /// The view keeps its exact cadence observable to ROM-backed audits; active playback
    /// consumes the lossless <see cref="EnemyAudioQueue.SoundRequests"/> list.
    /// </summary>
    public ushort? LastCeresDoorSoundEffectLibrary2 { get; private set; }

    /// <summary>Gets the installed Ceres door artwork required by initialization and animation.</summary>
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

        slot.SpritemapPointer = CeresDoorFrameDefinitions.InitialSpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.VramTilesIndex = 0;
        slot.PaletteIndex = EnemyPaletteBits.Palette2;
        slot.VariableA = (ushort)initialization.MainFunction;
        _doorPrograms[slot.SlotIndex] = AnimationProgramCursor<CeresDoorProgram>.Start(initialization.Program);
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (variant == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            SelectedCeresDoorVisual.LoadTiles(_presentation.Vram);
        }

        if (_ridleyEncounter.CeresStatus == 0 && variant == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            SelectedCeresDoorVisual.LoadNormalColors(_presentation.Cgram, CeresDoorVisualRomData.NormalTargetColor);
            return;
        }

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (_ridleyEncounter.CeresStatus != 0)
            SelectedCeresDoorVisual.LoadEscapeColors(_presentation.Cgram, CeresDoorVisualRomData.ActiveTargetColor);
        else
            SelectedCeresDoorVisual.LoadNormalColors(_presentation.Cgram, CeresDoorVisualRomData.ActiveTargetColor);
    }

    /// <summary>Dispatches the function word stored in Ceres-door variable A ($0FA8).</summary>
    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        CeresDoorFunction function =
            ClosedNativeWords.Decode<CeresDoorFunction>(slot.VariableA, "Ceres door main function");
        switch (function)
        {
            case CeresDoorFunction.HandleEarthquakeDuringEscape:
                RunCeresDoorEarthquake(baseEarthquakeType: 20);
                return;
            case CeresDoorFunction.HandleEarthquakeDuringEscapeInRidleysRoom:
                RunCeresDoorEarthquake(baseEarthquakeType: 29);
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
        if (_ridleyEncounter.CeresStatus < 2 || !_earthquake.EarthquakeTimer.IsZero)
            return;

        Func<ushort> readRandomNumber = _room.Random.Read;
        // One random word in 32 (its low twelve bits below 128) picks the stronger quake.
        ushort lowTwelveBits = unchecked((ushort)(readRandomNumber() % 4096));
        if (lowTwelveBits < 128)
        {
            // Rarely select the stronger sibling quake six entries after the base type.
            _earthquake.EarthquakeTimer = NativeCountdown.Start(4);
            _earthquake.EarthquakeType = unchecked((ushort)(baseEarthquakeType + 6));
            return;
        }

        _earthquake.EarthquakeTimer = NativeCountdown.Start(2);
        _earthquake.EarthquakeType = baseEarthquakeType;
    }

    /// <summary>Ports the 49-call destruction state at $A6:F7DC exactly.</summary>
    internal void RunCeresDoorRumbleAndExplosions(RoomEnemySlot slot)
    {
        NativeWordCounterStep rumble = NativeWordCounter.Decrement(slot.VariableD);
        slot.VariableD = rumble.Value;
        if (rumble.IsNegative)
        {
            // The 49th call underflows $0000 to $FFFF. Native code hides the door, returns
            // it to the perpetual elevator-animation function, and publishes $8000 so the
            // room's Mode-7 controller begins the rotation after Ridley's escape.
            slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
            slot.VariableA = (ushort)CeresDoorFunction.ElevatorAnimation;
            if (_ridleyEncounter.CeresStatus != 0)
                _ridleyEncounter.CeresStatus = CeresStatusDefinitions.DoorEscaped;
            return;
        }

        NativeWordCounterStep interval = NativeWordCounter.Decrement(slot.VariableE);
        slot.VariableE = interval.Value;
        if (!interval.IsNegative)
            return;

        slot.VariableE = CeresDoorRumbleInterval;
        NativeWordCounterStep burst = NativeWordCounter.Decrement(slot.VariableF);
        slot.VariableF = burst.IsNegative ? unchecked((ushort)(CeresDoorRumbleGeometryDefinitions.Count - 1)) : burst.Value;

        (short xOffset, short yOffset) = CeresDoorRumbleGeometryDefinitions.Offset(slot.VariableF);
        ushort explosionX = unchecked((ushort)(slot.Position.X.Whole + xOffset));
        ushort explosionY = unchecked((ushort)(slot.Position.Y.Whole + yOffset));

        // GenerateRandomNumber is called exactly once per explosion. Values below $4000
        // select animation $0C; all other values select animation three (smoke).
        ushort randomNumber = _room.Random.Generate();
        MiscDustAnimation animation = randomNumber < 16384
            ? CeresDoorExplosionAnimation
            : CeresDoorSmokeAnimation;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(explosionX, explosionY), animation);

        // QueueSound_Lib2_Max6 runs even if the finite enemy-projectile pool was full and
        // could not allocate the visual effect, so publish the request unconditionally.
        LastCeresDoorSoundEffectLibrary2 = SoundEffectLibrary2Sounds.CeresDoorExplosion.Value;
        // New audio consumers use the lossless list; retain LastCeresDoorSoundEffectLibrary2
        // as a debugger/test compatibility view until the older per-enemy seams are migrated.
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.CeresDoorExplosion, maximumQueued: 6);
    }

    /// <summary>Cycles door colors and the room's Mode-7 elevator frame from slot zero's frame counter.</summary>
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
        SelectedCeresDoorVisual.LoadMode7DoorFrame(_presentation.Vram, (frame & 2) >> 1);

        // Bits 3..5 of the frame counter: a new row of six colors every eight frames.
        int colorRow = (frame >> 3) % 8;
        SelectedCeresDoorVisual.LoadAnimationColors(_presentation.Cgram, colorRow);
    }

    /// <summary>Ports CeresDoor's initialization AI.</summary>
    internal void RunCeresDoorInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeCeresDoor(slot);
    }

    /// <summary>Ports CeresDoor's main AI.</summary>
    internal void RunCeresDoorMainAi(RoomEnemySlot slot, IEnemyAiFrame _, byte _1)
    {
        RunCeresDoorMain(slot);
    }

    /// <summary>Advances CeresDoor's typed animation.</summary>
    internal void AdvanceCeresDoorTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceCeresDoorAnimation(slot, context.Samus);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastCeresDoorSoundEffectLibrary2 = null;
        _doorPrograms.Clear();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCeresDoorSoundEffectLibrary2 = null;
    }
}
