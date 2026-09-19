using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroRinkaDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        IntroRinkaActorDefinition[] actors =
        [
            IntroRinkaDefinitions.RinkaActor,
            IntroRinkaDefinitions.SpawnerActor,
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroRinkaActorDefinition actual = actors[index];
            int address = IntroRinkaDefinitions.NativeBank | actual.Pointer;
            AssertEqual(ReadIntroRinkaWord(retail, address), actual.Initialization,
                $"intro Rinka actor {index} initialization callback");
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
            system.Step(guarded, samus, motherBrainExploding: false);
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.SpawnedCount,
            "intro Rinka spawner allocates both native waves");
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.ActiveCount,
            "all intro Rinkas remain active before Mother Brain explodes");

        for (int frame = 0; frame < 100; frame++)
            system.Step(guarded, samus, motherBrainExploding: true);
        AssertEqual(0, system.ActiveCount,
            "all intro Rinkas retire after Mother Brain begins exploding");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Rinka production path never rereads compiled definitions or physical tables");

        Console.WriteLine(
            "  Intro Rinka definitions: six actor words and twelve physical words match; both spawn waves and cleanup are table-independent.");
    }

    private static ushort ReadIntroRinkaWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class IntroRinkaDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            bool forbidden =
                address is >= 0x8bcf21 and < 0x8bcf2d or
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

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
