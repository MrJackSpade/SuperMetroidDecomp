using SuperMetroid.Verification.Input;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using System.Reflection;
using static SuperMetroid.Verification.Frontend.Intro.IntroDiscoveryCollisionVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Retail comparison and ROM-read boundary for intro Baby actor definitions, discovery instructions and demo input.</summary>
internal static class IntroBabyActorDefinitionsVerification
{
    /// <summary>Checks compiled actor placement and instruction sources against the cartridge, then advances guarded discovery and scientist scenes.</summary>
    internal static void VerifyIntroBabyActorDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Hold the pre-instruction inert so the initial running-left loop is exercised
        // independently of Samus's position-dependent stop-and-look redirect.
        var compiledInput = new DemoInputState();
        compiledInput.Clear();
        compiledInput.Enable();
        compiledInput.Load(IntroDemoInputObjectDefinitions.IntroBabyDiscovery, initializationParameter: 0);
        ushort NativeInputWord(ushort pointer) => (ushort)(
            SnesLittleEndian.Word(IntroBabyActorDefinitionsRomByte(retail, DemoInputRomData.BankBase | pointer), IntroBabyActorDefinitionsRomByte(retail, DemoInputRomData.BankBase | unchecked((ushort)(pointer + 1)))));
        var cartridgeInput = new NativeDemoInputReference(NativeInputWord);
        cartridgeInput.Enable();
        cartridgeInput.Load(DemoInputListWords.IntroBabyDiscoveryObject, initializationParameter: 0);
        for (int frame = 0; frame < 128; frame++)
        {
            compiledInput.Step((_, _) => { }, cue => throw new InvalidOperationException($"SR388 running-left loop called {cue}."));
            cartridgeInput.Step((_, _) => { }, NativeDemoInputReference.NoRoutines);
            AssertEqual(cartridgeInput.Snapshot(), NativeDemoInputReference.Of(compiledInput),
                $"SR388 initial demo loop object at frame {frame}");
        }
        // Native cinematic-object definitions $8B:CE5B/CE61/CE67/CE79, in compiled order.
        (IntroBabyActorDefinition Actor, ushort DefinitionPointer)[] actors =
        [
            (IntroBabyActorDefinitions.Egg, 0xce5b),
            (IntroBabyActorDefinitions.DeliveredBaby, 0xce61),
            (IntroBabyActorDefinitions.ExaminedBaby, 0xce67),
            (IntroBabyActorDefinitions.ConfusedBaby, 0xce79),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            IntroBabyActorDefinition actual = actors[index].Actor;
            int definitionAddress = IntroBabyActorDefinitions.NativeBank | actors[index].DefinitionPointer;
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((definitionAddress + 2)), actual.PreInstruction,
                $"intro baby actor {index} pre-instruction callback");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((definitionAddress + 4)), CinematicSpriteNativeListAddresses.Entry(actual.Program),
                $"intro baby actor {index} instruction list");

            // The definition's initialization callback supplies the compiled placement.
            int initializer = IntroBabyActorDefinitions.NativeBank |
                CartridgeImportSource.Require(retail).ReadCartridgeWord(definitionAddress);
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((initializer + 1)), actual.Position.X,
                $"intro baby actor {index} initializer X immediate");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((initializer + 7)), actual.Position.Y,
                $"intro baby actor {index} initializer Y immediate");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((initializer + 13)), actual.PaletteBits,
                $"intro baby actor {index} initializer palette immediate");
        }

        var guarded = NewIntroBabyActorDefinitionGuard(retail);
        byte[] nativeCollision = VerifyIntroDiscoveryCollision(retail);
        var discovery = new IntroBabyDiscoveryState(guarded,
            existingSamus: PrepareRetailSamusFixture(new SuperMetroid.Core.Samus.SamusState()));
        var referenceBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var reference = new IntroBabyDiscoveryState(referenceBus,
            existingSamus: PrepareRetailSamusFixture(new SuperMetroid.Core.Samus.SamusState()));
        for (int block = 0; block < discovery.Level.ForegroundEntries.Length; block++)
        {
            ushort expected = block * sizeof(ushort) < nativeCollision.Length
                ? (ushort)(SnesLittleEndian.Word(nativeCollision[block * 2], nativeCollision[block * 2 + 1]))
                : (ushort)0;
            AssertEqual(expected, discovery.Level.ForegroundEntries.Span[block],
                $"SR388 discovery physical block {block} preserves native word");
        }
        discovery.Samus.Position = discovery.Samus.Position.WithWholeX(0x00a8);
        reference.Samus.Position = reference.Samus.Position.WithWholeX(0x00a8);
        BindingFlags actorFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var egg = discovery.egg;
        var baby = discovery.confusedBaby;
        var nativeEgg = reference.egg;
        var nativeBaby = reference.confusedBaby;
        var demo = discovery.demo;
        var nativeDemo = reference.demo;
        int pageThreeFrame = -1;
        for (int frame = 0; frame < 1024; frame++)
        {
            discovery.Step((ushort)frame, introCrossfadeTimer: 0x007f);
            reference.Step((ushort)frame, introCrossfadeTimer: 0x007f);
            AssertEqual(nativeDemo.Cursor, demo.Cursor,
                $"SR388 demo list cursor at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.InstructionTimer.RawValue, demo.InstructionTimer.RawValue,
                $"SR388 demo countdown at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.Held, demo.Held,
                $"SR388 held input at frame {frame} matches ROM-backed controller");
            AssertEqual(nativeDemo.NewlyPressed, demo.NewlyPressed,
                $"SR388 input edge at frame {frame} matches ROM-backed controller");
            AssertEqual(reference.Samus.Position.X.Whole, discovery.Samus.Position.X.Whole,
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
            AssertEqual(nativeBaby.WholePosition.Y, baby.WholePosition.Y,
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
        var deliveredBaby = delivery.baby;
        var examinedBaby = examination.baby;
        for (ushort frame = 0; frame < 1024 &&
            (!delivery.PageFourRequested || !examination.PageFiveRequested); frame++)
        {
            delivery.Step(frame, introCrossfadeTimer: 0x007f);
            examination.Step(frame, introCrossfadeTimer: 0x007f);
        }
        AssertTrue(delivery.PageFourRequested,
            "delivered-baby production actor reaches page-four instruction");
        AssertTrue(examination.PageFiveRequested,
            "examined-baby production actor reaches page-five instruction");
        delivery.Step(0, introCrossfadeTimer: 0);
        examination.Step(0, introCrossfadeTimer: 0);
        AssertTrue(!deliveredBaby.IsActive && !examinedBaby.IsActive,
            "scientist actors delete at zero crossfade");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro egg/baby playback never rereads compiled definitions, instructions or discovery collision bytes");

        Console.WriteLine(
            "  Intro baby actors: definition, 138 discovery, 142 scientist and 768 collision bytes match; full guarded scenes pass.");
    }

    /// <summary>Reads a single retail byte only for the comparison oracle, outside the guarded production playback.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro Baby actor definitions")]
    private static byte IntroBabyActorDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Rejects production rereads of compiled Baby actor, instruction, demo-input and discovery-collision regions.</summary>
    private static CartridgeReadGuard NewIntroBabyActorDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address =>
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
                >= 0x8bba4b and < 0x8bba5e ||
                CinematicSpriteNativeListAddresses.InListRegion(address),
            address => $"Intro egg/baby actor reread compiled byte ${address:X6}.");

    /// <summary>Runs the compiled intro Baby actor and discovery playback comparison.</summary>
    [VerificationCommand("--intro-baby-actor-definitions")]
    internal static int IntroBabyActorDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroBabyActorDefinitions), () => VerifyIntroBabyActorDefinitions());
        return 0;
    }
}
