namespace SuperMetroid.Core.Assets;

/// <summary>Selected Ridley, Draygon and Spore Spawn OAM roots calculated from
/// extended-record geometry: two count bytes plus eight bytes per component.</summary>
internal static class BossOamFrameDefinitions
{
    /// <summary>$A6:E983, ExtendedSpritemap_Ridley_FacingLeft;
    /// ten four-component poses precede the one-component forward pose.</summary>
    private const ushort RidleyStart = 0xe983;
    /// <summary>$A5:A2DF, first selected left-facing one-component Draygon root.</summary>
    private const ushort DraygonLeftStart = 0xa2df;
    /// <summary>$A5:A607, first selected right-facing one-component Draygon root.</summary>
    private const ushort DraygonRightStart = 0xa607;
    /// <summary>$A5:A3C5, four selected left-facing one-component roots after BG2 frames.</summary>
    private const ushort DraygonLeftAfterBg2 = 0xa3c5;
    /// <summary>$A5:A6ED, four selected right-facing one-component roots after BG2 frames.</summary>
    private const ushort DraygonRightAfterBg2 = 0xa6ed;
    /// <summary>$A5:A40B, first of seven selected left-facing two-component roots.</summary>
    private const ushort DraygonLeftComposite = 0xa40b;
    /// <summary>$A5:A779, first of seven selected right-facing two-component roots.</summary>
    private const ushort DraygonRightComposite = 0xa779;
    /// <summary>$A5:EE65, dead Spore Spawn root followed by the one-component closed pose.</summary>
    private const ushort SporeSpawnStart = 0xee65;
    /// <summary>$A5:EE79, first of seven selected two-component opening/closing roots.</summary>
    private const ushort SporeSpawnOpening = 0xee79;
    /// <summary>$A5:EF3D, first of three fully-open roots after seven unused records.</summary>
    private const ushort SporeSpawnOpen = 0xef3d;

    /// <summary>Calculates the bank-$A6 extended-OAM root for a Ridley frame using the native 34-byte spacing between selected roots.</summary>
    /// <param name="index">Zero-based Ridley frame index within <see cref="EnemyExtendedFrameDefinitions.RidleyFrameCount"/>.</param>
    /// <returns>The bank-local pointer to that frame's extended-OAM record.</returns>
    /// <exception cref="IndexOutOfRangeException">The frame index is outside Ridley's compiled frame range.</exception>
    internal static ushort RidleyPointer(int index)
    {
        if ((uint)index >= EnemyExtendedFrameDefinitions.RidleyFrameCount) throw new IndexOutOfRangeException();
        return (ushort)(RidleyStart + 34 * index);
    }

    /// <summary>Each facing selects six and four one-component runs, then seven
    /// two-component roots, six roots growing from three to eight components,
    /// and a final eight-component pose.
    /// Summing the growing record sizes gives 26*t + 4*t*(t-1) from the
    /// three-component root. Gaps and BG2 roots are excluded by the run starts.</summary>
    internal static ushort DraygonPointer(int index)
    {
        if ((uint)index >= EnemyExtendedFrameDefinitions.DraygonOamFrameCount) throw new IndexOutOfRangeException();
        bool right = index >= 24;
        int pose = index % 24;
        if (pose < 6) return (ushort)((right ? DraygonRightStart : DraygonLeftStart) + 10 * pose);
        if (pose < 10) return (ushort)((right ? DraygonRightAfterBg2 : DraygonLeftAfterBg2) + 10 * (pose - 6));
        int compositeStart = right ? DraygonRightComposite : DraygonLeftComposite;
        if (pose < 17) return (ushort)(compositeStart + 18 * (pose - 10));
        int growth = pose - 17;
        return (ushort)(compositeStart + 7 * 18 + 26 * growth + 4 * growth * (growth - 1));
    }

    /// <summary>Two one-component roots, seven two-component opening/closing
    /// roots, then three fully-open roots. The unused middle run is not selected.</summary>
    internal static ushort SporeSpawnPointer(int index)
    {
        if ((uint)index >= EnemyExtendedFrameDefinitions.SporeSpawnOamFrameCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 2 ? SporeSpawnStart + 10 * index
            : index < 9 ? SporeSpawnOpening + 18 * (index - 2)
            : SporeSpawnOpen + 18 * (index - 9));
    }
}
