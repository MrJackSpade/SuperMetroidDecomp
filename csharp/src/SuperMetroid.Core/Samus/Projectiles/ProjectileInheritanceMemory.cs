using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Typed view of the projectile-inheritance WRAM block (#629).</summary>
[CpuBusBoundary("domain view: projectile-inheritance WRAM")]
internal readonly struct ProjectileInheritanceMemory(ISnesMemoryBus bus)
{
    /// <summary>Camera subspeed occupies the word before the four movement records.</summary>
    private const int CameraYSubspeedOffset = 0;
    /// <summary>Leftward movement begins immediately after camera Y subspeed.</summary>
    private const int FirstRecordOffset = 2;
    /// <summary>Each signed 16.16 direction record has a whole and fractional word.</summary>
    private const int RecordByteCount = 4;

    /// <summary>Provides byte and word access to the shared WRAM inheritance block.</summary>
    private WorkRamByteTable Records => new(bus, ProjectileInheritanceWorkRamAddresses.Records);

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

    /// <summary>Maps left, right, up, or down to its native 16.16 record offset.</summary>
    private static int RecordOffset(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left => FirstRecordOffset,
        SamusCollisionDirection.Right => FirstRecordOffset + RecordByteCount,
        SamusCollisionDirection.Up => FirstRecordOffset + RecordByteCount * 2,
        SamusCollisionDirection.Down => FirstRecordOffset + RecordByteCount * 3,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}
