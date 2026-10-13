using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Android;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Checks Android host binding of installed projectile art across startup, save loads, and edited overrides.</summary>
internal static class ProjectileHostBindingVerification
{
    /// <summary>Installs the repository ROM into a disposable root, verifies host binding, then removes it.</summary>
    public static int Run()
    {
        using var rootScratch = new TestScratchDirectory("projectile-host");
        string root = rootScratch.Root;
        Directory.CreateDirectory(root);
        try
        {
            return Verify(GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), Path.Combine(root, "installation")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    /// <summary>Compares host catalogs with installed files before and after a state load, restart, and invalid override.</summary>
    private static int Verify(GameInstallation installation)
    {
        string root = installation.Root;
        using (var session = new AndroidSessionData(root))
        {
            var content = session.Game.Artwork.ProjectileCompositions;
            if (content is not ProjectileSpriteCatalog) throw new InvalidDataException("Installed Android session did not bind projectile content.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
            CheckFrameBindings(session.Game.Artwork.ProjectileFrameBindings, installation.LoadProjectiles().FrameBindings);
            session.SaveSlot(0);
            CheckGrapple((GrappleTileAtlas)(session.Game.Artwork.GrappleArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.GrappleArtwork.")), installation.LoadProjectiles().GrappleTiles);
            CheckFlareCompositions((ChargeFlareSpriteCatalog)(session.Game.Artwork.ChargeFlareCompositions ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlareCompositions.")), installation.LoadProjectiles().FlareCompositions);
            session.LoadSlot(0);
            CheckFrameBindings(session.Game.Artwork.ProjectileFrameBindings, installation.LoadProjectiles().FrameBindings);
            CheckGrapple((GrappleTileAtlas)session.Game.Artwork.GrappleArtwork, installation.LoadProjectiles().GrappleTiles);
            CheckFlareCompositions((ChargeFlareSpriteCatalog)session.Game.Artwork.ChargeFlareCompositions, installation.LoadProjectiles().FlareCompositions);
            if (!ReferenceEquals(content, session.Game.Artwork.ProjectileCompositions)) throw new InvalidDataException("State load lost current host projectile content.");
            CheckBeams((BeamTileCatalog)(session.Game.Artwork.BeamArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.BeamArtwork.")), installation.LoadProjectiles().BeamTiles);
            CheckTrails((ProjectileTrailCatalog)(session.Game.Artwork.TrailArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.TrailArtwork.")), installation.LoadProjectiles().Trails);
            CheckFlare((ChargeFlarePlacementCatalog)(session.Game.Artwork.ChargeFlarePlacement ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlarePlacement.")), installation.LoadProjectiles().FlarePlacement);
        }
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string path = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        var document = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName)));
        var part = (document.Required("frames").AsObject().First(p => (p.Value ?? throw new InvalidOperationException("Verification requires p.Value.")).AsArray().Count > 0).Value ?? throw new InvalidOperationException("Verification requires a non-empty frame.")).Required(0);
        int x = part.Required("offsetX").GetValue<int>();
        part["offsetX"] = x == 255 ? 254 : x + 1;
        File.WriteAllText(path, document.ToJsonString());
        string beamName = BeamTileAtlasDefinitions.FileName(0);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, beamName)))
            image = IndexedPng.Read(input, new(64, 8));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, beamName)))
            IndexedPng.Write(output, new(64, 8), image.Pixels, image.Palette);
        var colors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, BeamPaletteDefinitions.FileName)));
        colors.Required("palettes").Required(BeamPaletteDefinitions.Key(0)).Required(0)["red"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteDefinitions.FileName), colors.ToJsonString());
        var trails = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName)));
        trails.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(ProjectileTrailVisualDefinitions.Frames[0]))["flipX"] = true;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName), trails.ToJsonString());
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            image = IndexedPng.Read(input, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var flare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlarePlacementDefinitions.FileName)));
        flare.Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlarePlacementDefinitions.FileName), flare.ToJsonString());
        var flareComposition = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlareSpriteDefinitions.FileName)));
        flareComposition.Required("frames").Required(ProjectileSpriteDefinitions.Name(ChargeFlareSpriteDefinitions.Selectors[0])).Required(0)["offsetX"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlareSpriteDefinitions.FileName), flareComposition.ToJsonString());
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, GrappleTileDefinitions.FileName)))
            image = IndexedPng.Read(input, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, GrappleTileDefinitions.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var grappleStyles = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSpriteDefinitions.FileName)));
        grappleStyles.Required("endpoint")["palette"] = 4;
        foreach (var style in grappleStyles.Required("segments").AsArray().RequiredElements()) style["palette"] = 4;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSpriteDefinitions.FileName), grappleStyles.ToJsonString());
        var grappleFlare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleFlarePlacementDefinitions.FileName)));
        grappleFlare.Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleFlarePlacementDefinitions.FileName), grappleFlare.ToJsonString());
        var grappleSwing = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSwingFrameDefinitions.FileName)));
        grappleSwing.Required("frames")[0] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSwingFrameDefinitions.FileName), grappleSwing.ToJsonString());
        using (var session = new AndroidSessionData(root))
        {
            var content = session.Game.Artwork.ProjectileCompositions;
            CheckGrapple((GrappleTileAtlas)(session.Game.Artwork.GrappleArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.GrappleArtwork.")), installation.LoadProjectiles().GrappleTiles);
            CheckFlareCompositions((ChargeFlareSpriteCatalog)(session.Game.Artwork.ChargeFlareCompositions ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlareCompositions.")), installation.LoadProjectiles().FlareCompositions);
            session.LoadSlot(0);
            CheckGrapple((GrappleTileAtlas)session.Game.Artwork.GrappleArtwork, installation.LoadProjectiles().GrappleTiles);
            CheckFlareCompositions((ChargeFlareSpriteCatalog)session.Game.Artwork.ChargeFlareCompositions, installation.LoadProjectiles().FlareCompositions);
            if (content is null || !ReferenceEquals(content, session.Game.Artwork.ProjectileCompositions))
                throw new InvalidDataException("Restarted host did not retain newly selected content across an old state load.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
            CheckBeams((BeamTileCatalog)(session.Game.Artwork.BeamArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.BeamArtwork.")), installation.LoadProjectiles().BeamTiles);
            CheckTrails((ProjectileTrailCatalog)(session.Game.Artwork.TrailArtwork ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.TrailArtwork.")), installation.LoadProjectiles().Trails);
            CheckFlare((ChargeFlarePlacementCatalog)(session.Game.Artwork.ChargeFlarePlacement ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlarePlacement.")), installation.LoadProjectiles().FlarePlacement);
        }
        File.WriteAllText(path, "invalid override");
        try
        {
            using var session = new AndroidSessionData(root);
            throw new InvalidOperationException("Invalid installed override was silently accepted.");
        }
        catch (InvalidDataException) { }
        Console.WriteLine("PASS installed Android projectile binding: startup, state load, restart with override, old-state rebind and invalid override rejection.");
        return 0;
    }

    /// <summary>Checks that the host selected the installed projectile frame binding table.</summary>
    private static void CheckFrameBindings(object? actual, ProjectileFrameBindingCatalog expected)
    {
        if (actual is not ProjectileFrameBindingCatalog catalog ||
            catalog.Resolve(0x86db) != expected.Resolve(0x86db))
            throw new InvalidDataException("Host did not bind current projectile frame choices.");
    }

    /// <summary>Compares rendered OAM for every native projectile composition pointer.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void CheckCatalog(ProjectileSpriteCatalog actual, ProjectileSpriteCatalog expected)
    {
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        {
            var a = new OamBuffer(); var b = new OamBuffer();
            actual.Draw(id, a, new(100, 100)); expected.Draw(id, b, new(100, 100));
            if (!a.LowTable.SequenceEqual(b.LowTable) || !a.HighTable.SequenceEqual(b.HighTable))
                throw new InvalidDataException("Host bound different composition content than the installed selection.");
        }
    }

    /// <summary>Checks installed trail tiles and each authored trail frame after host binding.</summary>
    private static void CheckTrails(ProjectileTrailCatalog actual, ProjectileTrailCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind trail content.");
        if (actual.Tiles is null || expected.Tiles is null ||
            !actual.Tiles.IceAndWave.Span.SequenceEqual(expected.Tiles.IceAndWave.Span) ||
            !actual.Tiles.Missile.Span.SequenceEqual(expected.Tiles.Missile.Span))
            throw new InvalidDataException("Host bound stale or missing trail PNGs.");
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
            if (actual.Resolve(frame) != expected.Resolve(frame))
                throw new InvalidDataException("Host bound stale trail appearance.");
    }

    /// <summary>Checks flare placement for both running states and every direction.</summary>
    private static void CheckFlare(ChargeFlarePlacementCatalog actual, ChargeFlarePlacementCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind flare placement.");
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < ChargeFlarePlacementDefinitions.DirectionCount; direction++)
            if (actual.Resolve(running, direction) != expected.Resolve(running, direction))
                throw new InvalidDataException("Host restored stale flare placement.");
    }

    /// <summary>Compares every charge-flare selector's rendered OAM after host binding.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void CheckFlareCompositions(ChargeFlareSpriteCatalog actual, ChargeFlareSpriteCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind flare compositions.");
        for (ushort selector = 0; selector < ChargeFlareSpriteDefinitions.Selectors.Length; selector++)
        {
            var a = new OamBuffer(); var b = new OamBuffer();
            actual.Draw(selector, a, new(100, 100)); expected.Draw(selector, b, new(100, 100));
            if (!a.LowTable.SequenceEqual(b.LowTable) || !a.HighTable.SequenceEqual(b.HighTable) || a.NextByteOffset != b.NextByteOffset)
                throw new InvalidDataException("Host restored stale flare composition content.");
        }
    }

    /// <summary>Checks Grapple flare placement, swing choices, sprite styles, and installed transfer tiles.</summary>
    private static void CheckGrapple(GrappleTileAtlas actual, GrappleTileAtlas expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind Grapple PNG.");
        CheckFlare((actual.FlarePlacement ?? throw new InvalidOperationException("Verification requires actual.FlarePlacement.")), (expected.FlarePlacement ?? throw new InvalidOperationException("Verification requires expected.FlarePlacement.")));
        if (actual.SwingFrames is null || expected.SwingFrames is null) throw new InvalidDataException("Host did not bind Grapple swing frames.");
        for (int angle = 0; angle < 256; angle++)
            if (actual.SwingFrames.Resolve((byte)angle) != expected.SwingFrames.Resolve((byte)angle))
                throw new InvalidDataException("Host restored stale Grapple swing frames.");
        if (actual.Sprites is null || expected.Sprites is null || actual.Sprites.Endpoint != expected.Sprites.Endpoint)
            throw new InvalidDataException("Host restored stale or missing Grapple endpoint style.");
        for (int frame = 0; frame < 4; frame++)
            if (actual.Sprites.Segment(frame) != expected.Sprites.Segment(frame))
                throw new InvalidDataException("Host restored stale Grapple segment style.");
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            if (!actual.Resolve(transfer.Asset).Span.SequenceEqual(expected.Resolve(transfer.Asset).Span))
                throw new InvalidDataException("Host restored stale Grapple PNG content.");
    }

    /// <summary>Checks each beam selection's installed tile data and loaded CGRAM palette.</summary>
    private static void CheckBeams(BeamTileCatalog actual, BeamTileCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind beam PNGs.");
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            if (!actual.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombinations.FromTableIndex(selection))).Span.SequenceEqual(expected.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombinations.FromTableIndex(selection))).Span))
                throw new InvalidDataException("Host restored stale beam artwork instead of current PNG selection.");
            var boundColors = new SuperMetroid.Core.Hardware.SnesCgram();
            var diskColors = new SuperMetroid.Core.Hardware.SnesCgram();
            (actual.Palettes ?? throw new InvalidDataException("Host palette catalog is absent.")).LoadTo(boundColors, SamusBeamCombinations.FromTableIndex(selection));
            (expected.Palettes ?? throw new InvalidOperationException("Verification requires expected.Palettes.")).LoadTo(diskColors, SamusBeamCombinations.FromTableIndex(selection));
            if (!new CgramPalettes(boundColors).ImageEquals(new CgramPalettes(diskColors)))
                throw new InvalidDataException("Host restored stale palette colors instead of current selection.");
        }
    }
}
