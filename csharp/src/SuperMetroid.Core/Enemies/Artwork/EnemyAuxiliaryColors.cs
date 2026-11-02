using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed auxiliary enemy colour frames: which family a palette names, how a colour resolves, and their identity.</summary>
internal static class EnemyAuxiliaryColors
{
    /// <summary>Selects one of the four installed auxiliary color families by its defined identity.</summary>
    internal static EnemyAuxiliaryColourFrames FamilyFor(EnemyAuxiliaryColorCatalog colors, EnemyAuxiliaryPalette palette) =>
        colors.families.TryGetValue(palette, out EnemyAuxiliaryColourFrames? family)
            ? family
            : throw new ArgumentOutOfRangeException(nameof(palette));

    /// <summary>Resolves one selected RGB5 word, rejecting a frame or colour outside the family.</summary>
    internal static RgbColor Resolve(EnemyAuxiliaryColorCatalog colors, EnemyAuxiliaryPalette palette, int frame, int color)
    {
        EnemyAuxiliaryColourFrames family = FamilyFor(colors, palette);
        if ((uint)frame >= family.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= family.ColorCount) throw new ArgumentOutOfRangeException(nameof(color));
        return family.Color(frame, color);
    }
}
