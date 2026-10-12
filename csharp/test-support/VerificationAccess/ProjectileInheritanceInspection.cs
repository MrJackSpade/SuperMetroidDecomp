using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Verification-only read view of the projectile-inheritance WRAM block (#629). Production writes
/// the block through <see cref="ProjectileInheritanceMemory"/> and reads only its overlapping launch
/// words; native-state comparisons inspect every aligned word of
/// <see cref="ProjectileInheritanceWorkRam.Records"/> in address order.
/// </summary>
[CpuBusBoundary("domain view: verification read of the projectile-inheritance WRAM block")]
internal readonly struct ProjectileInheritanceInspection(ISnesMemoryBus bus)
{
    /// <summary>Gets or sets records.</summary>
    private WorkRamByteTable Records => new(bus, ProjectileInheritanceWorkRam.Records);

    /// <summary>Bytes in the block: the camera Y subspeed word, then four signed 16.16 distances.</summary>
    internal int ByteCount => Records.Length;

    /// <summary>The little-endian word whose low byte is <paramref name="byteOffset"/> bytes into the block.</summary>
    internal ushort Word(int byteOffset) => Records.ReadWord(byteOffset);
}
