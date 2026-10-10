using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: InitAI_Evir loads its first bobbing half-cycle from Enemy.init1+1 indexed by Y,
    // the speed-table offset, reading absolute enemy RAM. In room $D4C2 a speed-8 body reads
    // slot 1's zero init1 high byte, while speed-12 bodies read slot 2's palette high byte (2).
    private static void VerifyEvirInitTimer()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xd4c2);

        int checkedBodies = 0;
        foreach (RoomEnemySlot slot in runtime.Enemies.Slots)
        {
            if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Evir || slot.Parameter1 != 0)
                continue;
            int speedOffset = (byte)slot.Parameter2 * 8;
            int relative = 0x0fb7 + speedOffset - 0x0f78;
            RoomEnemySlot aliased = runtime.Enemies.Slots[relative / 0x40];
            int byteOffset = relative % 0x40;
            ushort word = byteOffset switch
            {
                0x1f => aliased.PaletteIndex,
                0x3f => aliased.Parameter2,
                _ => throw new InvalidOperationException($"Unexpected aliased Evir byte +{byteOffset:X2}."),
            };
            AssertEqual((ushort)((word >> 8) >> 1), runtime.Enemies.EvirStates[slot.SlotIndex]!.MovementTimer,
                $"Evir body in slot {slot.SlotIndex} takes its first half-cycle from aliased enemy RAM");
            checkedBodies++;
        }
        AssertEqual(3, checkedBodies, "room $D4C2 has three Evir bodies");
        AssertEqual((ushort)1, runtime.Enemies.EvirStates[3]!.MovementTimer,
            "the speed-12 body reads slot 2's palette high byte (2) halved");
        Console.WriteLine("Evir init timer: $A8:8811's Y-indexed read takes absolute enemy RAM.");
    }
}
