using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Verification view of Samus's projectile-inheritance block in native byte order. Production's
/// <see cref="ProjectileInheritanceRecords"/> exposes only the behaviour gameplay needs; verification
/// also imports the block from native snapshots, inspects it as exact bytes, and transcribes the
/// cartridge's overlapping launch-velocity loads independently of production.
/// </summary>
internal readonly struct ProjectileInheritanceRecordsAccess(ProjectileInheritanceRecords inheritance)
{
    /// <summary>Distance records begin after the camera Y subspeed word.</summary>
    private const int FirstRecordOffset = 2;
    /// <summary>Each directional signed-distance record occupies four native bytes.</summary>
    private const int RecordByteCount = 4;

    /// <summary>The block's bytes in native address order.</summary>
    private byte[] Records => inheritance.Bytes;

    /// <summary><c>CameraYSubSpeed</c>, the block's first word.</summary>
    internal ushort CameraYSubspeed => Word(0);

    /// <summary>The integer word of <paramref name="direction"/>'s signed 16.16 distance record.</summary>
    internal ushort DistanceInteger(SamusCollisionDirection direction) => Word(RecordOffset(direction));

    /// <summary>
    /// The word the cartridge's launch-velocity load reads one byte before <paramref name="direction"/>'s
    /// record: the preceding field's high byte and the record's integer low byte.
    /// </summary>
    internal ushort WordBefore(SamusCollisionDirection direction) => Word(RecordOffset(direction) - 1);

    /// <summary>Writes the whole block from native snapshot bytes, in address order.</summary>
    internal void Load(ReadOnlySpan<byte> bytes) => Ensure.LengthEqual(bytes, Records.Length).CopyTo(Records);

    /// <summary>The whole block's bytes, in address order.</summary>
    internal byte[] Capture() => (byte[])Records.Clone();

    /// <summary>The four directional distance records' bytes, in address order, without the camera subspeed.</summary>
    internal byte[] CaptureDistanceRecords() => Capture()[FirstRecordOffset..];

    /// <summary>The little-endian word whose low byte is <paramref name="offset"/> bytes into the block.</summary>
    private ushort Word(int offset) => SnesLittleEndian.Word(Records[offset], Records[offset + 1]);

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
