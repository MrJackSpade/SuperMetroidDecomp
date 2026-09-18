using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySaveStationAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        AssertEqual(
            rom.ReadByte(SaveStationAnimationDefinitions.NativeSaveAnimationLoopsAddress),
            (byte)SaveStationAnimationDefinitions.SaveAnimationLoops,
            "save-station animation loop count");

        VerifySequentialRoomPlmPopulationLoader();

        Console.WriteLine(
            "Save-station animation definitions: the native 21-loop operand matches the " +
            "cartridge; the real station fixture completes with the retired byte absent.");
    }
}
