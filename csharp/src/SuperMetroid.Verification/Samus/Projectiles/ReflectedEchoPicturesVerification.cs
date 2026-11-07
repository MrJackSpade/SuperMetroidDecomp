using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Checks #1559: an enemy reflecting a beam echo (a Shinespark crash's echo or the Spazer special attack's trail)
/// restarts it as the uncharged beam shot its type names, whatever combination is loaded, so every loaded combination's
/// projectile sheet holds every picture of that shot's lists. The 13% movie's Metal Pirate turned a Shinespark echo into
/// a Plasma-Wave shot under the Power Beam.
/// </summary>
internal static class ReflectedEchoPicturesVerification
{
    /// <summary>Resolves every frame of every reflected echo's lists on every loaded combination's sheet.</summary>
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
                if (SamusProjectileSelection.BeamShotProgram(reflected.BeamCombination, reflected.IsChargedBeam, direction) is not { } program)
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
        foreach (SamusBeamCombination loaded in ProjectileSheetComposer.Combinations)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(ProjectileSheetFrames.For(loaded));
            foreach (string frame in frames)
            {
                // Both fail loudly, naming the sheet, when it holds no such frame.
                if (!sheet.ShowsNothing(frame))
                    _ = sheet.Frame(frame);
            }
        }
        Console.WriteLine($"  Reflected echoes: {frames.Count} shot pictures resolve on all {ProjectileSheetComposer.Combinations.Count} loaded combinations' sheets.");
    }

    /// <summary>Every frame <paramref name="start"/> and the lists it continues into show.</summary>
    private static HashSet<SamusProjectileFrame> ShownFrames(SamusProjectileProgram start)
    {
        var shown = new HashSet<SamusProjectileFrame>();
        var reached = new HashSet<SamusProjectileProgram>();
        var pending = new Queue<SamusProjectileProgram>([start]);
        while (pending.TryDequeue(out SamusProjectileProgram program))
        {
            if (!reached.Add(program))
                continue;
            foreach (AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue> step in SamusProjectileAnimations.Programs.Steps(program))
            {
                switch (step)
                {
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep show:
                        shown.Add(show.Frame);
                        break;
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.GoToStep goTo:
                        pending.Enqueue(goTo.Target);
                        break;
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.FallIntoStep fallInto:
                        pending.Enqueue(fallInto.Target);
                        break;
                    case AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.RepeatWhileCountingStep repeat:
                        pending.Enqueue(repeat.Target);
                        break;
                    default:
                        // The other steps show nothing and continue nowhere.
                        break;
                }
            }
        }
        return shown;
    }
}
