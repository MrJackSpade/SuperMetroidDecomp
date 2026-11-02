using System.Collections.Immutable;
using System.Text;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction;

/// <summary>The installed content identities of the sprite, enemy, boss-body and room sheets, the X-ray, Samus death and cinematic catalogs, which only the import pipeline and its diagnostics compare (#1502).</summary>
internal static class InstalledArtworkContentIdentity
{
    /// <summary>The installed content identity of a <see cref="InstalledEnemySheets"/>.</summary>
    extension(InstalledEnemySheets installed)
    {
        /// <summary>SHA-256 of every distinct sheet and each set's choice of them.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledEnemySheets), content =>
        {
            foreach ((EnemySheetKey key, SpriteSheet sheet) in installed.sheets.OrderBy(entry => entry.Key.Set, StringComparer.Ordinal)
                         .ThenBy(entry => entry.Key.Enemy))
            {
                content.Append("set", System.Text.Encoding.UTF8.GetBytes(key.Set));
                content.Append("enemy", System.Text.Encoding.UTF8.GetBytes(key.Enemy.ToString()));
                sheet.AppendIdentity(content);
            }
            foreach ((SpawnedObjectSheetKey key, SpawnedObjectSheet sheet) in installed.spawned.OrderBy(entry => entry.Key.Set, StringComparer.Ordinal)
                         .ThenBy(entry => entry.Key.TileBase))
            {
                content.Append("spawned-set", System.Text.Encoding.UTF8.GetBytes(key.Set));
                content.Append("tile-base", key.TileBase);
                content.Append("palette", sheet.ColourGroup.Number);
                sheet.Sheet.AppendIdentity(content);
            }
            AppendPictures(content, "pictures", installed.pictures.Select(entry => new PictureOwner(entry.Key.Set, entry.Key.Enemy.ToString(), entry.Value)));
            AppendPictures(content, "spawned-pictures", installed.spawnedPictures.Select(entry => new PictureOwner(entry.Key.Set,
                entry.Key.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), entry.Value)));
            AppendRotPictures(content, "rot-pictures", installed.rotPictures.Select(entry => new RotPictureOwner(entry.Key.Set, entry.Key.Enemy.ToString(), entry.Value)));
            AppendRotPictures(content, "spawned-rot-pictures", installed.spawnedRotPictures.Select(entry => new RotPictureOwner(entry.Key.Set,
                entry.Key.TileBase.ToString(System.Globalization.CultureInfo.InvariantCulture), entry.Value)));
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

    /// <summary>A sprite sheet's pixels and frames in a content identity.</summary>
    extension(SpriteSheet sheet)
    {
        /// <summary>Appends the sheet's pixels and frames to a content identity, frames in name order.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.AppendName("sheet", sheet.Image.Name);
            content.AppendName("palette", sheet.Image.Colours.ToString());
            foreach (BackgroundColourSet set in sheet.Image.BackgroundSets)
                content.AppendName("background set", set.ToString());
            content.Append("width", sheet.Image.Size.X);
            content.Append("height", sheet.Image.Size.Y);
            content.Append("pixels", System.Runtime.InteropServices.MemoryMarshal.AsBytes(sheet.Image.Pixels));
            // Each ink field is identified as its own plane, as the image was installed.
            ReadOnlySpan<SpriteImageInk> inks = sheet.Image.Inks;
            content.AppendBytes("palette indices", inks.SelectToArray(ink => ink.PaletteIndex));
            content.Append("standard colours", System.Runtime.InteropServices.MemoryMarshal.AsBytes(sheet.Image.StandardColours));
            content.AppendBytes("priorities", inks.SelectToArray(ink => ink.Priority));
            content.AppendBytes("part groups", inks.SelectToArray(ink => ink.PartGroup.Code));
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
                foreach (ImageColourGroup group in ImageColourGroup.Every)
                    content.Append("colour group", frame.ColourGroups.Contains(group) ? 1 : 0);
                foreach (SpriteSheetFramePart part in frame.Parts)
                {
                    content.Append("part corner x", part.Image.Corner.X);
                    content.Append("part corner y", part.Image.Corner.Y);
                    content.Append("part width", part.Image.Size.X);
                    content.Append("part height", part.Image.Size.Y);
                    content.Append("part offset x", part.Offset.X);
                    content.Append("part offset y", part.Offset.Y);
                }
            }
        }
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
            foreach (WrittenPictureSheet pictures in artwork.Written.Values.Concat(artwork.Bg3Pictures.Values).OrderBy(pictures => pictures.Name, StringComparer.Ordinal))
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
        /// <summary>Appends every block's inputs to a content identity.</summary>
        internal void AppendIdentity(SelectedPresentationHash content)
        {
            content.Append("blocks", appearances.tileset.Length);
            ImmutableArray<AppearanceInput>[][] artworks = appearances.sprites is null ? [appearances.tileset] : [appearances.tileset, appearances.sprites];
            foreach (ImmutableArray<AppearanceInput>[] artwork in artworks)
            {
                foreach (ImmutableArray<AppearanceInput> inputs in artwork)
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
        /// <summary>Identity of selected item appearances and room overlay coordinates and blocks, in record order.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledXrayOverlays), content =>
        {
            content.AppendAppearances("item metatiles", overlays.itemMetatiles);
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
            for (int index = 0; index < reveals.blocks.Length; index++)
                if (reveals.blocks[index] is { } visual)
                {
                    content.Append("kind and behaviour", index);
                    content.Append("top left", visual.TopLeft);
                    content.Append("top right", visual.TopRight);
                    content.Append("bottom left", visual.BottomLeft);
                    content.Append("bottom right", visual.BottomRight);
                }
            content.Append("has overlays", reveals.Overlays is null ? 0 : 1);
            if (reveals.Overlays is not null)
                content.Append("overlays", Convert.FromHexString(reveals.Overlays.ContentIdentity));
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

    /// <summary>The installed content identity of a <see cref="EndingPaletteSet"/>.</summary>
    extension(EndingPaletteSet palettes)
    {
        /// <summary>Identity of all selected static colors and every ordered logo-crossfade color.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("EndingPaletteCatalog", content =>
        {
            content.Append("palette-count", 7);
            content.Append("palette", palettes.escape.Bytes.Span);
            content.Append("palette", palettes.postCredits.Bytes.Span);
            content.Append("palette", palettes.credits.Bytes.Span);
            content.Append("palette", palettes.explosion.Bytes.Span);
            content.Append("palette", palettes.finalGunship.Bytes.Span);
            content.Append("palette", palettes.logoInitial.Bytes.Span);
            content.Append("palette", palettes.logoCrossfade.Bytes.Span);
        });
    }

    /// <summary>Appends each sheet's frames drawn over a rotting corpse's rows and the corpse each shows, in a stable order.</summary>
    private static void AppendRotPictures(SelectedPresentationHash content, string label, IEnumerable<RotPictureOwner> owners)
    {
        foreach (RotPictureOwner owner in owners.OrderBy(owner => owner.Set, StringComparer.Ordinal).ThenBy(owner => owner.Owner, StringComparer.Ordinal))
        {
            content.Append(label, System.Text.Encoding.UTF8.GetBytes($"{owner.Set}/{owner.Owner}"));
            foreach ((string frame, string corpse) in owner.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
                content.Append("rot-frame", System.Text.Encoding.UTF8.GetBytes($"{frame}={corpse}"));
        }
    }

    /// <summary>One sheet's frames drawn over a rotting corpse's rows.</summary>
    /// <param name="Set">The graphics set's name.</param>
    /// <param name="Owner">The enemy's name, or the spawned objects' tile base.</param>
    /// <param name="Frames">The frames and the kind of corpse whose rot each shows.</param>
    private sealed record RotPictureOwner(string Set, string Owner, IReadOnlyDictionary<string, string> Frames);

    /// <summary>Appends each sheet's frames drawn over uploaded characters and their regions, in a stable order.</summary>
    private static void AppendPictures(SelectedPresentationHash content, string label, IEnumerable<PictureOwner> owners)
    {
        foreach (PictureOwner owner in owners.OrderBy(owner => owner.Set, StringComparer.Ordinal).ThenBy(owner => owner.Owner, StringComparer.Ordinal))
        {
            content.Append(label, System.Text.Encoding.UTF8.GetBytes($"{owner.Set}/{owner.Owner}"));
            foreach ((string frame, LookRegion[][] layers) in owner.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.Append("frame", System.Text.Encoding.UTF8.GetBytes(frame));
                foreach (LookRegion[] layer in layers)
                {
                    content.Append("layer", layer.Length);
                    foreach (LookRegion region in layer)
                    {
                        content.Append("region", region.Ordinal);
                        content.Append("region-name", System.Text.Encoding.UTF8.GetBytes(region.Name));
                    }
                }
            }
        }
    }

    /// <summary>One sheet's frames drawn over uploaded characters.</summary>
    /// <param name="Set">The graphics set's name.</param>
    /// <param name="Owner">The enemy's name, or the spawned objects' tile base.</param>
    /// <param name="Frames">The frames and the layers each draws as.</param>
    private sealed record PictureOwner(string Set, string Owner, IReadOnlyDictionary<string, LookRegion[][]> Frames);

    /// <summary>Appends the tokens of <paramref name="inputs"/> to a content identity.</summary>
    private static void AppendInputs(SelectedPresentationHash content, ImmutableArray<AppearanceInput> inputs)
    {
        content.Append("inputs", inputs.Length);
        foreach (AppearanceInput input in inputs)
            content.AppendName("input", input.Token);
    }
}
