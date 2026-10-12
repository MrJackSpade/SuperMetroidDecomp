using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The projectile-inheritance WRAM block (#629).</summary>
[CpuBusBoundary("catalog: projectile-inheritance WRAM")]
internal static class ProjectileInheritanceWorkRam
{
    /// <summary>
    /// <c>$7E:0DA8-$7E:0DB9</c>, addressed through the bank-$00 low-WRAM mirror: <c>CameraYSubSpeed</c>
    /// ($0DA8) followed by four signed 16.16 distance records, integer word then fraction word:
    /// <c>ProjSpeed_DistanceSamusMovedLeft</c> ($0DAA), <c>...Right</c> ($0DAE), <c>...Up</c> ($0DB2)
    /// and <c>...Down</c> ($0DB6). The launch-velocity load reads the word one byte before each
    /// record, across the preceding field's high byte.
    /// </summary>
    internal static readonly CpuBusCell Records = CpuBusCell.WramBytes(nameof(Records), 0x000da8, 18,
        "u16 camera Y subspeed, then signed 16.16 distance[4] (left, right, up, down)");
}

/// <summary>Typed view of the projectile-inheritance WRAM block (#629).</summary>
[CpuBusBoundary("domain view: projectile-inheritance WRAM")]
internal readonly struct ProjectileInheritanceMemory(ISnesMemoryBus bus)
{
    private const int CameraYSubspeedOffset = 0;
    private const int FirstRecordOffset = 2;
    private const int RecordByteCount = 4;

    private WorkRamByteTable Records => new(bus, ProjectileInheritanceWorkRam.Records);

    /// <summary><c>CameraYSubSpeed</c>; its high byte leaks into leftward launch velocity.</summary>
    internal ushort CameraYSubspeed
    {
        set => Records.WriteWord(CameraYSubspeedOffset, value);
    }

    /// <summary>Stores one signed 16.16 distance: the integer word, then the fraction word.</summary>
    internal void Record(SamusCollisionDirection direction, int signedDisplacement)
    {
        int offset = RecordOffset(direction);
        Records.WriteWord(offset, SnesFixedPosition.WholeOf(signedDisplacement));
        Records.WriteWord(offset + 2, unchecked((ushort)signedDisplacement));
    }

    /// <summary>Zeroes the four distance records in address order, leaving the camera subspeed.</summary>
    internal void ClearMovement()
    {
        WorkRamByteTable records = Records;
        for (int offset = FirstRecordOffset; offset < records.Length; offset++)
            records[offset] = 0;
    }

    /// <summary>The overlapping word one byte before <paramref name="direction"/>'s record.</summary>
    internal ushort LaunchVelocityWord(SamusCollisionDirection direction) =>
        Records.ReadWord(RecordOffset(direction) - 1);

    private static int RecordOffset(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left => FirstRecordOffset,
        SamusCollisionDirection.Right => FirstRecordOffset + RecordByteCount,
        SamusCollisionDirection.Up => FirstRecordOffset + RecordByteCount * 2,
        SamusCollisionDirection.Down => FirstRecordOffset + RecordByteCount * 3,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}

/// <summary>
/// Owns the native projectile-inheritance WRAM records. They are deliberately not
/// reconstructed from net frame displacement: individual movement calls overwrite
/// one direction, and the velocity reader crosses adjacent field boundaries.
/// </summary>
internal static class SamusProjectileInheritance
{
    internal static EightEightVector<RoomPixels> ReadVelocity(ISnesAddressSpace bus, ushort directionWord, short baseSpeed)
    {
        var word = new SamusProjectileDirectionWord(directionWord);
        if (word.DirectionIndex > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${word.DirectionIndex:X2}.");
        SamusProjectileDirection direction = word.Direction;
        ProjectileInheritanceMemory memory = Memory(bus);
        ushort upward = memory.LaunchVelocityWord(SamusCollisionDirection.Up);
        int upContribution = (SnesLittleEndian.HighByteInPlace(upward)) == 0 ? 0 : (upward >> 2) | 0xc000;
        short x = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight =>
                baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Right),
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft =>
                -baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Left),
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        short y = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or
                SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft => -baseSpeed + upContribution,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.DownLeft =>
                baseSpeed + memory.LaunchVelocityWord(SamusCollisionDirection.Down),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        return new(x, y);
    }

    /// <summary>Native alpha tail clears directional pairs, but not camera Y subspeed.</summary>
    internal static void ClearMovement(ISnesAddressSpace bus) => Memory(bus).ClearMovement();

    internal static void PublishCameraYSubspeed(ISnesAddressSpace bus, ushort value)
    {
        ProjectileInheritanceMemory memory = Memory(bus);
        memory.CameraYSubspeed = value;
    }

    internal static void Record(ISnesAddressSpace bus, SamusCollisionDirection direction, int signedDisplacement) =>
        Memory(bus).Record(direction, signedDisplacement);

    private static ProjectileInheritanceMemory Memory(ISnesAddressSpace bus) =>
        new(SnesMemoryBusRequirement.Require(bus, "Projectile inheritance requires live WRAM."));
}
