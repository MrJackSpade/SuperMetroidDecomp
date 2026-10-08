using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySamusArmCannonDefinitions(SuperMetroidAddressSpace rom)
    {
        using var artworkDirectory = new TestTempDirectory("map-catalog");
        SuperMetroid.AssetExtraction.SamusArmCannonArtworkFiles.Extract(
            rom, artworkDirectory.Root, SupportedCartridge.Sha256);
        var artwork = SuperMetroid.AssetExtraction.SamusArmCannonArtworkFiles.Load(
            artworkDirectory.Root, null);
        for (ushort selectedHudItem = 0;
             selectedHudItem < SamusArmCannonDefinitions.HudItemCount;
             selectedHudItem++)
        {
            byte expected = rom.ReadByte(
                SamusArmCannonDefinitions.OpenFlagTable + selectedHudItem);
            AssertEqual(
                expected,
                SamusArmCannonDefinitions.DesiredOpenFlag(selectedHudItem),
                $"arm-cannon HUD item {selectedHudItem} matches cartridge table");

            var samus = new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                SelectedHudItem = selectedHudItem,
            };
            PrepareRetailSamusFixture(samus);
            samus.ArmCannon.Artwork = artwork;
            var guarded = new ArmCannonPolicyReadGuard(rom);

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

    private sealed class ArmCannonPolicyReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadByte(int address)
        {
            RejectPolicyRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectPolicyRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        private static void RejectPolicyRead(int address)
        {
            if (address >= SamusArmCannonDefinitions.OpenFlagTable &&
                address < SamusArmCannonDefinitions.OpenFlagTable +
                    SamusArmCannonDefinitions.HudItemCount)
            {
                throw new InvalidOperationException(
                    "Runtime arm-cannon HUD policy read from cartridge ROM.");
            }
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
