namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Allocation-free view of one frame's components, mirrored for right-facing frames.</summary>
internal readonly struct TorizoCollisionComponents(GoldenTorizoCollisionComponent[] source, bool mirrored)
{
    /// <summary>Number of ordered native components in this frame's collision drawing.</summary>
    internal int Length => source.Length;

    /// <summary>Projects one component into the requested facing, reflecting X and substituting each hitbox list's right-facing counterpart when needed.</summary>
    internal GoldenTorizoCollisionComponent this[int index]
    {
        get
        {
            GoldenTorizoCollisionComponent part = source[index];
            if (!mirrored) return part;
            return part with { Offset = part.Offset.MirroredX(), Hitboxes = part.Hitboxes.RightFacing };
        }
    }

    /// <summary>Enumerates components without materializing reflected frame records.</summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Index-based enumerator over one frame's reflected or original components.</summary>
    internal struct Enumerator(TorizoCollisionComponents view)
    {
        /// <summary>Current position before the next component, initially before index zero.</summary>
        private int index = -1;
        /// <summary>Advances to the next component while retaining native component order.</summary>
        public bool MoveNext() => ++index < view.Length;
        /// <summary>Resolves the current component through the view's facing projection.</summary>
        public readonly GoldenTorizoCollisionComponent Current => view[index];
    }
}
