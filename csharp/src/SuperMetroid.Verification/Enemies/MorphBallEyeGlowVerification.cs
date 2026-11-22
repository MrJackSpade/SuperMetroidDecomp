using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The Morph Ball eye's pupil glows with the face blocks in their room (#2622): the game draws the eye there in the face
/// blocks' colours, so their glow recolours its pupil. Its sheet under the room's graphics set is pre-coloured in every level
/// of the glow, which recolours its pupil.
/// </summary>
internal static class MorphBallEyeGlowVerification
{
    /// <summary>Checks the eye's sheet in the face blocks' room for their glow shades.</summary>
    internal static void VerifyMorphBallEyeShowsTheFaceBlocksGlow()
    {
        SpriteSheet shared = InstalledFixtures.EnemySheets.For(RoomEnemyGraphicsSetDefinitions.MorphBallDefault, EnemyDefinitionId.MorphBallEye);
        foreach (FaceBlockGlowLevel level in FaceBlockGlowAnimations.Levels)
        {
            PictureShade glow = FaceBlockShades.Glow(level);
            AssertTrue(shared.Image.Shades.TryGetValue(glow, out PictureRecolouring? recolouring) && recolouring is not null
                && recolouring.Substitutions.Any(substitution => substitution.Replaced.Colour != substitution.Shown),
                $"the eye in the face blocks' room is pre-coloured in their {level} glow, which recolours its pupil");
        }
    }

    /// <summary>Runs the Morph Ball eye glow check.</summary>
    [VerificationCommand("--morph-ball-eye-glow")]
    internal static int MorphBallEyeGlowCommand()
    {
        Suite(nameof(VerifyMorphBallEyeShowsTheFaceBlocksGlow), VerifyMorphBallEyeShowsTheFaceBlocksGlow);
        return 0;
    }
}
