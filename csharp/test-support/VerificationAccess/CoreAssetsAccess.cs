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
    /// <summary>Supplies VRAM loading access for focused atlas checks.</summary>
    extension(BeamTileAtlas self)
    {
        /// <summary>Loads the atlas transfer bytes into their native VRAM destination.</summary>
        /// <param name="vram">Video memory that receives the beam tiles.</param>
        internal void LoadTo(SnesVram vram) =>
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, self.Transfer.Span);
    }
}

/// <summary>Verification access to <see cref="BeamTileCatalog"/> members production does not use.</summary>
internal static class BeamTileCatalogAccess
{
    /// <summary>Creates beam catalogs from test-provided artwork files.</summary>
    extension(BeamTileCatalog)
    {
        /// <summary>Loads every beam tile sheet and combines it with optional palette catalogs for verification.</summary>
        /// <param name="files">Artwork files keyed by their defined beam-sheet names.</param>
        /// <param name="palettes">Optional beam palette catalog to attach.</param>
        /// <param name="hyperBeamFxColors">Optional Hyper Beam effect colors to attach.</param>
        /// <returns>The beam tile catalog constructed from the supplied verification assets.</returns>
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
    /// <summary>Exposes source range membership for tilemap definition checks.</summary>
    extension(CeresEscapeOverlayTilemapDefinitions)
    {
        /// <summary>Determines whether an address belongs to any Ceres escape overlay tilemap page.</summary>
        /// <param name="address">SNES byte address to test.</param>
        /// <returns><see langword="true"/> when a defined page contains the address.</returns>
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
    /// <summary>Exposes resolved Ridley palette bands for focused checks.</summary>
    extension(CeresRidleyColorCatalog self)
    {
        /// <summary>Resolves a color from Ceres Ridley's starting palette.</summary>
        /// <param name="color">Color index within the starting palette.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveStart(int color) => PrivateState.Field<CeresRidleyStartColorDefinitions>(self, "start").Resolve(color);

        /// <summary>Resolves a color from one row of Ceres Ridley's eye-fade palette.</summary>
        /// <param name="row">Eye-fade row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveEyeFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "eyeFade").Resolve(row, color);

        /// <summary>Resolves a color from one row of Ceres Ridley's body-fade palette.</summary>
        /// <param name="row">Body-fade row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveBodyFade(int row, int color) => PrivateState.Field<CeresRidleyFadeColorDefinitions>(self, "bodyFade").Resolve(row, color);

        /// <summary>Resolves a health-dependent Ceres Ridley paint color.</summary>
        /// <param name="row">Health-palette row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveHealth(int row, int color) => PrivateState.Field<CeresRidleyHealthPaintDefinitions>(self, "health").Resolve(row, color);

        /// <summary>Resolves a color from one row of the Ceres alarm palette.</summary>
        /// <param name="row">Alarm-palette row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveAlarm(int row, int color) => PrivateState.Field<CeresRidleyAlarmColorDefinitions>(self, "alarm").Resolve(row, color);

        /// <summary>Resolves a color from one row of the Ceres baby-paint palette.</summary>
        /// <param name="row">Baby-paint row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveBaby(int row, int color) => PrivateState.Field<CeresBabyPaintDefinitions>(self, "baby").Resolve(row, color);
    }
}

/// <summary>Verification access to <see cref="ChozoAndTubeColorCatalog"/> members production does not use.</summary>
internal static class ChozoAndTubeColorCatalogAccess
{
    /// <summary>Exposes statue palette colors for verification.</summary>
    extension(ChozoAndTubeColorCatalog self)
    {
        /// <summary>Resolves a Wrecked Ship Chozo statue palette color.</summary>
        /// <param name="color">Color index within the statue palette.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveWreckedShip(int color) => ((ushort)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.WreckedShip), (int)(color)))!);

        /// <summary>Resolves a Lower Norfair Chozo statue palette color.</summary>
        /// <param name="color">Color index within the statue palette.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveLowerNorfair(int color) => ((ushort)(PrivateState.Invoke(self, "ResolveStatue", (ChozoStatuePalette)(ChozoStatuePalette.LowerNorfair), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="CreditsPresentation"/> members production does not use.</summary>
internal static class CreditsPresentationAccess
{
    /// <summary>Constructs test presentations from precompiled credits rows.</summary>
    extension(CreditsPresentation)
    {
        /// <summary>Constructs a credits presentation from precompiled tilemap rows supplied by a verification fixture.</summary>
        /// <param name="rows">Nonempty 32-word credits rows in display order.</param>
        /// <returns>The verification-only credits presentation.</returns>
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
    /// <summary>Provides ordered array access for frame sequence assertions.</summary>
    extension(CrocomireBodyFrameSequence self)
    {
        /// <summary>Copies the Crocomire body-frame pointers into an array in sequence order.</summary>
        /// <returns>A new array containing every frame pointer.</returns>
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
    /// <summary>Exposes individual Crocomire palette bands for verification.</summary>
    extension(CrocomireColorCatalog self)
    {
        /// <summary>Resolves a color from Crocomire's fight-body palette band.</summary>
        /// <param name="color">Color index within the band.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveFightBody(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "FightBody"), (int)(color)))!);

        /// <summary>Resolves a color from Crocomire's initial wall palette band.</summary>
        /// <param name="color">Color index within the band.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveInitialWall(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialWall"), (int)(color)))!);

        /// <summary>Resolves a color from Crocomire's initial projectile palette band.</summary>
        /// <param name="color">Color index within the band.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveInitialProjectile(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "InitialProjectile"), (int)(color)))!);

        /// <summary>Resolves a color from Crocomire's skeleton-arm palette band.</summary>
        /// <param name="color">Color index within the band.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveSkeletonArm(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "SkeletonArm"), (int)(color)))!);

        /// <summary>Resolves a color from Crocomire's wall-spike palette band.</summary>
        /// <param name="color">Color index within the band.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveWallSpikes(int color) => ((ushort)(PrivateState.Invoke(self, "Get", PrivateState.StaticField<object>(PrivateState.Nested(typeof(CrocomireColorCatalog), "Band"), "WallSpikes"), (int)(color)))!);
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
    /// <summary>Exposes ordered array materialization for BG2 frame checks.</summary>
    extension(EnemyBg2FrameDefinitionSequence self)
    {
        /// <summary>Copies the BG2 frame definitions into an array in sequence order.</summary>
        /// <returns>A new array containing every frame definition.</returns>
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
    /// <summary>Provides access to base palette data for focused checks.</summary>
    extension(GameplayBasePaletteCatalog self)
    {
        /// <summary>Gets the common sprite colors stored by the gameplay base palette.</summary>
        internal ReadOnlySpan<ushort> CommonSprites => PrivateState.Field<ushort[]>(self, "commonSprites");
    }
}

/// <summary>Verification access to <see cref="GoldenTorizoStrideGeometryDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStrideGeometryDefinitionsAccess
{
    /// <summary>Exposes native frame identities for geometry definition checks.</summary>
    extension(GoldenTorizoStrideGeometryDefinitions)
    {
        /// <summary>Calculates the native frame identity for a Golden Torizo stride phase.</summary>
        /// <param name="phase">Zero-based stride phase.</param>
        /// <returns>The native byte identity for the phase's frame.</returns>
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
    /// <summary>Exposes explicit timing storage for map arrow checks.</summary>
    extension(MapArrowVisual self)
    {
        /// <summary>Gets the number of explicitly stored map-arrow duration overrides.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, byte>?>(self, "durationOverrides")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapMarkerTileArtwork"/> members production does not use.</summary>
internal static class MapMarkerTileArtworkAccess
{
    /// <summary>Exposes explicit marker artwork edit storage.</summary>
    extension(MapMarkerTileArtwork self)
    {
        /// <summary>Gets the number of explicitly stored marker-pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MapObjectTileArtwork"/> members production does not use.</summary>
internal static class MapObjectTileArtworkAccess
{
    /// <summary>Exposes pixel edit and payload storage for map objects.</summary>
    extension(MapObjectTileArtwork self)
    {
        /// <summary>Gets the number of stored highlighted-map-object pixel edits.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "highlightEdits")?.Count ?? 0;

        /// <summary>Gets the number of stored reserve-map-object pixel edits.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<Dictionary<int, byte>?>(self, "reserveEdits")?.Count ?? 0;

        /// <summary>Gets the byte count of the retained non-highlight, non-reserve character payload.</summary>
        internal int StoredOtherByteCount => PrivateState.Field<byte[]>(self, "otherCharacters").Length;
    }
}

/// <summary>Verification access to <see cref="MapSaveMarkerLayout"/> members production does not use.</summary>
internal static class MapSaveMarkerLayoutAccess
{
    /// <summary>Exposes coordinate override storage for map layout checks.</summary>
    extension(MapSaveMarkerLayout self)
    {
        /// <summary>Gets the number of individually stored X or Y coordinate overrides.</summary>
        internal int StoredCoordinateComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "coordinateOverrides").Values.Sum(point =>
            (point.X.HasValue ? 1 : 0) + (point.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="MapSpriteCatalog"/> members production does not use.</summary>
internal static class MapSpriteCatalogAccess
{
    /// <summary>Gets the world-map label associated with a native map-sprite identifier.</summary>
    /// <param name="catalog">Map sprite catalog to inspect.</param>
    /// <param name="id">Native map-sprite identifier.</param>
    /// <returns>The matching world-map label, or <see langword="null"/> when the identifier is not a label.</returns>
    private static WorldMapLabelComposition? WorldLabel(MapSpriteCatalog catalog, ushort id) =>
        (WorldMapLabelComposition?)PrivateState.Invoke(catalog, "GetWorldLabel", id);

    /// <summary>Exposes composition and storage details for map sprite checks.</summary>
    extension(MapSpriteCatalog self)
    {
        /// <summary>Determines whether a sprite or world label stores an authored composition.</summary>
        /// <param name="id">Native map-sprite identifier to inspect.</param>
        /// <returns><see langword="true"/> when an authored composition is stored.</returns>
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

        /// <summary>Gets the number of highlighted character-pixel edits stored by the catalog.</summary>
        internal int StoredHighlightPixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredHighlightPixelCount;

        /// <summary>Gets the number of reserve character-pixel edits stored by the catalog.</summary>
        internal int StoredReservePixelCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredReservePixelCount;

        /// <summary>Gets the retained byte count for other map-sprite character artwork.</summary>
        internal int StoredArtworkByteCount => PrivateState.Field<MapObjectTileArtwork>(self, "characters").StoredOtherByteCount;
    }
}

/// <summary>Verification access to <see cref="MenuBeveledSquareArtwork"/> members production does not use.</summary>
internal static class MenuBeveledSquareArtworkAccess
{
    /// <summary>Exposes stored edits for beveled-square artwork checks.</summary>
    extension(MenuBeveledSquareArtwork self)
    {
        /// <summary>Gets the number of explicitly stored beveled-square pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuCompactLetteringArtwork"/> members production does not use.</summary>
internal static class MenuCompactLetteringArtworkAccess
{
    /// <summary>Exposes glyph and pixel edit storage for compact lettering checks.</summary>
    extension(MenuCompactLetteringArtwork self)
    {
        /// <summary>Gets the byte count of the stored compact-letter glyph definitions.</summary>
        internal int StoredInkByteCount => PrivateState.Field<Dictionary<char, uint>>(self, "glyphs").Count * sizeof(uint);

        /// <summary>Gets the number of explicitly stored compact-letter pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Determines whether a compact-letter tile pixel has an explicit override.</summary>
        /// <param name="tile">Tile index to inspect.</param>
        /// <param name="x">Horizontal pixel coordinate within the tile.</param>
        /// <param name="y">Vertical pixel coordinate within the tile.</param>
        /// <returns><see langword="true"/> when the pixel is explicitly overridden.</returns>
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
    /// <summary>Exposes face and edit storage for large-font checks.</summary>
    extension(MenuLargeFontArtwork self)
    {
        /// <summary>Gets the byte count of the stored large-font face payload.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Gets the number of explicitly stored large-font pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Determines whether a large-font tile pixel has an explicit override.</summary>
        /// <param name="tile">Tile index to inspect.</param>
        /// <param name="x">Horizontal pixel coordinate within the tile.</param>
        /// <param name="y">Vertical pixel coordinate within the tile.</param>
        /// <returns><see langword="true"/> when the pixel is explicitly overridden.</returns>
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
    /// <summary>Exposes explicit edit storage for panel artwork checks.</summary>
    extension(MenuPanelTileArtwork self)
    {
        /// <summary>Gets the number of explicitly stored panel-tile pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderButtonArtwork"/> members production does not use.</summary>
internal static class MenuShoulderButtonArtworkAccess
{
    /// <summary>Exposes explicit edit storage for shoulder button checks.</summary>
    extension(MenuShoulderButtonArtwork self)
    {
        /// <summary>Gets the number of explicitly stored shoulder-button pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuShoulderHighlightArtwork"/> members production does not use.</summary>
internal static class MenuShoulderHighlightArtworkAccess
{
    /// <summary>Exposes explicit edit storage for shoulder highlight checks.</summary>
    extension(MenuShoulderHighlightArtwork self)
    {
        /// <summary>Gets the number of explicitly stored shoulder-highlight pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MenuSmallFontArtwork"/> members production does not use.</summary>
internal static class MenuSmallFontArtworkAccess
{
    /// <summary>Exposes face and edit storage for small-font checks.</summary>
    extension(MenuSmallFontArtwork self)
    {
        /// <summary>Gets the byte count of the stored small-font face payload.</summary>
        internal int StoredFaceByteCount => PrivateState.Field<byte[]>(self, "faces").Length;

        /// <summary>Gets the number of explicitly stored small-font pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;

        /// <summary>Determines whether a small-font tile pixel has an explicit override.</summary>
        /// <param name="tile">Tile index to inspect.</param>
        /// <param name="x">Horizontal pixel coordinate within the tile.</param>
        /// <param name="y">Vertical pixel coordinate within the tile.</param>
        /// <returns><see langword="true"/> when the pixel is explicitly overridden.</returns>
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
    /// <summary>Exposes explicit edit storage for thin-border artwork checks.</summary>
    extension(MenuThinBorderArtwork self)
    {
        /// <summary>Gets the number of explicitly stored thin-border pixel edits.</summary>
        internal int StoredEditCount => PrivateState.Field<Dictionary<int, byte>?>(self, "edits")?.Count ?? 0;
    }
}

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    /// <summary>Exposes ordered frame definitions for Mother Brain body checks.</summary>
    extension(MotherBrainBodyVisualDefinitions)
    {
        /// <summary>Gets all Mother Brain body visual frames in native frame order.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyVisualDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitions.Frame)];
    }
}

/// <summary>Verification access to <see cref="NorfairRidleyColorCatalog"/> members production does not use.</summary>
internal static class NorfairRidleyColorCatalogAccess
{
    /// <summary>Exposes initial and reveal paint colors for Ridley palette checks.</summary>
    extension(NorfairRidleyColorCatalog self)
    {
        /// <summary>Resolves a color from Norfair Ridley's initial paint palette.</summary>
        /// <param name="color">Color index within the initial palette.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveInitial(int color) => PrivateState.Field<NorfairRidleyInitialPaintDefinitions>(self, "initial").ColorAt(color);

        /// <summary>Resolves a color from one row of Norfair Ridley's reveal palette.</summary>
        /// <param name="row">Reveal-palette row to select.</param>
        /// <param name="color">Color index within the row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveReveal(int row, int color) => PrivateState.Field<NorfairRidleyRevealPaintDefinitions>(self, "reveal").ColorAt(row, color);
    }
}

/// <summary>Verification access to <see cref="PauseReserveTankPresentation"/> members production does not use.</summary>
internal static class PauseReserveTankPresentationAccess
{
    /// <summary>Exposes stored frames and anchors for reserve meter checks.</summary>
    extension(PauseReserveTankPresentation self)
    {
        /// <summary>Gets the number of reserve-tank meter frames stored as authored compositions.</summary>
        internal int StoredFrameCount => (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Full") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "EndCap") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Empty") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill1") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill2") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill3") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill4") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill5") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill6") is null ? 0 : 1) + (PrivateState.Property<SpriteComposition?>(PrivateState.Field<object>(self, "frames"), "Fill7") is null ? 0 : 1);

        /// <summary>Gets the number of individually stored reserve-meter anchor coordinates.</summary>
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

    /// <summary>Exposes duration, composition, and anchor storage for selector checks.</summary>
    extension(PauseSelectorPresentation self)
    {
        /// <summary>Gets the number of explicitly stored selector duration overrides.</summary>
        internal int StoredDurationCount => PrivateState.Field<Dictionary<int, int>?>(self, "durationOverrides")?.Count ?? 0;

        /// <summary>Gets whether any selector category stores authored composition parts.</summary>
        internal bool StoresCompositionParts => Categories(self).Any(category =>
            StoresParts(PrivateState.Field<PauseSelectorVisual>(category, "basis")) ||
            (PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Values.Any(StoresParts) ?? false));

        /// <summary>Gets the number of selector phase composition overrides stored across all categories.</summary>
        internal int StoredCompositionOverrideCount => Categories(self).Sum(category =>
            PrivateState.Field<Dictionary<int, PauseSelectorVisual>?>(category, "overrides")?.Count ?? 0);

        /// <summary>Gets the number of individually stored selector anchor coordinates.</summary>
        internal int StoredAnchorComponentCount => PrivateState.Field<Dictionary<string, (int? X, int? Y)>>(self, "anchorOverrides").Values.Sum(value => (value.X.HasValue ? 1 : 0) + (value.Y.HasValue ? 1 : 0));
    }
}

/// <summary>Verification access to <see cref="RoomFxBlendColors"/> members production does not use.</summary>
internal static class RoomFxBlendColorsAccess
{
    /// <summary>Exposes computed blend words for room effect checks.</summary>
    extension(RoomFxBlendColors self)
    {
        // The generated array is the requested output, never a retained stock-color cache.
        /// <summary>Creates the three resolved room-effect blend colors.</summary>
        /// <returns>A new array containing the primary, secondary, and optional third color.</returns>
        internal ushort[] CreateColors() => [PrivateState.Field<RoomFxPairColor>(self, "primary").CreateColor(), PrivateState.Field<RoomFxPairColor>(self, "secondary").CreateColor(), PrivateState.Field<RoomFxThirdColor?>(self, "thirdOverride")?.CreateColor() ?? 0];
    }
}

/// <summary>Verification access to <see cref="RoomFxPaletteBlendCatalog"/> members production does not use.</summary>
internal static class RoomFxPaletteBlendCatalogAccess
{
    /// <summary>Exposes selected room FX blend palettes for verification.</summary>
    extension(RoomFxPaletteBlendCatalog self)
    {
        /// <summary>Resolves the three blend colors selected by a room FX palette index.</summary>
        /// <param name="selection">Room FX blend selection byte.</param>
        /// <returns>The selected primary, secondary, and optional third color.</returns>
        internal ReadOnlySpan<ushort> Resolve(byte selection) => ((RoomFxBlendColors)(PrivateState.Invoke(self, "SelectColors", (byte)(selection)))!).CreateColors();
    }
}

/// <summary>Verification access to <see cref="SamusBodyTileDefinition"/> members production does not use.</summary>
internal static class SamusBodyTileDefinitionAccess
{
    /// <summary>Exposes body tile transfer geometry calculation for tests.</summary>
    extension(SamusBodyTileDefinition self)
    {
        /// <summary>Calculates transfer geometry using the supplied body's pose pointers and frame selections.</summary>
        /// <param name="body">Samus body catalog that owns the selected definitions.</param>
        /// <param name="upper">Whether the definition belongs to the upper-body table.</param>
        /// <param name="set">Tile-definition set index.</param>
        /// <param name="position">Definition position within the set.</param>
        /// <returns>A copy of the definition with its transfer geometry populated.</returns>
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
    /// <summary>Exposes charged-beam and pseudo-screw color resolution.</summary>
    extension(SamusChargeColorCatalog self)
    {
        /// <summary>Resolves one charged-beam or pseudo-screw palette color.</summary>
        /// <param name="pseudo">Whether to use the pseudo-screw palette instead of the charged-beam palette.</param>
        /// <param name="suit">Suit palette index.</param>
        /// <param name="phase">Charge animation phase.</param>
        /// <param name="color">Color index within the selected palette row.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveCharge(bool pseudo, int suit, int phase, int color) =>
            ((ushort)(PrivateState.Invoke((pseudo ? PrivateState.Field<object>(self, "pseudoScrew") : PrivateState.Field<object>(self, "chargedBeam")), "Resolve", (int)(suit), (int)(phase), (int)(color)))!);
    }
}

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    /// <summary>Exposes installed spritemap definitions for coverage checks.</summary>
    extension(SamusSpritemapArtworkCatalog self)
    {
        /// <summary>Gets every installed Samus spritemap definition.</summary>
        internal IReadOnlyCollection<SamusSpritemapDefinition> Definitions => PrivateState.Field<Dictionary<ushort, SamusSpritemapDefinition>>(self, "definitions").Values;
    }
}

/// <summary>Verification access to <see cref="SelectedPresentationHash"/> members production does not use.</summary>
internal static class SelectedPresentationHashAccess
{
    /// <summary>Adapts test frame collections to the presentation hash contract.</summary>
    extension(SelectedPresentationHash)
    {
        /// <summary>Creates a canonical identity from frames that each contain one word run.</summary>
        /// <param name="domain">Domain label included in the presentation identity.</param>
        /// <param name="frames">Frame words keyed by native frame pointer.</param>
        /// <returns>The canonical hexadecimal content identity.</returns>
        internal static string FromWordFrames(string domain, IReadOnlyDictionary<ushort, ushort[]> frames) =>
            SelectedPresentationHash.FromWordFrames(domain, frames.ToDictionary(pair => pair.Key, pair => new[] { pair.Value }));

        /// <summary>Creates a canonical identity from frames that each contain one word.</summary>
        /// <param name="domain">Domain label included in the presentation identity.</param>
        /// <param name="frames">Single frame words keyed by native frame pointer.</param>
        /// <returns>The canonical hexadecimal content identity.</returns>
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
    /// <summary>Exposes title palette words for presentation checks.</summary>
    extension(TitlePalettePresentation self)
    {
        /// <summary>Gets the title palette colors in stored CGRAM order.</summary>
        internal ReadOnlySpan<ushort> Colors => PrivateState.Field<ushort[]>(self, "colors");
    }
}

/// <summary>Verification access to <see cref="TourianStatueColorCatalog"/> members production does not use.</summary>
internal static class TourianStatueColorCatalogAccess
{
    /// <summary>Exposes resolved Tourian statue palette entries.</summary>
    extension(TourianStatueColorCatalog self)
    {
        /// <summary>Resolves one Tourian statue palette color.</summary>
        /// <param name="color">Color index within the statue palette.</param>
        /// <returns>The resolved SNES color word.</returns>
        internal ushort ResolveStatue(int color) => ((ushort)(PrivateState.Invoke(PrivateState.Field<object>(self, "statueColors"), "Read", (int)(color)))!);
    }
}
