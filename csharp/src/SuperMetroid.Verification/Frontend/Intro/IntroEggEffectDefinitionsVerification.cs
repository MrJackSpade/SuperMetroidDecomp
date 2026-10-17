using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Intro.IntroEggEffectProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled intro egg actor definitions and the particle and slime-drop lifetimes.</summary>
internal static class IntroEggEffectDefinitionsVerification
{
    /// <summary>Compares native effect records and exercises all six particles and four slime drops without source reads.</summary>
    internal static void VerifyIntroEggEffectDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < IntroEggEffectDefinitions.ParticleCount; index++)
            VerifyIntroEggEffectActor(retail, unchecked((ushort)(0xcecd + index * 6)), IntroEggEffectDefinitions.Particle(index),
                $"intro egg particle {index}");
        Suite(nameof(VerifyIntroEggEffectActor), () => VerifyIntroEggEffectActor(retail, 0xcef1, IntroEggEffectDefinitions.SlimeDrop,
            "intro egg slime drop"));
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroEggEffectDefinitions.Particle(IntroEggEffectDefinitions.ParticleCount),
            "intro egg particle definition boundary");

        var guarded = NewIntroEggEffectDefinitionGuard(retail);
        var particles = Enumerable.Range(0, IntroEggEffectDefinitions.ParticleCount)
            .Select(index => new IntroEggParticle((byte)index))
            .ToArray();
        for (int index = 0; index < particles.Length; index++)
        {
            particles[index].Step();
            var sprite = particles[index].sprite;
            AssertEqual((ushort)(0x8f7e + index * 7), CinematicSpritePoseSpritemaps.Of(sprite.Pose),
                $"intro egg fragment {index} selects its own first visual frame");
        }
        for (int frame = 0; frame < 128 && particles.Any(static actor => actor.IsActive); frame++)
            foreach (IntroEggParticle particle in particles)
                particle.Step();
        AssertTrue(particles.All(static actor => !actor.IsActive),
            "all six production egg particles complete their physical lifetime");

        var slimeDrops = Enumerable.Range(0, IntroEggEffectDefinitions.SlimeDropCount)
            .Select(index => new IntroEggSlimeDrop(new(0x0070, 0x0091), (byte)index))
            .ToArray();
        for (int frame = 0; frame < 192 && slimeDrops.Any(static actor => actor.IsActive); frame++)
            foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
                slimeDrop.Step();
        AssertTrue(slimeDrops.All(static actor => !actor.IsActive),
            "all four production slime drops complete motion and puddle animation");

        var impact = new IntroEggSlimeDrop(new(0x0070, 0x00b8), index: 0);
        var impactSprite = impact.sprite;
        for (int frame = 0; frame < 40; frame++)
        {
            impact.Step();
            AssertEqual((ushort)(0x8faf + frame / 10 * 7), CinematicSpritePoseSpritemaps.Of(impactSprite.Pose),
                $"intro slime impact frame {frame} selects native puddle art");
        }
        impact.Step();
        AssertTrue(!impact.IsActive, "intro slime impact deletes after its four ten-frame records");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro egg effects never reread compiled actor definitions or instruction lists");

        Console.WriteLine(
            "  Intro egg effects: 21 actor words and 76 instruction/delete bytes match; six fragments and four slime drops complete without source-list reads.");
    }

    /// <summary>Compares an actor's native pre-instruction and list pointers while leaving its initialization callback unowned.</summary>
    private static void VerifyIntroEggEffectActor(
        SuperMetroidAddressSpace retail,
        ushort definitionPointer,
        IntroEggEffectActorDefinition actual,
        string name)
    {
        // The six-byte header's first word (initialization callback) has no compiled owner.
        int address = IntroEggEffectDefinitions.NativeDefinitionBank | definitionPointer;
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 2)), actual.PreInstruction,
            $"{name} pre-instruction callback");
        AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actual.Program),
            $"{name} instruction list");
    }

    /// <summary>Rejects runtime rereads of compiled egg actor and instruction bytes.</summary>
    private static CartridgeReadGuard NewIntroEggEffectDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8bcecd and < 0x8bcef7 or
                >= 0x8bcd39 and < 0x8bcd83 or
                >= 0x8bce53 and < 0x8bce55,
            address => $"Intro egg effect reread compiled definition byte ${address:X6}.");

    /// <summary>Runs the intro egg effect comparison with the retail cartridge.</summary>
    [VerificationCommand("--intro-egg-effect-definitions")]
    internal static int IntroEggEffectDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroEggEffectDefinitions), () => VerifyIntroEggEffectDefinitions());
        return 0;
    }
}
