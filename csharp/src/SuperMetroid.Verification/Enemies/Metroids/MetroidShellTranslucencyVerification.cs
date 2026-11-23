using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>
/// A Metroid's shell is drawn translucent on every update (#2642): the game shows it only every other update, which a
/// display whose refresh does not match the game's showed as the shell pulsing in and out of sight. Through the empty frames
/// its animation still flickers between, the shell keeps showing its last picture at half strength.
/// </summary>
internal static class MetroidShellTranslucencyVerification
{
    /// <summary>How many updates the check draws the shell for: well past its first pass's alternating frames.</summary>
    private const int Updates = 40;

    /// <summary>Checks that each update draws the shell once, translucent.</summary>
    internal static void VerifyMetroidShellDrawsTranslucentEveryUpdate()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), installedBosses: InstalledFixtures.BossArtwork);
        enemies.DrawUnder(SpawnedObjectFixtures.SetLoading(EnemyDefinitionId.Metroid));
        var enemy = enemies.Places.InUpdateOrder[0];
        enemy.Position = enemy.Position.WithWhole(new(120, 100));
        enemy.Bind(EnemyDefinitionId.Metroid, enemy.KeptDefinition);
        enemy.Shading = PictureShading.Standard;
        enemy.SetSpawnWords(EnemyDefinitionId.Metroid, words => words);
        MetroidEnemyState state = enemies.MetroidBehavior.InitializeMetroid(enemy.HeldOccupant(), enemy.HeldOccupant().SpawnSettings<MetroidSpawnSettings>(), enemy)
            ?? throw new InvalidOperationException("The Metroid's setup built no state.");
        Offset16<ScreenPixels> place = state.Shell.Position.Whole.RelativeToCamera(Position16<RoomPixels>.Zero);
        for (int update = 0; update < Updates; update++)
        {
            var drawn = new SpriteFrameBuffer();
            drawn.BeginFrame();
            enemies.EffectActors.DrawRoomEffectActors(drawn, Position16<RoomPixels>.Zero, EnemyFormatDrawAccess.HitBurstSheets);
            drawn.FinalizeFrame();
            ImageSpriteDraw[] shell = [.. drawn.Draws.OfType<ImageSpriteDraw>().Where(draw => draw.Source.Kind == SpriteSourceKind.RoomEffectActor && draw.Translucent)];
            AssertTrue(shell.Length == 1 && shell[0].Origin == place, $"update {update} draws the shell once, translucent, where it stands");
            enemies.EffectActors.StepRoomEffectActors();
        }
    }

    /// <summary>Runs the Metroid shell translucency check.</summary>
    [VerificationCommand("--metroid-shell-translucency")]
    internal static int MetroidShellTranslucencyCommand()
    {
        Suite(nameof(VerifyMetroidShellDrawsTranslucentEveryUpdate), VerifyMetroidShellDrawsTranslucentEveryUpdate);
        return 0;
    }
}
