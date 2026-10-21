
namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Development-tool members of <see cref="CrocomireSkeletonVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class CrocomireSkeletonVisualToolingDefinitions
{
    /// <summary><c>ExtendedSpritemap_CrocomireCorpse_E</c>, $A4:E46A: first thirteen-component pose.</summary>
    internal const ushort FirstThirteenComponentFrame = 0xe46a;
    /// <summary>$A4:E5A8, ExtendedSpritemap_CrocomireCorpse_11; twelve components after three thirteen-component records.</summary>
    internal const ushort TwelveComponentCollapse = FirstThirteenComponentFrame + 3 * (2 + 8 * 13);
    /// <summary>$A4:E60A, ExtendedSpritemap_CrocomireCorpse_12; ten components.</summary>
    internal const ushort TenComponentCollapse = TwelveComponentCollapse + 2 + 8 * 12;
    /// <summary>$A4:E65C, ExtendedSpritemap_CrocomireCorpse_13; six components.</summary>
    internal const ushort SixComponentCollapse = TenComponentCollapse + 2 + 8 * 10;
    /// <summary>$A4:E68E, ExtendedSpritemap_CrocomireCorpse_14; three components.</summary>
    internal const ushort ThreeComponentCollapse = SixComponentCollapse + 2 + 8 * 6;
    /// <summary>$A4:E6A8, ExtendedSpritemap_CrocomireCorpse_15; first of twelve single-component roots.</summary>
    internal const ushort SingleComponentStart = ThreeComponentCollapse + 2 + 8 * 3;
    /// <summary>Roots follow their count-dependent record sizes. Thirteen
    /// five-component records lead to the nine-component pose; three thirteen-
    /// component records lead to the shrinking collapse poses, then twelve
    /// single-component records. Every root is selected, in ascending order.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= CrocomireSkeletonVisualDefinitions.FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        return index switch
        {
            <= 13 => (ushort)(CrocomireBodyVisualToolingDefinitions.FirstSkeletonFrame + (2 + 8 * 5) * index),
            <= 17 => (ushort)(FirstThirteenComponentFrame + (2 + 8 * 13) * (index - 14)),
            18 => TenComponentCollapse,
            19 => SixComponentCollapse,
            20 => ThreeComponentCollapse,
            _ => (ushort)(SingleComponentStart + (2 + 8) * (index - 21)),
        };
    }
}
