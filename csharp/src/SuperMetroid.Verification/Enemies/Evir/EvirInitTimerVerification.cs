using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Evir;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Evir's initial bobbing timer against its absolute enemy-RAM alias read.</summary>
internal static class EvirInitTimerVerification
{
    /// <summary>Confirms $A8:8811 indexes Enemy.init1+1 by speed offset, so room $D4C2's three bodies inherit high bytes from neighboring slots.</summary>
    internal static void VerifyEvirInitTimer()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.EastSandHall);

        int checkedBodies = 0;
        foreach (RoomEnemySlot slot in runtime.Enemies.SlotTable.Slots)
        {
            if (slot.DefinitionId != EnemyDefinitionId.Evir || slot.Spawn.Population.Parameter1 != 0)
                continue;
            int speedOffset = (byte)slot.Spawn.Population.Parameter2 * 8;
            int relative = 0x0fb7 + speedOffset - 0x0f78;
            RoomEnemySlot aliased = runtime.Enemies.SlotTable.Slots[relative / 0x40];
            int byteOffset = relative % 0x40;
            ushort word = byteOffset switch
            {
                0x1f => checked((ushort)aliased.PaletteIndex),
                0x3f => checked((ushort)aliased.Spawn.Population.Parameter2),
                _ => throw new InvalidOperationException($"Unexpected aliased Evir byte +{byteOffset:X2}."),
            };
            AssertEqual((ushort)((SnesLittleEndian.HighByte(word)) >> 1), (runtime.Enemies.EvirStates[slot.SlotIndex] ?? throw new InvalidOperationException("Verification requires runtime.Enemies.EvirStates[slot.SlotIndex].")).MovementTimer.RawValue,
                $"Evir body in slot {slot.SlotIndex} takes its first half-cycle from aliased enemy RAM");
            checkedBodies++;
        }
        AssertEqual(3, checkedBodies, "room $D4C2 has three Evir bodies");
        AssertEqual((ushort)1, (runtime.Enemies.EvirStates[3] ?? throw new InvalidOperationException("Verification requires runtime.Enemies.EvirStates[3].")).MovementTimer.RawValue,
            "the speed-12 body reads slot 2's palette high byte (2) halved");
        Console.WriteLine("Evir init timer: $A8:8811's Y-indexed read takes absolute enemy RAM.");
    }

    /// <summary>Runs the focused Evir enemy-RAM alias timer check.</summary>
    [VerificationCommand("--evir-init-timer")]
    internal static int EvirInitTimerCommand()
    {
        VerifyEvirInitTimer();
        return 0;
    }
}
