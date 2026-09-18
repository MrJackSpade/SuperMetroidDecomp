using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyHibashiDefinitions(SuperMetroidAddressSpace rom)
    {
        const int yOffsetTable = 0xa68dbb;
        const int yRadiusTable = 0xa68de7;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new HibashiDefinitionReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeHibashi", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        var applyFrame = typeof(RoomEnemySystem).GetMethod("ApplyHibashiActivityFrame", flags)!
            .CreateDelegate<Action<RoomEnemySlot, int>>(enemies);

        RoomEnemySlot graphics = enemies.Slots[0];
        graphics.Parameter2 = 0;
        graphics.YPosition = 0x0200;
        initialize(graphics);
        RoomEnemySlot hitbox = enemies.Slots[1];
        hitbox.Parameter2 = 1;
        hitbox.YPosition = graphics.YPosition;
        initialize(hitbox);

        for (int frameIndex = 0; frameIndex < 22; frameIndex++)
        {
            HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
            AssertEqual(ReadHibashiWord(rom, yOffsetTable + frameIndex * 2), frame.YOffset,
                $"Hibashi Y offset {frameIndex}");
            AssertEqual(ReadHibashiWord(rom, yRadiusTable + frameIndex * 2), frame.YRadius,
                $"Hibashi Y radius {frameIndex}");

            hitbox.XRadius = 0;
            applyFrame(graphics, frameIndex);
            AssertEqual(unchecked((ushort)(0x0200 - frame.YOffset)), hitbox.YPosition,
                $"Hibashi production Y position {frameIndex}");
            AssertEqual(frame.YRadius, hitbox.YRadius,
                $"Hibashi production Y radius {frameIndex}");
            AssertEqual(frameIndex, enemies.LastHibashiActivityFrameIndex!.Value,
                $"Hibashi production frame publication {frameIndex}");
            AssertEqual(frameIndex == 0 ? (ushort)8 : (ushort)0, hitbox.XRadius,
                $"Hibashi production X radius {frameIndex}");
        }

        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(-1),
            "Hibashi negative activity index");
        AssertThrows<ArgumentOutOfRangeException>(() => HibashiDefinitions.ActivityFrame(22),
            "Hibashi activity index beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => applyFrame(graphics, 22),
            "Hibashi production activity index beyond authored table");

        Console.WriteLine(
            "Hibashi definitions: 44 native words and all 22 real activity-frame hitboxes pass with table reads forbidden.");
    }

    private static ushort ReadHibashiWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class HibashiDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa68dbb and < 0xa68e13
            ? throw new InvalidOperationException(
                $"Hibashi attempted migrated hitbox read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
