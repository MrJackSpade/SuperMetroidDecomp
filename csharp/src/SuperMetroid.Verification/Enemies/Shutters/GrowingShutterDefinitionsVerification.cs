using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Shutters;

/// <summary>Checks bank $A2 growing-shutter selectors and speed records through the actual enemy initializer.</summary>
internal static class GrowingShutterDefinitionsVerification
{
    /// <summary>Compares native words and all valid initializer combinations, including wrapped section origins.</summary>
    internal static void VerifyCompiledGrowingShutters(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGrowingShutterInitialSelection), () => VerifyGrowingShutterInitialSelection(rom));
        Suite(nameof(VerifyGrowingShutterWholeSpeed), () => VerifyGrowingShutterWholeSpeed(rom));
        Suite(nameof(VerifyGrowingShutterFractionalSpeed), () => VerifyGrowingShutterFractionalSpeed(rom));
        ushort Word(int address) => SnesLittleEndian.Word(CompiledGrowingShuttersRomByte(rom, address), CompiledGrowingShuttersRomByte(rom, address + 1));
        var enemies = new RoomEnemySystem(new EnemySession());
        Action<RoomEnemySlot> initialize = ShuttersBehavior.InitializeGrowingShutter;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.ShutterGrowing, slot.Definition);
        for (ushort selector = 0; selector < 4; selector++)
        {
            for (int high = 0; high < 256; high++)
            {
                for (int speed = 0; speed < 24; speed++)
                {
                    slot.ExtraProperties = EnemyExtraPropertyWord.FromWord((ushort)(selector >> 1));
                    slot.CurrentInstruction = (ushort)(selector & 1);
                    slot.SetSpawnParameter2((ushort)((high << 8) | speed));
                    slot.Position = slot.Position.WithWholeY(selector < 2 ? (ushort)65530 : (ushort)5);
                    initialize(slot);
                    GrowingShutterEnemyState state = (enemies.GrowingShutterStates[0] ?? throw new InvalidOperationException("Verification requires enemies.GrowingShutterStates[0]."));
                    AssertEqual(Word(0xa2ea4e + selector * 2), ShuttersNativeWords.GrowingShutterFunctions.Word(state.Function ?? throw new InvalidOperationException("The initialized shutter has no behaviour.")), "Shutter real dispatch without bus");
                    AssertEqual(unchecked((short)Word(0xa2ea56 + speed * 4)), state.GrowthSpeed.WholePixels, "Shutter native whole speed");
                    AssertEqual(Word(0xa2ea58 + speed * 4), state.GrowthSpeed.Fraction, "Shutter native fractional speed");
                    int direction = selector < 2 ? 1 : -1;
                    for (int level = 0; level < GrowingShutterEnemyState.SectionCount; level++)
                        AssertEqual(unchecked((ushort)(slot.Position.Y.Whole + direction * 8 * level)), state.SectionOrigin(level).Y, $"Shutter wrapped origin {level}");
                    AssertEqual((ushort)0, slot.ExtraProperties.ToWord(), "Shutter consumes initial direction word");
                }
            }
        }
        AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.InitialFunction(4), "Shutter invalid initial selector");
        AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.Speed(24), "Shutter speed outside authored records");
        Console.WriteLine("Growing shutter definitions: 52 native words, 24576 real initializers and wrapped section origins match without a bus.");
    }

    /// <summary>Reads cartridge bytes used as the compiled shutter comparison oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled growing shutters")]
    private static byte CompiledGrowingShuttersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks all four native initial dispatch pointers and rejects out-of-range selectors.</summary>
    private static void VerifyGrowingShutterInitialSelection(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            int address = 0xa2ea4e + 2 * index;
            ushort native = SnesLittleEndian.Word(GrowingShutterInitialSelectionRomByte(rom, address), GrowingShutterInitialSelectionRomByte(rom, address + 1));
            AssertEqual(native, ShuttersNativeWords.GrowingShutterFunctions.Word(GrowingShutterDefinitions.InitialFunction(index)),
                "growing shutter named initial case matches original pointer");
        }
        foreach (int index in new[] { int.MinValue, -1, 4, 5, 65536, int.MaxValue })
            AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.InitialFunction(index),
                "growing shutter initial selector rejects invalid full-width inputs");
    }

    /// <summary>Reads a native initial dispatch pointer byte for selector verification.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for growing shutter initial selection")]
    private static byte GrowingShutterInitialSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks signed whole-pixel shutter velocities across high-byte selector aliases.</summary>
    private static void VerifyGrowingShutterWholeSpeed(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrowingShutterSpeedField), () => VerifyGrowingShutterSpeedField(rom, fractional: false));

    /// <summary>Checks unsigned fractional shutter velocities across high-byte selector aliases.</summary>
    private static void VerifyGrowingShutterFractionalSpeed(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyGrowingShutterSpeedField), () => VerifyGrowingShutterSpeedField(rom, fractional: true));

    /// <summary>Compares one native speed word for each authored index and rejects invalid low bytes.</summary>
    private static void VerifyGrowingShutterSpeedField(SuperMetroidAddressSpace rom, bool fractional)
    {
        for (int index = 0; index < 24; index++)
        {
            int address = 0xa2ea56 + 4 * index + (fractional ? 2 : 0);
            ushort native = SnesLittleEndian.Word(GrowingShutterSpeedFieldRomByte(rom, address), GrowingShutterSpeedFieldRomByte(rom, address + 1));
            for (int high = 0; high < 256; high++)
            {
                var speed = GrowingShutterDefinitions.Speed((ushort)((high << 8) | index));
                if (fractional)
                    AssertEqual(native, speed.Fraction, "growing shutter native unsigned fraction and high-byte aliases");
                else
                    AssertEqual(unchecked((short)native), speed.WholePixels, "growing shutter native signed whole word and high-byte aliases");
            }
        }
        for (int high = 0; high < 256; high++)
            foreach (int low in new[] { 24, 25, 127, 255 })
                AssertThrows<InvalidDataException>(() => GrowingShutterDefinitions.Speed((ushort)((high << 8) | low)),
                    "growing shutter speed rejects out-of-domain low bytes regardless of high byte");
    }

    /// <summary>Reads a native velocity byte for the shutter speed-field comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for growing shutter speed field")]
    private static byte GrowingShutterSpeedFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
