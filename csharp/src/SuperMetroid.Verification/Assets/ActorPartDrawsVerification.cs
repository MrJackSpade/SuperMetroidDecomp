using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds actors to the rule that one update publishes at most one frame per independently moving part (#1513), as
/// <see cref="PublishedPartDraws"/> defines it, from deterministic fixtures and the draw contract rather than from play:
/// Samus's body as one update of the installed body draws it, and every enemy frame the installed sheets picture in
/// pieces, which the enemy draw list publishes from one slot as one draw per piece (<see cref="ExtendedFrameParts"/>).
/// </summary>
internal static class ActorPartDrawsVerification
{
    /// <summary>Samus standing facing right, drawn once over the installed body and sheets.</summary>
    internal static void VerifySamusPartDraws()
    {
        SamusState samus = InstalledFixtures.BindSamus(new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        });
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        AssertTrue(samus.Presentation.Draw(samus, sprites, new(0, 0)), "standing Samus draws her body");
        sprites.FinalizeFrame();
        AssertTrue(sprites.Draws.Length > 0, "standing Samus publishes her body");
        IReadOnlyList<string> violations = PublishedPartDraws.Violations(sprites.Draws);
        AssertTrue(violations.Count == 0,
            $"standing Samus publishes more than one frame for a moving part:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
        Console.WriteLine($"Samus part draws: standing Samus publishes {sprites.Draws.Length} draws, one frame per moving part.");
    }

    /// <summary>
    /// Every enemy whose installed sheets picture a frame in pieces publishes that frame as several draws from its one
    /// slot in one update; lists each such enemy with its frames and their piece counts.
    /// </summary>
    internal static void VerifyEnemyPartDraws()
    {
        EnemyExtendedFramePlacementCatalog placements = InstalledFixtures.EnemyTiles.ExtendedFramePlacements
            ?? throw new InvalidOperationException("Verification requires the installed extended frame placements.");
        IReadOnlyDictionary<string, ExtendedFramePart> parts = ExtendedFrameParts.Of(placements);
        var pieced = new SortedDictionary<EnemyDefinitionId, SortedSet<string>>();
        int sheets = 0;
        foreach ((EnemySheetKey key, SpriteSheet sheet) in InstalledFixtures.EnemySheets.sheets)
        {
            sheets++;
            foreach (string name in sheet.frames.Keys)
            {
                // A frame's first piece names it once; its pictures at artwork steps or rots are drawn the same way.
                if (!parts.TryGetValue(ExtendedFrameParts.PicturedFrame(name), out ExtendedFramePart part) || part.Component != 0)
                    continue;
                if (!pieced.TryGetValue(key.Enemy, out SortedSet<string>? frames))
                    pieced.Add(key.Enemy, frames = new SortedSet<string>(StringComparer.Ordinal));
                frames.Add($"{part.Frame} ({part.Components} pieces)");
            }
        }
        AssertTrue(sheets > 0, "the installation holds enemy sheets");
        AssertTrue(pieced.Count == 0,
            $"{pieced.Count} enemies publish frames in several pieces from one slot in one update:{Environment.NewLine}"
            + string.Join(Environment.NewLine, pieced.Select(entry => $"{entry.Key}: {string.Join(", ", entry.Value)}")));
        Console.WriteLine($"Enemy part draws: the {sheets} installed enemy sheets picture every frame whole, one draw per slot.");
    }

    /// <summary>Runs the actors' one-frame-per-part rule over Samus's body and the enemies' draw contract.</summary>
    [VerificationCommand("--actor-part-draws")]
    internal static int ActorPartDrawsCommand()
    {
        Suite(nameof(VerifySamusPartDraws), VerifySamusPartDraws);
        Suite(nameof(VerifyEnemyPartDraws), VerifyEnemyPartDraws);
        return 0;
    }
}
