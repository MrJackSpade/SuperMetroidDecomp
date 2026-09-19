using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCeresDestructionActorDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorDefinition actor =
                CeresDestructionActorDefinitions.InitialActor(index);
            VerifyActor(retail, actor, $"initial destruction actor {index}");
        }
        VerifyWrappedActor(retail, CeresDestructionActorDefinitions.InitialActor(0),
            0x8bbf23, 0x8bbf29, 0x8bbf2f, 0x8bbf3a, 0x8bbf46,
            "destruction large asteroids");
        VerifyWrappedActor(retail, CeresDestructionActorDefinitions.InitialActor(1),
            0x8bbf77, 0x8bbf7d, 0x8bbf83, 0x8bbf8e, 0x8bbf9a,
            "destruction small asteroids");
        CeresDestructionActorDefinition vortex =
            CeresDestructionActorDefinitions.InitialActor(2);
        AssertEqual(ReadWord(retail, 0x8bbfa6), vortex.X, "destruction vortex X");
        AssertEqual(ReadWord(retail, 0x8bbfba), vortex.Y, "destruction vortex Y");
        AssertEqual(ReadWord(retail, 0x8bbfc0), vortex.Attributes,
            "destruction vortex attributes");
        AssertEqual(ReadWord(retail, 0x8bbfac), vortex.ActivePreInstruction,
            "destruction vortex no-op callback override");

        int[] xAddresses = [0x8bc83c, 0x8bc944, 0x8bc958, 0x8bc96c, 0x8bc980, 0x8bc993];
        int[] yAddresses = [0x8bc842, 0x8bc94a, 0x8bc95e, 0x8bc972, 0x8bc986, 0x8bc999];
        int[] attributeAddresses = [0x8bc848, 0x8bc950, 0x8bc964, 0x8bc978, 0x8bc98c, 0x8bc99f];
        for (int index = 0; index < CeresDestructionActorDefinitions.ZebesActorCount; index++)
        {
            CeresDestructionActorDefinition actor =
                CeresDestructionActorDefinitions.ZebesActor(index);
            VerifyActor(retail, actor, $"Zebes reveal actor {index}");
            AssertEqual(ReadWord(retail, xAddresses[index]), actor.X,
                $"Zebes reveal actor {index} X");
            AssertEqual(ReadWord(retail, yAddresses[index]), actor.Y,
                $"Zebes reveal actor {index} Y");
            AssertEqual(ReadWord(retail, attributeAddresses[index]), actor.Attributes,
                $"Zebes reveal actor {index} attributes");
        }

        CeresDestructionActorDefinition planet =
            CeresDestructionActorDefinitions.ZebesActor(0);
        AssertEqual(ReadWord(retail, 0x8bc857), planet.SlidePreInstruction,
            "Zebes planet slide callback");
        AssertEqual(ReadWord(retail, 0x8bc862), planet.SlideAcceleration,
            "Zebes planet slide acceleration");
        for (int index = 1; index <= 3; index++)
        {
            CeresDestructionActorDefinition star =
                CeresDestructionActorDefinitions.ZebesActor(index);
            AssertEqual(ReadWord(retail, 0x8bc902), star.SlidePreInstruction,
                $"Zebes star {index + 1} slide callback");
            AssertEqual(ReadWord(retail, 0x8bc90d), star.SlideAcceleration,
                $"Zebes star {index + 1} slide acceleration");
        }
        CeresDestructionActorDefinition completionStar =
            CeresDestructionActorDefinitions.ZebesActor(4);
        AssertEqual(ReadWord(retail, 0x8bc8b3), completionStar.SlidePreInstruction,
            "Zebes completion star slide callback");
        AssertEqual(ReadWord(retail, 0x8bc8be), completionStar.SlideAcceleration,
            "Zebes completion star slide acceleration");
        AssertTrue(completionStar.CompletesScene,
            "Zebes stars 5 retains scene-completion ownership");
        CeresDestructionActorDefinition title =
            CeresDestructionActorDefinitions.ZebesActor(5);
        AssertEqual((ushort)0, title.SlidePreInstruction,
            "PLANET ZEBES title cannot enter slide motion");
        AssertEqual((ushort)0, title.SlideAcceleration,
            "PLANET ZEBES title has no slide acceleration");

        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDestructionActorDefinitions.InitialActor(
                CeresDestructionActorDefinitions.InitialActorCount),
            "destruction initial actor definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => CeresDestructionActorDefinitions.ZebesActor(
                CeresDestructionActorDefinitions.ZebesActorCount),
            "Zebes reveal actor definition boundary");

        var guard = new CeresDestructionActorDefinitionReadGuard(retail);
        var state = new CeresDestructionCinematicState(guard);
        for (int frame = 0; frame < 5000 && !state.Finished; frame++)
            state.Step();
        AssertTrue(state.Finished,
            "Ceres destruction and Zebes reveal complete through production actor paths");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Ceres destruction never rereads compiled scene actor metadata");

        Console.WriteLine(
            "  Ceres destruction actors: 65 native words and the full station/Zebes production sequence pass with constructor, callback and motion metadata reads forbidden.");

        static void VerifyActor(
            SuperMetroidAddressSpace source,
            CeresDestructionActorDefinition actor,
            string name)
        {
            int address = CeresDestructionActorDefinitions.NativeBank | actor.Pointer;
            AssertEqual(ReadWord(source, address), actor.Initialization,
                $"{name} initialization callback");
            AssertEqual(ReadWord(source, address + 2), actor.DefinitionPreInstruction,
                $"{name} definition pre-instruction");
            AssertEqual(ReadWord(source, address + 4), actor.InstructionList,
                $"{name} instruction list");
        }

        static void VerifyWrappedActor(
            SuperMetroidAddressSpace source,
            CeresDestructionActorDefinition actor,
            int xAddress,
            int yAddress,
            int attributeAddress,
            int deltaAddress,
            int maskAddress,
            string name)
        {
            AssertEqual(ReadWord(source, xAddress), actor.X, $"{name} X");
            AssertEqual(ReadWord(source, yAddress), actor.Y, $"{name} Y");
            AssertEqual(ReadWord(source, attributeAddress), actor.Attributes,
                $"{name} attributes");
            AssertEqual(ReadWord(source, deltaAddress), unchecked((ushort)actor.HorizontalDelta),
                $"{name} speed");
            AssertEqual((ushort)0x01ff, ReadWord(source, maskAddress), $"{name} wrap mask");
            AssertTrue(actor.WrapX, $"{name} uses 512-pixel wrapping motion");
        }

        static ushort ReadWord(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
    }

    private sealed class CeresDestructionActorDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (IsForbidden(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ceres destruction reread compiled scene actor byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static bool IsForbidden(int address) =>
            address is >= 0x8bbf23 and < 0x8bbf25 or
                >= 0x8bbf29 and < 0x8bbf2b or
                >= 0x8bbf2f and < 0x8bbf31 or
                >= 0x8bbf3a and < 0x8bbf3c or
                >= 0x8bbf46 and < 0x8bbf48 or
                >= 0x8bbf77 and < 0x8bbf79 or
                >= 0x8bbf7d and < 0x8bbf7f or
                >= 0x8bbf83 and < 0x8bbf85 or
                >= 0x8bbf8e and < 0x8bbf90 or
                >= 0x8bbf9a and < 0x8bbf9c or
                >= 0x8bbfa6 and < 0x8bbfa8 or
                >= 0x8bbfac and < 0x8bbfae or
                >= 0x8bbfba and < 0x8bbfbc or
                >= 0x8bbfc0 and < 0x8bbfc2 or
                >= 0x8bc83c and < 0x8bc83e or
                >= 0x8bc842 and < 0x8bc844 or
                >= 0x8bc848 and < 0x8bc84a or
                >= 0x8bc857 and < 0x8bc859 or
                >= 0x8bc862 and < 0x8bc864 or
                >= 0x8bc8b3 and < 0x8bc8b5 or
                >= 0x8bc8be and < 0x8bc8c0 or
                >= 0x8bc902 and < 0x8bc904 or
                >= 0x8bc90d and < 0x8bc90f or
                >= 0x8bc944 and < 0x8bc946 or
                >= 0x8bc94a and < 0x8bc94c or
                >= 0x8bc950 and < 0x8bc952 or
                >= 0x8bc958 and < 0x8bc95a or
                >= 0x8bc95e and < 0x8bc960 or
                >= 0x8bc964 and < 0x8bc966 or
                >= 0x8bc96c and < 0x8bc96e or
                >= 0x8bc972 and < 0x8bc974 or
                >= 0x8bc978 and < 0x8bc97a or
                >= 0x8bc980 and < 0x8bc982 or
                >= 0x8bc986 and < 0x8bc988 or
                >= 0x8bc98c and < 0x8bc98e or
                >= 0x8bc993 and < 0x8bc995 or
                >= 0x8bc999 and < 0x8bc99b or
                >= 0x8bc99f and < 0x8bc9a1 or
                >= 0x8bce7f and < 0x8bce85 or
                >= 0x8bce8b and < 0x8bce97 or
                >= 0x8bcea3 and < 0x8bcea9 or
                >= 0x8bceaf and < 0x8bceb5 or
                >= 0x8bcef7 and < 0x8bcf0f;
    }
}
