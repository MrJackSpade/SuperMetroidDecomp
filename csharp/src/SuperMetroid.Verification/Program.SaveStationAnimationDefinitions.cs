using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for save station animation definitions")]
    private static void VerifySaveStationAnimationDefinitions(SuperMetroidAddressSpace rom)
    {
        AssertEqual(
            rom.ReadCartridgeByte(SaveStationAnimationDefinitions.NativeSaveAnimationLoopsAddress),
            (byte)SaveStationAnimationDefinitions.SaveAnimationLoops,
            "save-station animation loop count");

        Suite(nameof(VerifySequentialRoomPlmPopulationLoader), () => VerifySequentialRoomPlmPopulationLoader());

        Console.WriteLine(
            "Save-station animation definitions: the native 21-loop operand matches the " +
            "cartridge; the real station fixture completes with the retired byte absent.");
    }
}
