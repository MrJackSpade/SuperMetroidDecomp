using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Verification view of the native projectile-inheritance WRAM block (#629). Production's
/// <see cref="ProjectileInheritanceMemory"/> exposes only the behaviour gameplay needs; verification
/// also imports the block from native snapshots, inspects it as exact bytes, and transcribes the
/// cartridge's overlapping launch-velocity loads independently of production.
/// </summary>
[CpuBusBoundary("domain view: projectile-inheritance WRAM block as native bytes and overlapping words for verification")]
internal readonly struct ProjectileInheritanceRecordsAccess(ISnesMemoryBus bus)
{
    /// <summary>Distance records begin after the camera Y subspeed word.</summary>
    private const int FirstRecordOffset = 2;
    /// <summary>Each directional signed-distance record occupies four native bytes.</summary>
    private const int RecordByteCount = 4;

    /// <summary>Byte-table view over the projectile-inheritance WRAM block.</summary>
    private WorkRamByteTable Records => new(bus, ProjectileInheritanceWorkRam.Records);

    /// <summary><c>CameraYSubSpeed</c>, the block's first word.</summary>
    internal ushort CameraYSubspeed => Records.ReadWord(0);

    /// <summary>The integer word of <paramref name="direction"/>'s signed 16.16 distance record.</summary>
    internal ushort DistanceInteger(SamusCollisionDirection direction) => Records.ReadWord(RecordOffset(direction));

    /// <summary>
    /// The word the cartridge's launch-velocity load reads one byte before <paramref name="direction"/>'s
    /// record: the preceding field's high byte and the record's integer low byte.
    /// </summary>
    internal ushort WordBefore(SamusCollisionDirection direction) => Records.ReadWord(RecordOffset(direction) - 1);

    /// <summary>Writes the whole block from native snapshot bytes, in address order.</summary>
    internal void Load(ReadOnlySpan<byte> bytes) => Records.CopyFrom(bytes);

    /// <summary>The whole block's bytes, in address order.</summary>
    internal byte[] Capture()
    {
        WorkRamByteTable records = Records;
        var bytes = new byte[records.Length];
        records.CopyTo(bytes);
        return bytes;
    }

    /// <summary>The four directional distance records' bytes, in address order, without the camera subspeed.</summary>
    internal byte[] CaptureDistanceRecords() => Capture()[FirstRecordOffset..];

    /// <summary>Locates the left, right, up, or down distance record after camera subspeed.</summary>
    private static int RecordOffset(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left => FirstRecordOffset,
        SamusCollisionDirection.Right => FirstRecordOffset + RecordByteCount,
        SamusCollisionDirection.Up => FirstRecordOffset + RecordByteCount * 2,
        SamusCollisionDirection.Down => FirstRecordOffset + RecordByteCount * 3,
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}
