using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Movement;
using HorizontalDirection = SuperMetroid.Core.Spatial.HorizontalDirection;
using ProjectileLaunchInheritance = SuperMetroid.Core.Compatibility.ProjectileLaunchInheritance;
using SignedSixteenSixteen = SuperMetroid.Core.Spatial.SignedSixteenSixteen;
using SixteenSixteen = SuperMetroid.Core.Spatial.SixteenSixteen;
using VerticalDirection = SuperMetroid.Core.Spatial.VerticalDirection;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Verification view of Samus's projectile launch inheritance in the native snapshot's byte order: the camera vertical
/// subspeed word, then the leftward, rightward, upward and downward distances, each a whole-pixel word then a fraction
/// word. Production's <see cref="ProjectileLaunchInheritance"/> holds typed distances; verification imports them from
/// native snapshots, inspects them as exact bytes, and transcribes the native launch-velocity loads independently of
/// production.
/// </summary>
internal readonly struct ProjectileInheritanceRecordsAccess(ProjectileLaunchInheritance inheritance)
{
    /// <summary>Bytes in the native block: the camera subspeed word and four 16.16 distances.</summary>
    private const int ByteCount = 18;
    /// <summary>Distance records begin after the camera Y subspeed word.</summary>
    private const int FirstRecordOffset = 2;
    /// <summary>Each directional signed-distance record occupies four native bytes.</summary>
    private const int RecordByteCount = 4;

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
    internal void Load(ReadOnlySpan<byte> bytes)
    {
        Ensure.LengthEqual(bytes, ByteCount);
        ushort Read(int offset) => LittleEndianBytes.Word(bytes[offset], bytes[offset + 1]);
        SignedSixteenSixteen Distance(int offset) =>
            SignedSixteenSixteen.FromRaw(unchecked((int)SixteenSixteen.Compose(Read(offset), Read(offset + 2))));
        inheritance.CameraYSubspeed = Read(0);
        inheritance.Record(HorizontalDirection.Left, Distance(FirstRecordOffset));
        inheritance.Record(HorizontalDirection.Right, Distance(FirstRecordOffset + RecordByteCount));
        inheritance.Record(VerticalDirection.Up, Distance(FirstRecordOffset + RecordByteCount * 2));
        inheritance.Record(VerticalDirection.Down, Distance(FirstRecordOffset + RecordByteCount * 3));
    }

    /// <summary>The whole block's bytes, in address order.</summary>
    internal byte[] Capture()
    {
        var bytes = new byte[ByteCount];
        void Write(int offset, int word) =>
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)word));
        void WriteDistance(int offset, SignedSixteenSixteen distance)
        {
            Write(offset, distance.WholeWord);
            Write(offset + 2, distance.Fraction);
        }
        Write(0, inheritance.CameraYSubspeed);
        WriteDistance(FirstRecordOffset, inheritance.Distance(HorizontalDirection.Left));
        WriteDistance(FirstRecordOffset + RecordByteCount, inheritance.Distance(HorizontalDirection.Right));
        WriteDistance(FirstRecordOffset + RecordByteCount * 2, inheritance.Distance(VerticalDirection.Up));
        WriteDistance(FirstRecordOffset + RecordByteCount * 3, inheritance.Distance(VerticalDirection.Down));
        return bytes;
    }

    /// <summary>The four directional distance records' bytes, in address order, without the camera subspeed.</summary>
    internal byte[] CaptureDistanceRecords() => Capture()[FirstRecordOffset..];

    /// <summary>The little-endian word whose low byte is <paramref name="offset"/> bytes into the block.</summary>
    private ushort Word(int offset)
    {
        byte[] bytes = Capture();
        return LittleEndianBytes.Word(bytes[offset], bytes[offset + 1]);
    }

    /// <summary>Locates the left, right, up, or down distance record after camera subspeed.</summary>
    private static int RecordOffset(SamusCollisionDirection direction) => direction switch
    {
        SamusCollisionDirection.Left => FirstRecordOffset,
        SamusCollisionDirection.Right => FirstRecordOffset + RecordByteCount,
        SamusCollisionDirection.Up => FirstRecordOffset + RecordByteCount * 2,
        SamusCollisionDirection.Down => FirstRecordOffset + RecordByteCount * 3,
        SamusCollisionDirection.NonDirectionalProbe => throw new ArgumentOutOfRangeException(nameof(direction)),
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}
