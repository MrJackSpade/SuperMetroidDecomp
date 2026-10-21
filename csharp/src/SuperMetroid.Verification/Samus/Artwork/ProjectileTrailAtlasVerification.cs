using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        // The bound projectile page places the trail characters where the standard upload holds them.
        SpriteArtwork standard = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
            InstalledFixtures.StandardObjects.Transfer.Span);
        byte[] native = new ProjectileObjectPages(standard, InstalledFixtures.Projectiles.BeamTiles, atlas).For(SuperMetroid.Core.Progression.SamusBeamFlags.None).Characters.ToArray();
        int iceWaveByte = ProjectileObjectPageDefinitions.IceWaveTrailFirstCharacter * SpriteArtwork.CharacterByteCount;
        int missileByte = ProjectileObjectPageDefinitions.MissileTrailFirstCharacter * SpriteArtwork.CharacterByteCount;
        AssertTrue(standard.Characters.Slice(iceWaveByte, atlas.IceAndWave.Length).SequenceEqual(atlas.IceAndWave.Span) &&
            standard.Characters.Slice(missileByte, atlas.Missile.Length).SequenceEqual(atlas.Missile.Span),
            "Stock trail PNG reproduces the standard sheet's trail characters");
        for (int tile = 0; tile < 12; tile++)
        {
            var image = IndexedPng.Read(new MemoryStream(png), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
            image.Pixels[tile * 8] ^= 1;
            using var changed = new MemoryStream();
            IndexedPng.Write(changed, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            changed.Position = 0;
            var edited = ProjectileTrailAtlas.Load(changed);
            ReadOnlySpan<byte> actual = new ProjectileObjectPages(standard, InstalledFixtures.Projectiles.BeamTiles, edited).For(SuperMetroid.Core.Progression.SamusBeamFlags.None).Characters;
            int changedByte = tile < 8 ? iceWaveByte + tile * SpriteArtwork.CharacterByteCount
                : missileByte + (tile - 8) * SpriteArtwork.CharacterByteCount;
            for (int i = 0; i < native.Length; i++)
                AssertEqual((byte)(native[i] ^ (i == changedByte ? 0x80 : 0)), actual[i], "Trail pixel edit changes exactly its mapped bit, preserving gap and neighboring characters");
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
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        byte[] before = runtime.Vram.Bytes.ToArray();
        runtime.Artwork.TrailArtwork = catalog;
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        AssertTrue(before.SequenceEqual(runtime.Vram.Bytes), "Trail rebind is a page binding, not a VRAM write");
        Check(runtime);
        // Room initialization reloads the standard OBJ sheet; the bound trail characters stay on the page.
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        Check(runtime);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        using var saved = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(saved, runtime);
        saved.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Runtime.SuperMetroidRuntime>(saved);
        AssertTrue(restored.Artwork.TrailArtwork is null, "State does not embed selected trail PNGs");
        BindRetailRuntimeFixture(restored);
        restored.Artwork.TrailArtwork = catalog;
        restored.FrameBoundary.BeginAcceptedFrame(0);
        Check(restored);
        void Check(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            ReadOnlySpan<byte> page = (target.Artwork.ProjectileObjects
                ?? throw new InvalidOperationException("The runtime has no projectile page.")).For(SuperMetroid.Core.Progression.SamusBeamFlags.None).Characters;
            AssertTrue(page.Slice(ProjectileObjectPageDefinitions.IceWaveTrailFirstCharacter * SpriteArtwork.CharacterByteCount, atlas.IceAndWave.Length).SequenceEqual(atlas.IceAndWave.Span), "The bound page shows the current ice/wave trail PNG");
            AssertTrue(page.Slice(ProjectileObjectPageDefinitions.MissileTrailFirstCharacter * SpriteArtwork.CharacterByteCount, atlas.Missile.Length).SequenceEqual(atlas.Missile.Span), "The bound page shows the current missile trail PNG");
        }
    }
}
