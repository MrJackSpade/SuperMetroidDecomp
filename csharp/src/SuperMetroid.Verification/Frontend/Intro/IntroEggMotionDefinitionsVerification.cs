using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks compiled intro egg fragment and slime motion against native tables and actual actor updates.</summary>
internal static class IntroEggMotionDefinitionsVerification
{
    /// <summary>The cartridge's egg-shell gravity steps, a whole word then a fraction word per frame fallen.</summary>
    private const int ShellGravitySteps = 0x8baa02;

    /// <summary>
    /// Decodes the gravity steps the shell fragments read past their 38-step curve from the cartridge and compares them
    /// with the installed <see cref="ShellFragmentFallPastCurve"/> (#1500): every step the fragments read past the curve,
    /// and the rejection of the first step no fragment reads.
    /// </summary>
    internal static void VerifyShellFragmentFallPastCurve(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ShellFragmentFallPastCurve installed = InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve;
        const int stepsPastCurve = 3;
        for (int past = 0; past < stepsPastCurve; past++)
        {
            int address = ShellGravitySteps + (IntroEggMotionDefinitions.FragmentCurveFrames + past) * 4;
            SignedSixteenSixteen step = installed.Step(past);
            AssertEqual(cartridge.ReadCartridgeWord(address), step.UnsignedWholePixels, $"shell step {past} past the curve: whole word");
            AssertEqual(cartridge.ReadCartridgeWord(address + 2), step.Fraction, $"shell step {past} past the curve: fraction word");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => installed.Step(stepsPastCurve), "no fragment falls a fourth step past the curve");
        Console.WriteLine("Shell fragment fall past curve: the three steps past the gravity curve match the cartridge.");
    }

    /// <summary>Runs the installed shell-fragment steps past the curve against the cartridge.</summary>
    [VerificationCommand("--intro-shell-fall-past-curve")]
    internal static int ShellFragmentFallPastCurveCommand()
    {
        Suite(nameof(VerifyShellFragmentFallPastCurve), () => VerifyShellFragmentFallPastCurve(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Compares position and velocity words, then checks fixed-point motion through each actor's landing and deletion.</summary>
    internal static void VerifyCompiledIntroEggMotion(CartridgeImportAddressSpace rom)
    {
        ShellFragmentFallPastCurve pastCurve = InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve;
        ushort Word(int address) => LittleEndianBytes.Word(CompiledIntroEggMotionRomByte(rom, address), CompiledIntroEggMotionRomByte(rom, address + 1));
        void Compare(int address, int count, Func<int, SignedSixteenSixteen> sample)
        {
            for (int index = 0; index < count; index++)
            {
                var actual = sample(index);
                AssertEqual(Word(address + index * 4), actual.UnsignedWholePixels, "Egg motion native whole word");
                AssertEqual(Word(address + index * 4 + 2), actual.Fraction, "Egg motion native fraction word");
            }
        }
        for (int index = 0; index < 6; index++)
        {
            var actual = IntroEggMotion.FragmentInitialPosition(index);
            int address = IntroEggMotionDefinitions.InitialPositionReferenceAddress + index * 4;
            AssertEqual(unchecked((ushort)(Word(address) + 0x0010)), actual.X,
                $"Egg fragment {index} initial X");
            AssertEqual(unchecked((ushort)(Word(address + 2) + 0x003b)), actual.Y,
                $"Egg fragment {index} initial Y");
        }
        Compare(0x8ba9ea, 6, IntroEggMotion.FragmentX);
        Compare(0x8baa02, 41, frame => IntroEggMotion.FragmentY(frame, pastCurve));
        Compare(0x8bab35, 5, IntroEggMotion.SlimeX);
        Compare(0x8bab49, 62, frame => IntroEggMotion.SlimeY(frame, true));
        Compare(0x8bac41, 69, frame => IntroEggMotion.SlimeY(frame, false));
        var guarded = NewEggMotionGuard(rom);
        uint Move(uint position, int address) => unchecked(position + ((uint)Word(address) << 16) + Word(address + 2));
        int checkedFrames = 0;
        foreach (ushort fraction in new ushort[] { 0, 1, 0x7fff, 0xffff })
        {
            for (byte index = 0; index < 10; index++)
            {
                bool fragment = index < 6;
                byte actorIndex = fragment ? index : (byte)(index - 6);
                object actor = fragment ? new IntroEggParticle(actorIndex, pastCurve) : new IntroEggSlimeDrop(new(128, 96), actorIndex);
                IntroDiscoverySprite sprite = actor is IntroEggParticle eggParticle ? eggParticle.sprite : ((IntroEggSlimeDrop)actor).sprite;
                sprite.Position = new(sprite.Position.X.WithFraction(fraction), sprite.Position.Y.WithFraction(fraction));
                uint x = new FixedCoordinate(sprite.WholePosition.X, fraction).RawValue;
                uint y = new FixedCoordinate(sprite.WholePosition.Y, fraction).RawValue;
                bool motion = true;
                int motionFrame = 0;
                int frames = 0;
                while (sprite.IsActive && frames++ < 200)
                {
                    if (motion)
                    {
                        x = Move(x, (fragment ? 0x8ba9ea : 0x8bab35) + actorIndex * 4);
                        int yTable = fragment ? 0x8baa02 : (actorIndex & 1) != 0 ? 0x8bab49 : 0x8bac41;
                        y = Move(y, yTable + motionFrame * 4);
                        motionFrame++;
                        if (unchecked((short)((y >> 16) - 0xa8)) >= 0) motion = false;
                    }
                    if (actor is IntroEggParticle particle) particle.Step();
                    else ((IntroEggSlimeDrop)actor).Step();
                    AssertEqual(FixedCoordinate.WholeOf(unchecked((int)x)), sprite.WholePosition.X, "Egg actor whole X each frame");
                    AssertEqual((ushort)x, sprite.Position.X.Fraction, "Egg actor fraction X each frame");
                    AssertEqual(FixedCoordinate.WholeOf(unchecked((int)y)), sprite.WholePosition.Y, "Egg actor whole Y each frame");
                    AssertEqual((ushort)y, sprite.Position.Y.Fraction, "Egg actor fraction Y each frame");
                    checkedFrames++;
                }
                AssertTrue(!sprite.IsActive && !motion, "Egg actor reaches ground and deletes within its native curve");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotion.FragmentInitialPosition(6), "Fragment initial-position boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotion.FragmentY(41, pastCurve), "Fragment curve boundary after native overread");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotion.SlimeY(62, true), "Odd slime curve boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotion.SlimeY(69, false), "Even slime curve boundary");
        Console.WriteLine($"Egg motion definitions: twelve position words, 366 velocity words, and {checkedFrames} real actor frames match with all physical-table reads forbidden.");
    }

    /// <summary>Reads native egg motion bytes solely as the cartridge comparison oracle.</summary>
    private static byte CompiledIntroEggMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Rejects runtime rereads of compiled fragment positions and motion curves.</summary>
    private static CartridgeReadGuard NewEggMotionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8ba97c and < 0x8ba994 or
                >= 0x8ba9ea and < 0x8baaa6 or
                >= 0x8bab35 and < 0x8bad55,
            address => $"Runtime read compiled egg physical definition at {address:X6}.");
}
