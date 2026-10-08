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
            return ((BeamTileCatalog)PrivateState.Construct(typeof(BeamTileCatalog), (BeamTileAtlas[])(sheets), (BeamPaletteCatalog?)(palettes), (HyperBeamFxColorCatalog?)(hyperBeamFxColors)));
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
        internal ushort ResolveStart(int color) => PrivateState.Field<CeresRidleyStartColorDefinitions>(self, "start").Resolve(color);

        internal ushort ResolveEyeFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "eyeFade").Resolve(row, color);

        internal ushort ResolveBodyFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "bodyFade").Resolve(row, color);

        internal ushort ResolveHealth(int row, int color) => PrivateState.Field<CeresRidleyHealthPaintDefinitions>(self, "health").Resolve(row, color);

        internal ushort ResolveAlarm(int row, int color) => PrivateState.Field<CeresRidleyAlarmColorDefinitions>(self, "alarm").Resolve(row, color);

        internal ushort ResolveBaby(int row, int color) => PrivateState.Field<CeresBabyPaintDefinitions>(self, "baby").Resolve(row, color);
    }
}

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    extension(ChozoAndTubeColorCatalog self)
    {
        internal ushort ResolveWreckedShip(int color) => ((ushort)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color)))!);

        internal ushort ResolveLowerNorfair(int color) => ((ushort)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color)))!);
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
            return ((CreditsPresentation)PrivateState.Construct(typeof(CreditsPresentation), (CreditsLineDocument[]?)(null), (ushort[][]?)(rows.Select(row => row.ToArray()).ToArray()), (string)("VERIFICATION")));
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
        internal ushort ResolveFightBody(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "FightBody"), (int)(color)))!);

        internal ushort ResolveInitialWall(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialWall"), (int)(color)))!);

        internal ushort ResolveInitialProjectile(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialProjectile"), (int)(color)))!);

        internal ushort ResolveSkeletonArm(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "SkeletonArm"), (int)(color)))!);

        internal ushort ResolveWallSpikes(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "WallSpikes"), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="CrocomireSkeletonFrameSequence"/> members production does not use.</summary>
internal static class CrocomireSkeletonFrameSequenceAccess
{
    extension(CrocomireSkeletonFrameSequence self)
    {
        internal IEnumerator<EnemyExtendedFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < self.Length; index++)
                yield return self[index];
        }
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
                EndingFontAtlasFormat.TilesPerRow, out _, out _);
            return ((EndingFontAtlas)PrivateState.Construct(typeof(EndingFontAtlas), (byte[])(pixels)));
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
            if (PrivateState.Field<Dictionary<int, EnemySpritemapParts>>(self, "frames").TryGetValue((bank << 16) | pointer, out EnemySpritemapParts? found))
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
        internal static EnemyTileArtworkCatalog FromArtworkForVerification(IReadOnlyDictionary<ushort, RoomCharacterAtlas> sheets,
            IReadOnlyDictionary<ushort, EnemyPaletteSheet> palettes,
            CrocomireMeltingArtwork? crocomireMelting = null,
            EnemySpritemapCatalog? spritemaps = null,
            EnemyExtendedFrameCatalog? extendedFrames = null,
            KraidBackgroundArtwork? kraidBackground = null,
            KraidColorCatalog? kraidColors = null,
            GunshipLiftoffArtworkCatalog? gunshipLiftoff = null,
            CeresDoorVisualCatalog? ceresDoorVisual = null,
            IReadOnlyDictionary<ushort, int>? dmaSources = null,
            EnemyProjectileSpritemapCatalog? projectileSpritemaps = null,
            MagdollitePaletteCycle? magdollitePaletteCycle = null,
            WorkRobotPaletteCycle? workRobotPaletteCycle = null,
            CrocomireColorCatalog? crocomireColors = null,
            DraygonColorCatalog? draygonColors = null,
            PhantoonColorCatalog? phantoonColors = null,
            ChozoAndTubeColorCatalog? chozoAndTubeColors = null,
            SporeSpawnColorCatalog? sporeSpawnColors = null,
            DachoraColorCatalog? dachoraColors = null,
            ShitroidColorCatalog? shitroidColors = null,
            BabyMetroidCutsceneColorCatalog? babyMetroidCutsceneColors = null,
            BotwoonColorCatalog? botwoonColors = null,
            MotherBrainDeathColorCatalog? motherBrainDeathColors = null,
            ZebetiteColorCatalog? zebetiteColors = null,
            NorfairRidleyColorCatalog? norfairRidleyColors = null,
            TourianStatueColorCatalog? tourianStatueColors = null,
            PhantoonBg2FrameCatalog? phantoonBg2Frames = null,
            DraygonBg2FrameCatalog? draygonBg2Frames = null,
            RoomCharacterAtlas? motherBrainCorpse = null,
            RoomCharacterAtlas? motherBrainEscapeText = null,
            MotherBrainSpecialSpriteArtworkCatalog? motherBrainSpecialSprites = null,
            CrocomireSkeletonArtwork? crocomireSkeleton = null,
            CrocomireBg2FrameCatalog? crocomireBg2Frames = null,
            TorizoInstructionVramArtwork? torizoInstructionVram = null,
            CeresEscapeTileArtwork? ceresEscapeTiles = null,
            CeresEscapeOverlayTilemapCatalog? ceresEscapeOverlayTilemaps = null,
            EnemyAuxiliaryColorCatalog? auxiliaryColors = null,
            MotherBrainBodyBg2FrameCatalog? motherBrainBodyBg2Frames = null) =>
            ((EnemyTileArtworkCatalog)PrivateState.Construct(typeof(EnemyTileArtworkCatalog), (IReadOnlyDictionary<ushort, RoomCharacterAtlas>)(sheets), (IReadOnlyDictionary<ushort, EnemyPaletteSheet>)(palettes), (CrocomireMeltingArtwork?)(crocomireMelting), (EnemySpritemapCatalog?)(spritemaps), (EnemyExtendedFrameCatalog?)(extendedFrames), (KraidBackgroundArtwork?)(kraidBackground), (KraidColorCatalog?)(kraidColors), (GunshipLiftoffArtworkCatalog?)(gunshipLiftoff), (CeresDoorVisualCatalog?)(ceresDoorVisual), (IReadOnlyDictionary<ushort, int>?)(dmaSources), (EnemyProjectileSpritemapCatalog?)(projectileSpritemaps), (MagdollitePaletteCycle?)(magdollitePaletteCycle), (WorkRobotPaletteCycle?)(workRobotPaletteCycle), (CrocomireColorCatalog?)(crocomireColors), (DraygonColorCatalog?)(draygonColors), (PhantoonColorCatalog?)(phantoonColors), (ChozoAndTubeColorCatalog?)(chozoAndTubeColors), (SporeSpawnColorCatalog?)(sporeSpawnColors), (DachoraColorCatalog?)(dachoraColors), (ShitroidColorCatalog?)(shitroidColors), (BabyMetroidCutsceneColorCatalog?)(babyMetroidCutsceneColors), (BotwoonColorCatalog?)(botwoonColors), (MotherBrainDeathColorCatalog?)(motherBrainDeathColors), (ZebetiteColorCatalog?)(zebetiteColors), (NorfairRidleyColorCatalog?)(norfairRidleyColors), (TourianStatueColorCatalog?)(tourianStatueColors), (PhantoonBg2FrameCatalog?)(phantoonBg2Frames), (DraygonBg2FrameCatalog?)(draygonBg2Frames), (RoomCharacterAtlas?)(motherBrainCorpse), (RoomCharacterAtlas?)(motherBrainEscapeText), (MotherBrainSpecialSpriteArtworkCatalog?)(motherBrainSpecialSprites), (CrocomireSkeletonArtwork?)(crocomireSkeleton), (CrocomireBg2FrameCatalog?)(crocomireBg2Frames), (TorizoInstructionVramArtwork?)(torizoInstructionVram), (CeresEscapeTileArtwork?)(ceresEscapeTiles), (CeresEscapeOverlayTilemapCatalog?)(ceresEscapeOverlayTilemaps), (EnemyAuxiliaryColorCatalog?)(auxiliaryColors), (MotherBrainBodyBg2FrameCatalog?)(motherBrainBodyBg2Frames)));
    }

    extension(EnemyTileArtworkCatalog self)
    {
        /// <summary>
        /// This installation with its compositions replaced, as the installation loader produces for a
        /// composition override. The other parts are independent and shared; nothing is derived from
        /// the compositions at construction.
        /// </summary>
        internal EnemyTileArtworkCatalog WithSpritemaps(EnemySpritemapCatalog spritemaps)
        {
            ArgumentNullException.ThrowIfNull(spritemaps);
            var copy = (EnemyTileArtworkCatalog)((object)(PrivateState.Invoke(self, "MemberwiseClone"))!);
            PrivateState.SetProperty(copy, "Spritemaps", spritemaps);
            return copy;
        }

        /// <summary>This installation with its extended frames replaced; see <see cref="WithSpritemaps"/>.</summary>
        internal EnemyTileArtworkCatalog WithExtendedFrames(EnemyExtendedFrameCatalog extendedFrames)
        {
            ArgumentNullException.ThrowIfNull(extendedFrames);
            var copy = (EnemyTileArtworkCatalog)((object)(PrivateState.Invoke(self, "MemberwiseClone"))!);
            PrivateState.SetProperty(copy, "ExtendedFrames", extendedFrames);
            return copy;
        }
    }
}

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Queues both native records in their original order and at their original destinations.</summary>
        internal void QueueTo(VramWriteQueue queue)
        {
            ArgumentNullException.ThrowIfNull(queue);
            if (((byte[])(PrivateState.Invoke(self, "Transfer"))!).Length != EscapeTimerTileAtlasFormat.TotalByteCount)
                throw new InvalidDataException("Escape timer artwork no longer matches its native transfer pages.");
            queue.EnqueueAsset(VramAssetId.EscapeTimerFirstTiles,
                EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.FirstDestinationWord);
            queue.EnqueueAsset(VramAssetId.EscapeTimerSecondTiles,
                EscapeTimerTileAtlasFormat.SecondByteCount, EscapeTimerTileAtlasFormat.SecondDestinationWord);
        }
    }
}

/// <summary>Verification access to <see cref="GameplayBasePaletteCatalog"/> members production does not use.</summary>
internal static class GameplayBasePaletteCatalogAccess
{
    extension(GameplayBasePaletteCatalog self)
    {
        internal ReadOnlySpan<ushort> CommonSprites => PrivateState.Field<ushort[]>(self, "commonSprites");
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
            return PrivateState.StaticField<int>(typeof(GoldenTorizoStrideGeometryDefinitions), "FirstFrame") + PrivateState.StaticField<int>(typeof(GoldenTorizoStrideGeometryDefinitions), "FrameBytes") * phase;
        }
    }
}

/// <summary>Verification access to <see cref="KraidFootFrameSequence"/> members production does not use.</summary>
internal static class KraidFootFrameSequenceAccess
{
    extension(KraidFootFrameSequence self)
    {
        internal IEnumerator<EnemyExtendedFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < self.Length; index++)
                yield return self[index];
        }
    }
}

/// <summary>Verification access to <see cref="MapArrowVisual"/> members production does not use.</summary>
internal static class MapArrowVisualAccess
{
    extension(MapArrowVisual self)
    {
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, byte>?>(self, "durationOverrides")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    extension(MapMarkerTileArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    extension(MapObjectTileArtwork self)
    {
        internal int StoredHighlightPixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "highlightEdits")?.Count ?? 0;

        internal int StoredReservePixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "reserveEdits")?.Count ?? 0;

        internal int StoredOtherByteCount => PrivateState.Field<byte[]>(self, "otherCharacters").Length;
    }
}

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    extension(MapSaveMarkerLayout self)
    {
        internal int StoredCoordinateComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "coordinateOverrides").Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, ushort id) =>
        (WorldMapLabelComposition?)PrivateState.Invoke(catalog, "GetWorldLabel", id);

    extension(MapSpriteCatalog self)
    {
        internal bool StoresComposition(ushort id) => WorldLabel(self, id) is { } label
            ? PrivateState.Field<SpriteComposition?>(label, "authored") is not null
            : PrivateState.Invoke(self, "GetFrame", id) is not null;

        /// <summary>Stored horizontal advances: the label origin(s) plus each authored letter advance.</summary>
        internal int StoredLabelHorizontalCount(ushort id)
        {
            if (WorldLabel(self, id) is not { } label || PrivateState.Field<SpriteComposition?>(label, "authored") is not null)
                return 0;
            int origins = PrivateState.Field<ushort>(label, "identity") == MapSpriteDefinitions.WorldWreckedShip ? 2 : 1;
            return origins + (PrivateState.Field<Dictionary<int, int>?>(label, "letterAdvances")?.Count ?? 0);
        }

        internal int StoredHighlightPixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredHighlightPixelCount;

        internal int StoredReservePixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredReservePixelCount;

        internal int StoredArtworkByteCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredOtherByteCount;
    }
}

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    extension(MenuBeveledSquareArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    extension(MenuCompactLetteringArtwork self)
    {
        internal int StoredInkByteCount => PrivateState.Field<Dictionary<char, uint>>(self, "glyphs").Count * sizeof(uint);

        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        internal bool HasPixelOverride(int tile, int x, int y)
        {
            if (!MenuCompactLetteringArtwork.Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey(tile * 64 + y * 8 + x);
        }
    }
}

/// <summary>Verification access to <see cref="MenuLargeFontArtwork"/> members production does not use.</summary>
internal static class MenuLargeFontArtworkAccess
{
    extension(MenuLargeFontArtwork self)
    {
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        internal bool HasPixelOverride(int tile, int x, int y)
        {
            if (!MenuLargeFontArtwork.Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey(tile * 64 + y * 8 + x);
        }
    }
}

/// <summary>Verification access to <see cref="MenuPanelTileArtwork"/> members production does not use.</summary>
internal static class MenuPanelTileArtworkAccess
{
    extension(MenuPanelTileArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    extension(MenuShoulderButtonArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    extension(MenuShoulderHighlightArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    extension(MenuSmallFontArtwork self)
    {
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        internal bool HasPixelOverride(int tile, int x, int y)
        {
            if (!MenuSmallFontArtwork.Contains(tile) || (uint)x >= 8 || (uint)y >= 8) throw new ArgumentOutOfRangeException(nameof(tile));
            return PrivateState.Field<Dictionary<int, byte>?>(self, "edits") is { } edits && edits.ContainsKey((tile - MenuSmallFontArtwork.FirstTile) * 64 + y * 8 + x);
        }
    }
}

/// <summary>Verification access to <see cref="MenuThinBorderArtwork"/> members production does not use.</summary>
internal static class MenuThinBorderArtworkAccess
{
    extension(MenuThinBorderArtwork self)
    {
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
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
        internal ushort ResolveInitial(int color) => PrivateState.Field<NorfairRidleyInitialPaintDefinitions>(self, "initial").ColorAt(color);

        internal ushort ResolveReveal(int row, int color) => PrivateState.Field<NorfairRidleyRevealPaintDefinitions>(self, "reveal").ColorAt(row, color);
    }
}

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    extension(PauseReserveTankPresentation self)
    {
        internal int StoredFrameCount => (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Full") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "EndCap") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Empty") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill1") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill2") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill3") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill4") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill5") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill6") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill7") is null ? 0 : 1);

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

    extension(PauseSelectorPresentation self)
    {
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, int>?>(self, "durationOverrides")?.Count ?? 0;

        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(PrivateState.Field<PauseSelectorVisual>(category, "basis")) ||
            (PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Values.Any(StoresParts) ?? false));

        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Count ?? 0);

        internal int StoredAnchorComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "anchorOverrides").Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    extension(RoomFxBlendColors self)
    {
        // The generated array is the requested output, never a retained stock-color cache.
        internal ushort[] CreateColors() => [PrivateState.Field<RoomFxPairColor>(self, "primary").CreateColor(), PrivateState.Field<RoomFxPairColor>(self, "secondary").CreateColor(), PrivateState.Field<RoomFxThirdColor?>(self, "thirdOverride")?.CreateColor() ?? 0];
    }
}

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    extension(RoomFxPaletteBlendCatalog self)
    {
        internal ReadOnlySpan<ushort> Resolve(byte selection) => ((RoomFxBlendColors)(PrivateState.Invoke(self, "SelectColors", (byte)(selection)))!).CreateColors();
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
            object?[] packing = [upper, set, position, payloadBytes, null];
            if ((bool)PrivateState.InvokeStaticWithOut(typeof(SamusBodyTransferDefinitions), "TrySelectedPacking", packing)!)
            {
                firstSize = (ushort)packing[4]!;
                return true;
            }
            return SamusBodyTransferDefinitions.TryFirstSize(body, upper, set, position, payloadBytes, body.PosePointers, body.Frames, out firstSize);
        }
    }
}

/// <summary>Verification access to <see cref="SamusChargeColorCatalog"/> members production does not use.</summary>
internal static class SamusChargeColorCatalogAccess
{
    extension(SamusChargeColorCatalog self)
    {
        internal ushort ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            ((ushort)(PrivateState.Invoke((pseudo ? PrivateState.Field<object>(self, "pseudoScrew") : PrivateState.Field<object>(self, "chargedBeam")), "Resolve", (int)(suit), (int)(phase), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    extension(SamusSpritemapArtworkCatalog self)
    {
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => PrivateState.Field<Dictionary<ushort, SamusSpritemapDefinition>>(self, "definitions").Values;
    }
}

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
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
        internal ReadOnlySpan<ushort> Colors => PrivateState.Field<ushort[]>(self, "colors");
    }
}

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    extension(TourianStatueColorCatalog self)
    {
        internal ushort ResolveStatue(int color) => ((ushort)(PrivateState.Invoke(PrivateState.Field<object>(self, "statueColors"), "Read", (int)(color)))!);
    }
}
