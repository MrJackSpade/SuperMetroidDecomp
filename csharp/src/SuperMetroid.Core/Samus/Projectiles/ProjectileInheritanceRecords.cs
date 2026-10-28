using System.Buffers.Binary;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile-inheritance block, kept in the cartridge's byte layout (<c>$7E:0DA8-$7E:0DB9</c>):
/// <c>CameraYSubSpeed</c>, then four signed 16.16 distance records, integer word then fraction word, for left
/// (<c>ProjSpeed_DistanceSamusMovedLeft</c>), right, up and down. The records are deliberately not reconstructed from
/// net frame displacement: each movement call overwrites one direction, and the launch-velocity load reads the word one
/// byte before each record, across the preceding field's high byte. Samus owns the block.
/// </summary>
public sealed class ProjectileInheritanceRecords
{
    /// <summary>Bytes in the block: the camera subspeed word and four 16.16 records.</summary>
    internal const int ByteCount = 18;
    /// <summary>Camera subspeed occupies the word before the four movement records.</summary>
    private const int CameraYSubspeedOffset = 0;
    /// <summary>Leftward movement begins immediately after camera Y subspeed.</summary>
    private const int FirstRecordOffset = 2;
    /// <summary>Each signed 16.16 direction record has a whole and fractional word.</summary>
    private const int RecordByteCount = 4;

    /// <summary>The block's bytes in cartridge address order.</summary>
    internal readonly byte[] Bytes = new byte[ByteCount];

    /// <summary>
    /// The camera's vertical subspeed, the fraction of its vertical speed in 1/65536 pixels, 0 through 65535. It is
    /// written into the block's first word, whose high byte leaks into leftward launch velocity; other values are
    /// rejected.
    /// </summary>
    internal int CameraYSubspeed
    {
        set => WriteWord(CameraYSubspeedOffset, checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value)));
    }

    /// <summary>Stores one movement call's signed 16.16 distance: the integer word, then the fraction word.</summary>
    internal void Record(SamusCollisionDirection direction, int signedDisplacement)
    {
        int offset = RecordOffset(direction);
        WriteWord(offset, FixedCoordinate.WholeOf(signedDisplacement));
        WriteWord(offset + 2, unchecked((ushort)signedDisplacement));
    }

    /// <summary>The native alpha tail zeroes the four distance records, leaving the camera subspeed.</summary>
    internal void ClearMovement() => Array.Clear(Bytes, FirstRecordOffset, ByteCount - FirstRecordOffset);

    /// <summary>Combines a shot's base speed with the direction-specific overlapping words using native signed arithmetic.</summary>
    internal EightEightVector<RoomPixels> ReadVelocity(int directionWord, int baseSpeed)
    {
        var word = new SamusProjectileDirectionWord(checked((ushort)directionWord));
        if (word.DirectionIndex > 9)
            throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${word.DirectionIndex:X2}.");
        SamusProjectileDirection direction = word.Direction
            ?? throw new InvalidDataException($"Projectile velocity initialization received invalid direction ${word.DirectionIndex:X2}.");
        int upward = LaunchVelocityWord(SamusCollisionDirection.Up);
        int upContribution = upward <= byte.MaxValue ? 0 : (upward >> 2) | 3 << 14; // An arithmetic shift by two fills both top bits.
        int x = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.Right or SamusProjectileDirection.DownRight =>
                baseSpeed + LaunchVelocityWord(SamusCollisionDirection.Right),
            SamusProjectileDirection.DownLeft or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft =>
                -baseSpeed + LaunchVelocityWord(SamusCollisionDirection.Left),
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.UpFacingLeft => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        int y = unchecked((short)(direction switch
        {
            SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpRight or
                SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft => -baseSpeed + upContribution,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingRight or
                SamusProjectileDirection.DownFacingLeft or SamusProjectileDirection.DownLeft =>
                baseSpeed + LaunchVelocityWord(SamusCollisionDirection.Down),
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => 0,
            _ => throw new InvalidOperationException($"Undefined projectile direction {direction}."),
        }));
        return new(x, y);
    }

    /// <summary>The overlapping word one byte before <paramref name="direction"/>'s record.</summary>
    private ushort LaunchVelocityWord(SamusCollisionDirection direction) => ReadWord(RecordOffset(direction) - 1);

    /// <summary>Reads the little-endian word whose low byte is at <paramref name="offset"/>.</summary>
    private ushort ReadWord(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(Bytes.AsSpan(offset));

    /// <summary>Writes <paramref name="value"/> little-endian at <paramref name="offset"/>.</summary>
    private void WriteWord(int offset, int value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(Bytes.AsSpan(offset), checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value)));
    }

    /// <summary>Maps left, right, up, or down to its native 16.16 record offset.</summary>
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
