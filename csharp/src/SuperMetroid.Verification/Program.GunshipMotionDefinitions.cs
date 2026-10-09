using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the migrated gunship brake and hover tables against cartridge bytes and exercises their production consumers without permitting those consumers to reread the tables.</summary>
    /// <param name="rom">Cartridge address space providing the original motion-table bytes and unrelated room-enemy data.</param>
    private static void VerifyGunshipMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGunshipBrakeAlgorithm), () => VerifyGunshipBrakeAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverTimerAlgorithm), () => VerifyGunshipHoverTimerAlgorithm(rom));
        Suite(nameof(VerifyGunshipHoverDeltaSelection), () => VerifyGunshipHoverDeltaSelection(rom));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var brakeMethod = typeof(RoomEnemySystem).GetMethod("BouncePostCeresGunship", flags)!;
        var bobMethod = typeof(RoomEnemySystem).GetMethod("StepGunshipBob", flags)!;

        for (ushort frame = 0; frame < 17; frame++)
        {
            short expected = unchecked((short)ReadGunshipMotionWord(
                rom,
                0xa2a622 + frame * 2));

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new GunshipMotionDefinitionReadGuard(rom));
            var applyBrake = brakeMethod.CreateDelegate<Action<RoomEnemySlot, SamusState>>(enemies);
            RoomEnemySlot top = enemies.Slots[0];
            top.XPosition = 0x1234;
            top.YPosition = 0x2000;
            top.VariableE = frame;
            enemies.Slots[1].YPosition = 0x2100;
            enemies.Slots[2].YPosition = 0x2200;
            var samus = new SamusState { YPosition = 0x3000 };

            applyBrake(top, samus);

            AssertEqual(unchecked((ushort)(0x3000 + expected)), samus.YPosition,
                $"gunship production brake Samus Y {frame}");
            AssertEqual(unchecked((ushort)(0x2000 + expected)), top.YPosition,
                $"gunship production brake top Y {frame}");
            AssertEqual(unchecked((ushort)(0x2100 + expected)), enemies.Slots[1].YPosition,
                $"gunship production brake bottom Y {frame}");
            AssertEqual(unchecked((ushort)(0x2200 + expected)), enemies.Slots[2].YPosition,
                $"gunship production brake pad Y {frame}");
        }

        for (ushort phase = 0; phase < 4; phase++)
        {
            int address = 0xa2a7cf + phase * 2;
            byte expectedTimer = rom.ReadByte(address);
            sbyte expectedDelta = unchecked((sbyte)rom.ReadByte(address + 1));

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new GunshipMotionDefinitionReadGuard(rom));
            var stepBob = bobMethod.CreateDelegate<Action<RoomEnemySlot>>(enemies);
            RoomEnemySlot top = enemies.Slots[0];
            top.VariableC = phase;
            top.VariableD = 1;
            top.YPosition = 0x2000;
            enemies.Slots[1].YPosition = 0x2100;
            enemies.Slots[2].YPosition = 0x2200;

            stepBob(top);

            AssertEqual((ushort)expectedTimer, top.VariableD,
                $"gunship production bob timer {phase}");
            AssertEqual(unchecked((ushort)((phase + 1) & 3)), top.VariableC,
                $"gunship production bob phase {phase}");
            AssertEqual(unchecked((ushort)(0x2000 + expectedDelta)), top.YPosition,
                $"gunship production bob top Y {phase}");
            AssertEqual(unchecked((ushort)(0x2100 + expectedDelta)), enemies.Slots[1].YPosition,
                $"gunship production bob bottom Y {phase}");
            AssertEqual(unchecked((ushort)(0x2200 + expectedDelta)), enemies.Slots[2].YPosition,
                $"gunship production bob pad Y {phase}");
        }

        Console.WriteLine(
            "Gunship motion definitions: all seventeen brake deltas, four idle-bob records, and both production consumers pass with source reads forbidden.");
    }

    /// <summary>Compares every supported landing-brake delta with the original table and checks that invalid frame indices are rejected.</summary>
    /// <param name="rom">Address space containing the cartridge's reference brake deltas.</param>
    private static void VerifyGunshipBrakeAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort frame = 0; frame < 17; frame++)
            AssertEqual(unchecked((short)ReadGunshipMotionWord(rom,
                    GunshipMotionDefinitions.BrakeReferenceAddress + 2 * frame)),
                GunshipMotionDefinitions.LandingBrakeYDelta(frame), $"Original gunship brake {frame}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.LandingBrakeYDelta(17), "Brake upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.LandingBrakeYDelta(ushort.MaxValue), "Brake invalid maximum");
    }

    /// <summary>Checks the four idle-bob timer values against the cartridge table and verifies the phase range enforced by the catalog.</summary>
    /// <param name="rom">Address space containing the cartridge's reference hover records.</param>
    private static void VerifyGunshipHoverTimerAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(rom.ReadByte(GunshipMotionDefinitions.HoverTimerReferenceAddress + 2 * phase),
                GunshipMotionDefinitions.IdleBob(phase).Timer, $"Original gunship hover timer {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(4), "Hover timer upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(ushort.MaxValue), "Hover timer invalid maximum");
    }

    /// <summary>Checks that each idle-bob phase selects the matching signed vertical delta from the cartridge table.</summary>
    /// <param name="rom">Address space containing the cartridge's reference hover records.</param>
    private static void VerifyGunshipHoverDeltaSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort phase = 0; phase < 4; phase++)
            AssertEqual(unchecked((sbyte)rom.ReadByte(GunshipMotionDefinitions.HoverDeltaReferenceAddress + 2 * phase)),
                GunshipMotionDefinitions.IdleBob(phase).YDelta, $"Original gunship hover delta {phase}");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(4), "Hover delta upper bound");
        AssertThrows<InvalidDataException>(() => GunshipMotionDefinitions.IdleBob(ushort.MaxValue), "Hover delta invalid maximum");
    }
    /// <summary>Reads one little-endian 16-bit reference value from the cartridge image.</summary>
    /// <param name="bus">Address space holding the original motion table.</param>
    /// <param name="address">Address of the low byte; the high byte is read from the following address.</param>
    /// <returns>The two bytes combined with the low byte in the least significant position.</returns>
    private static ushort ReadGunshipMotionWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Wraps cartridge access for the production gunship-motion check, rejecting reads from migrated motion tables while forwarding other dependencies.</summary>
    /// <param name="source">Underlying address space used for allowed reads, writes, and any room-enemy fixture data.</param>
    private sealed class GunshipMotionDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, IRoomEnemyFixtureSource
    {
        /// <summary>Uses fixture-specific enemy data when available, otherwise resolves the pointer through the installed catalog.</summary>
        public RoomEnemyDefinition ReadEnemyDefinition(ushort pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyDefinition(pointer)
                : RoomEnemyDefinitionCatalog.Get(pointer);

        /// <summary>Uses fixture-specific population data when available, otherwise resolves the pointer through the installed definitions.</summary>
        public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyPopulation(pointer)
                : RoomEnemyPopulationDefinitions.Get(pointer);

        /// <summary>Uses fixture-specific graphics data when available, otherwise resolves the pointer through the installed definitions.</summary>
        public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer) =>
            source is IRoomEnemyFixtureSource fixture
                ? fixture.ReadEnemyGraphicsSet(pointer)
                : RoomEnemyGraphicsSetDefinitions.Get(pointer);

        /// <summary>Routes import-source byte reads through the same migrated-table guard as ordinary address-space reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects access to the brake and hover table ranges under test and forwards all other byte reads.</summary>
        /// <param name="address">Cartridge byte address requested by the consumer.</param>
        /// <returns>The underlying byte when the address is outside the protected motion-table ranges.</returns>
        /// <exception cref="InvalidOperationException">The consumer attempts to read a migrated gunship-motion table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa2a622 and < 0xa2a644 or
                >= 0xa2a7cf and < 0xa2a7d7
                ? throw new InvalidOperationException(
                    $"Gunship attempted migrated motion-definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged because this guard constrains table reads rather than address-space mutation.</summary>
        /// <param name="address">Cartridge byte address to write.</param>
        /// <param name="value">Byte value passed to the underlying address space.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
