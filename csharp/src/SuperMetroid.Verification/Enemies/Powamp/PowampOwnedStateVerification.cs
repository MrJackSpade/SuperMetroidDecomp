using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Powamp state each half owns: the balloon's spawn and grapple rise, and the body's velocity.</summary>
internal static class PowampOwnedStateVerification
{
    /// <summary>
    /// Initializes a balloon and its body and confirms the balloon keeps its spawn and full-word grapple rise, the body
    /// starts resting, and finishing inflation gives the body exactly half a pixel per update upward (the cartridge's
    /// $FFFF:$8000).
    /// </summary>
    internal static void VerifyPowampOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The balloon and its body are placed as their room places them, whose settings name the part each is (#2163).
        RoomEnemyPlace balloon = enemies.Places.InUpdateOrder[0];
        balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
        balloon.SetSpawnParameter2(EnemyDefinitionId.Powamp, ushort.MaxValue);
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.KeptDefinition);
        balloon.Position = balloon.Position.WithWhole(new(0x0180, 0x0020));
        PowampBehavior.InitializePowampBalloon(balloon, balloon.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>());
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[1];
        body.BindPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts));
        body.Bind(EnemyDefinitionId.Powamp, body.KeptDefinition);
        // The body rests below its balloon, inside the room.
        body.Position = body.Position.WithWhole(new(0x0180, 0x0040));
        PowampBehavior.InitializePowampBody(body);

        // Parameter one selects the balloon half, which has its own state (#1845).
        PowampBalloonState balloonState = balloon.SpeciesState<PowampBalloonState>()
            ?? throw new InvalidOperationException("Parameter one did not set up the balloon half.");
        PowampBodyState bodyState = body.SpeciesState<PowampBodyState>()
            ?? throw new InvalidOperationException("The body half has no body state.");
        AssertEqual(null, balloon.SpeciesState<PowampBodyState>(), "the balloon holds no body state");
        AssertEqual(new Position16<RoomPixels>(0x0180, 0x0020), balloonState.Spawn, "the balloon keeps its spawn");
        AssertEqual(65535, balloon.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>().GrappleTravelDistance, "the grapple rise keeps parameter two's full word");
        AssertEqual(new PowampBalloonInflation(PowampBalloonSequence.Sinking, PowampBalloonPose.Halfway), balloonState.Inflation,
            "the balloon starts deflated, at the end of its sinking sequence");
        AssertEqual(PowampAction.DeflatedResting, bodyState.Action, "the body starts resting");

        bodyState.Action = PowampAction.Inflating;
        bodyState.ActionTimer = UpdateCountdown.For(1);
        enemies.PowampBehavior.RunPowampMain(body.HeldOccupant(), body, bodyState, (body.Entity as PowampBodyEntity ?? throw new InvalidOperationException("The Powamp body has no entity.")).Balloon, RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(PowampAction.InflatedRiseToTargetHeight, bodyState.Action, "inflation finishes");
        AssertEqual(SignedSixteenSixteen.FromParts(-1, 0x8000), bodyState.YVelocity, "the body rises half a pixel per update");
        Console.WriteLine("Powamp owned state: both halves' values match.");
    }

    /// <summary>Runs the focused Powamp owned-state check.</summary>
    [VerificationCommand("--powamp-owned-state")]
    internal static int PowampOwnedStateCommand()
    {
        VerifyPowampOwnedState();
        return 0;
    }
}
