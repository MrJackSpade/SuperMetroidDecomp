using System.Collections.Immutable;
using System.Text;
using System.Globalization;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the sprite, enemy, boss-body and room sheets, the X-ray, Samus death and cinematic catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class InstalledArtworkContentIdentity
{
    /// <summary>The installed content identity of a <see cref="InstalledEnemySheets"/>.</summary>
    extension(InstalledEnemySheets installed)
    {
        /// <summary>SHA-256 of every variant, its sheets, and each population's choice of them (#1599).</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledEnemySheets), content =>
        {
            foreach ((string name, EnemyArtworkVariant<SpriteSheet> variant) in installed.variants.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("variant", name);
                variant.Sheet.AppendIdentity(content);
                AppendChanges(content, variant);
            }
            // Each spawned-object sheet once, own and common alike, then the sheet each variant draws each picture from (#1513).
            foreach (SpriteSheet sheet in installed.spawnedVariants.Values.SelectMany(variant => variant.Sheet.Sheets).Distinct<SpriteSheet>(ReferenceEqualityComparer.Instance)
                         .OrderBy(sheet => sheet.Image.Name, StringComparer.Ordinal))
                sheet.AppendIdentity(content);
            foreach ((string name, EnemyArtworkVariant<SpawnedObjectSheet> variant) in installed.spawnedVariants.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("spawned-variant", name);
                foreach ((string picture, SpriteSheetFrame frame) in variant.Sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    content.AppendName("picture", picture);
                    content.AppendName("held on", frame.Sheet.Name);
                }
                foreach (string picture in variant.Sheet.empty.Order(StringComparer.Ordinal))
                    content.AppendName("shows nothing", picture);
                AppendChanges(content, variant);
            }
            AppendArtwork(content, "standard", installed.Standard);
            foreach ((string population, PopulationArtwork artwork) in installed.populations.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                AppendArtwork(content, population, artwork);
        });
    }

    /// <summary>The installed content identity of a <see cref="BossBodySheets"/>.</summary>
    extension(BossBodySheets bodies)
    {
        /// <summary>The identity of every sheet's pixels and frames, in boss order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(BossBodySheets), content =>
        {
            foreach (BossBodyOwner owner in Enum.GetValues<BossBodyOwner>())
            {
                content.AppendName("boss", owner.ToString());
                bodies.Of(owner).Sheet.AppendIdentity(content);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledSpriteSheets"/>.</summary>
    extension(InstalledSpriteSheets installed)
    {
        /// <summary>SHA-256 of every sheet's pixels and frames, in identity order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSpriteSheets), content =>
        {
            foreach (SpriteSheetId sheet in SpriteSheetDefinitions.Sheets)
                installed.Get(sheet).AppendIdentity(content);
        });
    }

    /// <summary>A sprite sheet's pixels, maps, shades and frames in a content identity.</summary>
    extension(SpriteSheet sheet)
    {
        /// <summary>
        /// Appends the sheet to a content identity as it is installed (#1633): its pixels, its recolour-region and blend maps,
        /// its backdrop's colour key, each shade's substitutions and blending in shade order, and its frames in name order.
        /// </summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            SpriteImage image = sheet.Image;
            content.AppendName("sheet", image.Name);
            content.Append("width", image.Size.X);
            content.Append("height", image.Size.Y);
            content.Append("pixels", System.Runtime.InteropServices.MemoryMarshal.AsBytes(image.Pixels));
            content.Append("recolour regions", System.Runtime.InteropServices.MemoryMarshal.AsBytes(image.RecolourRegions));
            content.Append("blend map", System.Runtime.InteropServices.MemoryMarshal.AsBytes(image.Blending));
            if (image.Backdrop is { } backdrop)
                AppendKey(content, "backdrop", backdrop);
            foreach ((PictureShade shade, PictureRecolouring shown) in image.Shades.OrderBy(pair => pair.Key.Key, StringComparer.Ordinal))
            {
                content.AppendName("shade", shade.Key);
                content.Append("shade blends", shown.Blends ? 1 : 0);
                foreach (KeyedSubstitution substitution in shown.Substitutions)
                {
                    AppendKey(content, "recolours", substitution.Replaced);
                    content.AppendName("shows", InstalledColourText.Format(substitution.Shown));
                }
            }
            foreach (string name in sheet.FrameNames.Order(StringComparer.Ordinal))
            {
                SpriteSheetFrame frame = sheet.Frame(name);
                content.AppendName("frame", name);
                content.Append("corner x", frame.Corner.X);
                content.Append("corner y", frame.Corner.Y);
                content.Append("width", frame.Size.X);
                content.Append("height", frame.Size.Y);
                content.Append("origin x", frame.Origin.X);
                content.Append("origin y", frame.Origin.Y);
                content.AppendName("depth", frame.Depth?.ToString() ?? "none");
                content.Append("blends", frame.Blends ? 1 : 0);
            }
        }
    }

    /// <summary>Appends colour key <paramref name="key"/> to a content identity as the manifest writes it.</summary>
    private static void AppendKey(SelectedPresentationHash content, string label, ColourKey key)
    {
        content.AppendName(label, InstalledColourText.Format(key.Colour));
        content.Append(label + " region", key.Region);
    }

    /// <summary>The installed content identity of a <see cref="InstalledRoomArtwork"/>.</summary>
    extension(InstalledRoomArtwork artwork)
    {
        /// <summary>A content identity of every sheet, in tileset order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomArtwork), content =>
        {
            foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in artwork.Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
            {
                content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
                sheet.Sheet.AppendIdentity(content);
                sheet.Inputs.AppendIdentity(content);
            }
            foreach ((string name, RoomBackgroundImage background) in artwork.Backgrounds.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("background", name);
                background.Sheet.AppendIdentity(content);
                foreach (RoomBackgroundCell cell in background.Cells)
                {
                    content.Append("cell", cell.Cell);
                    content.AppendName("picture", cell.Picture);
                    AppendInputs(content, cell.Inputs);
                }
            }
            foreach (RoomFloorSheet floor in artwork.Floors.Values.OrderBy(floor => floor.Name, StringComparer.Ordinal))
            {
                content.AppendName("floor", floor.Name);
                floor.Sheet.AppendIdentity(content);
            }
            foreach (MessageBoxSheet messages in artwork.MessageBoxes.Values.OrderBy(messages => messages.Name, StringComparer.Ordinal))
            {
                content.AppendName("messages", messages.Name);
                messages.Sheet.AppendIdentity(content);
            }
            foreach (WrittenPictureSheet pictures in artwork.Written.Values.Concat(artwork.FxOverlayPictures.Values).OrderBy(pictures => pictures.Name, StringComparer.Ordinal))
            {
                content.AppendName("written", pictures.Name);
                pictures.Sheet.AppendIdentity(content);
                foreach ((string picture, ImmutableArray<AppearanceInput> inputs) in pictures.inputs.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                {
                    content.AppendName("picture", picture);
                    AppendInputs(content, inputs);
                }
            }
        });
    }

    /// <summary>A tileset's block appearance inputs in a content identity.</summary>
    extension(RoomBlockAppearances appearances)
    {
        /// <summary>Appends every block's inputs over each artwork the tileset's rooms show their blocks over to a content identity.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("blocks", appearances.Count);
            AppendArtwork("tileset", appearances.tileset);
            AppendArtwork("sprites", appearances.sprites);

            // Whether the tileset's rooms show their blocks over one artwork, and if so every block's inputs over it.
            void AppendArtwork(string artwork, ImmutableArray<AppearanceInput>[]? blocks)
            {
                content.Append(artwork, blocks is null ? 0 : 1);
                if (blocks is null)
                    return;
                foreach (ImmutableArray<AppearanceInput> inputs in blocks)
                {
                    content.Append("inputs", inputs.Length);
                    foreach (AppearanceInput input in inputs)
                        content.AppendName("input", input.Token);
                }
            }
        }
    }

    /// <summary>The installed content identity of a <see cref="RoomLevelArtworks"/>.</summary>
    extension(RoomLevelArtworks artworks)
    {
        /// <summary>SHA-256 of selected room geometry and both ordered planes of appearances.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(RoomLevelArtworks), content =>
        {
            foreach ((RoomLevelId level, RoomLevelArtwork layout) in artworks.layouts.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
            {
                content.AppendName("level", level.Name);
                content.Append("width", layout.Size.X);
                content.Append("height", layout.Size.Y);
                content.AppendAppearances("foreground", layout.Foreground.Span);
                content.AppendAppearances("background", layout.Background.Span);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledXrayOverlays"/>.</summary>
    extension(InstalledXrayOverlays overlays)
    {
        /// <summary>Identity of selected tank appearances and room overlay coordinates and blocks, in record order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledXrayOverlays), content =>
        {
            content.AppendAppearances("tank metatiles", overlays.tankBlocks);
            foreach ((XrayRoomOverlayDefinition overlay, XrayRoomOverlayVisual[] tiles) in overlays.rooms.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
            {
                content.Append("room overlay", Encoding.UTF8.GetBytes(overlay.Name));
                content.Append("tiles", tiles.Length);
                foreach (XrayRoomOverlayVisual tile in tiles)
                {
                    // Identity hashes the authored byte coordinates.
                    content.Append("x", Ensure.BetweenInclusive(0, byte.MaxValue, tile.Block.X));
                    content.Append("y", Ensure.BetweenInclusive(0, byte.MaxValue, tile.Block.Y));
                    content.AppendAppearances("block", [tile.Appearance]);
                }
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledXrayReveals"/>.</summary>
    extension(InstalledXrayReveals reveals)
    {
        /// <summary>Identity of every selected drawable rule and its installed overlays, excluding command mechanics.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledXrayReveals), content =>
        {
            // Each visual is identified by its collision kind's and its reaction's names, in that order (#1722).
            foreach (KeyValuePair<XrayRevealVisualKey, XrayRevealVisualBlocks> pair in reveals.blocks
                .OrderBy(entry => entry.Key.Type).ThenBy(entry => entry.Key.Reaction?.ToString() ?? string.Empty, StringComparer.Ordinal))
            {
                XrayRevealVisualBlocks visual = pair.Value;
                content.AppendName("kind", pair.Key.Type.ToString());
                content.AppendName("reaction", pair.Key.Reaction?.ToString() ?? string.Empty);
                content.Append("top left", visual.TopLeft);
                content.Append("top right", visual.TopRight);
                content.Append("bottom left", visual.BottomLeft);
                content.Append("bottom right", visual.BottomRight);
            }
            content.Append("extension exit", reveals.ExtensionExitBlock);
            content.Append("overlays", Convert.FromHexString(reveals.Overlays.ContentIdentity));
        });
    }

    /// <summary>The installed content identity of a <see cref="SuperMetroid.Core.Samus.Death.SamusDeathSteps"/>.</summary>
    extension(SuperMetroid.Core.Samus.Death.SamusDeathSteps steps)
    {
        /// <summary>SHA-256 of the explosion's death steps and the whiteout colours.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(SuperMetroid.Core.Samus.Death.SamusDeathSteps), content =>
        {
            Span<int> explosion = stackalloc int[SamusDeathExplosionTimingDefinitions.RecordCount];
            for (int frame = 0; frame < explosion.Length; frame++) explosion[frame] = steps.ExplosionStep(frame);
            content.AppendWords("explosion steps", explosion);
            Span<RgbColor> whiteout = stackalloc RgbColor[SamusPaletteDefinitions.Death.WhiteoutShadeCount];
            for (int step = 0; step < whiteout.Length; step++) whiteout[step] = steps.WhiteoutColour(step);
            content.AppendColors("whiteout", whiteout);
        });
    }

    /// <summary>The installed content identity of a <see cref="InstalledSamusDeathPalettes"/>.</summary>
    extension(InstalledSamusDeathPalettes palettes)
    {
        /// <summary>SHA-256 of selected suited/suitless colors, whiteout shades and explosion selectors.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSamusDeathPalettes), content =>
        {
            Span<RgbColor> stepColours = stackalloc RgbColor[InstalledSamusDeathPalettes.ColorCount];
            for (int suit = 0; suit < InstalledSamusDeathPalettes.SuitCount; suit++)
            for (int palette = 0; palette < SamusPaletteDefinitions.Death.PaletteCount; palette++)
            {
                for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++) stepColours[color] = palettes.SuitedColor(suit, palette, color);
                content.AppendColors("suited row", stepColours);
            }
            for (int palette = 0; palette < SamusPaletteDefinitions.Death.PaletteCount; palette++)
            {
                for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++) stepColours[color] = palettes.SuitlessColor(palette, color);
                content.AppendColors("suitless row", stepColours);
            }
            Span<RgbColor> whiteoutColors = stackalloc RgbColor[SamusPaletteDefinitions.Death.WhiteoutShadeCount];
            for (int index = 0; index < whiteoutColors.Length; index++) whiteoutColors[index] = palettes.WhiteoutColor(index);
            content.AppendColors("whiteout", whiteoutColors);
            Span<int> selectors = stackalloc int[SamusDeathExplosionTimingDefinitions.RecordCount];
            for (int frame = 0; frame < selectors.Length; frame++) selectors[frame] = palettes.ExplosionPaletteIndex(frame);
            content.AppendWords("explosion palette indices", selectors);
        });
    }

    /// <summary>The installed content identity of a <see cref="CeresDestructionActorPlacements"/>.</summary>
    extension(CeresDestructionActorPlacements placements)
    {
        /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("CeresDestructionActorLayout", content =>
        {
            content.Append("actors", CeresDestructionActorDefinitions.InitialActorCount);
            for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
            {
                CeresDestructionActorPlacement placement = placements[index];
                content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
                content.Append("x", placement.X);
                content.Append("y", placement.Y);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="CeresFlightActorPlacements"/>.</summary>
    extension(CeresFlightActorPlacements placements)
    {
        /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("CeresFlightActorLayout", content =>
        {
            content.Append("actors", CeresFlightActorDefinitions.RearViewActorCount);
            for (int index = 0; index < CeresFlightActorDefinitions.RearViewActorCount; index++)
            {
                CeresFlightActorPlacement placement = placements[index];
                content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
                content.Append("x", placement.X);
                content.Append("y", placement.Y);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="CeresRevealActorPlacements"/>.</summary>
    extension(CeresRevealActorPlacements placements)
    {
        /// <summary>Identity of ordered selected actor IDs and both initial visual coordinates.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("CeresRevealActorLayout", content =>
        {
            content.Append("actors", CeresDestructionActorDefinitions.ZebesActorCount);
            for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
            {
                CeresRevealActorPlacement placement = placements[index];
                content.Append("id", System.Text.Encoding.UTF8.GetBytes(placement.Id));
                content.Append("x", placement.X);
                content.Append("y", placement.Y);
            }
        });
    }

    /// <summary>The installed content identity of a <see cref="CeresFlightArtwork"/>.</summary>
    extension(CeresFlightArtwork flight)
    {
        /// <summary>
        /// Identity of the selected approach backdrop and placements; the characters (#1636), floor maps (#1693) and
        /// compositions (#1638) are import data, which the import's read adds (<see cref="Areas.Ceres.CeresFlightCharacters"/>).
        /// </summary>
        public string ContentIdentity => SelectedPresentationHash.Create("CeresFlightArtworkCatalog-v4", content =>
        {
            content.AppendColors("backdrop", [flight.Backdrop]);
            content.Append("actors", Convert.FromHexString(flight.Actors.ContentIdentity));
        });
    }

    /// <summary>The installed content identity of a <see cref="CeresDestructionArtwork"/>.</summary>
    extension(CeresDestructionArtwork destruction)
    {
        /// <summary>Identity of the selected destruction and Zebes-reveal actor placements.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("CeresDestructionArtworkCatalog-v4", content =>
        {
            content.Append("reveal-actors", Convert.FromHexString(destruction.RevealActors.ContentIdentity));
            content.Append("destruction-actors", Convert.FromHexString(destruction.DestructionActors.ContentIdentity));
        });
    }

    /// <summary>The installed content identity of an <see cref="IntroCinematicArtwork"/>, the opening the game binds.</summary>
    extension(IntroCinematicArtwork intro)
    {
        /// <summary>
        /// Identity of the opening's installed documents, from what the game bound of them (#1693): its backdrop and the Ceres
        /// scenes' backdrop and placements. Its characters, layouts, Mode 7 maps and compositions are import data the
        /// installation never holds (#1636, #1638, #1693), which the sheets composed from them carry the identity of.
        /// </summary>
        public string ContentIdentity => SelectedPresentationHash.Create("IntroCinematicArtworkCatalog-v5", content =>
        {
            content.AppendColors("backdrop", [intro.Backdrop]);
            content.Append("ceres-flight", Convert.FromHexString(intro.CeresFlight.ContentIdentity));
            content.Append("ceres-destruction", Convert.FromHexString(intro.CeresDestruction.ContentIdentity));
        });
    }

    /// <summary>The installed content identity of a <see cref="EndingBackdrops"/>.</summary>
    extension(EndingBackdrops backdrops)
    {
        /// <summary>Identity of each scene's backdrop, in scene order (#1544), then of the space's backdrops, in frame order (#1637).</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingBackdrops), content =>
        {
            content.AppendColors("backdrops", [backdrops.Escape, backdrops.Explosion, backdrops.Credits, backdrops.PostCredits]);
            content.AppendColors("space", SuperMetroid.AssetExtraction.Frontend.Ending.EndingSpaceBackdropColours.InFrameOrder(backdrops.Space));
        });
    }

    /// <summary>
    /// Appends the frames of <paramref name="variant"/>'s sheet encounters' looks change and the encounters that change each
    /// (#1538), and its pictures that show a rotting corpse and where the corpse lies in each (#1539), in a stable order.
    /// </summary>
    private static void AppendChanges<TSheet>(SelectedPresentationHash content, EnemyArtworkVariant<TSheet> variant)
        where TSheet : class
    {
        foreach ((string frame, EncounterArtwork[] encounters) in variant.changingEncounters.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.Append("frame", System.Text.Encoding.UTF8.GetBytes(frame));
            foreach (EncounterArtwork encounter in encounters)
                content.Append("encounter", System.Text.Encoding.UTF8.GetBytes(encounter.ToString()));
        }
        foreach ((string picture, CorpsePicturePlacement placement) in variant.corpsePictures.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.Append("corpse-frame", System.Text.Encoding.UTF8.GetBytes(
                $"{picture}@{placement.Top.ToString(CultureInfo.InvariantCulture)}"));
        }
    }

    /// <summary>Appends the variant <paramref name="artwork"/>, the artwork of <paramref name="population"/>, draws each enemy and owner in.</summary>
    private static void AppendArtwork(SelectedPresentationHash content, string population, PopulationArtwork artwork)
    {
        content.AppendName("population", population);
        foreach ((EnemyDefinitionId enemy, EnemyArtworkVariant<SpriteSheet> variant) in artwork.enemies.OrderBy(entry => entry.Key))
            content.AppendName($"enemy {enemy}", variant.Name);
        foreach ((SpawnedObjectOwner owner, EnemyArtworkVariant<SpawnedObjectSheet> variant) in artwork.spawned
                     .OrderBy(entry => SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Name(entry.Key), StringComparer.Ordinal))
            content.AppendName($"owner {SuperMetroid.AssetExtraction.Enemies.SpawnedObjectOwnerNames.Name(owner)}", variant.Name);
    }

    /// <summary>Appends the tokens of <paramref name="inputs"/> to a content identity.</summary>
    private static void AppendInputs(SelectedPresentationHash content, ImmutableArray<AppearanceInput> inputs)
    {
        content.Append("inputs", inputs.Length);
        foreach (AppearanceInput input in inputs)
            content.AppendName("input", input.Token);
    }
}
