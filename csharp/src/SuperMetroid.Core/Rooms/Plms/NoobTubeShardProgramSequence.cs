namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Ten native shard entry points: nine36-byte programs and one32-byte unreflected ninth program.</summary>
public readonly struct NoobTubeShardProgramSequence : IReadOnlyList<ushort>
{
    /// <summary>Ten entry pointers for the ten native shard spawn parameters, not a count of currently live projectiles.</summary>
    public int Count => 10;
    /// <summary>Calculates a bank-$86 instruction-list entry from $D47D; ordinal eight uses the compact $D59D program and ordinal nine begins at $D5BD.</summary>
    /// <param name="index">Zero-based shard ordinal 0..9, corresponding to native even parameter 2 times this index ($00..$12).</param>
    /// <returns>A 16-bit bank-relative program pointer, not a full cartridge address or spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside 0..9.</exception>
    public ushort this[int index] => (uint)index < Count
        ? (ushort)(NoobTubeProjectileInstructionProgramDefinitions.FirstShard+36*index-(index>8?4:0))
        : throw new IndexOutOfRangeException();
    /// <summary>Enumerates calculated entry pointers in native spawn-parameter order without retaining an array or mutable projectile state.</summary>
    /// <returns>An independent enumerator over all ten bank-relative pointers.</returns>
    public IEnumerator<ushort> GetEnumerator()
    {
        for(int index=0;index<Count;index++) yield return this[index];
    }
    /// <summary>Enumerates the same ten shard pointers through the non-generic collection interface.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
