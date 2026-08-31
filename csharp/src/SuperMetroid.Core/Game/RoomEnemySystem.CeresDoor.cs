using SuperMetroid.Core.Hardware;

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
    private const ushort CeresDoorVariantCount = 7;
    private const ushort CeresDoorInitialSpritemap = 0xfac7;
    private const ushort CeresDoorInitializerFunctionTable = 0xf72b;
    private const ushort CeresDoorInitialInstructionTable = 0xf52c;
    private const ushort CeresDoorRotatingDefaultFunction = 0xf7bd;
    private const ushort CeresDoorRotatingRumbleFunction = 0xf7dc;
    private const ushort CeresDoorElevatorAnimationFunction = 0xf850;
    private const ushort CeresDoorRumbleDuration = 0x0030;
    private const ushort CeresDoorRumbleInterval = 4;
    private const ushort CeresDoorRumbleSoundEffect = 0x0025;
    private const ushort CeresDoorSmokeAnimation = 3;
    private const ushort CeresDoorExplosionAnimation = 0x000c;
    private const ushort CeresDoorEscapedStatus = 0x8000;

    // $A6:F840 stores four interleaved signed X/Y pairs. The actor decrements its index
    // before reading this table, so a newly started sequence visits 3, 2, 1, 0, then repeats.
    private static readonly (short X, short Y)[] CeresDoorRumbleOffsets =
    [
        (-4, -8),
        (0, 4),
        (-2, 22),
        (2, 12),
    ];

    /// <summary>
    /// Most recent library-two sound queued by Ceres-door destruction during this enemy frame.
    /// The frontend does not yet synthesize SPC audio, so exposing the native queue request keeps
    /// its exact cadence observable to the debugger and ROM-backed audits.
    /// </summary>
    public ushort? LastCeresDoorSoundEffectLibrary2 { get; private set; }

    /// <summary>Ports <c>CeresDoor_Init</c> at $A6:F6C5 for every retail population variant.</summary>
    private void InitializeCeresDoor(RoomEnemySlot slot)
    {
        // Native Enemy.init0 ($0FB4) is populated from the record's parameter-one word and
        // remains mutable after load. RoomEnemySlot.Parameter1 is that exact WRAM field.
        // The earlier population "init" word seeds Enemy.instList before initialization;
        // it is not the variant despite its host-side historical name.
        ushort variant = slot.Parameter1;

        // Both ROM tables contain one word per initialization variant. Variants five and six
        // are the left/right OBJ walls spawned at $A6:A9A5 for Ridley's Mode-7 departure;
        // accepting all seven entries is required for the live escape sequence.
        if (variant >= CeresDoorVariantCount)
        {
            throw new InvalidDataException(
                $"Ceres door initialization variant ${variant:X4} exceeds its seven variants.");
        }

        slot.SpritemapPointer = CeresDoorInitialSpritemap;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.VramTilesIndex = 0;
        slot.PaletteIndex = 0x0400;
        int tableOffset = variant * 2;
        slot.VariableA = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(CeresDoorInitializerFunctionTable + tableOffset)));
        slot.CurrentInstruction = ReadWord(
            _bus!,
            0xa60000 | unchecked((ushort)(CeresDoorInitialInstructionTable + tableOffset)));
        slot.VariableB = 0;

        // CeresDoor_Func_1 performs this extra direct transfer only for variant two. The
        // source/destination are the literal reconstructed DMA record at $A6:F739.
        if (variant == 2)
        {
            // The source register used bank $B0 with a 16-bit address that increments
            // independently of the bank byte. Materialize that exact DMA source slice,
            // then use SnesVram's range-checked consecutive transfer primitive.
            byte[] tileBytes = new byte[0x0400];
            for (int byteIndex = 0; byteIndex < tileBytes.Length; byteIndex++)
                tileBytes[byteIndex] = _bus!.ReadByte(0xb00000 | ((0xc400 + byteIndex) & 0xffff));
            _vram!.LoadBytes(0xe000, tileBytes);
        }

        if (CeresStatus == 0 && variant == 3)
        {
            // The native destination $142 is a byte offset into target_palettes: colors
            // 161..175. This runtime exposes the final fade target directly in CGRAM.
            _cgram!.LoadFromBus(_bus!, 0xa6f4ee, colorCount: 15, destinationIndex: 0x142 / 2);
            return;
        }

        slot.PaletteIndex = 0x0e00;
        int source = CeresStatus != 0 ? 0xa6f50e : 0xa6f4ee;
        _cgram!.LoadFromBus(_bus!, source, colorCount: 15, destinationIndex: 0x1e2 / 2);
    }

    /// <summary>Dispatches the function word stored in Ceres-door variable A ($0FA8).</summary>
    private void RunCeresDoorMain(RoomEnemySlot slot)
    {
        switch (slot.VariableA)
        {
            case 0xf76b:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x0014);
                return;
            case 0xf770:
                RunCeresDoorEarthquake(baseEarthquakeType: 0x001d);
                return;

            case 0xf7a5:
                // Ridley's room overlay begins hidden. Odd status values reveal it and swap
                // to enemy palette seven; the actor remains present so later Mode-7 drawing
                // can retain the native priority relationship with Samus and Ridley.
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                if ((CeresStatus & 1) != 0)
                {
                    slot.PaletteIndex = 0x0e00;
                    slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
                }
                return;

            case CeresDoorRotatingDefaultFunction:
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
            slot.VariableF = unchecked((ushort)(CeresDoorRumbleOffsets.Length - 1));

        (short xOffset, short yOffset) = CeresDoorRumbleOffsets[slot.VariableF];
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
    }

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
        ushort transferPointer = ReadWord(_bus!, 0xa6f900 + (frame & 2));
        ApplyMode7TransferList(transferPointer);

        ushort sourcePointer = unchecked((ushort)(2 * (frame & 0x0038) - 0x078f));
        _cgram!.LoadFromBus(
            _bus!,
            0xa60000 | sourcePointer,
            colorCount: 6,
            destinationIndex: 0x52 / 2);
    }
}
