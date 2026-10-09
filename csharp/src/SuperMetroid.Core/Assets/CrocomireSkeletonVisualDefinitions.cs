namespace SuperMetroid.Core.Assets;

/// <summary>OAM composition roots for Crocomire's falling, collapsing, stable and river skeleton.</summary>
internal static class CrocomireSkeletonVisualDefinitions
{
    /// <summary>Crocomire's native visual bank $A4.</summary>
    internal const byte Bank = CrocomireBodyVisualDefinitions.Bank;
    /// <summary>Thirty-three corpse roots selected by the compiled bank-$A4 programs.</summary>
    internal const int FrameCount = 33;
    /// <summary>The collapse poses at $A4:E46A..E53E contain thirteen OAM components, not eight.</summary>
    internal const int MaximumComponents = 13;
    /// <summary><c>ExtendedSpritemap_CrocomireCorpse_E</c>, $A4:E46A: first thirteen-component pose.</summary>
    internal const ushort FirstThirteenComponentFrame = 0xe46a;

    /// <summary>$A4:E5A8, ExtendedSpritemap_CrocomireCorpse_11; twelve components after three thirteen-component records.</summary>
    private const ushort TwelveComponentCollapse = FirstThirteenComponentFrame + 3 * (2 + 8 * 13);
    /// <summary>$A4:E60A, ExtendedSpritemap_CrocomireCorpse_12; ten components.</summary>
    private const ushort TenComponentCollapse = TwelveComponentCollapse + 2 + 8 * 12;
    /// <summary>$A4:E65C, ExtendedSpritemap_CrocomireCorpse_13; six components.</summary>
    private const ushort SixComponentCollapse = TenComponentCollapse + 2 + 8 * 10;
    /// <summary>$A4:E68E, ExtendedSpritemap_CrocomireCorpse_14; three components.</summary>
    private const ushort ThreeComponentCollapse = SixComponentCollapse + 2 + 8 * 6;
    /// <summary>$A4:E6A8, ExtendedSpritemap_CrocomireCorpse_15; first of twelve single-component roots.</summary>
    private const ushort SingleComponentStart = ThreeComponentCollapse + 2 + 8 * 3;


    /// <summary>Tests whether a bank and pointer identify one of the compiled Crocomire skeleton OAM roots.</summary>
    /// <param name="bank">Bank byte that must match the skeleton's native visual bank.</param>
    /// <param name="pointer">Candidate extended-spritemap root pointer.</param>
    /// <returns><see langword="true"/> when the pointer is among the selected skeleton frames in the matching bank.</returns>
    internal static bool IsFrame(byte bank, ushort pointer)
    {
        if (bank != Bank) return false;
        for (int index = 0; index < FrameCount; index++)
            if (FramePointer(index) == pointer) return true;
        return false;
    }

    /// <summary>Roots follow their count-dependent record sizes. Thirteen
    /// five-component records lead to the nine-component pose; three thirteen-
    /// component records lead to the shrinking collapse poses, then twelve
    /// single-component records. Every root is selected, in ascending order.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        return index switch
        {
            <= 13 => (ushort)(CrocomireBodyVisualDefinitions.FirstSkeletonFrame + (2 + 8 * 5) * index),
            <= 17 => (ushort)(FirstThirteenComponentFrame + (2 + 8 * 13) * (index - 14)),
            18 => TenComponentCollapse,
            19 => SixComponentCollapse,
            20 => ThreeComponentCollapse,
            _ => (ushort)(SingleComponentStart + (2 + 8) * (index - 21)),
        };
    }

    /// <summary>Builds the extended-frame definition for a skeleton root in native sequence order.</summary>
    /// <param name="index">Zero-based index of one of the 33 selected corpse and collapse poses.</param>
    /// <returns>The bank, native OAM root pointer, and stable asset name for that pose.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled skeleton frame sequence.</exception>
    internal static EnemyExtendedFrameDefinition Frame(int index)
    {
        ushort pointer = FramePointer(index);
        return new(Bank, pointer, $"crocomire_skeleton_oam_{pointer:X4}");
    }
}
