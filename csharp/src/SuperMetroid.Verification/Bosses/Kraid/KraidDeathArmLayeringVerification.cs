using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Confirms #2646: as grown Kraid dies and sinks, his swinging arm shows in front of his body. The cartridge shows the
/// sinking arm, which the room's front tiles cover, in front of his grown belly (the player's hardware footage); the port
/// kept his grown body in front of the sprites through his death, so the arm passed behind it and read as his far hand.
/// His death now redraws his body behind the sprites.
/// </summary>
internal static class KraidDeathArmLayeringVerification
{
    /// <summary>Runs grown Kraid's death into his sink and checks his body's and his arm's depths.</summary>
    internal static void VerifyGrownKraidSinksWithArmInFrontOfBody()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        KraidEnemyState state = KraidPartsFixtures.PlaceKraid(enemies);
        state.InitializeBodyLayer();
        state.ShowBody();
        state.BringBodyInFront();
        AssertEqual(KraidBodyAppearance.InFrontOfSprites, state.BodyAppearance, "grown Kraid's body shows in front of the sprites");

        var dying = new KraidStageRun.Dying(KraidDeathPhase.ShowBody, KraidRisenHead.SelectingRoar());
        state.BodyState.Run = dying;
        var samus = RoomEnemyFrameFixtures.StandaloneSamus();
        enemies.KraidBehavior.Death.RunKraidDeathPhase(state.Parts.Body, state, dying, samus);
        AssertEqual(KraidBodyAppearance.BehindSprites, state.BodyAppearance, "his death redraws his grown body behind the sprites");
        AssertEqual(KraidDeathPhase.StartSinking, dying.Phase, "his redrawn body starts sinking next");

        enemies.KraidBehavior.Death.RunKraidDeathPhase(state.Parts.Body, state, dying, samus);
        enemies.KraidBehavior.Limbs.AdvanceKraidPartAnimation(state.ArmState);
        AssertEqual(ActorDepth.BehindFrontParts, state.ArmState.Pose?.Depth, "his sinking arm shows behind the room's front tiles");
        AssertEqual(KraidBodyAppearance.BehindSprites, state.BodyAppearance, "his body stays behind the sprites as he sinks");
        AssertTrue(GameplaySceneLayer.Order.IndexOf(GameplayLayer.ActorsBehindFrontParts) > GameplaySceneLayer.Order.IndexOf(GameplayLayer.BossBodyBehindActors),
            "the sinking arm draws in front of a body behind the sprites");
    }

    /// <summary>Runs the grown Kraid death arm layering check.</summary>
    [VerificationCommand("--kraid-death-arm-layering")]
    internal static int KraidDeathArmLayeringCommand()
    {
        Suite(nameof(VerifyGrownKraidSinksWithArmInFrontOfBody), VerifyGrownKraidSinksWithArmInFrontOfBody);
        return 0;
    }
}
