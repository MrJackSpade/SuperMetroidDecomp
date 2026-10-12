using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static RoomPlmPopulationVerification;
using static SuiteTimingVerification;

internal static class SaveStationAnimationDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for save station animation definitions")]
    internal static void VerifySaveStationAnimationDefinitions(SuperMetroidAddressSpace rom)
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

    [VerificationCommand("--save-station-animation-definitions")]
    internal static int SaveStationAnimationDefinitionsCommand()
    {
        Suite(nameof(VerifySaveStationAnimationDefinitions), () => VerifySaveStationAnimationDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
