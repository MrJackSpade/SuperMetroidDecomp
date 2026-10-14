using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;

/// <summary>Checks arm-cannon HUD selection policy and its transition through Samus's live update.</summary>
internal static class SamusArmCannonDefinitionsVerification
{
    /// <summary>Compares six opening flags with the cartridge and checks the second-sample transition without ROM policy reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus arm cannon definitions")]
    internal static void VerifySamusArmCannonDefinitions(SuperMetroidAddressSpace rom)
    {
        using var artworkDirectory = new TestScratchDirectory("map-catalog");
        SuperMetroid.AssetExtraction.SamusArmCannonArtworkFiles.Extract(
            rom, artworkDirectory.Root, SupportedCartridge.Sha256);
        var artwork = SuperMetroid.AssetExtraction.SamusArmCannonArtworkFiles.Load(
            artworkDirectory.Root, null);
        for (ushort selectedHudItem = 0;
             selectedHudItem < SamusArmCannonDefinitions.HudItemCount;
             selectedHudItem++)
        {
            byte expected = rom.ReadCartridgeByte(
                SamusArmCannonDefinitions.OpenFlagTable + selectedHudItem);
            AssertEqual(
                expected,
                SamusArmCannonDefinitions.DesiredOpenFlag(selectedHudItem),
                $"arm-cannon HUD item {selectedHudItem} matches cartridge table");

            var samus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                SelectedHudItem = selectedHudItem,
            };
            PrepareRetailSamusFixture(samus);
            samus.ArmCannon.Artwork = artwork;
            var guarded = NewArmCannonPolicyReadGuard(rom);

            // Starting a transition is the only path that rewrites the open flag; it also arms
            // the transition flag and advances the cover frame in the same call. Unchanged
            // flags and frame therefore prove that no transition began.
            SamusArmCannonState cannon = samus.ArmCannon;
            (byte Open, byte Close, ushort Frame) initial =
                (cannon.OpenFlag, cannon.CloseFlag, cannon.Frame);
            cannon.Update(guarded, samus);
            AssertEqual(initial, (cannon.OpenFlag, cannon.CloseFlag, cannon.Frame),
                $"arm-cannon HUD item {selectedHudItem} waits for a stable second sample");
            byte openBeforeSecond = cannon.OpenFlag;
            cannon.Update(guarded, samus);
            AssertEqual(
                expected != 0,
                cannon.OpenFlag != openBeforeSecond,
                $"arm-cannon HUD item {selectedHudItem} starts only its native opening transition");
            AssertEqual(
                expected,
                cannon.OpenFlag,
                $"arm-cannon HUD item {selectedHudItem} publishes its compiled open flag");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusArmCannonDefinitions.DesiredOpenFlag(
                SamusArmCannonDefinitions.HudItemCount),
            "arm-cannon policy rejects an out-of-range HUD selection");

        Console.WriteLine(
            "Arm-cannon policy: all six HUD selections match the cartridge and the real update path rejects runtime policy-table reads.");
    }

    /// <summary>Rejects live reads of the compiled arm-cannon HUD policy table.</summary>
    private static CartridgeReadGuard NewArmCannonPolicyReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address >= SamusArmCannonDefinitions.OpenFlagTable &&
                address < SamusArmCannonDefinitions.OpenFlagTable + SamusArmCannonDefinitions.HudItemCount,
            _ => "Runtime arm-cannon HUD policy read from cartridge ROM.");

    /// <summary>Runs the arm-cannon HUD policy and transition checks.</summary>
    [VerificationCommand("--arm-cannon-definitions")]
    internal static int ArmCannonDefinitionsCommand()
    {
        Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
