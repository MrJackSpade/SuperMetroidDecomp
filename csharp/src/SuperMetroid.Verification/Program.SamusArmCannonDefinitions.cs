using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusArmCannonDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<byte> compiled = SamusArmCannonDefinitions.AllOpenFlags;
        AssertEqual(
            SamusArmCannonDefinitions.HudItemCount,
            compiled.Length,
            "arm-cannon policy contains every native HUD selection");

        for (ushort selectedHudItem = 0;
             selectedHudItem < SamusArmCannonDefinitions.HudItemCount;
             selectedHudItem++)
        {
            byte expected = rom.ReadByte(
                SamusArmCannonDefinitions.OpenFlagTable + selectedHudItem);
            AssertEqual(
                expected,
                compiled[selectedHudItem],
                $"arm-cannon HUD item {selectedHudItem} matches cartridge table");

            var samus = new SamusState
            {
                Pose = SamusPoseIds.FacingRightNormalPose,
                SelectedHudItem = selectedHudItem,
            };
            var guarded = new ArmCannonPolicyReadGuard(rom);

            SamusArmCannonUpdateResult first = samus.ArmCannon.Update(guarded, samus);
            SamusArmCannonUpdateResult second = samus.ArmCannon.Update(guarded, samus);
            AssertTrue(
                !first.TransitionStarted,
                $"arm-cannon HUD item {selectedHudItem} waits for a stable second sample");
            AssertEqual(
                expected != 0,
                second.TransitionStarted,
                $"arm-cannon HUD item {selectedHudItem} starts only its native opening transition");
            AssertEqual(
                expected,
                second.OpenFlag,
                $"arm-cannon HUD item {selectedHudItem} publishes its compiled open flag");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusArmCannonDefinitions.DesiredOpenFlag(
                SamusArmCannonDefinitions.HudItemCount),
            "arm-cannon policy rejects an out-of-range HUD selection");

        Console.WriteLine(
            "Arm-cannon policy: all six HUD selections match the cartridge and the real update path rejects runtime policy-table reads.");
    }

    private sealed class ArmCannonPolicyReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= SamusArmCannonDefinitions.OpenFlagTable &&
                address < SamusArmCannonDefinitions.OpenFlagTable +
                    SamusArmCannonDefinitions.HudItemCount)
            {
                throw new InvalidOperationException(
                    "Runtime arm-cannon HUD policy read from cartridge ROM.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
