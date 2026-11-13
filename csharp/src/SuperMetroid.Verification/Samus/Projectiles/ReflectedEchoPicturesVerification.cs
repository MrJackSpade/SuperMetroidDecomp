using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Checks #1559: an enemy reflecting a beam echo (a Shinespark crash's echo or the Spazer special attack's trail)
/// restarts it as the uncharged beam shot its type names, whatever combination was equipped when the echo was released,
/// and the shot keeps the pictures of those beams (#1586), so every combination's projectile sheet holds every picture of
/// that shot's lists. The 13% movie's Metal Pirate turned a Shinespark echo released under the Power Beam into a
/// Plasma-Wave shot.
/// </summary>
internal static class ReflectedEchoPicturesVerification
{
    /// <summary>Resolves every frame of every reflected echo's lists on every combination's sheet.</summary>
    internal static void VerifyReflectedEchoPictures()
    {
        InstalledProjectileFrameBindings bindings = InstalledFixtures.Projectiles.FrameBindings;
        var frames = new HashSet<string>(StringComparer.Ordinal);
        foreach (SamusBeamEcho echo in Enum.GetValues<SamusBeamEcho>())
        {
            SamusProjectileType reflected = SamusProjectileType.EchoOf(echo);
            foreach (SamusProjectileDirection direction in Enum.GetValues<SamusProjectileDirection>())
            {
                // The program reflection restarts the echo in, as the enemy impact selects it.
                if (SamusProjectileSelection.BeamShotAnimation(reflected.BeamCombination, reflected.IsChargedBeam, direction) is not { } program)
                    continue;
                foreach (SamusProjectileFrame frame in ShownFrames(program))
                {
                    ProjectileSpriteId composition = bindings.Resolve(frame);
                    if (!composition.DrawsNothing)
                        frames.Add(ProjectileSheetFrames.ProjectileFrame(composition));
                }
            }
        }
        VerificationAssertions.AssertTrue(frames.Contains("Projectile.PlasmaWave.20"),
            "a reflected Shinespark echo shows the Plasma-Wave shot's pictures");
        foreach (SamusBeamCombination released in ProjectileSheetComposer.Combinations)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(ProjectileSheetFrames.For(released));
            foreach (string frame in frames)
            {
                // Both fail loudly, naming the sheet, when it holds no such frame.
                if (!sheet.ShowsNothing(frame))
                    _ = sheet.Frame(frame);
            }
        }
        Console.WriteLine($"  Reflected echoes: {frames.Count} shot pictures resolve on all {ProjectileSheetComposer.Combinations.Count} combinations' sheets.");
    }

    /// <summary>Every frame the animation starting at <paramref name="start"/> shows, in every segment it plays.</summary>
    private static HashSet<SamusProjectileFrame> ShownFrames(SamusProjectileSegment start) =>
        [.. SamusProjectileAnimations.StartingWith(start).Phases
            .SelectMany(phase => SamusProjectileAnimations.Segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
            .OfType<AnimationBeat<SamusProjectileFrame, NoAnimationCue>.Frame>()
            .Select(frame => frame.Shown)];
}
