using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds actors to the rule that one update publishes at most one frame per independently moving part (#1513), as
/// <see cref="PublishedPartDraws"/> defines it, from deterministic fixtures and the draw contract rather than from play:
/// Samus's body as one update of the installed body draws it, and an enemy frame placed in several pieces as one update
/// of the enemy draw list draws it.
/// </summary>
internal static class ActorPartDrawsVerification
{
    /// <summary>Samus standing facing right, drawn once over the installed body and sheets.</summary>
    internal static void VerifySamusPartDraws()
    {
        SamusState samus = InstalledFixtures.BindSamus(new SamusState(InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
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
    /// A walking space pirate showing an extended frame its compositions place in several pieces, drawn once through the
    /// enemy draw list over its room graphics set's sheets: the slot publishes the frame as one whole picture.
    /// </summary>
    internal static void VerifyEnemyPartDraws()
    {
        ImportEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
            { BossArtwork = artwork.Bosses };
        EnemyExtendedFrameCatalog compositions = artwork.Enemies.ExtendedFrames
            ?? throw new InvalidOperationException("Verification requires the installed extended frames.");
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft;
        enemies.DrawUnder(set);
        EnemyFrameId frame = EnemyDefinitionFrameDefinitions.Frames[EnemyDefinitionId.PirateGreenWalking]
            .First(shown => compositions.TryGetDisplay(shown, out ReadOnlyMemory<EnemyExtendedDrawComponent> pieces) && pieces.Length > 1);
        RoomEnemySlot pirate = enemies.SlotTable.Slots[0];
        pirate.Bind(EnemyDefinitionId.PirateGreenWalking, default(RoomEnemyDefinition));
        pirate.CollidesByFrameHitboxes = true;
        pirate.Frame = frame;
        pirate.Position = pirate.Position.WithWhole(new(120, 110));
        enemies.Drawing.Enqueue(pirate, reached: true);

        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.Drawing.Advance(Position16<RoomPixels>.Zero);
        enemies.Drawing.Draw(sprites, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        AssertEqual(1, sprites.Draws.Length, $"the pirate showing {frame} publishes one draw");
        IReadOnlyList<string> violations = PublishedPartDraws.Violations(sprites.Draws);
        AssertTrue(violations.Count == 0,
            $"the pirate showing {frame} publishes more than one frame for a moving part:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
        var image = sprites.Draws[0] as ImageSpriteDraw
            ?? throw new InvalidOperationException($"The pirate's draw is a {sprites.Draws[0].GetType().Name}, not a sheet frame.");
        AssertTrue(ReferenceEquals(InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.PirateGreenWalking).Frame(EnemySheetDefinitions.FrameName(frame)), image.Image),
            $"the pirate shows {frame} as its sheet's one whole picture");
        Console.WriteLine($"Enemy part draws: a pirate showing {frame}, placed in several pieces, publishes it as one whole picture.");
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
