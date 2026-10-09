using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A6 behavior for enemy $E23F, the invisible Ceres room-control actors collectively
/// named "Ceres door" by the disassembly. These records do more than draw door frames: their
/// population parameter selects ordinary doors, the rotating elevator, Ridley's room overlay,
/// and the two walls used during the Mode-7 escape. Keeping the complete family together makes
/// those shared state transitions explicit instead of scattering cinematic exceptions through
/// the generic enemy dispatcher.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Bank-$A6 function word that switches the rotating elevator actor into its timed rumble and destruction sequence.</summary>
    private const ushort CeresDoorRotatingRumbleFunction = 0xf7dc;
    /// <summary>Bank-$A6 function word that continues the rotating elevator's palette and Mode-7 door animation.</summary>
    private const ushort CeresDoorElevatorAnimationFunction = 0xf850;
    /// <summary>Initial countdown loaded by the elevator door before its 49-call destruction sequence.</summary>
    private const ushort CeresDoorRumbleDuration = 0x0030;
    /// <summary>Actor countdown interval between successive rumble explosions.</summary>
    private const ushort CeresDoorRumbleInterval = 4;
    /// <summary>Library-two sound effect queued for each rumble explosion.</summary>
    private const ushort CeresDoorRumbleSoundEffect = 0x0025;
    /// <summary>Animation identifier used when the explosion random value selects the smoke effect.</summary>
    private const ushort CeresDoorSmokeAnimation = 3;
    /// <summary>Animation identifier used when the explosion random value selects the blast effect.</summary>
    private const ushort CeresDoorExplosionAnimation = 0x000c;
    /// <summary>Room status published when the escape door finishes its destruction sequence.</summary>
    private const ushort CeresDoorEscapedStatus = 0x8000;


    /// <summary>
    /// Most recent library-two sound queued by Ceres-door destruction during this enemy frame.
    /// The compatibility view keeps its exact cadence observable to older debugger and
    /// ROM-backed audits; active playback consumes the lossless <see cref="SoundRequests"/> list.
    /// </summary>
    public ushort? LastCeresDoorSoundEffectLibrary2 { get; private set; }

    /// <summary>Gets the installed Ceres-door artwork catalog required by initialization and palette or tile animation.</summary>
    private CeresDoorVisualCatalog SelectedCeresDoorVisual =>
        TileArtwork?.CeresDoorVisual ?? throw new InvalidOperationException(
            "Ceres door requires installed visual artwork.");

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for every retail population variant.</summary>
    private void InitializeCeresDoor(RoomEnemySlot slot)
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
        slot.VariableA = initialization.MainFunction;
        slot.CurrentInstruction = initialization.InstructionList;
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (variant == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            SelectedCeresDoorVisual.LoadTiles(_vram!);
        }

        if (CeresStatus == 0 && variant == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            SelectedCeresDoorVisual.LoadNormalColors(_cgram!, CeresDoorVisualRomData.NormalTargetColor);
            return;
        }

        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        if (CeresStatus != 0)
            SelectedCeresDoorVisual.LoadEscapeColors(_cgram!, CeresDoorVisualRomData.ActiveTargetColor);
        else
            SelectedCeresDoorVisual.LoadNormalColors(_cgram!, CeresDoorVisualRomData.ActiveTargetColor);
    }

    /// <summary>Dispatches the function word stored in Ceres-door variable A ($0FA8).</summary>
    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        switch (slot.VariableA)
        {
            case CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscape:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x0014);
                return;
            case CeresEnemyCodePointers.Function_CeresDoor_HandleEarthquakeDuringEscapeInRidleysRoom:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x001d);
                return;

            case CeresEnemyCodePointers.Function_CeresDoor_RidleyEscapeMode7Wall:
                // Ridley's room overlay begins hidden. Odd status values reveal it and swap
                // to enemy palette seven; the actor remains present so later Mode-7 drawing
                // can retain the native priority relationship with Samus and Ridley.
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if ((CeresStatus & 1) != 0)
                {
                    slot.PaletteIndex = EnemyPaletteBits.Palette7;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case CeresDoorInitializationDefinitions.RotatingElevatorRoomDefaultFunction:
                RunCeresDoorPaletteAnimation();
                if (CeresStatus >= 2)
                {
                    // $A6:F7BD installs the destruction function and initializes all three
                    // actor-owned counters, but deliberately does not execute $F7DC until
                    // the following enemy frame.
                    slot.VariableA = CeresDoorRotatingRumbleFunction;
                    slot.VariableD = CeresDoorRumbleDuration;
                    slot.VariableE = 0;
                    slot.VariableF = 0;
                }
                return;

            case CeresDoorRotatingRumbleFunction:
                RunCeresDoorRumbleAndExplosions(slot);
                return;

            case CeresDoorElevatorAnimationFunction:
                RunCeresDoorPaletteAnimation();
                return;

            default:
                throw new InvalidDataException(
                    $"Ceres door main function $A6:{slot.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Ports the two entry points at $A6:F76B/$F770 without advancing the RNG.</summary>
    private void RunCeresDoorEarthquake(ushort baseEarthquakeType)
    {
        // The door cannot start a second quake while the shared timer is active. A separate
        // engine stage decrements that timer; this actor only chooses the next type/duration.
        if (CeresStatus < 2 || EarthquakeTimer != 0)
            return;

        Func<ushort> readRandomNumber = _readRandomNumber ?? throw new InvalidOperationException(
            "Ceres door earthquake AI requires a non-advancing random-seed reader.");
        ushort lowTwelveBits = unchecked((ushort)(readRandomNumber() & 0x0fff));
        if (lowTwelveBits < 0x0080)
        {
            // Rarely select the stronger sibling quake six entries after the base type.
            EarthquakeTimer = 4;
            EarthquakeType = unchecked((ushort)(baseEarthquakeType + 6));
            return;
        }

        EarthquakeTimer = 2;
        EarthquakeType = baseEarthquakeType;
    }

    /// <summary>Ports the 49-call destruction state at $A6:F7DC exactly.</summary>
    private void RunCeresDoorRumbleAndExplosions(RoomEnemySlot slot)
    {
        slot.VariableD = unchecked((ushort)(slot.VariableD - 1));
        if ((short)slot.VariableD < 0)
        {
            // The 49th call underflows $0000 to $FFFF. Native code hides the door, returns
            // it to the perpetual elevator-animation function, and publishes $8000 so the
            // room's Mode-7 controller begins the rotation after Ridley's escape.
            slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
            slot.VariableA = CeresDoorElevatorAnimationFunction;
            if (CeresStatus != 0)
                CeresStatus = CeresDoorEscapedStatus;
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
        ushort explosionX = unchecked((ushort)(slot.XPosition + xOffset));
        ushort explosionY = unchecked((ushort)(slot.YPosition + yOffset));

        // GenerateRandomNumber is called exactly once per explosion. Values below $4000
        // select animation $0C; all other values select animation three (smoke).
        ushort randomNumber = _nextRandom!();
        ushort animation = randomNumber < 0x4000
            ? CeresDoorExplosionAnimation
            : CeresDoorSmokeAnimation;
        SpawnRoomGraphicsDustExplosion(explosionX, explosionY, animation);

        // QueueSound_Lib2_Max6 runs even if the finite enemy-projectile pool was full and
        // could not allocate the visual effect, so publish the request unconditionally.
        LastCeresDoorSoundEffectLibrary2 = CeresDoorRumbleSoundEffect;
        // New audio consumers use the lossless list; retain LastCeresDoorSoundEffectLibrary2
        // as a debugger/test compatibility view until the older per-enemy seams are migrated.
        QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, CeresDoorRumbleSoundEffect), maximumQueued: 6);
    }

    /// <summary>Updates the rotating elevator door's Mode-7 tilemap frame and animated palette colors from the shared enemy frame counter.</summary>
    private void RunCeresDoorPaletteAnimation()
    {
        // $A6:F850 selects six colors by NMI counter bits 3..5. Enemy FrameCounter advances
        // at the same accepted-frame cadence in this runtime, so slot zero is the shared
        // timebase for the room-owned palette cycle.
        ushort frame = _slots[0].FrameCounter;

        // AnimateCeresElevatorPlatform at $A6:F8F1 does not belong to either arrival
        // projectile. It survives their touchdown deletion because the rotating-room door
        // actor keeps alternating these four Mode-7 tilemap bytes forever. Omitting this
        // queue made the moving OBJ pad flash correctly, then left the landed tile platform
        // frozen on whichever frame happened to be present at deletion.
        SelectedCeresDoorVisual.LoadMode7DoorFrame(_vram!, (frame & 2) >> 1);

        int colorRow = (frame & 0x0038) >> 3;
        SelectedCeresDoorVisual.LoadAnimationColors(_cgram!, colorRow);
    }
}
