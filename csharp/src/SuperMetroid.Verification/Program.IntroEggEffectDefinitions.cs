using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyIntroEggEffectDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyIntroEggEffectPrograms), () => VerifyIntroEggEffectPrograms(retail));
        for (int index = 0; index < IntroEggEffectDefinitions.ParticleCount; index++)
            VerifyIntroEggEffectActor(retail, unchecked((ushort)(0xcecd + index * 6)), IntroEggEffectDefinitions.Particle(index),
                $"intro egg particle {index}");
        Suite(nameof(VerifyIntroEggEffectActor), () => VerifyIntroEggEffectActor(retail, 0xcef1, IntroEggEffectDefinitions.SlimeDrop,
            "intro egg slime drop"));
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroEggEffectDefinitions.Particle(IntroEggEffectDefinitions.ParticleCount),
            "intro egg particle definition boundary");

        var guarded = new IntroEggEffectDefinitionReadGuard(retail);
        var particles = Enumerable.Range(0, IntroEggEffectDefinitions.ParticleCount)
            .Select(index => new IntroEggParticle((byte)index))
            .ToArray();
        for (int index = 0; index < particles.Length; index++)
        {
            particles[index].Step();
            var sprite = (IntroDiscoverySprite)typeof(IntroEggParticle)
                .GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(particles[index])!;
            AssertEqual((ushort)(0x8f7e + index * 7), sprite.SpriteMapPointer,
                $"intro egg fragment {index} selects its own first visual frame");
        }
        for (int frame = 0; frame < 128 && particles.Any(static actor => actor.IsActive); frame++)
            foreach (IntroEggParticle particle in particles)
                particle.Step();
        AssertTrue(particles.All(static actor => !actor.IsActive),
            "all six production egg particles complete their physical lifetime");

        var slimeDrops = Enumerable.Range(0, IntroEggEffectDefinitions.SlimeDropCount)
            .Select(index => new IntroEggSlimeDrop(0x0070, 0x0091, (byte)index))
            .ToArray();
        for (int frame = 0; frame < 192 && slimeDrops.Any(static actor => actor.IsActive); frame++)
            foreach (IntroEggSlimeDrop slimeDrop in slimeDrops)
                slimeDrop.Step();
        AssertTrue(slimeDrops.All(static actor => !actor.IsActive),
            "all four production slime drops complete motion and puddle animation");

        var impact = new IntroEggSlimeDrop(0x0070, 0x00b8, index: 0);
        var impactSprite = (IntroDiscoverySprite)typeof(IntroEggSlimeDrop)
            .GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(impact)!;
        for (int frame = 0; frame < 40; frame++)
        {
            impact.Step();
            AssertEqual((ushort)(0x8faf + frame / 10 * 7), impactSprite.SpriteMapPointer,
                $"intro slime impact frame {frame} selects native puddle art");
        }
        impact.Step();
        AssertTrue(!impact.IsActive, "intro slime impact deletes after its four ten-frame records");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro egg effects never reread compiled actor definitions or instruction lists");

        Console.WriteLine(
            "  Intro egg effects: 21 actor words and 76 instruction/delete bytes match; six fragments and four slime drops complete without source-list reads.");
    }

    private static void VerifyIntroEggEffectActor(
        SuperMetroidAddressSpace retail,
        ushort definitionPointer,
        IntroEggEffectActorDefinition actual,
        string name)
    {
        // The six-byte header's first word (initialization callback) has no compiled owner.
        int address = IntroEggEffectDefinitions.NativeDefinitionBank | definitionPointer;
        AssertEqual(ReadIntroEggEffectWord(retail, address + 2), actual.PreInstruction,
            $"{name} pre-instruction callback");
        AssertEqual(ReadIntroEggEffectWord(retail, address + 4), actual.InstructionList,
            $"{name} instruction list");
    }

    private static ushort ReadIntroEggEffectWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private sealed class IntroEggEffectDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x8bcecd and < 0x8bcef7 or
                >= 0x8bcd39 and < 0x8bcd83 or
                >= 0x8bce53 and < 0x8bce55)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Intro egg effect reread compiled definition byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
