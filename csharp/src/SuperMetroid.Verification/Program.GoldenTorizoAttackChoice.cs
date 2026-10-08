using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: Instruction_GoldenTorizo_CallY_OrY2_ForAttack ($AA:D53B) adds NMI_FrameCounter
    // ($05B6), not the 8-bit counter at $05B5. In the 100% movie the two disagree in bit 3
    // when the Torizo lands from a backward jump: native throws sonic booms, not chozo orbs.
    private static void VerifyGoldenTorizoAttackChoice()
    {
        const ushort landedFacingLeft = 0xcdaf;
        const ushort sonicBoomsFacingLeft = 0xcbed;
        const ushort chozoOrbsFacingLeft = 0xcb41;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.GoldenTorizo);
        var samus = runtime.Samus!;
        RoomEnemySlot torizo = runtime.Enemies.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.GoldenTorizoDefinition);

        // Native inputs at the movie's landing: (X >> 1) + carry + $05B6 has bit 3 clear,
        // while the same sum with $05B5 ($2D) has it set.
        samus.XPosition = 0x01b8;
        samus.Missiles = 187;
        torizo.CurrentInstruction = landedFacingLeft;
        torizo.InstructionTimer = 1;
        runtime.Enemies.StepFrame(
            runtime.Camera!.XPosition,
            runtime.Camera.YPosition,
            timeIsFrozen: false,
            samus,
            0,
            runtime.LevelData,
            0,
            runtime.Projectiles,
            nmiFrameCounter8: 0x2d,
            nmiFrameCounter: 0xefd8);
        AssertTrue(torizo.CurrentInstruction > sonicBoomsFacingLeft &&
                torizo.CurrentInstruction < 0xcc57,
            $"NMI_FrameCounter selects the sonic-boom list, not chozo orbs (${chozoOrbsFacingLeft:X4}); " +
            $"instruction ${torizo.CurrentInstruction:X4}");
        Console.WriteLine("Golden Torizo attack choice: the landing chooser adds NMI_FrameCounter, not the 8-bit counter.");
    }
}
