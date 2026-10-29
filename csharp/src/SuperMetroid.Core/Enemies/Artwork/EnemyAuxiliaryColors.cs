using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>The installed auxiliary enemy colour frames: which family a palette names, how a colour resolves, and their identity.</summary>
internal static class EnemyAuxiliaryColors
{
    /// <summary>Hashes each palette's frames, tagged by its place in the definitions' face-block, corpse, body and belly order.</summary>
    internal static string ContentIdentity(EnemyAuxiliaryColorCatalog colors) =>
        SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
        {
            Span<RgbColor> row = stackalloc RgbColor[16];
            for (int tag = 0; tag < EnemyAuxiliaryColorDefinitions.All.Length; tag++)
            {
                EnemyAuxiliaryColourFrames family = FamilyFor(colors, EnemyAuxiliaryColorDefinitions.All[tag].Id);
                content.Append("palette", tag);
                content.Append("frames", family.FrameCount);
                for (int frame = 0; frame < family.FrameCount; frame++)
                {
                    for (int color = 0; color < family.ColorCount; color++) row[color] = family.Color(frame, color);
                    content.AppendColors("row", row[..family.ColorCount]);
                }
            }
        });

    /// <summary>Selects one of the four installed auxiliary color families by its defined identity.</summary>
    private static EnemyAuxiliaryColourFrames FamilyFor(EnemyAuxiliaryColorCatalog colors, EnemyAuxiliaryPalette palette) =>
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
