using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.WorkRobots;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the enemies' and statues' colour catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class ActorColourContentIdentity
{
    /// <summary>The installed content identity of a <see cref="TourianStatuePalettes"/>.</summary>
    extension(TourianStatuePalettes palettes)
    {
        /// <summary>Canonical selected RGB5 colors and ordered eye glows, independent of JSON encoding.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("TourianStatueColorCatalog-v1", content =>
            {
                Span<RgbColor> colours = stackalloc RgbColor[TourianStatueColorFormat.BaseColorCount];
                for (int color = 0; color < colours.Length; color++) colours[color] = palettes.baseColors.Read(color);
                content.AppendColors("baseColors", colours);
                for (int color = 0; color < TourianStatueColorFormat.StatueColorCount; color++) colours[color] = palettes.statueColors.Read(color);
                content.AppendColors("statueColors", colours[..TourianStatueColorFormat.StatueColorCount]);
                for (int color = 0; color < TourianStatueColorDefinitions.GreyColorCount; color++) colours[color] = palettes.greyColors.Read(color);
                content.AppendColors("greyColors", colours[..TourianStatueColorDefinitions.GreyColorCount]);
                content.AppendColorFrames("eyeColors", Enumerable.Range(0, TourianStatueColorDefinitions.EyeGlowCount).Select(glow =>
                    Enumerable.Range(0, TourianStatueColorDefinitions.EyeColorCount).Select(color => palettes.ResolveEye(glow, color)).ToArray()).ToArray());
            });
    }

    /// <summary>The installed content identity of a <see cref="EnemyPaletteSheet"/>.</summary>
    extension(EnemyPaletteSheet sheet)
    {
        /// <summary>Canonical selected RGB5 colors in OBJ color-index order, independent of JSON encoding.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("EnemyPaletteSheet-v1", content =>
            {
                content.AppendColors("colors", sheet.colors);
            });
    }

    /// <summary>The installed content identity of a <see cref="MagdollitePaletteCycle"/>.</summary>
    extension(MagdollitePaletteCycle cycle)
    {
        /// <summary>Canonical selected RGB5 colors and ordered cycle frames, independent of JSON encoding.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("MagdollitePaletteCycle-v1", content =>
            {
                content.Append("frames", MagdollitePaletteDefinitions.FrameCount);
                Span<RgbColor> frameColours = stackalloc RgbColor[MagdollitePaletteDefinitions.AnimatedColorCount];
                for (int frame = 0; frame < MagdollitePaletteDefinitions.FrameCount; frame++)
                {
                    for (int color = 0; color < frameColours.Length; color++) frameColours[color] = cycle.Resolve(frame, color);
                    content.AppendColors("row", frameColours);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="WorkRobotPaletteCycle"/>.</summary>
    extension(WorkRobotPaletteCycle cycle)
    {
        /// <summary>Canonical selected RGB5 colors and ordered cycle frames, independent of JSON encoding.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1", content =>
            {
                content.Append("frames", WorkRobotPaletteTimingDefinitions.RecordCount);
                Span<RgbColor> frameColours = stackalloc RgbColor[WorkRobotPaletteDefinitions.ColorCount];
                for (int frame = 0; frame < WorkRobotPaletteTimingDefinitions.RecordCount; frame++)
                {
                    for (int color = 0; color < frameColours.Length; color++)
                        frameColours[color] = cycle.Resolve(frame, color);
                    content.AppendColors("row", frameColours);
                }
            });
    }

    /// <summary>The installed content identity of a <see cref="ChozoAndTubeColorCatalog"/>.</summary>
    extension(ChozoAndTubeColorCatalog colors)
    {
        /// <summary>Hashes the tube cracks, then the Wrecked Ship and Lower Norfair statues.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
            {
                Span<RgbColor> tube = stackalloc RgbColor[ChozoAndTubeColorDefinitions.ColorCount];
                for (int color = 0; color < tube.Length; color++) tube[color] = colors.ResolveTubeCracks(color);
                content.AppendColors("tubeCracks", tube);
                content.AppendColors("wreckedShip", StatueColours(colors, ChozoStatuePalette.WreckedShip));
                content.AppendColors("lowerNorfair", StatueColours(colors, ChozoStatuePalette.LowerNorfair));
            });
    }

    /// <summary>The installed content identity of a <see cref="DachoraColorCatalog"/>.</summary>
    extension(DachoraColorCatalog colors)
    {
        /// <summary>Hashes the normal palette, then the speed and shine frames.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("DachoraColorCatalog-v1", content =>
            {
                content.AppendColors("normal", colors.selection.Normal);
                content.AppendColorFrames("speed", DachoraFrames(colors, DachoraPalettePhase.Speed));
                content.AppendColorFrames("shine", DachoraFrames(colors, DachoraPalettePhase.Shine));
            });
    }

    /// <summary>The installed content identity of a <see cref="EnemyAuxiliaryColorCatalog"/>.</summary>
    extension(EnemyAuxiliaryColorCatalog colors)
    {
        /// <summary>Hashes each palette's frames, tagged by its place in the definitions' face-block, corpse, body and belly order.</summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
            {
                Span<RgbColor> frameColours = stackalloc RgbColor[16];
                for (int tag = 0; tag < EnemyAuxiliaryColorDefinitions.All.Length; tag++)
                {
                    EnemyAuxiliaryColourFrames family = EnemyAuxiliaryColors.FamilyFor(colors, EnemyAuxiliaryColorDefinitions.All[tag].Id);
                    content.Append("palette", tag);
                    content.Append("frames", family.FrameCount);
                    for (int frame = 0; frame < family.FrameCount; frame++)
                    {
                        for (int color = 0; color < family.ColorCount; color++) frameColours[color] = family.Color(frame, color);
                        content.AppendColors("row", frameColours[..family.ColorCount]);
                    }
                }
            });
    }

    /// <summary>Expands a statue variant into its ordered 32-color palette image for identity hashing.</summary>
    private static RgbColor[] StatueColours(ChozoAndTubeColorCatalog colors, ChozoStatuePalette palette) =>
        Enumerable.Range(0, ChozoAndTubeColorDefinitions.ColorCount).Select(color => ChozoAndTubeColors.Statue(colors, palette, color)).ToArray();

    /// <summary>Materializes four selected animated frames for stable content hashing.</summary>
    private static RgbColor[][] DachoraFrames(DachoraColorCatalog colors, DachoraPalettePhase phase) =>
        Enumerable.Range(0, DachoraColorDefinitions.AnimatedFrameCount)
            .Select(frame => Enumerable.Range(0, DachoraColorDefinitions.ColorsPerFrame)
                .Select(color => DachoraColors.Resolve(colors, phase, frame, color)).ToArray()).ToArray();
}
