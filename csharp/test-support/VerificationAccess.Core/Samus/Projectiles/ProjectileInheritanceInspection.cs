using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Verification-only read view of Samus's projectile-inheritance block. Production reads only its overlapping
/// launch words; native-state comparisons inspect every aligned word of
/// <see cref="ProjectileInheritanceRecords"/> in address order.
/// </summary>
internal readonly struct ProjectileInheritanceInspection(ProjectileInheritanceRecords inheritance)
{
    /// <summary>The block's bytes in native address order.</summary>
    private byte[] Records => inheritance.Bytes;

    /// <summary>Bytes in the block: the camera Y subspeed word, then four signed 16.16 distances.</summary>
    internal int ByteCount => Records.Length;

    /// <summary>The little-endian word whose low byte is <paramref name="byteOffset"/> bytes into the block.</summary>
    internal ushort Word(int byteOffset) => SnesLittleEndian.Word(Records[byteOffset], Records[byteOffset + 1]);
}
