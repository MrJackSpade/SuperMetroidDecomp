namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Each dust pair adds one tick per two poses, reaching its terminal hold after four poses.</summary>
internal readonly record struct GunshipDustDurations(int Length, ushort Initial)
{
    /// <summary>Advances the cloud's hold by one tick after each pair of poses, capped at the terminal hold.</summary>
    internal ushort this[int frame] => (uint)frame < Length
        ? (ushort)(Initial + Math.Min(frame / 2, 2)) : throw new IndexOutOfRangeException();
}
