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
    /// <summary>Provides verification helpers for <see cref="BeamTileAtlas"/>.</summary>
    extension(BeamTileAtlas self)
    {
        /// <summary>Loads to.</summary>
        [PpuLayoutBoundary("codec: beam tile transfer at its native VRAM word")]
        internal void LoadTo(SnesVram vram) =>
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, self.Transfer.Span);
    }
}

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="BeamTileCatalog"/>.</summary>
    extension(BeamTileCatalog)
    {
        /// <summary>Loads.</summary>
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
    /// <summary>Provides verification helpers for <see cref="CeresEscapeOverlayTilemapDefinitions"/>.</summary>
    extension(CeresEscapeOverlayTilemapDefinitions)
    {
        /// <summary>Checks whether an address lies within any escape-overlay tilemap page.</summary>
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
    /// <summary>Provides verification helpers for <see cref="CeresRidleyColorCatalog"/>.</summary>
    extension(CeresRidleyColorCatalog self)
    {
        /// <summary>Resolves start.</summary>
        internal Bgr555 ResolveStart(int color) => PrivateState.Field<CeresRidleyStartColorDefinitions>(self, "start").Resolve(color);

        /// <summary>Resolves eye fade.</summary>
        internal Bgr555 ResolveEyeFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "eyeFade").Resolve(row, color);

        /// <summary>Resolves body fade.</summary>
        internal Bgr555 ResolveBodyFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "bodyFade").Resolve(row, color);

        /// <summary>Resolves health.</summary>
        internal Bgr555 ResolveHealth(int row, int color) => PrivateState.Field<CeresRidleyHealthPaintDefinitions>(self, "health").Resolve(row, color);

        /// <summary>Resolves alarm.</summary>
        internal Bgr555 ResolveAlarm(int row, int color) => PrivateState.Field<CeresRidleyAlarmColorDefinitions>(self, "alarm").Resolve(row, color);

        /// <summary>Resolves baby.</summary>
        internal Bgr555 ResolveBaby(int row, int color) => PrivateState.Field<CeresBabyPaintDefinitions>(self, "baby").Resolve(row, color);
    }
}

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="ChozoAndTubeColorCatalog"/>.</summary>
    extension(ChozoAndTubeColorCatalog self)
    {
        /// <summary>Resolves wrecked ship.</summary>
        internal Bgr555 ResolveWreckedShip(int color) => ((Bgr555)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color)))!);

        /// <summary>Resolves lower norfair.</summary>
        internal Bgr555 ResolveLowerNorfair(int color) => ((Bgr555)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="CreditsPresentation"/> members production does not use.</summary>
internal static class CreditsPresentationAccess
{
    /// <summary>Provides verification helpers for <see cref="CreditsPresentation"/>.</summary>
    extension(CreditsPresentation)
    {
        /// <summary>From compiled rows for verification.</summary>
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
    /// <summary>Provides verification helpers for <see cref="CrocomireBodyFrameSequence"/>.</summary>
    extension(CrocomireBodyFrameSequence self)
    {
        /// <summary>To array.</summary>
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
    /// <summary>Provides verification helpers for <see cref="CrocomireColorCatalog"/>.</summary>
    extension(CrocomireColorCatalog self)
    {
        /// <summary>Resolves fight body.</summary>
        internal Bgr555 ResolveFightBody(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "FightBody"), (int)(color)))!);

        /// <summary>Resolves initial wall.</summary>
        internal Bgr555 ResolveInitialWall(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialWall"), (int)(color)))!);

        /// <summary>Resolves initial projectile.</summary>
        internal Bgr555 ResolveInitialProjectile(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialProjectile"), (int)(color)))!);

        /// <summary>Resolves skeleton arm.</summary>
        internal Bgr555 ResolveSkeletonArm(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "SkeletonArm"), (int)(color)))!);

        /// <summary>Resolves wall spikes.</summary>
        internal Bgr555 ResolveWallSpikes(int color) => ((Bgr555)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "WallSpikes"), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="EndingFontAtlas"/> members production does not use.</summary>
internal static class EndingFontAtlasAccess
{
    /// <summary>Provides verification helpers for <see cref="EndingFontAtlas"/>.</summary>
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
    /// <summary>Provides verification helpers for <see cref="EnemyBg2FrameDefinitionSequence"/>.</summary>
    extension(EnemyBg2FrameDefinitionSequence self)
    {
        /// <summary>To array.</summary>
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
    /// <summary>Provides verification helpers for <see cref="EnemySpritemapCatalog"/>.</summary>
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
    /// <summary>Provides verification helpers for <see cref="EnemyTileArtworkCatalog"/>.</summary>
    extension(EnemyTileArtworkCatalog)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileArtworkCatalog FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            (EnemyTileArtworkCatalog)PrivateState.Construct(typeof(EnemyTileArtworkCatalog), Ensure.NotNull(parts));
    }

    /// <summary>Provides verification helpers for <see cref="EnemyTileArtworkCatalog"/>.</summary>
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
    /// <summary>Provides verification helpers for <see cref="EscapeTimerTileAtlas"/>.</summary>
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
    /// <summary>Provides verification helpers for <see cref="GameplayBasePaletteCatalog"/>.</summary>
    extension(GameplayBasePaletteCatalog self)
    {
        /// <summary>Gets common sprites.</summary>
        internal ReadOnlySpan<ushort> CommonSprites => PrivateState.Field<ushort[]>(self, "commonSprites");
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    /// <summary>Provides verification helpers for <see cref="GoldenTorizoStrideGeometryDefinitions"/>.</summary>
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        /// <summary>Native frame identity.</summary>
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
    /// <summary>Provides verification helpers for <see cref="MapArrowVisual"/>.</summary>
    extension(MapArrowVisual self)
    {
        /// <summary>Gets stored duration count.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, byte>?>(self, "durationOverrides")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MapMarkerTileArtwork"/>.</summary>
    extension(MapMarkerTileArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MapObjectTileArtwork"/>.</summary>
    extension(MapObjectTileArtwork self)
    {
        /// <summary>Gets stored highlight pixel count.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "highlightEdits")?.Count ?? 0;

        /// <summary>Gets stored reserve pixel count.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "reserveEdits")?.Count ?? 0;

        /// <summary>Gets stored other byte count.</summary>
        internal int StoredOtherByteCount => PrivateState.Field<byte[]>(self, "otherCharacters").Length;
    }
}

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    /// <summary>Provides verification helpers for <see cref="MapSaveMarkerLayout"/>.</summary>
    extension(MapSaveMarkerLayout self)
    {
        /// <summary>Gets stored coordinate component count.</summary>
        internal int StoredCoordinateComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "coordinateOverrides").Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    /// <summary>World label.</summary>
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, MapSpriteId id) =>
        (WorldMapLabelComposition?)PrivateState.Invoke(catalog, "GetWorldLabel", id);

    /// <summary>Provides verification helpers for <see cref="MapSpriteCatalog"/>.</summary>
    extension(MapSpriteCatalog self)
    {
        /// <summary>Stores composition.</summary>
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

        /// <summary>Gets stored highlight pixel count.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredHighlightPixelCount;

        /// <summary>Gets stored reserve pixel count.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredReservePixelCount;

        /// <summary>Gets stored artwork byte count.</summary>
        internal int StoredArtworkByteCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredOtherByteCount;
    }
}

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MenuBeveledSquareArtwork"/>.</summary>
    extension(MenuBeveledSquareArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MenuCompactLetteringArtwork"/>.</summary>
    extension(MenuCompactLetteringArtwork self)
    {
        /// <summary>Gets stored ink byte count.</summary>
        internal int StoredInkByteCount => PrivateState.Field<Dictionary<char, uint>>(self, "glyphs").Count * sizeof(uint);

        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks whether the selected tile pixel has an authored override.</summary>
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
    /// <summary>Provides verification helpers for <see cref="MenuLargeFontArtwork"/>.</summary>
    extension(MenuLargeFontArtwork self)
    {
        /// <summary>Gets stored face byte count.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks whether the selected tile pixel has an authored override.</summary>
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
    /// <summary>Provides verification helpers for <see cref="MenuPanelTileArtwork"/>.</summary>
    extension(MenuPanelTileArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MenuShoulderButtonArtwork"/>.</summary>
    extension(MenuShoulderButtonArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MenuShoulderHighlightArtwork"/>.</summary>
    extension(MenuShoulderHighlightArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    /// <summary>Provides verification helpers for <see cref="MenuSmallFontArtwork"/>.</summary>
    extension(MenuSmallFontArtwork self)
    {
        /// <summary>Gets stored face byte count.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Checks whether the selected tile pixel has an authored override.</summary>
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
    /// <summary>Provides verification helpers for <see cref="MenuThinBorderArtwork"/>.</summary>
    extension(MenuThinBorderArtwork self)
    {
        /// <summary>Gets stored edit count.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    /// <summary>Provides verification helpers for <see cref="MotherBrainBodyVisualDefinitions"/>.</summary>
    extension(MotherBrainBodyVisualDefinitions)
    {
        /// <summary>Gets frames.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyVisualDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitions.Frame)];
    }
}

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="NorfairRidleyColorCatalog"/>.</summary>
    extension(NorfairRidleyColorCatalog self)
    {
        /// <summary>Resolves initial.</summary>
        internal Bgr555 ResolveInitial(int color) => PrivateState.Field<NorfairRidleyInitialPaintDefinitions>(self, "initial").ColorAt(color);

        /// <summary>Resolves reveal.</summary>
        internal Bgr555 ResolveReveal(int row, int color) => PrivateState.Field<NorfairRidleyRevealPaintDefinitions>(self, "reveal").ColorAt(row, color);
    }
}

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    /// <summary>Provides verification helpers for <see cref="PauseReserveTankPresentation"/>.</summary>
    extension(PauseReserveTankPresentation self)
    {
        /// <summary>Gets stored frame count.</summary>
        internal int StoredFrameCount => (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Full") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "EndCap") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Empty") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill1") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill2") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill3") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill4") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill5") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill6") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill7") is null ? 0 : 1);

        /// <summary>Gets stored anchor component count.</summary>
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

    /// <summary>Provides verification helpers for <see cref="PauseSelectorPresentation"/>.</summary>
    extension(PauseSelectorPresentation self)
    {
        /// <summary>Gets stored duration count.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, int>?>(self, "durationOverrides")?.Count ?? 0;

        /// <summary>Reports whether any category stores composition parts.</summary>
        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(PrivateState.Field<PauseSelectorVisual>(category, "basis")) ||
            (PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Values.Any(StoresParts) ?? false));

        /// <summary>Gets stored composition override count.</summary>
        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Count ?? 0);

        /// <summary>Gets stored anchor component count.</summary>
        internal int StoredAnchorComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "anchorOverrides").Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    /// <summary>Provides verification helpers for <see cref="RoomFxBlendColors"/>.</summary>
    extension(RoomFxBlendColors self)
    {
        // The generated array is the requested output, never a retained stock-color cache.
        /// <summary>Creates colors.</summary>
        internal Bgr555[] CreateColors() => [PrivateState.Field<RoomFxPairColor>(self, "primary").CreateColor(), PrivateState.Field<RoomFxPairColor>(self, "secondary").CreateColor(), PrivateState.Field<RoomFxThirdColor?>(self, "thirdOverride")?.CreateColor() ?? Bgr555.Black];
    }
}

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="RoomFxPaletteBlendCatalog"/>.</summary>
    extension(RoomFxPaletteBlendCatalog self)
    {
        /// <summary>Resolves.</summary>
        internal ReadOnlySpan<Bgr555> Resolve(RoomFxPaletteBlend selection) => ((RoomFxBlendColors)(PrivateState.Invoke(self, "SelectColors", selection))!).CreateColors();
    }
}

/// <summary>Verification access to <see cref="SamusBodyTileDefinition"/> members production does not use.</summary>
internal static class SamusBodyTileDefinitionAccess
{
    /// <summary>Provides verification helpers for <see cref="SamusBodyTileDefinition"/>.</summary>
    extension(SamusBodyTileDefinition self)
    {
        /// <summary>With transfer geometry.</summary>
        internal SamusBodyTileDefinition WithTransferGeometry(SamusBodyArtworkCatalog body, bool upper, int set, int position) =>
            self.WithTransferGeometry(body, upper, set, position, body.PosePointers, body.Frames);
    }
}

/// <summary>Verification access to <see cref="SamusBodyTransferDefinitions"/> members production does not use.</summary>
internal static class SamusBodyTransferDefinitionsAccess
{
    /// <summary>Provides verification helpers for <see cref="SamusBodyTransferDefinitions"/>.</summary>
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
    /// <summary>Provides verification helpers for <see cref="SamusChargeColorCatalog"/>.</summary>
    extension(SamusChargeColorCatalog self)
    {
        /// <summary>Resolves charge.</summary>
        internal Bgr555 ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            ((Bgr555)(PrivateState.Invoke((pseudo ? PrivateState.Field<object>(self, "pseudoScrew") : PrivateState.Field<object>(self, "chargedBeam")), "Resolve", (int)(suit), (int)(phase), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="SamusSpritemapArtworkCatalog"/>.</summary>
    extension(SamusSpritemapArtworkCatalog self)
    {
        /// <summary>Gets definitions.</summary>
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => PrivateState.Field<Dictionary<ushort, SamusSpritemapDefinition>>(self, "definitions").Values;
    }
}

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
    /// <summary>Provides verification helpers for <see cref="SelectedPresentationHash"/>.</summary>
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

    /// <summary>Provides verification helpers for <see cref="SelectedPresentationHash"/>.</summary>
    extension(SelectedPresentationHash)
    {
        /// <summary>From word frames.</summary>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>From word frames.</summary>
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
    /// <summary>Provides verification helpers for <see cref="TitlePalettePresentation"/>.</summary>
    extension(TitlePalettePresentation self)
    {
        /// <summary>Gets colors.</summary>
        internal ReadOnlySpan<Bgr555> Colors => PrivateState.Field<Bgr555[]>(self, "colors");
    }
}

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    /// <summary>Provides verification helpers for <see cref="TourianStatueColorCatalog"/>.</summary>
    extension(TourianStatueColorCatalog self)
    {
        /// <summary>Resolves statue.</summary>
        internal Bgr555 ResolveStatue(int color) => ((Bgr555)(PrivateState.Invoke(PrivateState.Field<object>(self, "statueColors"), "Read", (int)(color)))!);
    }
}
