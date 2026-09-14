using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Android;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static class ProjectileHostBindingVerification
{
    public static int Run(string romPath)
    {
        string root = Path.GetFullPath(Path.Combine("csharp/test-temp", "projectile-host-" + Guid.NewGuid().ToString("N")));
        var installation = GameAssetInstaller.Install(romPath, root);
        var field = typeof(SuperMetroidGame).GetField("projectileCompositions", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var beamField = typeof(SuperMetroidGame).GetField("beamArtwork", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var trailField = typeof(SuperMetroidGame).GetField("trailArtwork", BindingFlags.Instance | BindingFlags.NonPublic)!;
        using (var session = new AndroidSessionData(root))
        {
            var content = field.GetValue(session.Game);
            if (content is not ProjectileSpriteCatalog) throw new InvalidDataException("Installed Android session did not bind projectile content.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
            session.SaveSlot(0);
            session.LoadSlot(0);
            if (!ReferenceEquals(content, field.GetValue(session.Game))) throw new InvalidDataException("State load lost current host projectile content.");
            CheckBeams((BeamTileCatalog)beamField.GetValue(session.Game)!, installation.LoadProjectiles().BeamTiles);
            CheckTrails((ProjectileTrailCatalog)trailField.GetValue(session.Game)!, installation.LoadProjectiles().Trails);
        }
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string path = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        var document = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName)))!;
        var part = document["frames"]!.AsObject().First(p => p.Value!.AsArray().Count > 0).Value![0]!;
        int x = part["offsetX"]!.GetValue<int>();
        part["offsetX"] = x == 255 ? 254 : x + 1;
        File.WriteAllText(path, document.ToJsonString());
        string beamName = BeamTileAtlasDefinitions.FileName(0);
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, beamName)))
            image = IndexedPng.Read(input, 64, 8);
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, beamName)))
            IndexedPng.Write(output, 64, 8, image.Pixels, image.Palette);
        var colors = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, BeamPaletteDefinitions.FileName)))!;
        colors["palettes"]![BeamPaletteDefinitions.Key(0)]![0]!["red"] = 17;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteDefinitions.FileName), colors.ToJsonString());
        var trails = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName)))!;
        trails["frames"]![ProjectileTrailVisualDefinitions.Name(ProjectileTrailVisualDefinitions.Frames[0])]!["flipX"] = true;
        File.WriteAllText(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName), trails.ToJsonString());
        using (var input = File.OpenRead(Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            image = IndexedPng.Read(input, ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height);
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName)))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        using (var session = new AndroidSessionData(root))
        {
            var content = field.GetValue(session.Game);
            session.LoadSlot(0);
            if (content is null || !ReferenceEquals(content, field.GetValue(session.Game)))
                throw new InvalidDataException("Restarted host did not retain newly selected content across an old state load.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
            CheckBeams((BeamTileCatalog)beamField.GetValue(session.Game)!, installation.LoadProjectiles().BeamTiles);
            CheckTrails((ProjectileTrailCatalog)trailField.GetValue(session.Game)!, installation.LoadProjectiles().Trails);
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

    private static void CheckCatalog(ProjectileSpriteCatalog actual, ProjectileSpriteCatalog expected)
    {
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        {
            var a = new OamBuffer(); var b = new OamBuffer();
            actual.Draw(id, a, 100, 100); expected.Draw(id, b, 100, 100);
            if (!a.LowTable.SequenceEqual(b.LowTable) || !a.HighTable.SequenceEqual(b.HighTable))
                throw new InvalidDataException("Host bound different composition content than the installed selection.");
        }
    }

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

    private static void CheckBeams(BeamTileCatalog actual, BeamTileCatalog expected)
    {
        if (actual is null) throw new InvalidDataException("Host did not bind beam PNGs.");
        for (int selection = 0; selection < BeamTileAtlasDefinitions.SelectionCount; selection++)
        {
            if (!actual.Resolve(BeamTileCatalog.AssetFor(selection)).Span.SequenceEqual(expected.Resolve(BeamTileCatalog.AssetFor(selection)).Span))
                throw new InvalidDataException("Host restored stale beam artwork instead of current PNG selection.");
            var boundColors = new SuperMetroid.Core.Hardware.SnesCgram();
            var diskColors = new SuperMetroid.Core.Hardware.SnesCgram();
            (actual.Palettes ?? throw new InvalidDataException("Host palette catalog is absent.")).LoadTo(boundColors, selection);
            expected.Palettes!.LoadTo(diskColors, selection);
            if (!boundColors.Colors.SequenceEqual(diskColors.Colors))
                throw new InvalidDataException("Host restored stale palette colors instead of current selection.");
        }
    }
}
