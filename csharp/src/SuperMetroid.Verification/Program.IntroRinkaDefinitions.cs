using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    /// <summary>
    /// Checks the intro Rinka actor and physical definitions against the cartridge, then verifies
    /// both timed spawn waves, animation progression, and cleanup without runtime table rereads.
    /// </summary>
    private static void VerifyIntroRinkaDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyIntroRinkaPrograms), () => VerifyIntroRinkaPrograms(retail));
        // Native six-byte definitions $8B:CF21 (Rinka) and $8B:CF27 (spawner). Their first
        // word, the initialization callback, has no port counterpart.
        (IntroRinkaActorDefinition Actor, ushort NativePointer)[] actors =
        [
            (IntroRinkaDefinitions.RinkaActor, 0xcf21),
            (IntroRinkaDefinitions.SpawnerActor, 0xcf27),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroRinkaActorDefinition actual = actors[index].Actor;
            int address = IntroRinkaDefinitions.NativeBank | actors[index].NativePointer;
            AssertEqual(ReadIntroRinkaWord(retail, address + 2), actual.PreInstruction,
                $"intro Rinka actor {index} pre-instruction callback");
            AssertEqual(ReadIntroRinkaWord(retail, address + 4), actual.InstructionList,
                $"intro Rinka actor {index} instruction list");
        }

        for (int parameter = 0; parameter < IntroRinkaDefinitions.RinkaCount; parameter++)
        {
            IntroRinkaPhysicalDefinition actual = IntroRinkaDefinitions.Rinka(parameter);
            AssertEqual(ReadIntroRinkaWord(retail,
                    IntroRinkaDefinitions.InitialXReferenceAddress + parameter * 2),
                actual.X, $"intro Rinka {parameter} initial X");
            AssertEqual(unchecked((ushort)(ReadIntroRinkaWord(retail,
                    IntroRinkaDefinitions.InitialYReferenceAddress + parameter * 2) - 8)),
                actual.Y, $"intro Rinka {parameter} biased initial Y");
            AssertEqual(unchecked((short)ReadIntroRinkaWord(retail,
                    IntroRinkaDefinitions.XWholeVelocityReferenceAddress + parameter * 2)),
                actual.XWholeVelocity, $"intro Rinka {parameter} signed X whole velocity");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroRinkaDefinitions.Rinka(IntroRinkaDefinitions.RinkaCount),
            "intro Rinka definition boundary");

        var guarded = new IntroRinkaDefinitionReadGuard(retail);
        var system = new IntroRinkaSystem();
        var samus = new SamusState
        {
            XPosition = 0x0200,
            YPosition = 0x0100,
        };
        for (int frame = 0; frame < 220; frame++)
        {
            system.Step(guarded, samus, motherBrainExploding: false, explosionsAllocated: false);
            int expectedCount = frame < 74 ? 0 : frame < 202 ? 2 : 4;
            AssertEqual(expectedCount, system.LiveRinkas.Count,
                $"intro Rinka two-wave spawn count at frame {frame}");
            // The spawner holds slot 14 and its Rinkas take lower free slots, so the native
            // descending walk steps each new Rinka in its spawn call (capture update 1793).
            if (frame >= 74)
            {
                int relative = frame - 74;
                ushort expectedSprite = relative < 30
                    ? (ushort)(0x8c8d + relative / 10 * 0x16)
                    : ((relative - 30) / 10 % 4) switch
                    {
                        0 or 2 => (ushort)0x8ca3,
                        1 => (ushort)0x8c8d,
                        _ => (ushort)0x8cb9,
                    };
                AssertEqual(expectedSprite, system.Rinka(0).SpriteMapPointer,
                    $"intro first Rinka selects its native frame at call {frame}");
            }
        }
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.LiveRinkas.Count,
            "intro Rinka spawner allocates both native waves");
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.ActiveCount,
            "all intro Rinkas remain active before Mother Brain explodes");

        for (int frame = 0; frame < 100; frame++)
            system.Step(guarded, samus, motherBrainExploding: true, explosionsAllocated: false);
        AssertEqual(0, system.ActiveCount,
            "all intro Rinkas retire after Mother Brain begins exploding");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Rinka production path never rereads compiled definitions, lists or physical tables");

        Console.WriteLine(
            "  Intro Rinka definitions: six actor, twelve physical and 48 instruction bytes match; both timed spawn waves, animation and cleanup are ROM-table independent.");
    }

    /// <summary>Reads one little-endian 16-bit word from two adjacent cartridge bytes.</summary>
    /// <param name="bus">Cartridge address space containing the native definition bytes.</param>
    /// <param name="address">Address of the low byte of the word.</param>
    /// <returns>The two bytes combined in little-endian order.</returns>
    private static ushort ReadIntroRinkaWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    /// <summary>
    /// Wraps cartridge access and rejects reads of the Rinka definitions, instruction lists,
    /// and initial physical tables that production logic is expected to use from compiled data.
    /// </summary>
    /// <param name="source">Underlying cartridge address space for all permitted reads and writes.</param>
    private sealed class IntroRinkaDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of attempts to reread protected compiled Rinka data.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Reads a cartridge byte through the same protected-range check as ordinary address-space reads.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Throws for protected native Rinka data ranges and forwards every other read to the wrapped source.</summary>
        public byte ReadByte(int address)
        {
            bool forbidden =
                address is >= 0x8bcf21 and < 0x8bcf2d or
                >= 0x8bcdeb and < 0x8bce1b or
                >= 0x8bb8b5 and < 0x8bb8c5 or
                >= 0x8bb985 and < 0x8bb98d;
            if (forbidden)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Intro Rinka reread compiled byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Forwards writes unchanged to the wrapped cartridge address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
