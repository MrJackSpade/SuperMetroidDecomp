using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.MamaTurtles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the asymmetric Mama Turtle shell contour and its effect
/// on sleeping-parent collision geometry.</summary>
internal static class MamaTurtleShellContourVerification
{
    /// <summary>
    /// Compares the complete asymmetric shell contour with the pinned cartridge and drives
    /// the real sleeping-parent collision path with the old source range forbidden.
    /// </summary>
    internal static void VerifyMamaTurtleShellContourDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < MamaTurtleShellContourDefinitions.EntryCount; index++)
        {
            // The right contour's word for distance zero is never read: an actor at her origin takes the left contour, so
            // the port keeps no offset for it (#2375); every other word is compared.
            if (index == MamaTurtleShellContourDefinitions.UnreadRightOfOriginWord)
                continue;
            int address = MamaTurtleShellContourDefinitions.SourceAddress + index * 2;
            short expected = unchecked((short)rom.ReadCartridgeWord(address));
            AssertEqual(expected, MamaTurtleShellContourDefinitions.GetRawOffset(index),
                $"Mama Turtle shell contour word {index}");
        }

        for (short difference = -23; difference <= 23; difference++)
        {
            int distance = Math.Abs((int)difference);
            int index = difference < 0
                ? distance + MamaTurtleShellContourDefinitions.HalfWidth
                : distance;
            AssertTrue(MamaTurtleShell.TryGetSurface(difference, out int surface),
                $"Mama Turtle shell stands over difference {difference}");
            AssertEqual(MamaTurtleShellContourDefinitions.GetRawOffset(index), surface,
                $"Mama Turtle shell contour direction/index for difference {difference}");
        }
        AssertTrue(!MamaTurtleShell.TryGetSurface(-24, out _),
            "Mama Turtle shell stands over no actor at left distance twenty-four");
        AssertTrue(!MamaTurtleShell.TryGetSurface(24, out _),
            "Mama Turtle shell stands over no actor at right distance twenty-four");

        var mama = new RoomEnemyPlace(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0200))};
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 0)) };
        Action<RoomEnemyPlace, SamusState> runAsleep = MamaTurtleBehavior.RunMamaTurtleAsleep;

        int productionCases = 0;
        for (short difference = -30; difference <= 30; difference++)
        {
            mama.Radius = mama.Radius.WithY(0xffff);
            mama.Contact = EnemyContact.None;
            mama.Processing = EnemyProcessing.None;
            mama.Presence = EnemyPresence.None;
            samus.Position = samus.Position.WithWholeX(unchecked((ushort)(mama.Position.X.Whole - difference)));
            runAsleep(mama, samus);

            if (Math.Abs((int)difference) >= MamaTurtleShellContourDefinitions.HalfWidth)
            {
                AssertEqual((ushort)0, mama.Radius.Y,
                    $"sleeping Mama ignores out-of-contour difference {difference}");
                AssertTrue(mama.Contact == EnemyContact.None && mama.Processing == EnemyProcessing.None && mama.Presence == EnemyPresence.None,
                    $"sleeping Mama remains nonsolid outside contour at {difference}");
            }
            else
            {
                AssertTrue(MamaTurtleShell.TryGetSurface(difference, out int offset), $"sleeping Mama's shell stands over {difference}");
                AssertEqual(unchecked((ushort)-offset), mama.Radius.Y,
                    $"sleeping Mama production radius at difference {difference}");
                AssertTrue(mama.Contact.HasAny(EnemyContact.SolidToSamus),
                    $"sleeping Mama production solidity at difference {difference}");
            }
            productionCases++;
        }

        Console.WriteLine(
            $"  Mama Turtle shell contour: 48 native words and {productionCases} " +
            "production distance cases pass without a ROM bus.");
    }

    /// <summary>Runs the native contour and production collision checks from the CLI.</summary>
    [VerificationCommand("--mama-turtle-shell-contour")]
    internal static int MamaTurtleShellContourCommand()
    {
        Suite(nameof(VerifyMamaTurtleShellContourDefinitions), () => VerifyMamaTurtleShellContourDefinitions());
        return 0;
    }
}
