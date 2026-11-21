using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Ripper;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the grapple Ripper and jet Ripper state the variants own: the 16.16 velocity from the speed record.</summary>
internal static class RipperVariantOwnedStateVerification
{
    /// <summary>
    /// A jet Ripper initialized toward negative speed holds its record's negative half as one 16.16 velocity, a reversal
    /// loads the positive half.
    /// </summary>
    internal static void VerifyRipperVariantOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.JetRipper, place.KeptDefinition);
        const int speedRecord = 3;
        place.SetSpawnParameter1(EnemyDefinitionId.JetRipper, speedRecord);
        place.SetSpawnParameter2(EnemyDefinitionId.JetRipper, 0);
        RipperBehavior.InitializeJetRipper(place);
        RipperEnemyState state = place.SpeciesState<RipperEnemyState>()
            ?? throw new InvalidOperationException("The Ripper II has no state.");

        SignedSixteenSixteen positive = EnemyLinearSpeedTable.Speed(speedRecord);
        SignedSixteenSixteen negative = -positive;
        AssertEqual(positive, place.HeldOccupant().SpawnSettings<RipperSpawnSettings>().Speed, "the selected speed");
        SignedSixteenSixteen speed = place.HeldOccupant().SpawnSettings<RipperSpawnSettings>().Speed;
        AssertEqual(negative, RipperBehavior.FlyingVelocity(speed, state.Heading), "the negative half is loaded");
        RipperBehavior.ReverseRipperVariant(place);
        AssertEqual(positive, RipperBehavior.FlyingVelocity(speed, state.Heading), "a reversal loads the positive half");
        Console.WriteLine("Ripper variant owned state: the record's velocity halves and the reversal match.");
    }

    /// <summary>Runs the focused Ripper variant owned-state check.</summary>
    [VerificationCommand("--ripper-variant-owned-state")]
    internal static int RipperVariantOwnedStateCommand()
    {
        VerifyRipperVariantOwnedState();
        return 0;
    }
}
