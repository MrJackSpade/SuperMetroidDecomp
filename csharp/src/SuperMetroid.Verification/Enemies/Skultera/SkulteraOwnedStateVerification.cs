using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Skultera;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Skultera state the species owns: its oscillation phase wrap and speeds.</summary>
internal static class SkulteraOwnedStateVerification
{
    /// <summary>
    /// Initializes a Skultera and confirms its speeds are the linear-speed record's halves, its radius and angle step
    /// come from parameter two, and the phase wraps through a full turn in both directions as the cartridge's
    /// low-byte sum does.
    /// </summary>
    internal static void VerifySkulteraOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.Skultera, place.Definition);
        const int speedRecord = 3;
        place.SetSpawnParameter1(EnemyDefinitionId.Skultera, speedRecord);
        place.SetSpawnParameter2(EnemyDefinitionId.Skultera, LittleEndianBytes.Word(20, 10));
        SkulteraBehavior.InitializeSkultera(place.HeldOccupant(), place);
        SkulteraEnemyState state = enemies.SkulteraStates[0]
            ?? throw new InvalidOperationException("The initialized Skultera has no state.");

        SignedSixteenSixteen right = EnemyLinearSpeedTable.Speed(speedRecord);
        SignedSixteenSixteen left = -right;
        AssertEqual(right, SkulteraBehavior.SwimVelocity(place.HeldOccupant(), HorizontalDirection.Right), "rightward speed");
        AssertEqual(left, SkulteraBehavior.SwimVelocity(place.HeldOccupant(), HorizontalDirection.Left), "leftward speed");
        SkulteraSpawnSettings settings = place.HeldOccupant().SpawnSettings<SkulteraSpawnSettings>();
        AssertEqual(new VerifySkulteraOwnedStateCase(20, 10), new VerifySkulteraOwnedStateCase(settings.Radius, settings.AngleStep.ToTrigonometryAngle().RawValue), "radius and angle step come from parameter two");

        // The wave steps forward while the Skultera swims the way it started and backward once a turn has brought it
        // the other way (#2450).
        state.Angle = RotationAngle.FromSteps(250);
        state.AdvanceAngle(settings);
        AssertEqual((byte)4, state.Angle.ToTrigonometryAngle().RawValue, "the phase wraps forward through a full turn");
        state.Heading = settings.StartDirection == HorizontalDirection.Left ? HorizontalDirection.Right : HorizontalDirection.Left;
        state.AdvanceAngle(settings);
        AssertEqual((byte)250, state.Angle.ToTrigonometryAngle().RawValue, "the phase wraps backward through a full turn");
        Console.WriteLine("Skultera owned state: speeds and phase wrap match.");
    }

    /// <summary>Runs the focused Skultera owned-state check.</summary>
    [VerificationCommand("--skultera-owned-state")]
    internal static int SkulteraOwnedStateCommand()
    {
        VerifySkulteraOwnedState();
        return 0;
    }
}
