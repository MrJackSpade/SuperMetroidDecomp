using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Input;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyIntroBabyActorDefinitions()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach ((int start, int end) in new[]
        {
            (IntroBabyDiscoveryInstructionDefinitions.EggStart,
                IntroBabyDiscoveryInstructionDefinitions.EggEnd),
            (IntroBabyDiscoveryInstructionDefinitions.BabyStart,
                IntroBabyDiscoveryInstructionDefinitions.BabyEnd),
            (IntroBabyDiscoveryInstructionDefinitions.DeletePointer,
                IntroBabyDiscoveryInstructionDefinitions.DeletePointer + 2),
        })
            for (int pointer = start; pointer < end; pointer++)
                AssertEqual(retail.ReadByte(IntroBabyActorDefinitions.NativeBank | pointer),
                    IntroBabyDiscoveryInstructionDefinitions.ReadByte((ushort)pointer),
                    $"SR388 egg/baby instruction byte $8B:{pointer:X4}");
        foreach ((int start, int end) in new[]
        {
            (IntroBabyDiscoveryInputDefinitions.ListStart,
                IntroBabyDiscoveryInputDefinitions.ListEnd),
            (IntroBabyDiscoveryInputDefinitions.HeaderStart,
                IntroBabyDiscoveryInputDefinitions.HeaderEnd),
        })
            for (int pointer = start; pointer < end; pointer++)
                AssertEqual(retail.ReadByte(DemoInputRomData.BankBase | pointer),
                    IntroBabyDiscoveryInputDefinitions.ReadByte((ushort)pointer),
                    $"SR388 discovery demo byte $91:{pointer:X4}");
        AssertThrows<InvalidDataException>(() =>
            IntroBabyDiscoveryInputDefinitions.ReadWord(
                IntroBabyDiscoveryInputDefinitions.ListEnd),
            "SR388 demo reader rejects adjacent bank-$91 code");
        // Hold the pre-instruction inert so the initial running-left loop is exercised
        // independently of Samus's position-dependent stop-and-look redirect.
        var compiledInput = new DemoInputState();
        var cartridgeInput = new DemoInputState();
        compiledInput.Clear();
        cartridgeInput.Clear();
        compiledInput.Enable();
        cartridgeInput.Enable();
        compiledInput.LoadObject(retail, IntroBabyDiscoveryInputDefinitions.HeaderStart,
            definitionWord: IntroBabyDiscoveryInputDefinitions.ReadWord);
        cartridgeInput.LoadObject(retail, IntroBabyDiscoveryInputDefinitions.HeaderStart);
        for (int frame = 0; frame < 128; frame++)
        {
            compiledInput.Step(retail, preInstruction: static (_, _) => { },
                instructionWord: IntroBabyDiscoveryInputDefinitions.ReadWord);
            cartridgeInput.Step(retail, preInstruction: static (_, _) => { });
            AssertEqual(cartridgeInput.InstructionPointer, compiledInput.InstructionPointer,
                $"SR388 initial demo loop cursor at frame {frame}");
            AssertEqual(cartridgeInput.InstructionTimer, compiledInput.InstructionTimer,
                $"SR388 initial demo loop timer at frame {frame}");
            AssertEqual(cartridgeInput.Held, compiledInput.Held,
                $"SR388 initial demo held input at frame {frame}");
            AssertEqual(cartridgeInput.NewlyPressed, compiledInput.NewlyPressed,
                $"SR388 initial demo input edge at frame {frame}");
        }
        AssertThrows<InvalidDataException>(
            () => IntroBabyDiscoveryInstructionDefinitions.ReadWord(
                IntroBabyDiscoveryInstructionDefinitions.EggEnd),
            "SR388 discovery reader rejects scientist-scene lists");
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
        byte[] nativeCollision = RomDataReader.ReadFixedBank(retail,
            IntroBabyDiscoveryCollisionDefinitions.SourceAddress,
            IntroBabyDiscoveryCollisionDefinitions.SourceByteCount);
        AssertTrue(IntroBabyDiscoveryCollisionDefinitions.SourceBytes.SequenceEqual(nativeCollision),
            "SR388 discovery physical level matches every cartridge source byte");
        var discovery = new IntroBabyDiscoveryState(guarded);
        var referenceBus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var reference = new IntroBabyDiscoveryState(referenceBus,
            demoWordReader: pointer => (ushort)(
                referenceBus.ReadByte(DemoInputRomData.BankBase | pointer) |
                referenceBus.ReadByte(DemoInputRomData.BankBase |
                    unchecked((ushort)(pointer + 1))) << 8));
        for (int block = 0; block < discovery.Level.ForegroundEntries.Length; block++)
        {
            ushort expected = block * sizeof(ushort) < nativeCollision.Length
                ? (ushort)(nativeCollision[block * 2] | nativeCollision[block * 2 + 1] << 8)
                : (ushort)0;
            AssertEqual(expected, discovery.Level.ForegroundEntries.Span[block],
                $"SR388 discovery physical block {block} preserves native word");
        }
        discovery.Samus.XPosition = 0x00a8;
        reference.Samus.XPosition = 0x00a8;
        BindingFlags actorFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var egg = (IntroDiscoverySprite)typeof(IntroBabyDiscoveryState)
            .GetField("egg", actorFlags)!.GetValue(discovery)!;
        var baby = (IntroDiscoverySprite)typeof(IntroBabyDiscoveryState)
            .GetField("confusedBaby", actorFlags)!.GetValue(discovery)!;
        var nativeEgg = (IntroDiscoverySprite)typeof(IntroBabyDiscoveryState)
            .GetField("egg", actorFlags)!.GetValue(reference)!;
        var nativeBaby = (IntroDiscoverySprite)typeof(IntroBabyDiscoveryState)
            .GetField("confusedBaby", actorFlags)!.GetValue(reference)!;
        var demo = (DemoInputState)typeof(IntroBabyDiscoveryState)
            .GetField("demo", actorFlags)!.GetValue(discovery)!;
        var nativeDemo = (DemoInputState)typeof(IntroBabyDiscoveryState)
            .GetField("demo", actorFlags)!.GetValue(reference)!;
        int pageThreeFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            discovery.Step((ushort)frame, introCrossfadeTimer: 0x007f);
            reference.Step((ushort)frame, introCrossfadeTimer: 0x007f);
            AssertEqual(nativeDemo.InstructionPointer, demo.InstructionPointer,
                $"SR388 demo list cursor at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.InstructionTimer, demo.InstructionTimer,
                $"SR388 demo countdown at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.Held, demo.Held,
                $"SR388 held input at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.NewlyPressed, demo.NewlyPressed,
                $"SR388 input edge at frame {frame} matches ROM-backed controller");
            AssertEqual(reference.Samus.XPosition, discovery.Samus.XPosition,
                $"SR388 Samus X at frame {frame} matches ROM-backed controller");
            AssertEqual(reference.EggHatchingStarted, discovery.EggHatchingStarted,
                $"SR388 egg hatch transition at frame {frame} matches ROM-backed actor");
            AssertEqual(reference.PageThreeRequested, discovery.PageThreeRequested,
                $"SR388 page-three request at frame {frame} matches ROM-backed actor");
            AssertEqual(reference.ActiveEggParticleCount, discovery.ActiveEggParticleCount,
                $"SR388 egg particle count at frame {frame} matches ROM-backed actor");
            AssertEqual(nativeEgg.SpriteMapPointer, egg.SpriteMapPointer,
                $"SR388 egg visual frame {frame} matches ROM-backed instruction playback");
            AssertEqual(nativeBaby.SpriteMapPointer, baby.SpriteMapPointer,
                $"SR388 baby visual frame {frame} matches ROM-backed instruction playback");
            AssertEqual(nativeBaby.YPosition, baby.YPosition,
                $"SR388 baby Y motion at frame {frame} matches ROM-backed instruction playback");
            if (discovery.PageThreeRequested)
            {
                pageThreeFrame = frame;
                break;
            }
        }
        AssertTrue(discovery.EggHatchingStarted && pageThreeFrame >= 0,
            "intro egg hatches and requests page three within the native program");
        discovery.Step((ushort)(pageThreeFrame + 1), introCrossfadeTimer: 0);
        reference.Step((ushort)(pageThreeFrame + 1), introCrossfadeTimer: 0);
        AssertEqual(nativeEgg.IsActive, egg.IsActive,
            "SR388 egg reverse-crossfade deletion matches ROM-backed actor");
        AssertTrue(!egg.IsActive,
            "SR388 egg actually deletes on the first reverse-crossfade frame");
        AssertEqual(nativeDemo.Enabled, demo.Enabled,
            "SR388 end-demo enable state matches ROM-backed controller");
        AssertEqual(reference.Samus.InputLocked, discovery.Samus.InputLocked,
            "SR388 end-demo Samus input lock matches ROM-backed controller");

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
            "intro egg/baby playback never rereads compiled definitions, instructions, demo input or discovery collision bytes");

        Console.WriteLine(
            "  Intro baby actors: definition, 138 instruction/delete, 72 demo-input and 768 collision bytes match; full guarded discovery and scientist scenes pass.");
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
                (address >= IntroBabyDiscoveryCollisionDefinitions.SourceAddress &&
                    address < IntroBabyDiscoveryCollisionDefinitions.SourceAddress +
                        IntroBabyDiscoveryCollisionDefinitions.SourceByteCount) ||
                address is >= 0x8bce5b and < 0x8bce6d or
                >= 0x8bce79 and < 0x8bce7f or
                >= 0x8bcb33 and < 0x8bcb9f or
                >= 0x8bcc2b and < 0x8bcc47 or
                >= 0x8bce53 and < 0x8bce55 or
                >= 0x8ba8d5 and < 0x8ba8e8 or
                >= 0x8bad55 and < 0x8bad68 or
                >= 0x8bad93 and < 0x8bada6 or
                >= 0x8bba4b and < 0x8bba5e;
            forbidden |=
                (address >= (DemoInputRomData.BankBase |
                    IntroBabyDiscoveryInputDefinitions.ListStart) &&
                 address < (DemoInputRomData.BankBase |
                    IntroBabyDiscoveryInputDefinitions.ListEnd)) ||
                (address >= (DemoInputRomData.BankBase |
                    IntroBabyDiscoveryInputDefinitions.HeaderStart) &&
                 address < (DemoInputRomData.BankBase |
                    IntroBabyDiscoveryInputDefinitions.HeaderEnd));
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
