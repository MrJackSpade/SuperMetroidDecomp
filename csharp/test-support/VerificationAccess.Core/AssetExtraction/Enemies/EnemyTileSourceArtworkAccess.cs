using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="EnemyTileSourceArtwork"/> members production does not use.</summary>
internal static class EnemyTileSourceArtworkAccess
{
    /// <summary>Constructs a partial enemy-art catalog for focused verification.</summary>
    extension(EnemyTileSourceArtwork)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileSourceArtwork FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            new EnemyTileSourceArtwork(Ensure.NotNull(parts));
    }

    /// <summary>Exposes <see cref="EnemyTileSourceArtwork"/> state that only verification reads.</summary>
    extension(EnemyTileSourceArtwork self)
    {
        /// <summary>
        /// Complete installed enemy presentation, including display bindings, special uploads
        /// and colors. Gameplay definitions are excluded.
        /// This is computed from the installed content rather than kept with the game state.
        /// </summary>
        public string ContentIdentity =>
            SelectedPresentationHash.Create("enemy-bundle-v1", content =>
            {
                foreach ((EnemyDefinitionId definition, RoomCharacterAtlas sheet, EnemyPaletteSheet palette) in self.SheetsInDefinitionOrder)
                {
                    content.Append("definition", System.Text.Encoding.UTF8.GetBytes(definition.ToString()));
                    content.Append("tiles", sheet.Characters.Span);
                    content.AppendIdentity("palette", palette.ContentIdentity);
                }
                content.AppendIdentity("Spritemaps", self.Spritemaps?.ContentIdentity);
                content.AppendIdentity("ExtendedFrames", self.ExtendedFrames?.ContentIdentity);
                content.AppendIdentity("GunshipLiftoff", self.GunshipLiftoff?.ContentIdentity);
                content.AppendIdentity("ProjectileSpritemaps", self.ProjectileSpritemaps?.ContentIdentity);
                content.AppendIdentity("MagdollitePaletteCycle", self.MagdollitePaletteCycle?.ContentIdentity);
                content.AppendIdentity("WorkRobotPaletteCycle", self.WorkRobotPaletteCycle?.ContentIdentity);
                content.AppendIdentity("ChozoAndTubeColors", self.ChozoAndTubeColors?.ContentIdentity);
                content.AppendIdentity("DachoraColors", self.DachoraColors?.ContentIdentity);
                content.AppendIdentity("AuxiliaryColors", self.AuxiliaryColors?.ContentIdentity);
            });
        /// <summary>Each installed sheet with its palette, in definition order.</summary>
        internal IEnumerable<EnemyInstalledSheet> SheetsInDefinitionOrder =>
            self.sheets.OrderBy(pair => pair.Key).Select(pair => new EnemyInstalledSheet(pair.Key, pair.Value,
                self.TryGetPalette(pair.Key, out EnemyPaletteSheet? palette) ? palette
                    : throw new InvalidDataException($"Installed enemy {pair.Key} has no palette.")));
    }
}
