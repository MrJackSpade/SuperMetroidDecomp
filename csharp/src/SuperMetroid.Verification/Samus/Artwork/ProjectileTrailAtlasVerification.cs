using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>Checks the installed projectile trail pixels hold the standard sheet's trail characters.</summary>
internal static class ProjectileTrailAtlasVerification
{
    /// <summary>Confirms stock atlas parity and rejection of malformed trail PNGs.</summary>
    internal static void VerifyProjectileTrailAtlas(ISnesAddressSpace bus)
    {
        byte[] png = ProjectileTrailAtlasExtractor.Extract(bus);
        var atlas = ProjectileTrailAtlas.Load(new MemoryStream(png));
        // The bound projectile page places the trail characters where the standard upload holds them.
        SpriteArtwork standard = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
            InstalledFixtures.StandardObjects.Characters.Span);
        int iceWaveByte = ProjectileObjectPageDefinitions.IceWaveTrailFirstCharacter * SpriteArtwork.CharacterByteCount;
        int missileByte = ProjectileObjectPageDefinitions.MissileTrailFirstCharacter * SpriteArtwork.CharacterByteCount;
        AssertTrue(standard.Characters.Slice(iceWaveByte, atlas.IceAndWave.Length).SequenceEqual(atlas.IceAndWave.Span) &&
            standard.Characters.Slice(missileByte, atlas.Missile.Length).SequenceEqual(atlas.Missile.Span),
            "Stock trail PNG reproduces the standard sheet's trail characters");
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
        Console.WriteLine("Trail PNG: native trail characters reproduced; malformed PNGs rejected.");
    }
}
