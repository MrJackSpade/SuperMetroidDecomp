using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
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
        // The bound projectile page places the trail characters where the standard upload holds them.
        SpriteArtwork standard = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
            InstalledFixtures.StandardObjects.Characters.Span);
        byte[] native = new ProjectileObjectPages(standard, InstalledFixtures.Projectiles.BeamTiles, atlas).For(SuperMetroid.Core.Progression.SamusBeamCombination.Power).Characters.ToArray();
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
            IndexedPng.Write(changed, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            changed.Position = 0;
            var edited = ProjectileTrailAtlas.Load(changed);
            ReadOnlySpan<byte> actual = new ProjectileObjectPages(standard, InstalledFixtures.Projectiles.BeamTiles, edited).For(SuperMetroid.Core.Progression.SamusBeamCombination.Power).Characters;
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
}
