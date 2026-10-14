using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>Checks editable projectile trail pixels reach their exact native VRAM bytes.</summary>
internal static class ProjectileTrailAtlasVerification
{
    /// <summary>Confirms stock atlas parity and isolation of each of twelve edited trail characters.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyProjectileTrailAtlas(ISnesAddressSpace bus)
    {
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var atlas = ProjectileTrailAtlas.Load(new MemoryStream(png));
        Suite(nameof(VerifyTrailAtlasBinding), () => VerifyTrailAtlasBinding(bus, png));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        byte[] native = runtime.Vram.Bytes.ToArray();
        atlas.LoadTo(runtime.Vram);
        AssertTrue(native.SequenceEqual(runtime.Vram.Bytes), "Stock trail PNG preserves complete VRAM after production room/NMI setup");
        for (int tile = 0; tile < 12; tile++)
        {
            var image = IndexedPng.Read(new MemoryStream(png), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
            image.Pixels[tile * 8] ^= 1;
            using var changed = new MemoryStream();
            IndexedPng.Write(changed, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            changed.Position = 0;
            var edited = ProjectileTrailAtlas.Load(changed);
            var actual = new SnesVram(); actual.LoadBytes(0, native);
            edited.LoadTo(actual);
            int changedByte = tile < 8 ? ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2 + tile * 32
                : ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2 + (tile - 8) * 32;
            for (int i = 0; i < native.Length; i++)
                AssertEqual((byte)(native[i] ^ (i == changedByte ? 0x80 : 0)), actual.Bytes[i], "Trail pixel edit changes exactly its mapped bit, preserving gap and neighboring characters");
        }
        using var wrong = new MemoryStream();
        IndexedPng.Write(wrong, new(8, 8), new byte[64], SnesGraphics.DiagnosticPalette(16));
        wrong.Position = 0;
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(wrong), "Wrong trail PNG dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var invalidPixels = new byte[ProjectileTrailAtlasDefinitions.Width * ProjectileTrailAtlasDefinitions.Height];
        invalidPixels[0] = 16;
        IndexedPng.Write(invalidIndex, new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height),
            invalidPixels, SnesGraphics.DiagnosticPalette(32));
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(invalidIndex), "Trail PNG indices exceeding four bits rejected");
        AssertThrows<InvalidDataException>(() => ProjectileTrailAtlas.Load(new MemoryStream(new byte[12])), "Malformed trail PNG rejected");
        Console.WriteLine("Trail PNG: native room/NMI parity and twelve edited pixels preserve every other VRAM byte, including the interleaved graphics gap.");
    }

    /// <summary>Checks edited trail artwork survives NMI upload, room reinitialization, and debugger restore.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyTrailAtlasBinding(ISnesAddressSpace bus, byte[] png)
    {
        var image = IndexedPng.Read(new MemoryStream(png), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        image.Pixels[0] ^= 1;
        image.Pixels[64] ^= 1;
        using var stream = new MemoryStream();
        IndexedPng.Write(stream, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        stream.Position = 0;
        var atlas = ProjectileTrailAtlas.Load(stream);
        var catalog = ProjectileTrailCatalog.Load(new MemoryStream(ProjectileTrailExtractor.Extract(bus)), atlas);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        byte[] before = runtime.Vram.Bytes.ToArray();
        runtime.Artwork.TrailArtwork = catalog;
        runtime.NmiHandler.RunNmi(0, false);
        AssertTrue(before.SequenceEqual(runtime.Vram.Bytes), "Trail rebind does not change retained lag-frame VRAM");
        runtime.NmiHandler.RunNmi(0, true);
        Check(runtime);
        // Room initialization queues a new standard OBJ upload even after the first
        // host rebind is consumed. Its typed replacements must survive that upload.
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.NmiHandler.RunNmi(0, true);
        Check(runtime);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, runtime);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Runtime.SuperMetroidRuntime>(saved);
        AssertTrue(restored.Artwork.TrailArtwork is null, "State does not embed selected trail PNGs");
        BindRetailRuntimeFixture(restored);
        restored.Artwork.TrailArtwork = catalog;
        restored.NmiHandler.RunNmi(0, true);
        Check(restored);
        void Check(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            AssertTrue(target.Vram.Bytes.Slice(ProjectileTrailAtlasDefinitions.IceWaveDestinationWord * 2, atlas.IceAndWave.Length).SequenceEqual(atlas.IceAndWave.Span), "Accepted NMI installs current ice/wave trail PNG");
            AssertTrue(target.Vram.Bytes.Slice(ProjectileTrailAtlasDefinitions.MissileDestinationWord * 2, atlas.Missile.Length).SequenceEqual(atlas.Missile.Span), "Accepted NMI installs current missile trail PNG");
        }
    }
}
