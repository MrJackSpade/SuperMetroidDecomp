using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyIntroBabyActorDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        IntroBabyActorDefinition[] actors =
        [
            IntroBabyActorDefinitions.Egg,
            IntroBabyActorDefinitions.DeliveredBaby,
            IntroBabyActorDefinitions.ExaminedBaby,
            IntroBabyActorDefinitions.ConfusedBaby,
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroBabyActorDefinition actual = actors[index];
            int definitionAddress = IntroBabyActorDefinitions.NativeBank | actual.Pointer;
            AssertEqual(ReadIntroBabyActorWord(retail, definitionAddress), actual.Initialization,
                $"intro baby actor {index} initialization callback");
            AssertEqual(ReadIntroBabyActorWord(retail, definitionAddress + 2), actual.PreInstruction,
                $"intro baby actor {index} pre-instruction callback");
            AssertEqual(ReadIntroBabyActorWord(retail, definitionAddress + 4), actual.InstructionList,
                $"intro baby actor {index} instruction list");

            int initializer = IntroBabyActorDefinitions.NativeBank | actual.Initialization;
            AssertEqual(ReadIntroBabyActorWord(retail, initializer + 1), actual.X,
                $"intro baby actor {index} initializer X immediate");
            AssertEqual(ReadIntroBabyActorWord(retail, initializer + 7), actual.Y,
                $"intro baby actor {index} initializer Y immediate");
            AssertEqual(ReadIntroBabyActorWord(retail, initializer + 13), actual.PaletteBits,
                $"intro baby actor {index} initializer palette immediate");
        }

        var guarded = new IntroBabyActorDefinitionReadGuard(retail);
        var discovery = new IntroBabyDiscoveryState(guarded);
        discovery.Samus.XPosition = 0x00a8;
        discovery.Step(nmiFrameCounter: 0, introCrossfadeTimer: 0x007f);
        AssertTrue(discovery.EggHatchingStarted,
            "intro egg production pre-instruction starts hatching below native X threshold");

        IntroScientistCutsceneState delivery = IntroScientistCutsceneState.CreateDelivery();
        IntroScientistCutsceneState examination = IntroScientistCutsceneState.CreateExamination();
        for (ushort frame = 0; frame < 1024 &&
            (!delivery.PageFourRequested || !examination.PageFiveRequested); frame++)
        {
            delivery.Step(guarded, frame, introCrossfadeTimer: 0x007f);
            examination.Step(guarded, frame, introCrossfadeTimer: 0x007f);
        }
        AssertTrue(delivery.PageFourRequested,
            "delivered-baby production actor reaches page-four instruction");
        AssertTrue(examination.PageFiveRequested,
            "examined-baby production actor reaches page-five instruction");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro egg and baby actors never reread compiled definition/initializer records");

        Console.WriteLine(
            "  Intro baby actors: twelve definition and twelve initializer words match; egg and both scientist scenes are source-record independent.");
    }

    private static ushort ReadIntroBabyActorWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class IntroBabyActorDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            bool forbidden =
                address is >= 0x8bce5b and < 0x8bce6d or
                >= 0x8bce79 and < 0x8bce7f or
                >= 0x8ba8d5 and < 0x8ba8e8 or
                >= 0x8bad55 and < 0x8bad68 or
                >= 0x8bad93 and < 0x8bada6 or
                >= 0x8bba4b and < 0x8bba5e;
            if (forbidden)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Intro egg/baby actor reread compiled byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
