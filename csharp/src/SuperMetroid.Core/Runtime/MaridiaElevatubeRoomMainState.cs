using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Shared translation of Maridia elevatube room main <c>$8F:E2B6</c> and the four door
/// callbacks that initialize or release it. The state preserves the cartridge's four
/// scratch words so their signed fixed-point behavior remains debugger-visible.
/// </summary>
public sealed class MaridiaElevatubeRoomMainState
{
    /// <summary>Whether the selected room state names main routine $8F:E2B6.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Native RoomMainASMVar1: fractional half of the tube's tracked position.</summary>
    public ushort PositionSubposition { get; private set; }

    /// <summary>Native RoomMainASMVar2: whole-pixel half of the tube's tracked position.</summary>
    public ushort Position { get; private set; }

    /// <summary>Native RoomMainASMVar3: signed 8.8 vertical velocity.</summary>
    public ushort Velocity { get; private set; }

    /// <summary>Native RoomMainASMVar4: signed 8.8 acceleration.</summary>
    public ushort Acceleration { get; private set; }

    /// <summary>Clears shared room-main scratch and selects whether $E2B6 owns this room.</summary>
    public void Reset(bool active)
    {
        IsActive = active;
        PositionSubposition = 0;
        Position = 0;
        Velocity = 0;
        Acceleration = 0;
    }

    /// <summary>Applies door callback $8F:E26C for entry from Oasis to the south.</summary>
    public void SetUpFromSouth(SamusState samus)
    {
        EnsureActive(DoorCodes.DoorASM_SetupElevatubeFromSouth);
        ArgumentNullException.ThrowIfNull(samus);
        PositionSubposition = 0;
        Position = MaridiaElevatubeRomData.SouthStartingPosition;
        Velocity = MaridiaElevatubeRomData.SouthStartingVelocity;
        Acceleration = MaridiaElevatubeRomData.SouthAcceleration;
        samus.InputLocked = true;
    }

    /// <summary>Applies door callback $8F:E291 for entry from Plasma Spark to the north.</summary>
    public void SetUpFromNorth(SamusState samus)
    {
        EnsureActive(DoorCodes.DoorASM_SetupElevatubeFromNorth);
        ArgumentNullException.ThrowIfNull(samus);
        PositionSubposition = 0;
        Position = MaridiaElevatubeRomData.NorthStartingPosition;
        Velocity = MaridiaElevatubeRomData.NorthStartingVelocity;
        Acceleration = MaridiaElevatubeRomData.NorthAcceleration;
        samus.InputLocked = true;
    }

    /// <summary>
    /// Applies Samus command one used by both exit callbacks. Room loading has already
    /// selected a non-elevatube main routine, so no host-side movement state survives.
    /// </summary>
    public void ResetOnExit(SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        samus.InputLocked = false;
        IsActive = false;
    }

    /// <summary>
    /// Runs one call of $8F:E2B6: pin X to $0080, integrate signed 8.8 velocity into both
    /// the room scratch position and Samus's 16.16 position through the ordinary bank-$94
    /// vertical collision dispatcher, then apply the cartridge's bounded acceleration.
    /// </summary>
    public BlockMoveResult? Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState? samus,
        ushort nmiFrameCounter,
        RoomPlmSystem plms)
    {
        if (!IsActive)
            return null;
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(plms);
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Maridia elevatube room main requires an active Samus state.");

        // Native stores the whole and fractional X words separately every frame.
        activeSamus.Kinematics.SetXFixed(
            (uint)MaridiaElevatubeRomData.SamusCenterX << 16);

        int displacement = unchecked((short)Velocity) << 8;
        uint trackedPosition = ((uint)Position << 16) | PositionSubposition;
        trackedPosition = unchecked(trackedPosition + (uint)displacement);
        Position = unchecked((ushort)(trackedPosition >> 16));
        PositionSubposition = unchecked((ushort)trackedPosition);

        BlockMoveResult movement = SamusBlockCollision.MoveVertical(
            bus,
            level,
            activeSamus.Kinematics,
            displacement,
            scanLeftToRight: (nmiFrameCounter & 1) == 0,
            includeSolidEnemies: false,
            plms: plms);

        // The assembly performs an unsigned range test after biasing by $0E20. This is
        // precisely the inclusive signed range -$0E20..+$0E20; a candidate outside it is
        // rejected rather than clamped, leaving the preceding velocity intact.
        ushort candidate = unchecked((ushort)(Velocity + Acceleration));
        ushort biased = unchecked((ushort)(candidate + MaridiaElevatubeRomData.MaximumSpeed));
        if (biased < MaridiaElevatubeRomData.SpeedRangeExclusiveEnd)
            Velocity = candidate;

        return movement;
    }

    private void EnsureActive(ushort setupPointer)
    {
        if (IsActive)
            return;
        throw new InvalidDataException(
            $"Elevatube setup callback $8F:{setupPointer:X4} selected a room without main $8F:E2B6.");
    }
}

/// <summary>Immediate values and entry point owned by Maridia's elevatube cartridge code.</summary>
internal static class MaridiaElevatubeRomData
{
    /// <summary>Whole X coordinate written on every room-main call.</summary>
    public const ushort SamusCenterX = 0x0080;

    /// <summary>South-entry RoomMainASMVar2.</summary>
    public const ushort SouthStartingPosition = 0x09c0;

    /// <summary>South-entry RoomMainASMVar3, or -1.0 pixels/frame in signed 8.8.</summary>
    public const ushort SouthStartingVelocity = 0xff00;

    /// <summary>South-entry RoomMainASMVar4, or -0.125 pixels/frame² in signed 8.8.</summary>
    public const ushort SouthAcceleration = 0xffe0;

    /// <summary>North-entry RoomMainASMVar2.</summary>
    public const ushort NorthStartingPosition = 0x0040;

    /// <summary>North-entry RoomMainASMVar3, or +1.0 pixels/frame in signed 8.8.</summary>
    public const ushort NorthStartingVelocity = 0x0100;

    /// <summary>North-entry RoomMainASMVar4, or +0.125 pixels/frame² in signed 8.8.</summary>
    public const ushort NorthAcceleration = 0x0020;

    /// <summary>Bias and absolute velocity limit used by $8F:E2F1.</summary>
    public const ushort MaximumSpeed = 0x0e20;

    /// <summary>Exclusive biased comparison endpoint used by $8F:E2F4.</summary>
    public const ushort SpeedRangeExclusiveEnd = 0x1c41;
}
