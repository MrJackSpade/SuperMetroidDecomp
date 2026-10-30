using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Security.Cryptography;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Frontend.Map;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Fx;

/// <summary>Verifies that installed map and projectile overrides survive import, loading, and host startup.</summary>
internal static class MapInstallationVerification
{
    /// <summary>Opt-in full importer transaction; all ROM/audio/fixture data stays in ignored test-temp.</summary>
    internal static void VerifyMapInstallation(string sourceRom)
    {
        using var rootScratch = new TestScratchDirectory("map-installation");
        string root = rootScratch.Root;
        byte[] sourceHash = SHA256.HashData(File.ReadAllBytes(sourceRom));
        Console.WriteLine($"Full installation fixture: {root}");
        var installation = GameAssetInstaller.Install(sourceRom, root, progress: new ImmediateInstallProgress());
        if (OperatingSystem.IsWindows())
        {
            // An exclusive handle makes a startup ROM read fail deterministically.
            // A complete installation must instead validate and use extracted assets.
            using var lockedRom = new FileStream(installation.RomPath, FileMode.Open,
                FileAccess.Read, FileShare.None);
            AssertTrue(GameAssetInstaller.OpenOrRepair(root) is not null,
                "complete host startup does not open the installed ROM");
        }
        var stock = installation.LoadMaps();
        var projectileStock = installation.LoadProjectiles();
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string projectileOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        var projectileDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName)));
        var part = (projectileDocument.Required("frames").AsObject().First(pair => (pair.Value ?? throw new InvalidOperationException("Verification requires pair.Value.")).AsArray().Count > 0).Value ?? throw new InvalidOperationException("Verification requires a non-empty frame.")).Required(0);
        int offset = part.Required("offsetX").GetValue<int>();
        part["offsetX"] = offset == 255 ? 254 : offset + 1;
        File.WriteAllText(projectileOverride, projectileDocument.ToJsonString());
        string beamName = BeamTileAtlasDefinitions.FileName(0);
        string beamOverride = Path.Combine(installation.ProjectileOverrideDirectory, beamName);
        IndexedPngImage beamImage;
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, beamName)))
            beamImage = IndexedPng.Read(input, new(BeamTileAtlasDefinitions.Width, BeamTileAtlasDefinitions.Height));
        beamImage.Pixels[0] ^= 1;
        using (var output = File.Create(beamOverride))
            IndexedPng.Write(output, new(beamImage.Size.X, beamImage.Size.Y), beamImage.Pixels, beamImage.Colours);
        string beamPaletteOverride = Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteDefinitions.FileName);
        var beamColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, BeamPaletteDefinitions.FileName)));
        beamColors.Required("palettes").Required(BeamPaletteSources.Key(0)).Required(0)["red"] = 17;
        File.WriteAllText(beamPaletteOverride, beamColors.ToJsonString());
        string trailOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName);
        var trailDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName)));
        trailDocument.Required("frames").Required(ProjectileTrailVisualDefinitions.FrameNames[0])["flipX"] = true;
        File.WriteAllText(trailOverride, trailDocument.ToJsonString());
        string trailPngOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName);
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName)))
        {
            var trailImage = IndexedPng.Read(input, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
            trailImage.Pixels[0] ^= 1;
            using var output = File.Create(trailPngOverride);
            IndexedPng.Write(output, new(trailImage.Size.X, trailImage.Size.Y), trailImage.Pixels, trailImage.Colours);
        }
        var projectileEdited = installation.LoadProjectiles();
        string flareOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlarePlacementDefinitions.FileName);
        var flareDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlarePlacementDefinitions.FileName)));
        flareDocument.Required("offsets").Required(ChargeFlarePlacementOffsets.Key(false, 0))["x"] = 17;
        File.WriteAllText(flareOverride, flareDocument.ToJsonString());
        string flareCompositionOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlareSpriteDefinitions.FileName);
        var flareComposition = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlareSpriteDefinitions.FileName)));
        flareComposition.Required("frames").Required(ChargeFlareSpriteSelection.Selectors[0].Name).Required(0)["offsetX"] = 17;
        File.WriteAllText(flareCompositionOverride, flareComposition.ToJsonString());
        string grappleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleTileDefinitions.FileName);
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, GrappleTileDefinitions.FileName)))
        {
            var grappleImage = IndexedPng.Read(input, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
            grappleImage.Pixels[0] ^= 1;
            using var output = File.Create(grappleOverride);
            IndexedPng.Write(output, new(grappleImage.Size.X, grappleImage.Size.Y), grappleImage.Pixels, grappleImage.Colours);
        }
        string grappleStyleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleSpriteDefinitions.FileName);
        var grappleStyle = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSpriteDefinitions.FileName)));
        grappleStyle.Required("endpoint")["palette"] = 4;
        File.WriteAllText(grappleStyleOverride, grappleStyle.ToJsonString());
        var grappleFlare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleFlarePlacementDefinitions.FileName)));
        grappleFlare.Required("offsets").Required(ChargeFlarePlacementOffsets.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleFlarePlacementDefinitions.FileName), grappleFlare.ToJsonString());
        var grappleSwing = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSwingFrameDefinitions.FileName)));
        grappleSwing.Required("frames")[0] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSwingFrameDefinitions.FileName), grappleSwing.ToJsonString());
        projectileEdited = installation.LoadProjectiles();
        AssertTrue(projectileStock.SelectedSha256 != projectileEdited.SelectedSha256, "installed projectile override selected");
        Directory.CreateDirectory(installation.MapOverrideDirectory);
        string paletteOverride = Path.Combine(installation.MapOverrideDirectory, MapStaticPalettesFormat.FileName);
        var colors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapStaticPalettesFormat.FileName)));
        int oldRed = colors.Required("pause").Required(0).Required("red").GetValue<int>();
        colors.Required("pause").Required(0)["red"] = (oldRed + 1) % 32;
        File.WriteAllText(paletteOverride, colors.ToJsonString());
        string roomFxArtworkOverride = Path.Combine(installation.MapOverrideDirectory,
            RoomFxAnimatedTileAtlasFormat.FileName);
        using (var input = File.OpenRead(Path.Combine(installation.MapDirectory,
                   RoomFxAnimatedTileAtlasFormat.FileName)))
        {
            IndexedPngImage image = IndexedPng.Read(input,
                new(RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height));
            image.Pixels[0] = (byte)((image.Pixels[0] + 1) % RoomFxAnimatedTileAtlasFormat.ColorCount);
            using var output = File.Create(roomFxArtworkOverride);
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        }
        string roomFxTilemapOverride = Path.Combine(installation.MapOverrideDirectory,
            RoomFxLayer3TilemapFormat.FileName);
        var roomFxTilemaps = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, RoomFxLayer3TilemapFormat.FileName)));
        int lavaTileColumn = roomFxTilemaps.Required("pages").Required("Lava").Required(0).Required("tileColumn").GetValue<int>();
        roomFxTilemaps.Required("pages").Required("Lava").Required(0)["tileColumn"] =
            (lavaTileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns;
        File.WriteAllText(roomFxTilemapOverride, roomFxTilemaps.ToJsonString());
        string roomFxBlendOverride = Path.Combine(installation.MapOverrideDirectory,
            RoomFxPaletteBlendDefinitions.FileName);
        var roomFxBlends = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, RoomFxPaletteBlendDefinitions.FileName)));
        string lavaBlendKey = RoomFxPaletteBlendRules.Key(RoomFxPaletteBlend.Lava);
        int lavaBlendRed = roomFxBlends.Required("blends").Required(lavaBlendKey).Required(0).Required("red").GetValue<int>();
        roomFxBlends.Required("blends").Required(lavaBlendKey).Required(0)["red"] = (lavaBlendRed + 1) % 32;
        File.WriteAllText(roomFxBlendOverride, roomFxBlends.ToJsonString());
        string powerBombColorOverride = Path.Combine(installation.MapOverrideDirectory,
            PowerBombFixedColorFormat.FileName);
        var powerBombColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, PowerBombFixedColorFormat.FileName)));
        int powerBombRed = powerBombColors.Required("preExplosion").Required(0).Required("red").GetValue<int>();
        powerBombColors.Required("preExplosion").Required(0)["red"] = (powerBombRed + 1) % 32;
        File.WriteAllText(powerBombColorOverride, powerBombColors.ToJsonString());
        string samusVisorColorOverride = Path.Combine(installation.MapOverrideDirectory,
            SamusVisorColorFormat.FileName);
        var samusVisorColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, SamusVisorColorFormat.FileName)));
        int visorRed = samusVisorColors.Required("colors").Required(3).Required("red").GetValue<int>();
        samusVisorColors.Required("colors").Required(3)["red"] = (visorRed + 1) % 32;
        File.WriteAllText(samusVisorColorOverride, samusVisorColors.ToJsonString());
        string samusHurtColorOverride = Path.Combine(installation.MapOverrideDirectory,
            SamusHurtColorFormat.FileName);
        var samusHurtColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, SamusHurtColorFormat.FileName)));
        int hurtRed = samusHurtColors.Required("hurt").Required(5).Required("red").GetValue<int>();
        samusHurtColors.Required("hurt").Required(5)["red"] = (hurtRed + 1) % 32;
        File.WriteAllText(samusHurtColorOverride, samusHurtColors.ToJsonString());
        string samusHyperBeamColorOverride = Path.Combine(installation.MapOverrideDirectory,
            SamusHyperBeamColorFormat.FileName);
        var samusHyperBeamColors = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.MapDirectory, SamusHyperBeamColorFormat.FileName)));
        int hyperRed = samusHyperBeamColors.Required("frames").Required(4).Required(7).Required("red").GetValue<int>();
        samusHyperBeamColors.Required("frames").Required(4).Required(7)["red"] = (hyperRed + 1) % 32;
        File.WriteAllText(samusHyperBeamColorOverride, samusHyperBeamColors.ToJsonString());
        string stationOverride = Path.Combine(installation.MapOverrideDirectory, MapStationLayoutFormat.FileName);
        var stations = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapStationLayoutFormat.FileName)));
        stations.Required("markers").Required("Brinstar.Missile.0")["x"] = 80;
        File.WriteAllText(stationOverride, stations.ToJsonString());
        string landmarkOverride = Path.Combine(installation.MapOverrideDirectory, MapLandmarkFormat.FileName);
        var landmarks = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapLandmarkFormat.FileName)));
        landmarks.Required("markers").Required("Boss.Phantoon")["x"] = 160;
        File.WriteAllText(landmarkOverride, landmarks.ToJsonString());
        string saveMarkerOverride = Path.Combine(installation.MapOverrideDirectory, MapSaveMarkerFormat.FileName);
        var saveMarkers = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapSaveMarkerFormat.FileName)));
        saveMarkers.Required("markers").Required("Maridia.Save.0")["x"] = 104;
        File.WriteAllText(saveMarkerOverride, saveMarkers.ToJsonString());
        string arrowOverride = Path.Combine(installation.MapOverrideDirectory, MapArrowFormat.FileName);
        var arrows = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapArrowFormat.FileName)));
        arrows.Required("arrows").Required("Left")["x"] = 24;
        File.WriteAllText(arrowOverride, arrows.ToJsonString());
        string screenOverride = Path.Combine(installation.MapOverrideDirectory, MapScreenDefinitions.FileName);
        var screens = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapScreenDefinitions.FileName)));
        screens.Required("pages").Required(MapScreenDefinitions.WorldForeground.Name).Required(0)["flipX"] = true;
        File.WriteAllText(screenOverride, screens.ToJsonString());
        string backdropOverride = Path.Combine(installation.MapOverrideDirectory, PauseBackdropDefinitions.FileName);
        var backdrops = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseBackdropDefinitions.FileName)));
        backdrops.Required("areas").Required("Maridia").Required(170)["flipX"] = true;
        backdrops.Required("buttons").Required(300)["flipX"] = true;
        File.WriteAllText(backdropOverride, backdrops.ToJsonString());
        string wireframeOverride = Path.Combine(installation.MapOverrideDirectory, PauseWireframeDefinitions.FileName);
        var wireframes = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseWireframeDefinitions.FileName)));
        wireframes.Required("frames").Required("PowerSuit").Required(0)["flipX"] = true;
        File.WriteAllText(wireframeOverride, wireframes.ToJsonString());
        string selectorOverride = Path.Combine(installation.MapOverrideDirectory, PauseSelectorDefinitions.FileName);
        var selectors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseSelectorDefinitions.FileName)));
        selectors.Required("anchors").Required("Beam.Charge")["x"] = 48;
        File.WriteAllText(selectorOverride, selectors.ToJsonString());
        string reserveOverride = Path.Combine(installation.MapOverrideDirectory, PauseReserveTankDefinitions.FileName);
        var reserve = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseReserveTankDefinitions.FileName)));
        reserve.Required("anchors").Required(0)["x"] = 40;
        File.WriteAllText(reserveOverride, reserve.ToJsonString());
        string reserveUiOverride = Path.Combine(installation.MapOverrideDirectory, PauseReserveUiDefinitions.FileName);
        var reserveUi = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseReserveUiDefinitions.FileName)));
        reserveUi.Required("digits").Required("cells").Required(5)["flipX"] = true;
        File.WriteAllText(reserveUiOverride, reserveUi.ToJsonString());
        string equipmentBaseOverride = Path.Combine(installation.MapOverrideDirectory, PauseEquipmentBaseDefinitions.FileName);
        var equipmentBase = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseEquipmentBaseDefinitions.FileName)));
        equipmentBase.Required("cells").Required(194)["flipX"] = true;
        File.WriteAllText(equipmentBaseOverride, equipmentBase.ToJsonString());
        string equipmentLabelOverride = Path.Combine(installation.MapOverrideDirectory, PauseEquipmentLabelArtworkDefinitions.FileName);
        var equipmentLabels = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, PauseEquipmentLabelArtworkDefinitions.FileName)));
        equipmentLabels.Required("labels").Required("Beam.Charge").Required("cells").Required(0)["flipX"] = true;
        File.WriteAllText(equipmentLabelOverride, equipmentLabels.ToJsonString());
        string escapeTimerOverride = Path.Combine(installation.MapOverrideDirectory, EscapeTimerPresentationDefinitions.FileName);
        var escapeTimer = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, EscapeTimerPresentationDefinitions.FileName)));
        escapeTimer.Required("anchors").Required("Minutes")["x"] = -27;
        File.WriteAllText(escapeTimerOverride, escapeTimer.ToJsonString());
        string escapeTimerTileOverride = Path.Combine(
            installation.MapOverrideDirectory, EscapeTimerTileAtlasFormat.FileName);
        using (var input = File.OpenRead(Path.Combine(
            installation.MapDirectory, EscapeTimerTileAtlasFormat.FileName)))
        {
            var timerImage = IndexedPng.Read(input,
                new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
            timerImage.Pixels[0] ^= 1;
            using var output = File.Create(escapeTimerTileOverride);
            IndexedPng.Write(output, new(timerImage.Size.X, timerImage.Size.Y),
                timerImage.Pixels, timerImage.Colours);
        }
        string gameplayHudOverride = Path.Combine(installation.MapOverrideDirectory, GameplayHudDefinitions.FileName);
        var gameplayHud = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, GameplayHudDefinitions.FileName)));
        gameplayHud.Required("digits").Required("healthAnchor")["x"] = 4;
        File.WriteAllText(gameplayHudOverride, gameplayHud.ToJsonString());
        string gameOverOverride = Path.Combine(installation.MapOverrideDirectory, GameOverPresentationDefinitions.FileName);
        var gameOver = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, GameOverPresentationDefinitions.FileName)));
        gameOver.Required("babyAnchor")["x"] = 132;
        File.WriteAllText(gameOverOverride, gameOver.ToJsonString());
        string gameOptionsOverride = Path.Combine(installation.MapOverrideDirectory, GameOptionsPresentationDefinitions.FileName);
        var gameOptions = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, GameOptionsPresentationDefinitions.FileName)));
        gameOptions.Required("cursorAnchors").Required(GameOptionsNames.MenuName(GameOptionsPage.Primary)).Required(0)["x"] = 32;
        File.WriteAllText(gameOptionsOverride, gameOptions.ToJsonString());
        string fileSelectOverride = Path.Combine(installation.MapOverrideDirectory, FileSelectPresentationDefinitions.FileName);
        var fileSelect = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, FileSelectPresentationDefinitions.FileName)));
        fileSelect.Required("mainCursorAnchors").Required(0)["x"] = 22;
        File.WriteAllText(fileSelectOverride, fileSelect.ToJsonString());
        string spriteOverride = Path.Combine(installation.MapOverrideDirectory, MapSpriteFormat.JsonFile);
        var sprites = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.MapDirectory, MapSpriteFormat.JsonFile)));
        sprites.Required("frames").Required("Marker.Boss").Required(0)["offsetX"] = 12;
        File.WriteAllText(spriteOverride, sprites.ToJsonString());
        var artworkOverrides = new List<string> { escapeTimerTileOverride };
        foreach (var atlas in new[] { new VerifyMapInstallationCase(WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.ForegroundHeight, 16, WorldMapArtworkFormat.Width),
            new VerifyMapInstallationCase(WorldMapArtworkFormat.BackgroundFile, WorldMapArtworkFormat.BackgroundHeight, 4, WorldMapArtworkFormat.Width),
            new VerifyMapInstallationCase(MapSpriteFormat.PngFile, MapSpriteFormat.Height, 16, MapSpriteFormat.Width),
            new VerifyMapInstallationCase(PauseTileAtlasFormat.FileName, MapTileAtlasFormat.Height, 16, MapTileAtlasFormat.Width) })
        {
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.MapDirectory, atlas.First)))
                image = IndexedPng.Read(input, new(checked((ushort)atlas.Width), checked((ushort)atlas.Second)));
            image.Pixels[0] = (byte)((image.Pixels[0] + 1) % atlas.Third);
            string path = Path.Combine(installation.MapOverrideDirectory, atlas.First);
            using (var output = File.Create(path)) IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            artworkOverrides.Add(path);
        }
        var edited = installation.LoadMaps();
        AssertEqual(24, edited.Arrows.Get(SuperMetroid.Core.Frontend.Map.MapScrollDirection.Left).Position.X, "full installation consumes arrow override");
        AssertEqual(104, edited.SaveMarkers.Get(SuperMetroid.Core.Progression.AreaId.Maridia, 0).X, "full installation consumes save-marker override");
        AssertEqual(160, edited.Landmarks.Get("Boss.Phantoon").X, "full installation consumes landmark override");
        AssertEqual(80, edited.Stations.Get("Brinstar.Missile.0").X, "full installation consumes station position override");
        AssertTrue(stock.GameOptions.ContentIdentity != edited.GameOptions.ContentIdentity,
            "full installation consumes options-menu override");
        AssertTrue(stock.FileSelect.ContentIdentity != edited.FileSelect.ContentIdentity,
            "full installation consumes file-select override");
        AssertTrue(!stock.EscapeTimerTiles.Resolve(SuperMetroid.Core.Assets.InstalledArtworkId.EscapeTimerFirstTiles).Span.SequenceEqual(
            edited.EscapeTimerTiles.Resolve(SuperMetroid.Core.Assets.InstalledArtworkId.EscapeTimerFirstTiles).Span),
            "full installation consumes escape-timer artwork override");
        AssertTrue(stock.RoomFxAnimatedTiles.TryResolve(
                AnimatedTileImageSources.Of(new AnimatedTileImage(AnimatedTileObject.MaridiaSandCeiling, 0)), 0x40,
                out ReadOnlyMemory<byte> stockRoomFx) &&
            edited.RoomFxAnimatedTiles.TryResolve(
                AnimatedTileImageSources.Of(new AnimatedTileImage(AnimatedTileObject.MaridiaSandCeiling, 0)), 0x40,
                out ReadOnlyMemory<byte> editedRoomFx) &&
            !stockRoomFx.Span.SequenceEqual(editedRoomFx.Span),
            "full installation consumes room-FX animation artwork override");
        AssertTrue(!SuperMetroid.AssetExtraction.Frontend.Map.InstalledAreaMapPresentation.RoomFxPagesOf(stock).Resolve(SuperMetroid.Core.Fx.RoomFxType.Lava).Span.SequenceEqual(
            SuperMetroid.AssetExtraction.Frontend.Map.InstalledAreaMapPresentation.RoomFxPagesOf(edited).Resolve(SuperMetroid.Core.Fx.RoomFxType.Lava).Span),
            "full installation consumes room-FX BG3 tilemap override");
        AssertTrue(!stock.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlend.Lava).SequenceEqual(
            edited.RoomFxPaletteBlends.Resolve(RoomFxPaletteBlend.Lava)),
            "full installation consumes room-FX palette-blend override");
        AssertTrue(stock.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0) !=
            edited.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0),
            "full installation consumes Power Bomb fixed-color override");
        AssertTrue(stock.SamusVisorColors.Resolve(3) != edited.SamusVisorColors.Resolve(3),
            "full installation consumes Samus visor color override");
        AssertTrue(stock.SamusHurtColors.Resolve(SamusHurtColorVariant.Hurt, 5) !=
            edited.SamusHurtColors.Resolve(SamusHurtColorVariant.Hurt, 5),
            "full installation consumes Samus hurt color override");
        AssertTrue(stock.SamusHyperBeamColors.Resolve(4, 7) !=
            edited.SamusHyperBeamColors.Resolve(4, 7),
            "full installation consumes Hyper Beam color override");
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "full installation consumes edited palette override");
        // Synthetic sentinels, not a copy of the player's real files.
        var preserved = new Dictionary<string, byte[]>
        {
            [projectileOverride] = File.ReadAllBytes(projectileOverride),
            [beamOverride] = File.ReadAllBytes(beamOverride),
            [beamPaletteOverride] = File.ReadAllBytes(beamPaletteOverride),
            [trailOverride] = File.ReadAllBytes(trailOverride),
            [trailPngOverride] = File.ReadAllBytes(trailPngOverride),
            [flareOverride] = File.ReadAllBytes(flareOverride),
            [flareCompositionOverride] = File.ReadAllBytes(flareCompositionOverride),
            [grappleOverride] = File.ReadAllBytes(grappleOverride),
            [paletteOverride] = File.ReadAllBytes(paletteOverride),
            [roomFxArtworkOverride] = File.ReadAllBytes(roomFxArtworkOverride),
            [roomFxTilemapOverride] = File.ReadAllBytes(roomFxTilemapOverride),
            [roomFxBlendOverride] = File.ReadAllBytes(roomFxBlendOverride),
            [powerBombColorOverride] = File.ReadAllBytes(powerBombColorOverride),
            [samusVisorColorOverride] = File.ReadAllBytes(samusVisorColorOverride),
            [samusHurtColorOverride] = File.ReadAllBytes(samusHurtColorOverride),
            [samusHyperBeamColorOverride] = File.ReadAllBytes(samusHyperBeamColorOverride),
            [stationOverride] = File.ReadAllBytes(stationOverride),
            [landmarkOverride] = File.ReadAllBytes(landmarkOverride),
            [saveMarkerOverride] = File.ReadAllBytes(saveMarkerOverride),
            [arrowOverride] = File.ReadAllBytes(arrowOverride),
            [screenOverride] = File.ReadAllBytes(screenOverride),
            [backdropOverride] = File.ReadAllBytes(backdropOverride),
            [wireframeOverride] = File.ReadAllBytes(wireframeOverride),
            [selectorOverride] = File.ReadAllBytes(selectorOverride),
            [reserveOverride] = File.ReadAllBytes(reserveOverride),
            [reserveUiOverride] = File.ReadAllBytes(reserveUiOverride),
            [equipmentBaseOverride] = File.ReadAllBytes(equipmentBaseOverride),
            [equipmentLabelOverride] = File.ReadAllBytes(equipmentLabelOverride),
            [escapeTimerOverride] = File.ReadAllBytes(escapeTimerOverride),
            [gameplayHudOverride] = File.ReadAllBytes(gameplayHudOverride),
            [gameOverOverride] = File.ReadAllBytes(gameOverOverride),
            [gameOptionsOverride] = File.ReadAllBytes(gameOptionsOverride),
            [fileSelectOverride] = File.ReadAllBytes(fileSelectOverride),
            [spriteOverride] = File.ReadAllBytes(spriteOverride),
            [Path.Combine(root, "SuperMetroid.ini")] = "[Testing]\nInvincibility=true\n"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.save.json")] = "{\"fixture\":\"player-save\"}"u8.ToArray(),
            [Path.Combine(root, "SuperMetroid.srm")] = "synthetic-legacy-sram-sentinel"u8.ToArray(),
            [Path.Combine(root, "debug-states", "SuperMetroid-debug-slot-0.smstate")] = "synthetic-state-sentinel"u8.ToArray(),
            [Path.Combine(root, "input-recordings", "session.inputs")] = new byte[] { 0, 1, 0, 2 }
        };
        foreach (string path in artworkOverrides) preserved.Add(path, File.ReadAllBytes(path));
        foreach (var pair in preserved)
        {
            Directory.CreateDirectory((Path.GetDirectoryName(pair.Key) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(pair.Key).")));
            File.WriteAllBytes(pair.Key, pair.Value);
        }
        string manifestPath = Path.Combine(installation.MapDirectory, AreaMapCatalogFormat.ManifestFile);
        var manifest = RequiredJson.Parse(File.ReadAllText(manifestPath));
        manifest["version"] = AreaMapCatalogFormat.Version - 1;
        File.WriteAllText(manifestPath, manifest.ToJsonString());
        byte[] outdatedManifest = File.ReadAllBytes(manifestPath);

        using (var cancellation = new CancellationTokenSource())
        {
            var cancelBeforePublish = new ImmediateInstallProgress(message =>
            {
                if (message.StartsWith("Finishing setup", StringComparison.Ordinal)) cancellation.Cancel();
            });
            AssertThrows<OperationCanceledException>(() => GameAssetInstaller.EnsureInstalled(root, cancellation.Token, cancelBeforePublish),
                "cancelled full extraction does not publish its staging directory");
        }
        AssertTrue(outdatedManifest.SequenceEqual(File.ReadAllBytes(manifestPath)), "cancelled upgrade retains original installed catalog");
        AssertTrue(!Directory.EnumerateDirectories(root, ".game.install-*").Any(), "cancelled upgrade cleans its own staging directory");
        CheckPreserved();
        var repaired = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress())
            ?? throw new InvalidOperationException("Installation vanished during map upgrade.");
        AssertEqual(edited.ContentIdentity, repaired.LoadMaps().ContentIdentity, "full installer upgrade retains selected override identity");
        AssertEqual(projectileEdited.SelectedSha256, repaired.LoadProjectiles().SelectedSha256, "full installer replacement preserves projectile override");
        AssertEqual(AreaMapCatalogFormat.Version, RequiredJson.Parse(File.ReadAllText(manifestPath)).Required("version").GetValue<int>(), "upgrade publishes current catalog version");
        CheckPreserved();
        // A composition-only installation must upgrade even if its map manifest is current.
        string projectileManifestPath = Path.Combine(installation.ProjectileDirectory, ProjectilePresentationFiles.ManifestFileName);
        var oldProjectileManifest = RequiredJson.Parse(File.ReadAllText(projectileManifestPath));
        oldProjectileManifest["version"] = 1;
        oldProjectileManifest.AsObject().Remove("beamHashes");
        File.WriteAllText(projectileManifestPath, oldProjectileManifest.ToJsonString());
        var beamUpgrade = GameAssetInstaller.EnsureInstalled(root)
            ?? throw new InvalidOperationException("Installation vanished during beam upgrade.");
        AssertEqual(projectileEdited.SelectedSha256, beamUpgrade.LoadProjectiles().SelectedSha256, "composition-only upgrade retains beam and composition overrides");
        CheckPreserved();
        var unchanged = GameAssetInstaller.EnsureInstalled(root, progress: new ImmediateInstallProgress(_ =>
            throw new InvalidOperationException("Complete installation unexpectedly extracted again.")));
        AssertTrue(unchanged is not null, "complete installation fast path remains available");
        AssertTrue(sourceHash.SequenceEqual(SHA256.HashData(File.ReadAllBytes(sourceRom))), "full installer leaves original ROM unchanged");
        File.WriteAllText(paletteOverride, "broken override");
        _ = GameAssetInstaller.EnsureInstalled(root);
        AssertThrows<InvalidDataException>(() => repaired.LoadMaps(), "startup preserves and reports invalid user override instead of repairing it");
        AssertEqual("broken override", File.ReadAllText(paletteOverride), "installer never replaces broken user override");
        Console.WriteLine("Full installer: fresh import, cancelled upgrade, real version replacement, no-op restart and player/override preservation pass.");

        void CheckPreserved()
        {
            foreach (var pair in preserved)
                AssertTrue(pair.Value.SequenceEqual(File.ReadAllBytes(pair.Key)), $"installer preserves {Path.GetRelativePath(root, pair.Key)} exactly");
        }
    }

    /// <summary>Forwards installer progress to the console and an optional verification callback.</summary>
    internal sealed class ImmediateInstallProgress(Action<string>? onReport = null) : IProgress<string>
    {
        /// <summary>Emits one installation step and forwards it to the registered callback.</summary>
        public void Report(string message) { Console.WriteLine(message); onReport?.Invoke(message); }
    }

    /// <summary>Runs the full map installation audit using the repository's retail cartridge input.</summary>
    [VerificationCommand("--map-installation")]
    internal static int MapInstallationCommand()
    {
        string installationRom = RepositoryRomPath;
        Suite(nameof(VerifyMapInstallation), () => VerifyMapInstallation(installationRom));
        return 0;
    }
}
