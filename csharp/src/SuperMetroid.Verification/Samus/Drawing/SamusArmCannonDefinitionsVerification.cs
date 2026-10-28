using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Drawing;
using SuperMetroid.Verification.Harness;

/// <summary>Checks arm-cannon HUD selection policy and its transition through Samus's live update.</summary>
internal static class SamusArmCannonDefinitionsVerification
{
    /// <summary>Compares six opening flags with the cartridge and checks the second-sample transition without ROM policy reads.</summary>
    internal static void VerifySamusArmCannonDefinitions(SuperMetroidAddressSpace rom)
    {
        using var artworkDirectory = new TestScratchDirectory("map-catalog");
        SuperMetroid.AssetExtraction.Samus.SamusArmCannonArtworkFiles.Extract(
            rom, artworkDirectory.Root, SupportedCartridge.Sha256);
        var artwork = SuperMetroid.AssetExtraction.Samus.SamusArmCannonArtworkFiles.Load(
            artworkDirectory.Root, null);
        foreach (HudItem selectedHudItem in Enum.GetValues<HudItem>())
        {
            byte expected = rom.ReadCartridgeByte(
                SamusArmCannonDefinitions.OpenFlagTable + (int)selectedHudItem);
            AssertEqual(
                expected,
                SamusArmCannonOpening.DesiredOpenFlag(selectedHudItem),
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
            Initial initial =
                new Initial(cannon.OpenFlag, cannon.CloseFlag, cannon.Frame);
            cannon.Update(samus);
            AssertEqual(initial, new Initial(cannon.OpenFlag, cannon.CloseFlag, cannon.Frame),
                $"arm-cannon HUD item {selectedHudItem} waits for a stable second sample");
            int openBeforeSecond = cannon.OpenFlag;
            cannon.Update(samus);
            AssertEqual(
                expected != 0,
                cannon.OpenFlag != openBeforeSecond,
                $"arm-cannon HUD item {selectedHudItem} starts only its native opening transition");
            AssertEqual(
                expected,
                cannon.OpenFlag,
                $"arm-cannon HUD item {selectedHudItem} publishes its compiled open flag");
        }

        // An imported toggle word at its top wraps to zero on a stable sample, as the native 16-bit INC does, and the
        // closed Beam selection starts no transition while it counts back up.
        var wrapping = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, SelectedHudItem = HudItem.Beam };
        PrepareRetailSamusFixture(wrapping);
        wrapping.ArmCannon.Artwork = artwork;
        wrapping.ArmCannon.ToggleFlag = ushort.MaxValue;
        wrapping.ArmCannon.Update(wrapping);
        AssertEqual(0, wrapping.ArmCannon.ToggleFlag, "a stable sample wraps the toggle word from 65535 to zero");
        wrapping.ArmCannon.Update(wrapping);
        AssertEqual(1, wrapping.ArmCannon.ToggleFlag, "the wrapped toggle word counts up again");
        AssertEqual(new VerifySamusArmCannonDefinitionsCase(0, 0, 0), new VerifySamusArmCannonDefinitionsCase(wrapping.ArmCannon.OpenFlag, wrapping.ArmCannon.CloseFlag, wrapping.ArmCannon.Frame),
            "the closed Beam selection starts no transition");
        AssertThrows<ArgumentOutOfRangeException>(() => wrapping.ArmCannon.ToggleFlag = ushort.MaxValue + 1,
            "the toggle word rejects a value above sixteen bits");
        AssertThrows<ArgumentOutOfRangeException>(() => wrapping.ArmCannon.OpenFlag = byte.MaxValue + 1,
            "the open flag rejects a value above a byte");

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusArmCannonOpening.DesiredOpenFlag(
                (HudItem)Enum.GetValues<HudItem>().Length),
            "arm-cannon policy rejects an out-of-range HUD selection");

        Console.WriteLine(
            "Arm-cannon policy: all six HUD selections match the cartridge and the real update path rejects runtime policy-table reads.");
    }

    /// <summary>
    /// Checks the cover's draw order for every pose drawing-mode byte: the drawing handler reads only the low
    /// nibble, where zero hides the cover, two draws it after the body and every other value before it.
    /// </summary>
    internal static void VerifyArmCannonDrawOrder()
    {
        var cannon = new SamusArmCannonState();
        for (int mode = 0; mode <= byte.MaxValue; mode++)
        {
            cannon.DrawingMode = (ushort)mode;
            int lowNibble = mode % 16;
            SamusArmCannonDrawOrder expected = lowNibble == 0 ? SamusArmCannonDrawOrder.Hidden
                : lowNibble == 2 ? SamusArmCannonDrawOrder.AfterBody
                : SamusArmCannonDrawOrder.BeforeBody;
            AssertEqual(expected, cannon.DrawOrder, $"arm-cannon drawing mode {mode} draw order");
        }
        Console.WriteLine("Arm-cannon draw order: all 256 drawing-mode bytes select hidden, before-body or after-body by their low nibble.");
    }

    /// <summary>Rejects live reads of the compiled arm-cannon HUD policy table.</summary>
    private static CartridgeReadGuard NewArmCannonPolicyReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address >= SamusArmCannonDefinitions.OpenFlagTable &&
                address < SamusArmCannonDefinitions.OpenFlagTable + Enum.GetValues<HudItem>().Length,
            _ => "Runtime arm-cannon HUD policy read from cartridge ROM.");

    /// <summary>Runs the arm-cannon HUD policy and transition checks.</summary>
    [VerificationCommand("--arm-cannon-definitions")]
    internal static int ArmCannonDefinitionsCommand()
    {
        Suite(nameof(VerifySamusArmCannonDefinitions), () => VerifySamusArmCannonDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifyArmCannonDrawOrder), VerifyArmCannonDrawOrder);
        return 0;
    }
}
