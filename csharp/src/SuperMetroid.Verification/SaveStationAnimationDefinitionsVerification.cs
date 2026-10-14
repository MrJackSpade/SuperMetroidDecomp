using SuperMetroid.Core.Hardware;
using static RoomPlmPopulationVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Checks the native save-station animation loop operand and its installed room PLM fixture.</summary>
internal static class SaveStationAnimationDefinitionsVerification
{
    /// <summary>Compares the native 21-loop operand, then exercises the sequential station population loader.</summary>
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

    /// <summary>Runs the retail-ROM save-station animation check from the verification command line.</summary>
    [VerificationCommand("--save-station-animation-definitions")]
    internal static int SaveStationAnimationDefinitionsCommand()
    {
        Suite(nameof(VerifySaveStationAnimationDefinitions), () => VerifySaveStationAnimationDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
