using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroProjectileArtwork(ISnesAddressSpace bus, ProjectileSpriteCatalog stock, ProjectileSpriteCatalog edited, ushort sprite)
    {
        var intro = new IntroCinematicState(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!.Invoke(intro, null);
        typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!.Invoke(intro, null);
        var projectiles = (SamusProjectileSystem)typeof(IntroCinematicState).GetField("flashbackProjectiles", flags)!.GetValue(intro)!;
        var game = new SuperMetroidGame(bus);
        var introField = typeof(SuperMetroidGame).GetField("intro", flags)!;
        introField.SetValue(game, intro);
        byte[] Draw(IntroCinematicState scene) => ((OamBuffer)typeof(IntroCinematicState)
            .GetMethod("PrepareMotherBrainOam", flags)!.Invoke(scene, null)!).LowTable.ToArray();
        foreach (ushort family in new ushort[] { 0x10, 0x700 })
        {
            projectiles.Reset();
            var shot = projectiles.Slots[0];
            shot.Type = family; shot.InstructionPointer = 1; shot.SpritemapPointer = sprite;
            shot.XPosition = 100; shot.YPosition = 100;
            game.BindProjectileCompositions(null);
            byte[] native = Draw(intro);
            game.BindProjectileCompositions(stock);
            AssertTrue(native.SequenceEqual(Draw(intro)), "Intro stock projectile binding preserves native OAM");
            game.BindProjectileCompositions(edited);
            AssertTrue(!native.SequenceEqual(Draw(intro)), "Intro projectile/explosion passes consume edited composition");
        }
        projectiles.Reset();
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        VerifyIntroTrailPng(bus, intro, game, json, Draw);
        var trails = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var document = JsonNode.Parse(json)!;
        ushort frame = ProjectileTrailVisualDefinitions.Frames[0];
        document["frames"]![ProjectileTrailVisualDefinitions.Name(frame)]!["flipX"] = true;
        var editedTrails = ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
        var side = projectiles.TrailSlots[0].Left;
        side.InstructionPointer = (ushort)(frame + 4); side.InstructionTimer = 100;
        side.XPosition = 100; side.YPosition = 100; side.TileNumberAttributes = trails.Resolve(frame);
        byte[] nativeTrail = Draw(intro);
        game.BindTrailArtwork(trails);
        AssertTrue(nativeTrail.SequenceEqual(Draw(intro)), "Intro stock trail binding preserves native OAM");
        game.BindTrailArtwork(editedTrails);
        AssertTrue(!nativeTrail.SequenceEqual(Draw(intro)), "Intro trail pass consumes selected appearance");
        AssertEqual(97, side.InstructionTimer, "Intro preparation advances trail exactly once per call");
        game.BindTrailArtwork(null); game.BindProjectileCompositions(null);
        using var without = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindTrailArtwork(trails); game.BindProjectileCompositions(stock);
        using var with = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.ToArray().SequenceEqual(with.ToArray()), "Intro snapshot excludes host appearance catalogs");
        with.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(with);
        var restoredIntro = (IntroCinematicState)introField.GetValue(restored)!;
        AssertTrue(restoredIntro.TrailArtwork is null && restoredIntro.ProjectileCompositions is null, "Restored intro requires host content rebind");
        restored.BindTrailArtwork(editedTrails); restored.BindProjectileCompositions(edited);
        game.BindTrailArtwork(editedTrails); game.BindProjectileCompositions(edited);
        AssertTrue(Draw(intro).SequenceEqual(Draw(restoredIntro)), "Restored cinematic draws current artwork at saved animation position");
        Console.WriteLine("Intro projectile artwork: live/explosion/trail stock parity, visible edits, native timing and nonserialized state rebind pass.");
    }

    private static void VerifyIntroTrailPng(ISnesAddressSpace bus, IntroCinematicState intro,
        SuperMetroidGame game, byte[] json, Func<IntroCinematicState, byte[]> draw)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var vramField = typeof(IntroCinematicState).GetField("vram", flags)!;
        var vram = (SnesVram)vramField.GetValue(intro)!;
        byte[] native = vram.Bytes.ToArray();
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var stock = ProjectileTrailCatalog.Load(new MemoryStream(json), ProjectileTrailAtlas.Load(new MemoryStream(png)));
        game.BindTrailArtwork(stock);
        draw(intro);
        AssertTrue(native.AsSpan().SequenceEqual(vram.Bytes), "Intro stock trail PNG preserves every VRAM byte, including compressed cinematic sprites");
        var image = IndexedPng.Read(new MemoryStream(png), ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height);
        image.Pixels[0] ^= 1;
        image.Pixels[64] ^= 1;
        using var changed = new MemoryStream();
        IndexedPng.Write(changed, image.Width, image.Height, image.Pixels, image.Palette);
        changed.Position = 0;
        var edited = ProjectileTrailCatalog.Load(new MemoryStream(json), ProjectileTrailAtlas.Load(changed));
        using var saved = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, game);
        saved.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(saved);
        var restoredIntro = (IntroCinematicState)typeof(SuperMetroidGame).GetField("intro", flags)!.GetValue(restored)!;
        foreach (var pair in new[] { (game, intro), (restored, restoredIntro) })
        {
            pair.Item1.BindTrailArtwork(edited);
            var actual = (SnesVram)vramField.GetValue(pair.Item2)!;
            AssertTrue(native.AsSpan().SequenceEqual(actual.Bytes), "Intro PNG rebind does not mutate the retained display before preparation");
            draw(pair.Item2);
            for (int i = 0; i < native.Length; i++)
            {
                bool changedByte = i == ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2 ||
                    i == ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2;
                AssertEqual((byte)(native[i] ^ (changedByte ? 0x80 : 0)), actual.Bytes[i], "Intro and restored intro replace exactly the two edited trail pixels");
            }
            // Binding no replacement retains native/debug behavior without inventing
            // a ROM-backed restore on every draw. Explicitly restore stock for callers.
            pair.Item1.BindTrailArtwork(stock);
            draw(pair.Item2);
        }
        game.BindTrailArtwork(null);
    }
}
