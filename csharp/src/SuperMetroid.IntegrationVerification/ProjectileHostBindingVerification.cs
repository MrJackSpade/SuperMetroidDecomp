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
        using (var session = new AndroidSessionData(root))
        {
            var content = field.GetValue(session.Game);
            if (content is not ProjectileSpriteCatalog) throw new InvalidDataException("Installed Android session did not bind projectile content.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
            session.SaveSlot(0);
            session.LoadSlot(0);
            if (!ReferenceEquals(content, field.GetValue(session.Game))) throw new InvalidDataException("State load lost current host projectile content.");
        }
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string path = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        var document = JsonNode.Parse(File.ReadAllText(Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName)))!;
        var part = document["frames"]!.AsObject().First(p => p.Value!.AsArray().Count > 0).Value![0]!;
        int x = part["offsetX"]!.GetValue<int>();
        part["offsetX"] = x == 255 ? 254 : x + 1;
        File.WriteAllText(path, document.ToJsonString());
        using (var session = new AndroidSessionData(root))
        {
            var content = field.GetValue(session.Game);
            session.LoadSlot(0);
            if (content is null || !ReferenceEquals(content, field.GetValue(session.Game)))
                throw new InvalidDataException("Restarted host did not retain newly selected content across an old state load.");
            CheckCatalog((ProjectileSpriteCatalog)content, installation.LoadProjectiles().Catalog);
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
}
