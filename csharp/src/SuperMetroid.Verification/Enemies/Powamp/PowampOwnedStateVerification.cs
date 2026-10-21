using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
namespace SuperMetroid.Verification.Enemies.Powamp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Powamp state each half owns: the balloon's spawn and grapple rise, the body's velocity, and the cleared record.</summary>
internal static class PowampOwnedStateVerification
{
    /// <summary>
    /// Initializes a balloon and its body and confirms the balloon keeps its spawn and full-word grapple rise, the body
    /// starts resting, finishing inflation gives the body exactly half a pixel per update upward (the cartridge's
    /// $FFFF:$8000), and a kill empties the half's record.
    /// </summary>
    internal static void VerifyPowampOwnedState()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot balloon = enemies.SlotTable.Slots[0];
        balloon.Bind(EnemyDefinitionId.Powamp, balloon.Definition);
        balloon.SetSpawnParameter1(1);
        balloon.SetSpawnParameter2(ushort.MaxValue);
        balloon.Position = balloon.Position.WithWhole(new(0x0180, 0x0020));
        enemies.PowampBehavior.InitializePowamp(balloon);
        RoomEnemySlot body = enemies.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Powamp, body.Definition);
        enemies.PowampBehavior.InitializePowamp(body);

        PowampEnemyState balloonState = balloon.RequireSpeciesState<PowampEnemyState>();
        PowampEnemyState bodyState = body.RequireSpeciesState<PowampEnemyState>();
        AssertTrue(balloonState.IsBalloon && !bodyState.IsBalloon, "parameter one selects the balloon half");
        AssertEqual(new Position16<RoomPixels>(0x0180, 0x0020), balloonState.BalloonSpawn, "the balloon keeps its spawn");
        AssertEqual(65535, balloonState.BalloonGrappleTravelDistance, "the grapple rise keeps parameter two's full word");
        AssertEqual(PowampEnemyFunction.DeflatedResting, bodyState.Function, "the body starts resting");

        bodyState.Function = PowampEnemyFunction.Inflating;
        bodyState.FunctionTimer = WrappingCountdown.Start(1);
        enemies.PowampBehavior.RunPowampMain(body, bodyState, null);
        AssertEqual(PowampEnemyFunction.InflatedRiseToTargetHeight, bodyState.Function, "inflation finishes");
        AssertEqual(SnesSignedSixteenSixteen.FromParts(-1, 0x8000), bodyState.YVelocity, "the body rises half a pixel per update");

        body.Clear();
        AssertEqual(SnesSignedSixteenSixteen.Zero, bodyState.YVelocity, "a kill empties the body's velocity");
        AssertEqual(null, bodyState.Function, "a kill empties the body's function");
        Console.WriteLine("Powamp owned state: both halves' values and the cleared record match.");
    }

    /// <summary>Runs the focused Powamp owned-state check.</summary>
    [VerificationCommand("--powamp-owned-state")]
    internal static int PowampOwnedStateCommand()
    {
        VerifyPowampOwnedState();
        return 0;
    }
}
