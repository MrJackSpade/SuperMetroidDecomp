using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.IntegrationVerification.Gameplay;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.Sessions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks a host's binding of installed projectile art at startup and with edited overrides.</summary>
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

    /// <summary>Compares host catalogs with installed files at startup, after a restart with overrides, and with an invalid override.</summary>
    private static int Verify(GameInstallation installation)
    {
        string root = installation.Root;
        using (var session = new InstalledGameSession(root))
        {
            CheckFrameBindings(session.Game.Artwork.ProjectileFrameBindings, installation.LoadProjectiles().FrameBindings);
            CheckGrapple(session.Game.Artwork, installation.LoadProjectiles().GrappleTiles);
            CheckBeams(session.Game.Artwork.BeamPalettes ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.BeamPalettes."), installation.LoadProjectiles().BeamTiles);
            CheckFlare(session.Game.Artwork.ChargeFlarePlacement ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlarePlacement."), installation.LoadProjectiles().FlarePlacement);
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
            IndexedPng.Write(output, new(64, 8), image.Pixels, image.Colours);
        var colors = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, BeamPaletteDefinitions.FileName)));
        colors.Required("palettes").Required(BeamPaletteSources.Key(0)).Required(0)["red"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteDefinitions.FileName), colors.ToJsonString());
        var trails = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName)));
        trails.Required("frames").Required(ProjectileTrailVisualDefinitions.FrameNames[0])["flipX"] = true;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName), trails.ToJsonString());
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            image = IndexedPng.Read(input, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var flare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlarePlacementDefinitions.FileName)));
        flare.Required("offsets").Required(ChargeFlarePlacementOffsets.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlarePlacementDefinitions.FileName), flare.ToJsonString());
        var flareComposition = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ChargeFlareSpriteDefinitions.FileName)));
        flareComposition.Required("frames").Required(ChargeFlareSpriteSelection.Selectors[0].Name).Required(0)["offsetX"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlareSpriteDefinitions.FileName), flareComposition.ToJsonString());
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, GrappleTileDefinitions.FileName)))
            image = IndexedPng.Read(input, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, GrappleTileDefinitions.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var grappleStyles = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSpriteDefinitions.FileName)));
        grappleStyles.Required("endpoint")["palette"] = 4;
        foreach (var style in grappleStyles.Required("segments").AsArray().RequiredElements()) style["palette"] = 4;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSpriteDefinitions.FileName), grappleStyles.ToJsonString());
        var grappleFlare = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleFlarePlacementDefinitions.FileName)));
        grappleFlare.Required("offsets").Required(ChargeFlarePlacementOffsets.Key(false, 0))["x"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleFlarePlacementDefinitions.FileName), grappleFlare.ToJsonString());
        var grappleSwing = RequiredJson.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, GrappleSwingFrameDefinitions.FileName)));
        grappleSwing.Required("frames")[0] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, GrappleSwingFrameDefinitions.FileName), grappleSwing.ToJsonString());
        using (var session = new InstalledGameSession(root))
        {
            CheckGrapple(session.Game.Artwork, installation.LoadProjectiles().GrappleTiles);
            CheckBeams(session.Game.Artwork.BeamPalettes ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.BeamPalettes."), installation.LoadProjectiles().BeamTiles);
            CheckFlare(session.Game.Artwork.ChargeFlarePlacement ?? throw new InvalidOperationException("Verification requires session.Game.Artwork.ChargeFlarePlacement."), installation.LoadProjectiles().FlarePlacement);
        }
        File.WriteAllText(path, "invalid override");
        try
        {
            using var session = new InstalledGameSession(root);
            throw new InvalidOperationException("Invalid installed override was silently accepted.");
        }
        catch (InvalidDataException) { }
        Console.WriteLine("PASS installed projectile binding: startup, restart with override and invalid override rejection.");
        return 0;
    }

    /// <summary>Checks that the host selected the installed projectile frame binding table.</summary>
    private static void CheckFrameBindings(object? actual, InstalledProjectileFrameBindings expected)
    {
        if (actual is not InstalledProjectileFrameBindings catalog ||
            catalog.Resolve(0x86db) != expected.Resolve(0x86db))
            throw new InvalidDataException("Host did not bind current projectile frame choices.");
    }

    /// <summary>Checks flare placement for both running states and every direction.</summary>
    private static void CheckFlare(ChargeFlarePlacements actual, ChargeFlarePlacements expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind flare placement.");
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < ChargeFlarePlacementDefinitions.DirectionCount; direction++)
            if (actual.Resolve(running, direction) != expected.Resolve(running, direction))
                throw new InvalidDataException("Host restored stale flare placement.");
    }

    /// <summary>Checks the Grapple flare placement and swing choices the host bound.</summary>
    private static void CheckGrapple(GameArtwork actual, GrappleTileAtlas expected)
    {
        CheckFlare((actual.GrappleFlarePlacement ?? throw new InvalidOperationException("Verification requires actual.GrappleFlarePlacement.")), (expected.FlarePlacement ?? throw new InvalidOperationException("Verification requires expected.FlarePlacement.")));
        if (actual.GrappleSwingFrames is null || expected.SwingFrames is null) throw new InvalidDataException("Host did not bind Grapple swing frames.");
        for (int angle = 0; angle < 256; angle++)
            if (actual.GrappleSwingFrames.Resolve(new ByteAngle((byte)angle)) != expected.SwingFrames.Resolve(new ByteAngle((byte)angle)))
                throw new InvalidDataException("Host restored stale Grapple swing frames.");
    }

    /// <summary>Checks each beam selection's loaded CGRAM palette.</summary>
    private static void CheckBeams(InstalledBeamPalettes actual, BeamTileCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind beam palettes.");
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            var boundColors = new SamusColourRig();
            var diskColors = new SamusColourRig();
            actual.LoadTo(boundColors.Samus.Beam, SamusBeamCombinations.FromTableIndex(selection));
            (expected.Palettes ?? throw new InvalidOperationException("Verification requires expected.Palettes.")).LoadTo(diskColors.Samus.Beam, SamusBeamCombinations.FromTableIndex(selection));
            if (!new CgramPalettes(boundColors).ImageEquals(new CgramPalettes(diskColors)))
                throw new InvalidDataException("Host restored stale palette colors instead of current selection.");
        }
    }
}
