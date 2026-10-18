using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the escape passage PLM spawned by Crateria mainstreet setup.</summary>
internal static class CrateriaMainstreetEscapePassageVerification
{
    /// <summary>Confirms setup $8F:9194 opens both block pairs only after the critters-escaped event.</summary>
    /// <remarks>#1269: PLM $84:BB30 starts at ($3D,$0B), moves four blocks right, and clears another pair. Missing it blocked Samus's spring ball in the 100% movie.</remarks>
    internal static void VerifyCrateriaMainstreetEscapePassage()
    {
        RoomLevelData Load(bool crittersEscaped)
        {
            var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
            if (crittersEscaped)
                runtime.System.SetEvent(EventNumber.CrittersEscaped);
            runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.CrateriaMainstreet);
            runtime.StepFrame(0);
            runtime.StepFrame(0);
            return runtime.Room.LoadedLevelData;
        }

        ushort Word(RoomLevelData level, int x) =>
            level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)x), 0x0b))).PackedWord.Raw;

        RoomLevelData opened = Load(crittersEscaped: true);
        foreach (int x in new[] { 0x3d, 0x3e, 0x41, 0x42 })
            AssertEqual((ushort)0x00ff, Word(opened, x), $"block (${x:X2},$0B) opens when the critters escaped");

        RoomLevelData shut = Load(crittersEscaped: false);
        AssertTrue(Word(shut, 0x3d) != 0x00ff, "the passage stays shut when the critters were left behind");
        Console.WriteLine("Crateria mainstreet escape passage: setup $8F:9194 opens both pairs only after the critters escaped.");
    }

    /// <summary>Runs the focused mainstreet escape passage verification command.</summary>
    [VerificationCommand("--crateria-mainstreet-escape-passage")]
    internal static int CrateriaMainstreetEscapePassageCommand()
    {
        VerifyCrateriaMainstreetEscapePassage();
        return 0;
    }
}
