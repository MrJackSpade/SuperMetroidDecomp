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
}
