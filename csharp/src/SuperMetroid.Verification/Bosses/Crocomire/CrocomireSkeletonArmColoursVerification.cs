using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Crocomire's skeleton shows its arm in the arm's colours (#2643), which his death loads into the skeleton arm's row, not
/// black: his skeleton-arm colours recolour the arm of every skeleton frame showing it.
/// </summary>
internal static class CrocomireSkeletonArmColoursVerification
{
    /// <summary>A skeleton frame that shows the arm held level, as the skeleton first stands.</summary>
    private const EnemyFrameId ArmHeldLevel = EnemyFrameId.CrocomireBodySkeletonArmHeldLevel;

    /// <summary>Checks that his skeleton-arm colours recolour colours of the arm frame's picture.</summary>
    internal static void VerifyCrocomireSkeletonArmShowsItsColours()
    {
        SpriteSheet sheet = InstalledFixtures.EnemySheets.For(RoomEnemyGraphicsSetDefinitions.Crocomire, EnemyDefinitionId.Crocomire);
        SpriteSheetFrame frame = sheet.Frame(EnemySheetDefinitions.FrameName(ArmHeldLevel));
        PictureRecolouring arm = frame.Sheet.Shade(CrocomireShades.SkeletonArm);
        HashSet<ColourKey> shown = [];
        for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
        for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
        {
            int pixel = y * frame.Sheet.Size.X + x;
            if (frame.Sheet.Pixels[pixel].A != 0)
                shown.Add(frame.Sheet.KeyAt(pixel));
        }
        AssertTrue(arm.Substitutions.Any(substitution => shown.Contains(substitution.Replaced) && substitution.Shown != new RgbColor(0, 0, 0)), $"his skeleton-arm colours recolour {ArmHeldLevel}'s arm in its bone colours");
    }

    /// <summary>Runs the Crocomire skeleton arm colours check.</summary>
    [VerificationCommand("--crocomire-skeleton-arm-colours")]
    internal static int CrocomireSkeletonArmColoursCommand()
    {
        Suite(nameof(VerifyCrocomireSkeletonArmShowsItsColours), VerifyCrocomireSkeletonArmShowsItsColours);
        return 0;
    }
}
