using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks the save-station animation loop operand against the cartridge and runs the sequential room-PLM population verification.</summary>
    private static void VerifySaveStationAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        AssertEqual(
            rom.ReadByte(SaveStationAnimationDefinitions.NativeSaveAnimationLoopsAddress),
            (byte)SaveStationAnimationDefinitions.SaveAnimationLoops,
            "save-station animation loop count");

        Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());

        Console.WriteLine(
            "Save-station animation definitions: the native 21-loop operand matches the " +
            "cartridge; the real station fixture completes with the retired byte absent.");
    }
}
