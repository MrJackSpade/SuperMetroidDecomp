using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Installed artwork does not imply that every Torizo combat instruction is
    /// compiled yet. This independent, cartridge-backed runtime verifies that
    /// the handoff from compiled awakening to the native walking program stays
    /// playable while the ROM-free migration remains open.
    /// </summary>
    private static void VerifyGoldenTorizoCartridgeCombatFallback(
        string sourceRom, GameInstallation installation)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.Enemies.TileArtwork = installation.LoadEnemyTiles();
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GoldenTorizo);
        var samus = runtime.Samus!;
        samus.XPosition = 0x0180;
        samus.YPosition = 0x0150;
        samus.MaxHealth = 1499;
        samus.Health = 1499;
        RoomEnemySlot boss = runtime.Enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);
        bool enteredCombat = false;
        for (int frame = 0; frame < 850; frame++)
        {
            ushort input = frame is >= 500 and < 900 && frame % 20 == 0
                ? (ushort)SnesButton.X : (ushort)0;
            try
            {
                runtime.StepFrame(input);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Cartridge-backed Golden Torizo failed at frame {frame}, " +
                    $"instruction=${boss.CurrentInstruction:X4}.", error);
            }
            enteredCombat |= boss.CurrentInstruction >=
                GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg;
        }
        AssertTrue(enteredCombat,
            "cartridge-backed installed-art Golden Torizo reaches walking combat; " +
            $"instruction=${boss.CurrentInstruction:X4}, health={samus.Health}");
        Console.WriteLine("Golden Torizo cartridge fallback: 850 wake/combat frames with installed artwork.");
    }
}
