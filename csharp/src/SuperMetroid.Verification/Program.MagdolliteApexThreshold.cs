using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;
using static MagdollitePhaseDefinitionsVerification;

internal static class MagdolliteApexThresholdVerification
{
    // #1269: Magdollite's .doneGrowing ($A8:B259) is reached from the maximum-rise test
    // before Y is loaded from the growth index, so it compares the arm height against
    // MagdolliteArmHeightThreshold + EnemyIndex ($A0:9073). In the 100% movie the Crateria
    // arm in slot two reads $AE9D there and grows one final pillar segment at the apex.
    internal static void VerifyMagdolliteApexThreshold()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyMagdollitePhaseDefinitions(rom);
        for (int slot = 0; slot < MagdollitePhaseDefinitions.ApexThresholdSlotCount; slot++)
        {
            int address = 0xa80000 | MagdollitePhaseDefinitions.ArmHeightThresholdTable + slot * 0x40;
            AssertEqual(ReadMagdolliteWord(rom, address), MagdollitePhaseDefinitions.ApexThreshold(slot),
                $"Magdollite apex threshold overread for slot {slot}");
        }
        AssertThrows<InvalidDataException>(
            () => MagdollitePhaseDefinitions.ApexThreshold(MagdollitePhaseDefinitions.ApexThresholdSlotCount),
            "Magdollite apex threshold beyond the enemy slots");

        // Slot two (the movie): $006C - $AE9D wraps positive, so BPL grows to phase eight.
        VerifyApex(headSlot: 1, grows: true);
        // Slot one: $006C - $20AF is negative, so the arm stops at phase seven.
        VerifyApex(headSlot: 0, grows: false);
        Console.WriteLine("Magdollite apex threshold: the maximum-rise branch compares against the enemy-index overread.");

        static void VerifyApex(int headSlot, bool grows)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewMagdolliteApexGuard(new TestAddressSpace()));
            Action<RoomEnemySlot, SamusState?> initialize = enemies.MagdolliteBehavior.InitializeMagdollite;
            Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?> rise = enemies.MagdolliteBehavior.RunMagdolliteBodyRising;
            for (int part = 0; part < 3; part++)
            {
                RoomEnemySlot member = enemies.SlotTable.Slots[headSlot + part];
                member.EnemyDefinitionPointer = EnemyDefinitionId.Magdollite;
                member.Parameter1 = (ushort)part;
                member.Parameter2 = 0;
                member.Position = member.Position.WithWhole(new(0x01d8, 0x02a0));
                initialize(member, null);
            }
            MagdolliteEnemyState head = enemies.MagdolliteStates[headSlot]!;
            head.NegativeSpeedWhole = head.NegativeSpeedFraction = 0;
            RoomEnemySlot body = enemies.SlotTable.Slots[headSlot + 1];
            MagdolliteEnemyState state = enemies.MagdolliteStates[headSlot + 1]!;
            state.Function = MagdolliteEnemyFunction.BodyRising;
            state.BodyPhaseOffset = 14;
            body.Position = body.Position.WithWholeY(0x02a0);
            state.VerticalWholeDisplacement = unchecked((ushort)-MagdollitePhaseDefinitions.MaximumRise);

            rise(body, state, null);
            string context = $"Magdollite body in slot {body.SlotIndex}";
            AssertEqual(MagdolliteEnemyFunction.NoOpAtApex, state.Function, $"{context} reaches its apex");
            AssertEqual((ushort)(grows ? 16 : 14), state.BodyPhaseOffset, $"{context} apex phase");
            AssertEqual((ushort)(grows ? 0x02a8 : 0x02a0), body.Position.Y.Whole, $"{context} apex Y");
        }
    }

    private static CartridgeReadGuard NewMagdolliteApexGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(0xa8af55, 0xa8b717 - 0xa8af55,
            address => $"Magdollite attempted a migrated threshold read ${address:X6}.");

    [VerificationCommand("--magdollite-apex-threshold")]
    internal static int MagdolliteApexThresholdCommand()
    {
        VerifyMagdolliteApexThreshold();
        return 0;
    }
}
