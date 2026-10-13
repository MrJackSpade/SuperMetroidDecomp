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
    extension(BeamTileAtlas self)
    {
        [PpuLayoutBoundary("codec: beam tile transfer at its native VRAM word")]
        internal void LoadTo(SnesVram vram) =>
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, self.Transfer.Span);
    }
}

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    extension(BeamTileCatalog)
    {
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
            return new BeamTileCatalog(sheets, palettes, hyperBeamFxColors);
        }
    }
}

/// <summary>Verification access to <see cref="CeresEscapeOverlayTilemapDefinitions"/> members production does not use.</summary>
internal static class CeresEscapeOverlayTilemapDefinitionsAccess
{
    extension(CeresEscapeOverlayTilemapDefinitions)
    {
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
    extension(CeresRidleyColorCatalog self)
    {
        internal Bgr555 ResolveStart(int color) => self.start.Resolve(color);

        internal Bgr555 ResolveEyeFade(int row, int color) => self.eyeFade.Resolve(row, color);

        internal Bgr555 ResolveBodyFade(int row, int color) => self.bodyFade.Resolve(row, color);

        internal Bgr555 ResolveHealth(int row, int color) => self.health.Resolve(row, color);

        internal Bgr555 ResolveAlarm(int row, int color) => self.alarm.Resolve(row, color);

        internal Bgr555 ResolveBaby(int row, int color) => self.baby.Resolve(row, color);
    }
}

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    extension(ChozoAndTubeColorCatalog self)
    {
        internal Bgr555 ResolveWreckedShip(int color) => ((Bgr555)(self.ResolveStatue((ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color)))!);

        internal Bgr555 ResolveLowerNorfair(int color) => ((Bgr555)(self.ResolveStatue((ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="CreditsPresentation"/> members production does not use.</summary>
internal static class CreditsPresentationAccess
{
    extension(CreditsPresentation)
    {
        internal static CreditsPresentation FromCompiledRowsForVerification(
            params ushort[][] rows)
        {
            if (rows.Length == 0 || rows.Any(row =>
                row.Length != CreditsPresentationDefinitions.TilemapWidth))
            {
                throw new ArgumentException(
                    "Verification credits rows must be nonempty 32-word rows.", "rows");
            }
            return new CreditsPresentation(null, rows.Select(row => row.ToArray()).ToArray(), "VERIFICATION");
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireBodyFrameSequence"/> members production does not use.</summary>
internal static class CrocomireBodyFrameSequenceAccess
{
    extension(CrocomireBodyFrameSequence self)
    {
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
    extension(CrocomireColorCatalog self)
    {
        internal Bgr555 ResolveFightBody(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.FightBody, (int)(color)))!);

        internal Bgr555 ResolveInitialWall(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.InitialWall, (int)(color)))!);

        internal Bgr555 ResolveInitialProjectile(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.InitialProjectile, (int)(color)))!);

        internal Bgr555 ResolveSkeletonArm(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.SkeletonArm, (int)(color)))!);

        internal Bgr555 ResolveWallSpikes(int color) => ((Bgr555)(self.Get(CrocomireColorCatalog.Band.WallSpikes, (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="EndingFontAtlas"/> members production does not use.</summary>
internal static class EndingFontAtlasAccess
{
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
            return new EndingFontAtlas(pixels);
        }
    }
}

/// <summary>Verification access to <see cref="EnemyBg2FrameDefinitionSequence"/> members production does not use.</summary>
internal static class EnemyBg2FrameDefinitionSequenceAccess
{
    extension(EnemyBg2FrameDefinitionSequence self)
    {
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
    extension(EnemySpritemapCatalog self)
    {
        /// <summary>Returns a known installed frame; callers must reject missing artwork.</summary>
        internal bool TryGet(byte bank, ushort pointer, out EnemySpritemapParts parts)
        {
            if (self.frames.TryGetValue(((int)new SnesAddress(bank, pointer)), out EnemySpritemapParts? found))
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
    extension(EnemyTileArtworkCatalog)
    {
        /// <summary>Explicitly partial constructed artwork; never an installed production catalog.</summary>
        internal static EnemyTileArtworkCatalog FromArtworkForVerification(EnemyTileArtworkParts parts) =>
            new EnemyTileArtworkCatalog(Ensure.NotNull(parts));
    }
}

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Queues both native records in their original order and at their original destinations.</summary>
        [PpuLayoutBoundary("codec: escape timer queued DMA records at their native VRAM words")]
        internal void QueueTo(VramWriteQueue queue)
        {
            Ensure.NotNull(queue);
            if (((byte[])(self.Transfer())!).Length != EscapeTimerTileAtlasFormat.TotalByteCount)
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
    extension(GameplayBasePaletteCatalog self)
    {
        internal ReadOnlySpan<Bgr555> CommonSprites => self.commonSprites;
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        internal static int NativeFrameIdentity(int phase)
        {
            if ((uint)phase >= 10) throw new ArgumentOutOfRangeException(nameof(phase));
            return GoldenTorizoStrideGeometryDefinitions.FirstFrame + GoldenTorizoStrideGeometryDefinitions.FrameBytes * phase;
        }
    }
}

/// <summary>Verification access to <see cref="MapArrowVisual"/> members production does not use.</summary>
internal static class MapArrowVisualAccess
{
    extension(MapArrowVisual self)
    {
        internal int StoredDurationCount => self.durationOverrides?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    extension(MapMarkerTileArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    extension(MapObjectTileArtwork self)
    {
        internal int StoredHighlightPixelCount => self.highlightEdits?.Count ?? 0;

        internal int StoredReservePixelCount => self.reserveEdits?.Count ?? 0;

        internal int StoredOtherByteCount => self.otherCharacters.Length;
    }
}

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    extension(MapSaveMarkerLayout self)
    {
        internal int StoredCoordinateComponentCount => self.coordinateOverrides.Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, MapSpriteId id) =>
        catalog.GetWorldLabel(id);

    extension(MapSpriteCatalog self)
    {
        internal bool StoresComposition(MapSpriteId id) => WorldLabel(self, id) is { } label
            ? label.authored is not null
            : self.GetFrame(id) is not null;

        /// <summary>Stored horizontal advances: the label origin(s) plus each authored letter advance.</summary>
        internal int StoredLabelHorizontalCount(MapSpriteId id)
        {
            if (WorldLabel(self, id) is not { } label || label.authored is not null)
                return 0;
            int origins = label.identity == MapSpriteId.WorldWreckedShip ? 2 : 1;
            return origins + (label.letterAdvances?.Count ?? 0);
        }

        internal int StoredHighlightPixelCount => self.characters.StoredHighlightPixelCount;

        internal int StoredReservePixelCount => self.characters.StoredReservePixelCount;

        internal int StoredArtworkByteCount => self.characters.StoredOtherByteCount;
    }
}

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    extension(MenuBeveledSquareArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    extension(MenuCompactLetteringArtwork self)
    {
        internal int StoredInkByteCount => self.glyphs.Count * sizeof(uint);

        internal int StoredEditCount => self.edits?.Count ?? 0;

        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuCompactLetteringArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuLargeFontArtwork"/> members production does not use.</summary>
internal static class MenuLargeFontArtworkAccess
{
    extension(MenuLargeFontArtwork self)
    {
        internal int StoredFaceByteCount => self.faces.Length;

        internal int StoredEditCount => self.edits?.Count ?? 0;

        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuLargeFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey(tile * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuPanelTileArtwork"/> members production does not use.</summary>
internal static class MenuPanelTileArtworkAccess
{
    extension(MenuPanelTileArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    extension(MenuShoulderButtonArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    extension(MenuShoulderHighlightArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    extension(MenuSmallFontArtwork self)
    {
        internal int StoredFaceByteCount => self.faces.Length;

        internal int StoredEditCount => self.edits?.Count ?? 0;

        internal bool HasPixelOverride(int tile, Position16<CharacterPixels> pixel)
        {
            if (!MenuSmallFontArtwork.Contains(tile) || pixel.X >= 8 || pixel.Y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return self.edits is { } edits && edits.ContainsKey((tile - MenuSmallFontArtwork.FirstTile) * 64 + pixel.Y * 8 + pixel.X);
        }
    }
}

/// <summary>Verification access to <see cref="MenuThinBorderArtwork"/> members production does not use.</summary>
internal static class MenuThinBorderArtworkAccess
{
    extension(MenuThinBorderArtwork self)
    {
        internal int StoredEditCount => self.edits?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    extension(MotherBrainBodyVisualDefinitions)
    {
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyVisualDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitions.Frame)];
    }
}

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    extension(NorfairRidleyColorCatalog self)
    {
        internal Bgr555 ResolveInitial(int color) => self.initial.ColorAt(color);

        internal Bgr555 ResolveReveal(int row, int color) => self.reveal.ColorAt(row, color);
    }
}

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    extension(PauseReserveTankPresentation self)
    {
        internal int StoredFrameCount => (self.frames.Full is null ? 0 : 1) + (self.frames.EndCap is null ? 0 : 1) + (self.frames.Empty is null ? 0 : 1) + (self.frames.Fill1 is null ? 0 : 1) + (self.frames.Fill2 is null ? 0 : 1) + (self.frames.Fill3 is null ? 0 : 1) + (self.frames.Fill4 is null ? 0 : 1) + (self.frames.Fill5 is null ? 0 : 1) + (self.frames.Fill6 is null ? 0 : 1) + (self.frames.Fill7 is null ? 0 : 1);

        internal int StoredAnchorComponentCount => self.anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="PauseSelectorPresentation"/> members production does not use.</summary>
internal static class PauseSelectorPresentationAccess
{
    /// <summary>Whether a selector visual stores authored parts rather than its calculated stock composition.</summary>
    private static bool StoresParts(PauseSelectorVisual visual) => visual.authored is not null;

    /// <summary>The reserve, beam and equipment phase compositions.</summary>
    private static PauseSelectorPresentation.PhaseComposition[] Categories(PauseSelectorPresentation presentation) =>
        [presentation.reserve, presentation.beam, presentation.equipment];

    extension(PauseSelectorPresentation self)
    {
        internal int StoredDurationCount => self.durationOverrides?.Count ?? 0;

        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(category.basis) || (category.overrides?.Values.Any(StoresParts) ?? false));

        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            category.overrides?.Count ?? 0);

        internal int StoredAnchorComponentCount => self.anchorOverrides.Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    extension(RoomFxBlendColors self)
    {
        // The generated array is the requested output, never a retained stock-color cache.
        internal Bgr555[] CreateColors() => [self.primary.CreateColor(), self.secondary.CreateColor(), self.thirdOverride?.CreateColor() ?? Bgr555.Black];
    }
}

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    extension(RoomFxPaletteBlendCatalog self)
    {
        internal ReadOnlySpan<Bgr555> Resolve(RoomFxPaletteBlend selection) => ((RoomFxBlendColors)(self.SelectColors(selection))!).CreateColors();
    }
}

/// <summary>Verification access to <see cref="SamusBodyTileDefinition"/> members production does not use.</summary>
internal static class SamusBodyTileDefinitionAccess
{
    extension(SamusBodyTileDefinition self)
    {
        internal SamusBodyTileDefinition WithTransferGeometry(SamusBodyArtworkCatalog body, bool upper, int set, int position) =>
            self.WithTransferGeometry(body, upper, set, position, body.PosePointers, body.Frames);
    }
}

/// <summary>Verification access to <see cref="SamusBodyTransferDefinitions"/> members production does not use.</summary>
internal static class SamusBodyTransferDefinitionsAccess
{
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
            if (SamusBodyTransferDefinitions.TrySelectedPacking(upper, set, position, payloadBytes, out firstSize))
                return true;
            return SamusBodyTransferDefinitions.TryFirstSize(body, new SamusBodyTileSlot(upper, set, position), payloadBytes, body.PosePointers, body.Frames, out firstSize);
        }
    }
}

/// <summary>Verification access to <see cref="SamusChargeColorCatalog"/> members production does not use.</summary>
internal static class SamusChargeColorCatalogAccess
{
    extension(SamusChargeColorCatalog self)
    {
        internal Bgr555 ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            ((Bgr555)((pseudo ? self.pseudoScrew : self.chargedBeam).Resolve((int)(suit), (int)(phase), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    extension(SamusSpritemapArtworkCatalog self)
    {
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => self.definitions.Values;
    }
}

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
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

    extension(SelectedPresentationHash)
    {
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

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
    extension(TitlePalettePresentation self)
    {
        internal ReadOnlySpan<Bgr555> Colors => self.colors;
    }
}

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    extension(TourianStatueColorCatalog self)
    {
        internal Bgr555 ResolveStatue(int color) => ((Bgr555)(self.statueColors.Read((int)(color)))!);
    }
}

/// <summary>Verification view of one stored loading-palette channel input.</summary>
internal static class LoadingPaletteChannelsAccess
{
    extension(LoadingPaletteInputView.Channels self)
    {
        /// <summary>The stored red (0), green (1) or blue (2) input; null when that channel is calculated.</summary>
        internal int? Component(int channel) => channel switch
        {
            0 => self.red,
            1 => self.green,
            2 => self.blue,
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "A palette channel is 0..2."),
        };
    }
}
