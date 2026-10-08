namespace SuperMetroid.Core.Assets;

/// <summary>Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators.</summary>
public static class ProjectileTrailVisualDefinitions
{
    /// <summary>Asset filename for editable OBJ appearances of the authored projectile trail records.</summary>
    public const string FileName = "projectile-trails.json";
    /// <summary>Supported JSON schema revision requiring all 42 native timed-record appearance keys.</summary>
    public const int Version = 1;
    /// <summary>$90:B4CB/B52D ice poses and the four-pose wave/missile layouts, calculated without a stored address table.</summary>
    public static FrameSequence Frames => default;

    /// <summary>Calculated sequence of timed-record duration-word addresses in bank $90: seventeen left-ice, seventeen right-ice, four wave, and four missile records, omitting movement commands and terminators.</summary>
    public readonly struct FrameSequence : IReadOnlyList<ushort>
    {
        /// <summary>Forty-two appearance-bearing timed records across the four native trail lists.</summary>
        public int Count => 42;
        /// <summary>Sequence length for array-style consumers, equal to <see cref="Count"/>.</summary>
        public int Length => Count;
        /// <summary>Gets the native duration-word address of a timed record, accounting for interleaved ice-trail downward-movement commands.</summary>
        /// <param name="index">Zero-based record index: 0..16 left ice, 17..33 right ice, 34..37 wave, or 38..41 missile.</param>
        /// <returns>The record's 16-bit pointer in bank $90, used as its appearance identity rather than its subsequent OBJ attribute-word address.</returns>
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
        /// <summary>Enumerates the four lists' timed-record identities in left-ice, right-ice, wave, then missile order without executing trail instructions.</summary>
        /// <returns>An enumerator over all 42 bank-$90 record pointers.</returns>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Formats a native trail-record pointer as the canonical appearance-document key; formatting alone does not validate that the pointer belongs to <see cref="Frames"/>.</summary>
    /// <param name="frame">Bank-$90 duration-word pointer identifying a timed trail record.</param>
    /// <returns><c>trail_</c> followed by four uppercase hexadecimal digits.</returns>
    public static string Name(ushort frame) => $"trail_{frame:X4}";
}
