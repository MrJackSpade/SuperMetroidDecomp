using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Compares surface-turn tables with cartridge data and exercises production crawl branches with those tables guarded.</summary>
    /// <param name="rom">Retail ROM supplying the independent turn-definition words.</param>
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

        Suite(nameof(VerifyYardTurnProductionBranches), () => VerifyYardTurnProductionBranches(rom, records));
        Console.WriteLine(
            "Yard turn definitions: all 48 native words, four suppression aliases, and " +
            "all 24 real outside/inside crawl branches pass with the complete turn table forbidden.");
    }

    /// <summary>Runs both outside- and inside-turn branches for each listed crawling state using controlled room tiles.</summary>
    /// <param name="rom">ROM backing the guard that rejects runtime reads of the migrated turn table.</param>
    /// <param name="records">Movement states and native table addresses used to select each branch's expected instruction.</param>
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

    /// <summary>Detects runtime reads from the migrated Yard turn-definition table while forwarding other access.</summary>
    /// <param name="source">Underlying address space used outside the guarded table range.</param>
    private sealed class YardTurnReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-import reads through the guarded byte-read implementation.</summary>
        /// <param name="address">Cartridge address of the requested byte.</param>
        /// <returns>The underlying byte when it is outside the guarded table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated turn table and delegates all other reads.</summary>
        /// <param name="address">Address of the requested byte.</param>
        /// <returns>The underlying byte for an address outside the turn table.</returns>
        public byte ReadByte(int address) => address is >= 0xa3cce2 and < 0xa3cd42
            ? throw new InvalidOperationException(
                $"Yard attempted migrated turn-definition read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards writes to the wrapped address space unchanged.</summary>
        /// <param name="address">Destination address for the byte.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
