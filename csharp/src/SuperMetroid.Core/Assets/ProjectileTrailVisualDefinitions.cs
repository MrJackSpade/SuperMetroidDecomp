namespace SuperMetroid.Core.Assets;

/// <summary>Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators.</summary>
public static class ProjectileTrailVisualDefinitions
{
    public const string FileName = "projectile-trails.json";
    public const int Version = 1;
    /// <summary>$90:B4CB/B52D ice poses and the four-pose wave/missile layouts, calculated without a stored address table.</summary>
    public static FrameSequence Frames => default;

    public readonly struct FrameSequence : IReadOnlyList<ushort>
    {
        public int Count => 42;
        public int Length => Count;
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index < 34)
                {
                    int frame = index % 17;
                    int steps = frame < 6 ? 0 : frame < 8 ? 1 : frame - 6;
                    ushort start = index < 17 ? Game.ProjectileTrailDefinitions.LeftIce : Game.ProjectileTrailDefinitions.RightIce;
                    return (ushort)(start + 4 * frame + 2 * steps);
                }
                return (ushort)((index < 38 ? Game.ProjectileTrailDefinitions.Wave : Game.ProjectileTrailDefinitions.Missile) + 4 * ((index - 34) % 4));
            }
        }
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public static string Name(ushort frame) => $"trail_{frame:X4}";
}
