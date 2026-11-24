using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Norfair Ridley's tail and wings flash with his hurt flash (#2658): the game draws them with no palette bits while he
/// flashes ($A6:D4EF), so they show their parts' own row, and his room's shared sheet recolours every colour of their
/// pictures under the hit flash. The import folded his row into their parts, so the flash found them already in it and
/// left them as they were.
/// </summary>
internal static class RidleyTailAndWingsFlashVerification
{
    /// <summary>His tail's tip, links and wings in every pose, facing either way.</summary>
    private static readonly EnemyFrameId[] Frames =
    [
        .. RidleySupplementalVisualDefinitions.TailTipFrames.InTurnOrder,
        .. RidleySupplementalVisualDefinitions.LinkFrames,
        .. RidleySupplementalVisualDefinitions.WingBeat.Select(pose => pose.FacingLeft),
        .. RidleySupplementalVisualDefinitions.WingBeat.Select(pose => pose.FacingRight),
    ];

    /// <summary>Checks that the hit flash recolours every colour of each tail and wing picture, changing some.</summary>
    internal static void VerifyRidleyTailAndWingsShowHisHurtFlash()
    {
        SpawnedObjectSheet shared = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.Ridley, SpawnedObjectOwner.Shared);
        foreach (EnemyFrameId frameId in Frames.Distinct())
        {
            SpriteSheetFrame frame = shared.Frame(EnemySheetDefinitions.EncounterFrameName(frameId));
            PictureRecolouring flash = frame.Sheet.Shade(EnemyShades.HurtFlash);
            Dictionary<ColourKey, RgbColor> flashed = flash.Substitutions.ToDictionary(substitution => substitution.Replaced, substitution => substitution.Shown);
            HashSet<ColourKey> shown = [];
            for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
            {
                int pixel = y * frame.Sheet.Size.X + x;
                if (frame.Sheet.Pixels[pixel].A != 0)
                    shown.Add(frame.Sheet.KeyAt(pixel));
            }
            ColourKey[] unflashed = [.. shown.Where(key => !flashed.ContainsKey(key))];
            AssertTrue(shown.Count > 0 && unflashed.Length == 0,
                $"his hurt flash recolours every colour of {frameId}'s picture; {unflashed.Length} of {shown.Count} are not ({string.Join(", ", unflashed)})");
            AssertTrue(shown.Any(key => flashed[key] != key.Colour), $"his hurt flash changes {frameId}'s colours");
        }
        Console.WriteLine("  Ridley tail and wings: his hurt flash recolours every tail and wing picture.");
    }

    /// <summary>Runs the Ridley tail and wings flash check.</summary>
    [VerificationCommand("--ridley-tail-wings-flash")]
    internal static int RidleyTailAndWingsFlashCommand()
    {
        Suite(nameof(VerifyRidleyTailAndWingsShowHisHurtFlash), VerifyRidleyTailAndWingsShowHisHurtFlash);
        return 0;
    }
}
