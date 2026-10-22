using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks that Samus's projectile, bomb and trail draws are frames of the bound projectile sheet (#1342, #1392).</summary>
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
    /// Asserts one entry names its source, shows the named frame of the projectile sheet the equipped beams select and
    /// is placed at its camera-relative position.
    /// </summary>
    internal static void AssertProjectileEntry(IReadOnlyList<SpriteDraw> draws, SpriteSource source, string frame,
        ProjectileDrawArtwork artwork, Position16<ScreenPixels> origin, string subject)
    {
        AssertEqual(1, draws.Count, $"{subject} publishes one whole-frame entry");
        SpriteDraw draw = draws[0];
        AssertEqual(source, draw.Source, $"{subject} names its slot as the source");
        AssertTrue(draw is ImageSpriteDraw, $"{subject} is drawn from a pre-coloured sheet");
        var image = (ImageSpriteDraw)draw;
        SamusBeamCombination beam = new SamusBeamLoadoutWord(artwork.EquippedBeams).Combination
            ?? throw new InvalidOperationException($"{subject} uses an equipped-beam word with no combination.");
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(ProjectileSheetDefinitions.For(beam));
        AssertTrue(ReferenceEquals(sheet.Image, image.Image.Sheet), $"{subject} is drawn from the {beam} beam's sheet");
        AssertEqual(sheet.Frame(frame), image.Image, $"{subject} shows its frame {frame}");
        AssertEqual(origin, draw.Origin, $"{subject} is placed at its camera-relative position");
    }
}
