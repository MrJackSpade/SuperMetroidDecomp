using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the easy-to-miss graphics-index write in the shared Ceres elevator
    /// initializer at <c>$86:A301</c>.
    /// </summary>
    static void VerifyCeresElevatorArrivalGraphicsIndex()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyCompiledDefinitions(rom);
        var memory = new TestAddressSpace();

        // Give both one-entry spritemaps conspicuous source attributes. With native
        // graphics index zero, AddEnemyProjectileSpritemap must preserve bytes $34/$A5.
        // A leaked enemy graphics word would add a base tile and OR another OBJ palette.
        byte[] spritemap =
        [
            0x01, 0x00, // One component.
            0x00, 0x00, // X offset zero, small OBJ.
            0x00,       // Y offset zero.
            0x34, 0xa5, // Complete ROM-authored tile/attribute word.
        ];
        memory.WriteBytes(0x8db1ba, spritemap);
        memory.WriteBytes(0x8db1d0, spritemap);
        memory.WriteBytes(0x8d846d, spritemap);
        var bus = new CeresArrivalDefinitionReadGuard(memory);

        var samus = new SamusState { XPosition = 0x0080, YPosition = 0x0000 };
        var arrival = new CeresElevatorArrivalState(bus, samus);
        arrival.Step(samus);

        var oam = new OamBuffer();
        oam.BeginFrame();
        arrival.Draw(oam, cameraX: 0, cameraY: 0);

        VerificationAssert.AssertEqual(8, oam.NextByteOffset,
            "Ceres pad and level-data concealer each emitted one OBJ");
        VerificationAssert.AssertEqual(0x34, oam.LowTable[2],
            "Ceres moving pad retained its room-graphics tile byte");
        VerificationAssert.AssertEqual(0xa5, oam.LowTable[3],
            "Ceres moving pad retained its room-graphics attribute byte");
        VerificationAssert.AssertEqual(0x34, oam.LowTable[6],
            "Ceres level-data concealer retained its room-graphics tile byte");
        VerificationAssert.AssertEqual(0xa5, oam.LowTable[7],
            "Ceres level-data concealer retained its room-graphics attribute byte");

        int frames = 1;
        while (!arrival.IsComplete && frames++ < 200)
            arrival.Step(samus);
        VerificationAssert.AssertTrue(arrival.IsComplete,
            "Ceres arrival's compiled instruction programs delete both projectiles");
        VerificationAssert.AssertEqual(
            CeresElevatorArrivalDefinitions.LandingSamusY,
            samus.YPosition,
            "Ceres arrival retains the native landing coordinate");

        Console.WriteLine(
            "  Ceres elevator: definitions/programs match cartridge data, run without bank-$86 reads, and retain native graphics/landing behavior.");
    }

    private static void VerifyCompiledDefinitions(ISnesAddressSpace rom)
    {
        static ushort Word(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | (source.ReadByte(address + 1) << 8)));

        foreach (CeresElevatorProjectileDefinition definition in new[]
        {
            CeresElevatorArrivalDefinitions.MovingPad,
            CeresElevatorArrivalDefinitions.StationaryPlatform,
        })
        {
            int address = CeresElevatorArrivalDefinitions.BankBase | definition.DefinitionPointer;
            VerificationAssert.AssertEqual(Word(rom, address), definition.Initialization,
                "Ceres elevator compiled initialization callback");
            VerificationAssert.AssertEqual(Word(rom, address + 2), definition.PreInstruction,
                "Ceres elevator compiled pre-instruction callback");
            VerificationAssert.AssertEqual(Word(rom, address + 4), definition.InitialInstruction,
                "Ceres elevator compiled instruction-list identity");
            VerificationAssert.AssertEqual(Word(rom, address + 6), definition.PackedRadius,
                "Ceres elevator compiled radius");
            VerificationAssert.AssertEqual(Word(rom, address + 8), definition.Properties,
                "Ceres elevator compiled properties");
        }

        ushort[] pointers = [0xa28b, 0xa28d, 0xa291, 0xa295, 0xa299, 0xa29d];
        foreach (ushort pointer in pointers)
        {
            CeresElevatorProjectileInstruction instruction =
                CeresElevatorArrivalDefinitions.ReadInstruction(pointer);
            int address = CeresElevatorArrivalDefinitions.BankBase | pointer;
            switch (instruction.Operation)
            {
                case CeresElevatorProjectileOperation.Frame:
                    VerificationAssert.AssertEqual(Word(rom, address), instruction.Duration,
                        "Ceres elevator compiled frame duration");
                    VerificationAssert.AssertEqual(Word(rom, address + 2), instruction.SpritemapPointer,
                        "Ceres elevator compiled spritemap identity");
                    VerificationAssert.AssertEqual(unchecked((ushort)(pointer + 4)), instruction.NextInstruction,
                        "Ceres elevator compiled frame continuation");
                    break;
                case CeresElevatorProjectileOperation.Delete:
                    VerificationAssert.AssertEqual(CeresElevatorArrivalDefinitions.DeleteOpcode, Word(rom, address),
                        "Ceres elevator compiled delete opcode");
                    break;
                case CeresElevatorProjectileOperation.Goto:
                    VerificationAssert.AssertEqual(CeresElevatorArrivalDefinitions.GotoOpcode, Word(rom, address),
                        "Ceres elevator compiled goto opcode");
                    VerificationAssert.AssertEqual(Word(rom, address + 2), instruction.NextInstruction,
                        "Ceres elevator compiled goto target");
                    break;
            }
        }
    }

    private sealed class CeresArrivalDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x86a28b and < 0x86a2a1 ||
                address is >= 0x86a387 and < 0x86a3a3)
            {
                throw new InvalidOperationException(
                    $"Ceres arrival read compiled bank-$86 byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
