using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Verification access to <see cref="BeamTileAtlas"/> members production does not use.</summary>
internal static class BeamTileAtlasAccess
{
    /// <summary>Loads an installed beam tile transfer into its native VRAM destination.</summary>
    extension(BeamTileAtlas self)
    {
        /// <summary>Writes the atlas transfer bytes at the beam tile base word.</summary>
        [PpuLayoutBoundary("codec: beam tile transfer at its native VRAM word")]
        internal void LoadTo(SnesVram vram) =>
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, self.Transfer.Span);
    }
}

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    /// <summary>Builds a focused beam catalog from named PNG bytes and optional palette catalogs.</summary>
    extension(BeamTileCatalog)
    {
        /// <summary>Requires every beam artwork file and decodes each under its authored selection.</summary>
        internal static BeamTileCatalog Load(IReadOnlyDictionary<string, byte[]> files,
            BeamPaletteCatalog? palettes = null, HyperBeamFxColorCatalog? hyperBeamFxColors = null)
        {
            var sheets = new BeamTileAtlas[BeamTileAtlasDefinitions.ArtworkCount];
            for (int i = 0; i < sheets.Length; i++)
            {
                string name = BeamTileAtlasDefinitions.FileName(BeamTileAtlasDefinitions.SelectionAt(i));
                if (!files.TryGetValue(name, out var png) || png is null)
                    throw new InvalidDataException($"Missing beam artwork {name}.");
                sheets[i] = BeamTileAtlas.Load(new MemoryStream(png, writable: false), BeamTileAtlasDefinitions.SelectionAt(i));
            }
            return ((BeamTileCatalog)PrivateState.Construct(typeof(BeamTileCatalog), (BeamTileAtlas[])(sheets), (BeamPaletteCatalog?)(palettes), (HyperBeamFxColorCatalog?)(hyperBeamFxColors)));
        }
    }
}

/// <summary>Verification access to <see cref="CeresEscapeOverlayTilemapDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsAccess
{
    /// <summary>Provides range checks against all authored Ceres escape overlay pages.</summary>
    extension(CeresEscapeOverlayTilemapDefinitions)
    {
        /// <summary>Checks whether a byte falls within any page's source-word interval.</summary>
        internal static bool ContainsByteAddress(int address)
        {
            foreach (CeresEscapeOverlayTilemapDefinition page in CeresEscapeOverlayTilemapDefinitions.All)
                if (address >= page.SourceAddress &&
                    address < page.SourceAddress + page.WordCount * sizeof(ushort))
                    return true;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="CeresRidleyColorCatalog"/> members production does not use.</summary>
internal static class CeresRidleyColorCatalogAccess
{
    /// <summary>Exposes phase-specific color definitions held privately by the installed catalog.</summary>
    extension(CeresRidleyColorCatalog self)
    {
        /// <summary>Reads an initial Ceres Ridley color before the fade phases.</summary>
        internal Bgr555 ResolveStart(int color) => PrivateState.Field<CeresRidleyStartColorDefinitions>(self, "start").Resolve(color);

        /// <summary>Reads one eye-fade row and color independently of body fade.</summary>
        internal Bgr555 ResolveEyeFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "eyeFade").Resolve(row, color);

        /// <summary>Reads one body-fade row and color independently of eye fade.</summary>
        internal Bgr555 ResolveBodyFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "bodyFade").Resolve(row, color);

        /// <summary>Reads a health-dependent paint row from the installed catalog.</summary>
        internal Bgr555 ResolveHealth(int row, int color) => PrivateState.Field<CeresRidleyHealthPaintDefinitions>(self, "health").Resolve(row, color);

        /// <summary>Reads the Ceres alarm-paint sequence at a selected row.</summary>
        internal Bgr555 ResolveAlarm(int row, int color) => PrivateState.Field<CeresRidleyAlarmColorDefinitions>(self, "alarm").Resolve(row, color);

        /// <summary>Reads the baby-Metroid paint sequence at a selected row.</summary>
        internal Bgr555 ResolveBaby(int row, int color) => PrivateState.Field<CeresBabyPaintDefinitions>(self, "baby").Resolve(row, color);
    }
}

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    /// <summary>Uses the catalog's private statue resolver for the two special palettes.</summary>
    extension(ChozoAndTubeColorCatalog self)
    {
        /// <summary>Selects Wrecked Ship statue colors through the production palette branch.</summary>
        internal Bgr555 ResolveWreckedShip(int color) => ((Bgr555)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color)))!);

        /// <summary>Selects Lower Norfair statue colors through the production palette branch.</summary>
        internal Bgr555 ResolveLowerNorfair(int color) => ((Bgr555)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="CreditsPresentation"/> members production does not use.</summary>
internal static class CreditsPresentationAccess
{
    /// <summary>Constructs a credits presenter from explicit compiled tilemap rows.</summary>
    extension(CreditsPresentation)
    {
        /// <summary>Copies nonempty 32-word rows into a verification-only presentation.</summary>
        internal static CreditsPresentation FromCompiledRowsForVerification(
            params ushort[][] rows)
        {
            if (rows.Length == 0 || rows.Any(row =>
                row.Length != CreditsPresentationDefinitions.TilemapWidth))
            {
                throw new ArgumentException(
                    "Verification credits rows must be nonempty 32-word rows.", "rows");
            }
            return ((CreditsPresentation)PrivateState.Construct(typeof(CreditsPresentation), (CreditsLineDocument[]?)(null), (ushort[][]?)(rows.Select(row => row.ToArray()).ToArray()), (string)("VERIFICATION")));
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireBodyFrameSequence"/> members production does not use.</summary>
internal static class CrocomireBodyFrameSequenceAccess
{
    /// <summary>Materializes the sequence through its public indexed access.</summary>
    extension(CrocomireBodyFrameSequence self)
    {
        /// <summary>Copies each compiled Crocomire body frame word in sequence order.</summary>
        internal ushort[] ToArray()
        {
            var result = new ushort[self.Length];
            for (int i = 0; i < result.Length; i++) result[i] = self[i];
            return result;
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireColorCatalog"/> members production does not use.</summary>
internal static class CrocomireColorCatalogAccess
{
    /// <summary>Reads independently editable Crocomire paint bands through the private selector.</summary>
    extension(CrocomireColorCatalog self)
    {
        /// <summary>Reads body colors used during the active fight.</summary>
        internal Bgr555 ResolveFightBody(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "FightBody"), (int)(color)))!);

        /// <summary>Reads the initial wall's Crocomire color band.</summary>
        internal Bgr555 ResolveInitialWall(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialWall"), (int)(color)))!);

        /// <summary>Reads the initial projectile's Crocomire color band.</summary>
        internal Bgr555 ResolveInitialProjectile(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialProjectile"), (int)(color)))!);

        /// <summary>Reads the exposed skeleton-arm color band.</summary>
        internal Bgr555 ResolveSkeletonArm(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "SkeletonArm"), (int)(color)))!);

        /// <summary>Reads the wall-spike color band for the Crocomire room.</summary>
        internal Bgr555 ResolveWallSpikes(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "WallSpikes"), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="EndingFontAtlas"/> members production does not use.</summary>
internal static class EndingFontAtlasAccess
{
    /// <summary>Reconstructs the ending font from exact-size planar bytes for legacy fixtures.</summary>
    extension(EndingFontAtlas)
    {
        /// <summary>Constructs the fallback cartridge-backed atlas used by focused legacy ending tests.</summary>
        internal static EndingFontAtlas FromPlanarBytes(ReadOnlySpan<byte> planar)
        {
            if (planar.Length != EndingFontAtlasFormat.ByteCount)
                throw new InvalidDataException(
                    $"Ending font contains {planar.Length} planar bytes; expected {EndingFontAtlasFormat.ByteCount}.");
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, EndingFontAtlasFormat.BitsPerPixel,
                EndingFontAtlasFormat.TilesPerRow, out _);
            return ((EndingFontAtlas)PrivateState.Construct(typeof(EndingFontAtlas), (byte[])(pixels)));
        }
    }
}

/// <summary>Verification access to <see cref="EnemyBg2FrameDefinitionSequence"/> members production does not use.</summary>
internal static class EnemyBg2FrameDefinitionSequenceAccess
{
    /// <summary>Materializes BG2 frame definitions through the compiled sequence indexer.</summary>
    extension(EnemyBg2FrameDefinitionSequence self)
    {
        /// <summary>Copies all enemy BG2 frame definitions in authored order.</summary>
        internal EnemyBg2FrameDefinition[] ToArray()
        {
            var result = new EnemyBg2FrameDefinition[self.Length];
            for (int index = 0; index < result.Length; index++) result[index] = self[index];
            return result;
        }
    }
}

/// <summary>Verification access to <see cref="EnemySpritemapCatalog"/> members production does not use.</summary>
internal static class EnemySpritemapCatalogAccess
{
    /// <summary>Inspects the installed frame dictionary by native bank and pointer.</summary>
    extension(EnemySpritemapCatalog self)
    {
        /// <summary>Returns a known installed frame; callers must reject missing artwork.</summary>
        internal bool TryGet(byte bank, ushort pointer, out EnemySpritemapParts parts)
        {
            if (PrivateState.Field<Dictionary<int, EnemySpritemapParts>>(self, "frames").TryGetValue(((int)new SnesAddress(bank, pointer)), out EnemySpritemapParts? found))
            {
                parts = found;
                return true;
            }
            parts = EnemySpritemapParts.Empty;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="EnemyTileArtworkCatalog"/> members production does not use.</summary>
internal static class EnemyTileArtworkCatalogAccess
{
    /// <summary>Constructs a partial enemy-art catalog for focused verification.</summary>
    extension(EnemyTileArtworkCatalog)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileArtworkCatalog FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            (EnemyTileArtworkCatalog)PrivateState.Construct(typeof(EnemyTileArtworkCatalog), Ensure.NotNull(parts));
    }

    /// <summary>Produces shallow copies with one presentation part replaced.</summary>
    extension(EnemyTileArtworkCatalog self)
    {
        /// <summary>
        /// This installation with its compositions replaced, as the installation loader produces for a
        /// composition override. The other parts are independent and shared; nothing is derived from
        /// the compositions at construction.
        /// </summary>
        internal EnemyTileArtworkCatalog WithSpritemaps(EnemySpritemapCatalog spritemaps)
        {
            Ensure.NotNull(spritemaps);
            var copy = (EnemyTileArtworkCatalog)((object)(PrivateState.Invoke(self, "MemberwiseClone"))!);
            PrivateState.SetProperty(copy, "Spritemaps", spritemaps);
            return copy;
        }

        /// <summary>This installation with its extended frames replaced; see <see cref="WithSpritemaps"/>.</summary>
        internal EnemyTileArtworkCatalog WithExtendedFrames(EnemyExtendedFrameCatalog extendedFrames)
        {
            Ensure.NotNull(extendedFrames);
            var copy = (EnemyTileArtworkCatalog)((object)(PrivateState.Invoke(self, "MemberwiseClone"))!);
            PrivateState.SetProperty(copy, "ExtendedFrames", extendedFrames);
            return copy;
        }
    }
}

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    /// <summary>Checks and queues the two native escape-timer DMA transfers.</summary>
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Queues both native records in their original order and at their original destinations.</summary>
        [PpuLayoutBoundary("codec: escape timer queued DMA records at their native VRAM words")]
        internal void QueueTo(VramWriteQueue queue)
        {
            Ensure.NotNull(queue);
            if (((byte[])(PrivateState.Invoke(self, "Transfer"))!).Length != EscapeTimerTileAtlasFormat.TotalByteCount)
                throw new InvalidDataException("Escape timer artwork no longer matches its native transfer pages.");
            queue.EnqueueAsset(VramAssetId.EscapeTimerFirstTiles,
                EscapeTimerTileAtlasFormat.FirstByteCount, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.FirstDestinationWord));
            queue.EnqueueAsset(VramAssetId.EscapeTimerSecondTiles,
                EscapeTimerTileAtlasFormat.SecondByteCount, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.SecondDestinationWord));
        }
    }
}

/// <summary>Verification access to <see cref="GameplayBasePaletteCatalog"/> members production does not use.</summary>
internal static class GameplayBasePaletteCatalogAccess
{
    /// <summary>Exposes the catalog's stock common-sprite CGRAM words for comparison.</summary>
    extension(GameplayBasePaletteCatalog self)
    {
        /// <summary>Reads the common-sprite palette without copying or applying overrides.</summary>
        internal ReadOnlySpan<ushort> CommonSprites => PrivateState.Field<ushort[]>(self, "commonSprites");
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    /// <summary>Calculates native frame addresses for the ten stride phases.</summary>
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        /// <summary>Offsets the first frame by the fixed record width after validating the phase.</summary>
        internal static int NativeFrameIdentity(int phase)
        {
            if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
            return PrivateState.StaticField<int>(typeof(GoldenTorizoStrideGeometryDefinitions), "FirstFrame") + PrivateState.StaticField<int>(typeof(GoldenTorizoStrideGeometryDefinitions), "FrameBytes") * phase;
        }
    }
}

/// <summary>Verification access to <see cref="MapArrowVisual"/> members production does not use.</summary>
internal static class MapArrowVisualAccess
{
    /// <summary>Inspects whether arrow timing edits are stored independently of stock values.</summary>
    extension(MapArrowVisual self)
    {
        /// <summary>Counts explicitly overridden arrow-frame durations.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, byte>?>(self, "durationOverrides")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    /// <summary>Inspects sparse edits to map-marker tile pixels.</summary>
    extension(MapMarkerTileArtwork self)
    {
        /// <summary>Counts only explicitly edited marker bytes.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    /// <summary>Distinguishes sparse highlight and reserve edits from full other-character bytes.</summary>
    extension(MapObjectTileArtwork self)
    {
        /// <summary>Counts edited highlight pixels in the sparse override dictionary.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "highlightEdits")?.Count ?? 0;

        /// <summary>Counts edited reserve pixels in the sparse override dictionary.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "reserveEdits")?.Count ?? 0;

        /// <summary>Measures the separate non-highlight, non-reserve character payload.</summary>
        internal int StoredOtherByteCount => PrivateState.Field<byte[]>(self, "otherCharacters").Length;
    }
}

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    /// <summary>Counts explicitly stored X and Y coordinates of save-marker layout edits.</summary>
    extension(MapSaveMarkerLayout self)
    {
        /// <summary>Counts each non-null coordinate component separately across marker overrides.</summary>
        internal int StoredCoordinateComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "coordinateOverrides").Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    /// <summary>Accesses a compiled world-map label through the catalog's private selector.</summary>
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, MapSpriteId id) =>
        (WorldMapLabelComposition?)PrivateState.Invoke(catalog, "GetWorldLabel", id);

    /// <summary>Inspects authored compositions and sparse map-object pixel storage.</summary>
    extension(MapSpriteCatalog self)
    {
        /// <summary>Checks whether a world label has authored parts or a non-label has an installed frame.</summary>
        internal bool StoresComposition(MapSpriteId id) => WorldLabel(self, id) is { } label
            ? PrivateState.Field<SpriteComposition?>(label, "authored") is not null
            : PrivateState.Invoke(self, "GetFrame", id) is not null;

        /// <summary>Stored horizontal advances: the label origin(s) plus each authored letter advance.</summary>
        internal int StoredLabelHorizontalCount(MapSpriteId id)
        {
            if (WorldLabel(self, id) is not { } label || PrivateState.Field<SpriteComposition?>(label, "authored") is not null)
                return 0;
            int origins = PrivateState.Field<MapSpriteId>(label, "identity") == MapSpriteId.WorldWreckedShip ? 2 : 1;
            return origins + (PrivateState.Field<Dictionary<int, int>?>(label, "letterAdvances")?.Count ?? 0);
        }

        /// <summary>Counts sparse highlight pixels in the map-object character art.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredHighlightPixelCount;

        /// <summary>Counts sparse reserve pixels in the map-object character art.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredReservePixelCount;

        /// <summary>Measures the catalog's independent map-object character byte payload.</summary>
        internal int StoredArtworkByteCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredOtherByteCount;
    }
}

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    /// <summary>Inspects sparse bevel-tile edits in the menu artwork.</summary>
    extension(MenuBeveledSquareArtwork self)
    {
        /// <summary>Counts explicit bevel pixel replacements only.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    /// <summary>Inspects glyph ink storage and sparse compact-letter pixel edits.</summary>
    extension(MenuCompactLetteringArtwork self)
    {
        /// <summary>Measures one 32-bit ink value per stored glyph.</summary>
        internal int StoredInkByteCount => PrivateState.Field<Dictionary<char, uint>>(self, "glyphs").Count * sizeof(uint);

        /// <summary>Counts explicit compact-letter pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks the sparse edit key for a valid tile and eight-by-eight pixel coordinate.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuCompactLetteringArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuLargeFontArtwork"/> members production does not use.</summary>
internal static class MenuLargeFontArtworkAccess
{
    /// <summary>Inspects large-font face bytes and sparse pixel edits.</summary>
    extension(MenuLargeFontArtwork self)
    {
        /// <summary>Measures the stored font-face payload before pixel overrides.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Counts explicit large-font pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks the sparse edit key for a valid tile and eight-by-eight pixel coordinate.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuLargeFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuPanelTileArtwork"/> members production does not use.</summary>
internal static class MenuPanelTileArtworkAccess
{
    /// <summary>Inspects explicit pixel edits to menu panel tiles.</summary>
    extension(MenuPanelTileArtwork self)
    {
        /// <summary>Counts sparse menu-panel byte replacements.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    /// <summary>Inspects explicit edits to shoulder-button artwork.</summary>
    extension(MenuShoulderButtonArtwork self)
    {
        /// <summary>Counts sparse shoulder-button byte replacements.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    /// <summary>Inspects explicit edits to shoulder-button highlight artwork.</summary>
    extension(MenuShoulderHighlightArtwork self)
    {
        /// <summary>Counts sparse highlight byte replacements.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    /// <summary>Inspects small-font face bytes and sparse pixel edits.</summary>
    extension(MenuSmallFontArtwork self)
    {
        /// <summary>Measures the stored small-font face payload.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Counts explicit small-font pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks the sparse edit key relative to the small font's first tile.</summary>
        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuSmallFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey((tile - MenuSmallFontArtwork.FirstTile) * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuThinBorderArtwork"/> members production does not use.</summary>
internal static class MenuThinBorderArtworkAccess
{
    /// <summary>Inspects sparse edits to thin menu border tiles.</summary>
    extension(MenuThinBorderArtwork self)
    {
        /// <summary>Counts explicit border byte replacements.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    /// <summary>Enumerates all compiled Mother Brain body-frame definitions.</summary>
    extension(MotherBrainBodyVisualDefinitions)
    {
        /// <summary>Materializes the indexed frame definitions in animation order.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyVisualDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitions.Frame)];
    }
}

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    /// <summary>Inspects initial and reveal paint sequences held by the installed catalog.</summary>
    extension(NorfairRidleyColorCatalog self)
    {
        /// <summary>Reads a pre-reveal Ridley color.</summary>
        internal Bgr555 ResolveInitial(int color) => PrivateState.Field<NorfairRidleyInitialPaintDefinitions>(self, "initial").ColorAt(color);

        /// <summary>Reads a row and color from Ridley's reveal paint sequence.</summary>
        internal Bgr555 ResolveReveal(int row, int color) => PrivateState.Field<NorfairRidleyRevealPaintDefinitions>(self, "reveal").ColorAt(row, color);
    }
}

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    /// <summary>Inspects which reserve-tank frames and anchor components are actually stored.</summary>
    extension(PauseReserveTankPresentation self)
    {
        /// <summary>Counts non-null compositions among the ten reserve-tank visual phases.</summary>
        internal int StoredFrameCount => (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Full") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "EndCap") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Empty") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill1") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill2") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill3") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill4") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill5") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill6") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill7") is null ? 0 : 1);

        /// <summary>Counts separately supplied X and Y coordinates across reserve-tank anchors.</summary>
        internal int StoredAnchorComponentCount => PrivateState.Field<Dictionary<int, (int? X, int? Y)>>(self, "anchorOverrides").Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="PauseSelectorPresentation"/> members production does not use.</summary>
internal static class PauseSelectorPresentationAccess
{
    /// <summary>Whether a selector visual stores authored parts rather than its calculated stock composition.</summary>
    private static bool StoresParts(PauseSelectorVisual visual) => PrivateState.Field<object?>(visual, "authored") is not null;

    /// <summary>The reserve, beam and equipment phase compositions.</summary>
    private static object[] Categories(PauseSelectorPresentation presentation) =>
        [.. new[] { "reserve", "beam", "equipment" }.Select(name => PrivateState.Field<object>(presentation, name))];

    /// <summary>Inspects timing, composition, and anchor overrides across reserve, beam, and equipment selectors.</summary>
    extension(PauseSelectorPresentation self)
    {
        /// <summary>Counts explicit selector-frame duration overrides.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, int>?>(self, "durationOverrides")?.Count ?? 0;

        /// <summary>Checks authored parts in any selector basis or override frame.</summary>
        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(PrivateState.Field<PauseSelectorVisual>(category, "basis")) ||
            (PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Values.Any(StoresParts) ?? false));

        /// <summary>Counts replaced selector compositions across all three categories.</summary>
        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Count ?? 0);

        /// <summary>Counts explicitly supplied X and Y anchor components.</summary>
        internal int StoredAnchorComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "anchorOverrides").Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    /// <summary>Expands pair colors and the optional third color into a fresh output array.</summary>
    extension(RoomFxBlendColors self)
    {
        /// <summary>Creates requested blend colors on demand; no stock-color array is retained.</summary>
        internal Bgr555[] CreateColors() => [PrivateState.Field<RoomFxPairColor>(self, "primary").CreateColor(), PrivateState.Field<RoomFxPairColor>(self, "secondary").CreateColor(), PrivateState.Field<RoomFxThirdColor?>(self, "thirdOverride")?.CreateColor() ?? Bgr555.Black];
    }
}

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    /// <summary>Selects room-FX blend colors through the catalog's private branch.</summary>
    extension(RoomFxPaletteBlendCatalog self)
    {
        /// <summary>Resolves the selected blend and materializes its three RGB5 colors.</summary>
        internal ReadOnlySpan<Bgr555> Resolve(RoomFxPaletteBlend selection) => ((RoomFxBlendColors)(PrivateState.Invoke(self, "SelectColors", selection))!).CreateColors();
    }
}

/// <summary>Verification access to <see cref="SamusBodyTileDefinition"/> members production does not use.</summary>
internal static class SamusBodyTileDefinitionAccess
{
    /// <summary>Uses the catalog's own pose pointers and frame table for transfer geometry.</summary>
    extension(SamusBodyTileDefinition self)
    {
        /// <summary>Derives transfer layout from one upper or lower tile slot and installed body art.</summary>
        internal SamusBodyTileDefinition WithTransferGeometry(SamusBodyArtworkCatalog body, bool upper, int set, int position) =>
            self.WithTransferGeometry(body, upper, set, position, body.PosePointers, body.Frames);
    }
}

/// <summary>Verification access to <see cref="SamusBodyTransferDefinitions"/> members production does not use.</summary>
internal static class SamusBodyTransferDefinitionsAccess
{
    /// <summary>Checks canonical transfer size using in-group OAM and the separate pixel payload.</summary>
    extension(SamusBodyTransferDefinitions)
    {
        /// <summary>
        /// Canonical in-group OAM establishes the first-row footprint; the remainder
        /// of the separately owned packed pixel payload is the second transfer.
        /// Physical cross-group selectors remain valid but do not redefine a glyph's
        /// canonical composition from an unrelated pose's OAM.
        /// </summary>
        internal static bool TryFirstSize(SamusBodyArtworkCatalog body, bool upper, int set, int position,
            int payloadBytes, out ushort firstSize)
        {
            object?[] packing = [upper, set, position, payloadBytes, null];
            if ((bool)PrivateState.InvokeStaticWithOut(typeof(SamusBodyTransferDefinitions), "TrySelectedPacking", packing)!)
            {
                firstSize = (ushort)packing[4]!;
                return true;
            }
            return SamusBodyTransferDefinitions.TryFirstSize(body, new SamusBodyTileSlot(upper, set, position), payloadBytes, body.PosePointers, body.Frames, out firstSize);
        }
    }
}

/// <summary>Verification access to <see cref="SamusChargeColorCatalog"/> members production does not use.</summary>
internal static class SamusChargeColorCatalogAccess
{
    /// <summary>Reads the pseudo-screw or charged-beam suit palette for one phase.</summary>
    extension(SamusChargeColorCatalog self)
    {
        /// <summary>Selects the appropriate charge branch, suit, phase, and color.</summary>
        internal Bgr555 ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            ((Bgr555)(PrivateState.Invoke((pseudo ? PrivateState.Field<object>(self, "pseudoScrew") : PrivateState.Field<object>(self, "chargedBeam")), "Resolve", (int)(suit), (int)(phase), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    /// <summary>Inspects installed Samus compositions by their private native-pointer index.</summary>
    extension(SamusSpritemapArtworkCatalog self)
    {
        /// <summary>Enumerates the stored spritemap definitions without recreating stock art.</summary>
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => PrivateState.Field<Dictionary<ushort, SamusSpritemapDefinition>>(self, "definitions").Values;
    }
}

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
    /// <summary>Appends raw word rows with boundaries preserved, including empty rows.</summary>
    extension(SelectedPresentationHash hash)
    {
        /// <summary>Ordered raw word rows retain their boundaries, including empty rows: the independent oracle of <see cref="SelectedPresentationHash.AppendColorFrames"/>.</summary>
        internal void AppendWordFrames(string label, IReadOnlyList<ushort[]> frames)
        {
            hash.Append(label, frames.Count);
            foreach (ushort[] frame in frames)
                hash.AppendWords("row", frame);
        }
    }

    /// <summary>Builds stable hashes for single runs, scalar words, and multiple runs per frame.</summary>
    extension(SelectedPresentationHash)
    {
        /// <summary>Wraps each word-array frame as one run before the canonical keyed hash.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Wraps each scalar frame as a one-word run before the canonical keyed hash.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Preserves draw-run and word order while canonicalizing frame-key insertion order.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[][]> frames) =>
            SelectedPresentationHash.Create(domain, content =>
            {
                foreach ((ushort pointer, ushort[][] runs) in frames.OrderBy(pair => pair.Key))
                {
                    content.Append("frame", pointer);
                    content.Append("runs", runs.Length);
                    foreach (ushort[] run in runs)
                        content.AppendWords("words", run);
                }
            });
    }
}

/// <summary>Verification access to <see cref="TitlePalettePresentation"/> members production does not use.</summary>
internal static class TitlePalettePresentationAccess
{
    /// <summary>Exposes installed title colors without re-reading the cartridge.</summary>
    extension(TitlePalettePresentation self)
    {
        /// <summary>Reads the stored RGB5 color span used by the title presentation.</summary>
        internal ReadOnlySpan<Bgr555> Colors => PrivateState.Field<Bgr555[]>(self, "colors");
    }
}

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    /// <summary>Inspects the installed Tourian statue color definition.</summary>
    extension(TourianStatueColorCatalog self)
    {
        /// <summary>Reads one statue color through its private paint definition.</summary>
        internal Bgr555 ResolveStatue(int color) => ((Bgr555)(PrivateState.Invoke(PrivateState.Field<object>(self, "statueColors"), "Read", (int)(color)))!);
    }
}
