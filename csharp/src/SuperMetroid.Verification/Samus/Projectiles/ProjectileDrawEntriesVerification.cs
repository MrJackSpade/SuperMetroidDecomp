using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks that Samus's projectile, bomb and trail draws are whole-frame entries over the bound projectile page (#1342).</summary>
internal static class ProjectileDrawEntriesVerification
{
    /// <summary>Draws into a fresh frame and returns its finalized draws.</summary>
    internal static IReadOnlyList<SpriteDraw> DrawnEntries(Action<SpriteFrameBuffer> draw)
    {
        var frame = new SpriteFrameBuffer();
        frame.BeginFrame();
        draw(frame);
        frame.FinalizeFrame();
        return [.. frame.Draws];
    }

    /// <summary>
    /// Asserts one entry names its source, the given frame, the page the equipped beams bind and its camera-relative
    /// position, and that the page shows the equipped combination's beam characters.
    /// </summary>
    internal static void AssertProjectileEntry(IReadOnlyList<SpriteDraw> draws, SpriteSource source, SpriteFrame frame,
        ProjectileDrawArtwork artwork, Position16<ScreenPixels> origin, string subject)
    {
        AssertEqual(1, draws.Count, $"{subject} publishes one whole-frame entry");
        SpriteDraw draw = draws[0];
        AssertEqual(source, draw.Source, $"{subject} names its slot as the source");
        AssertTrue(ReferenceEquals(frame, draw.Frame), $"{subject} shows its composition's frame");
        AssertTrue(ReferenceEquals(artwork.Page, draw.Artwork), $"{subject} binds the equipped beams' projectile page");
        AssertEqual(origin, draw.Origin, $"{subject} is placed at its camera-relative position");
        SamusBeamCombination beam = new SamusBeamLoadoutWord(artwork.EquippedBeams).Combination
            ?? throw new InvalidOperationException($"{subject} uses an equipped-beam word with no combination.");
        ReadOnlySpan<byte> expected = InstalledFixtures.Projectiles.BeamTiles.Resolve(BeamTileCatalog.AssetFor(beam)).Span;
        AssertTrue(draw.Artwork.Characters.Slice(ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount,
            expected.Length).SequenceEqual(expected), $"{subject}'s page shows the {beam} beam characters");
    }
}
