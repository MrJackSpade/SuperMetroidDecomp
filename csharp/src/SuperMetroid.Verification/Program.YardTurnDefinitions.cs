using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyYardTurnDefinitions(SuperMetroidAddressSpace rom)
    {
        (YardMovementFunction Movement, bool Disabled, int Address)[] records =
        [
            (YardMovementFunction.CrawlingUpsideUpMovingLeft, false, 0xa3cce2),
            (YardMovementFunction.CrawlingUpsideLeftMovingDown, false, 0xa3ccea),
            (YardMovementFunction.CrawlingUpsideDownMovingRight, false, 0xa3ccf2),
            (YardMovementFunction.CrawlingUpsideRightMovingUp, false, 0xa3ccfa),
            (YardMovementFunction.CrawlingUpsideUpMovingRight, false, 0xa3cd02),
            (YardMovementFunction.CrawlingUpsideRightMovingDown, false, 0xa3cd0a),
            (YardMovementFunction.CrawlingUpsideDownMovingLeft, false, 0xa3cd12),
            (YardMovementFunction.CrawlingUpsideLeftMovingUp, false, 0xa3cd1a),
            (YardMovementFunction.CrawlingUpsideUpMovingLeft, true, 0xa3cd22),
            (YardMovementFunction.CrawlingUpsideDownMovingRight, true, 0xa3cd2a),
            (YardMovementFunction.CrawlingUpsideUpMovingRight, true, 0xa3cd32),
            (YardMovementFunction.CrawlingUpsideDownMovingLeft, true, 0xa3cd3a),
        ];

        static ushort Word(ISnesAddressSpace source, int address) =>
            (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

        foreach ((YardMovementFunction movement, bool disabled, int address) in records)
        {
            YardTurnDefinition definition =
                YardTurnDefinitions.ForMovement(movement, disabled);
            AssertEqual(unchecked((short)Word(rom, address)), definition.LookaheadX,
                $"Yard turn {movement}/{disabled} lookahead X");
            AssertEqual(unchecked((short)Word(rom, address + 2)), definition.LookaheadY,
                $"Yard turn {movement}/{disabled} lookahead Y");
            AssertEqual(Word(rom, address + 4), definition.OutsideTurnInstructionList,
                $"Yard turn {movement}/{disabled} outside list");
            AssertEqual(Word(rom, address + 6), definition.InsideTurnInstructionList,
                $"Yard turn {movement}/{disabled} inside list");
        }

        // The four vertical crawling states do not consult the suppression flag.
        foreach (YardMovementFunction movement in new[]
        {
            YardMovementFunction.CrawlingUpsideLeftMovingDown,
            YardMovementFunction.CrawlingUpsideRightMovingUp,
            YardMovementFunction.CrawlingUpsideRightMovingDown,
            YardMovementFunction.CrawlingUpsideLeftMovingUp,
        })
        {
            AssertEqual(
                YardTurnDefinitions.ForMovement(movement, false),
                YardTurnDefinitions.ForMovement(movement, true),
                $"Yard vertical turn {movement} ignores suppression flag");
        }
        AssertThrows<InvalidDataException>(
            () => YardTurnDefinitions.ForMovement(YardMovementFunction.Airborne, false),
            "Yard airborne state has no surface-turn definition");

        VerifyYardTurnProductionBranches(rom, records);
        Console.WriteLine(
            "Yard turn definitions: all 48 native words, four suppression aliases, and " +
            "all 24 real outside/inside crawl branches pass with the complete turn table forbidden.");
    }

    private static void VerifyYardTurnProductionBranches(
        SuperMetroidAddressSpace rom,
        IReadOnlyList<(YardMovementFunction Movement, bool Disabled, int Address)> records)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var emptyWords = new ushort[64 * 64];
        var solidWords = Enumerable.Repeat((ushort)0x8000, emptyWords.Length).ToArray();
        RoomLevelData empty = CreateYardTurnLevel(emptyWords);
        RoomLevelData solid = CreateYardTurnLevel(solidWords);

        foreach ((YardMovementFunction movement, bool disabled, _) in records)
        {
            YardTurnDefinition definition =
                YardTurnDefinitions.ForMovement(movement, disabled);
            VerifyYardTurnProductionBranch(
                rom, movement, disabled, empty, definition.OutsideTurnInstructionList,
                $"Yard outside turn {movement}/{disabled}");
            VerifyYardTurnProductionBranch(
                rom, movement, disabled, solid, definition.InsideTurnInstructionList,
                $"Yard inside turn {movement}/{disabled}");
        }

        static RoomLevelData CreateYardTurnLevel(ushort[] words) =>
            new(64, 64, words, new byte[words.Length], new ushort[words.Length], new byte[8]);

        static void VerifyYardTurnProductionBranch(
            SuperMetroidAddressSpace rom,
            YardMovementFunction movement,
            bool disabled,
            RoomLevelData level,
            ushort expectedInstruction,
            string context)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new YardTurnReadGuard(rom));
            var run = typeof(RoomEnemySystem)
                .GetMethod("RunYardCrawlingMovement", flags)!
                .CreateDelegate<Action<RoomEnemySlot, YardEnemyState, RoomLevelData>>(enemies);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.XPosition = 0x0200;
            slot.YPosition = 0x0200;
            slot.XRadius = 8;
            slot.YRadius = 8;
            slot.CurrentInstruction = 0xffff;
            var state = new YardEnemyState(slot)
            {
                MovementFunction = movement,
                TurnTransitionDisabled = disabled,
                CrawlingXVelocity = 0x0100,
                CrawlingYVelocity = 0x0100,
            };

            run(slot, state, level);

            AssertEqual(expectedInstruction, slot.CurrentInstruction,
                $"{context} production instruction");
            AssertTrue(state.TurnTransitionDisabled,
                $"{context} disables the next turn transition");
            AssertEqual((ushort)0, state.TurnTransitionDisableCounter,
                $"{context} clears transition-disable counter");
        }
    }

    private sealed class YardTurnReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa3cce2 and < 0xa3cd42
            ? throw new InvalidOperationException(
                $"Yard attempted migrated turn-definition read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
