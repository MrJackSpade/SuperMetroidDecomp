using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: during the escape the Crateria mainstreet's state runs room setup $8F:9194,
    // which spawns PLM $84:BB30 at block ($3D,$0B). When the critters escaped (event $0F) its
    // list opens two air blocks there, moves four blocks right and opens two more. The port
    // only ran the setup's quake, so in the 100% movie Samus's spring ball hit a bombable
    // block native had already cleared.
    /// <summary>Verifies room setup opens both Crateria mainstreet passage pairs only when the critters-escaped event is set.</summary>
    private static void VerifyCrateriaMainstreetEscapePassage()
    {
        RoomLevelData Load(bool crittersEscaped)
        {
            var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
            if (crittersEscaped)
                runtime.System.SetEvent(EventNumber.CrittersEscaped);
            runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.CrateriaMainstreet);
            runtime.StepFrame(0);
            runtime.StepFrame(0);
            return runtime.LevelData!;
        }

        ushort Word(RoomLevelData level, int x) =>
            level.GetCollisionBlockByIndex(level.GetBlockIndex(x, 0x0b)).LevelWord;

        RoomLevelData opened = Load(crittersEscaped: true);
        foreach (int x in new[] { 0x3d, 0x3e, 0x41, 0x42 })
            AssertEqual((ushort)0x00ff, Word(opened, x), $"block (${x:X2},$0B) opens when the critters escaped");

        RoomLevelData shut = Load(crittersEscaped: false);
        AssertTrue(Word(shut, 0x3d) != 0x00ff, "the passage stays shut when the critters were left behind");
        Console.WriteLine("Crateria mainstreet escape passage: setup $8F:9194 opens both pairs only after the critters escaped.");
    }
}
