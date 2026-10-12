using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledIntroEggMotion(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(CompiledIntroEggMotionRomByte(rom, address), CompiledIntroEggMotionRomByte(rom, address + 1)));
        void Compare(int address, int count, Func<int, (ushort Whole, ushort Fraction)> sample)
        {
            for (int index = 0; index < count; index++)
            {
                var actual = sample(index);
                AssertEqual(Word(address + index * 4), actual.Whole, "Egg motion native whole word");
                AssertEqual(Word(address + index * 4 + 2), actual.Fraction, "Egg motion native fraction word");
            }
        }
        for (int index = 0; index < 6; index++)
        {
            var actual = IntroEggMotionDefinitions.FragmentInitialPosition(index);
            int address = IntroEggMotionDefinitions.InitialPositionReferenceAddress + index * 4;
            AssertEqual(unchecked((ushort)(Word(address) + 0x0010)), actual.X,
                $"Egg fragment {index} initial X");
            AssertEqual(unchecked((ushort)(Word(address + 2) + 0x003b)), actual.Y,
                $"Egg fragment {index} initial Y");
        }
        Compare(0x8ba9ea, 6, IntroEggMotionDefinitions.FragmentX);
        Compare(0x8baa02, 41, IntroEggMotionDefinitions.FragmentY);
        Compare(0x8bab35, 5, IntroEggMotionDefinitions.SlimeX);
        Compare(0x8bab49, 62, frame => IntroEggMotionDefinitions.SlimeY(frame, true));
        Compare(0x8bac41, 69, frame => IntroEggMotionDefinitions.SlimeY(frame, false));
        var guarded = NewEggMotionGuard(rom);
        uint Move(uint position, int address) => unchecked(position + ((uint)Word(address) << 16) + Word(address + 2));
        int checkedFrames = 0;
        foreach (ushort fraction in new ushort[] { 0, 1, 0x7fff, 0xffff })
        {
            for (byte index = 0; index < 10; index++)
            {
                bool fragment = index < 6;
                byte actorIndex = fragment ? index : (byte)(index - 6);
                object actor = fragment ? new IntroEggParticle(actorIndex) : new IntroEggSlimeDrop(new(128, 96), actorIndex);
                var sprite = (IntroDiscoverySprite)actor.GetType().GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(actor)!;
                sprite.Position = new(sprite.Position.X.WithFraction(fraction), sprite.Position.Y.WithFraction(fraction));
                uint x = new SnesFixedPosition(sprite.WholePosition.X, fraction).RawValue;
                uint y = new SnesFixedPosition(sprite.WholePosition.Y, fraction).RawValue;
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
                    AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)x)), sprite.WholePosition.X, "Egg actor whole X each frame");
                    AssertEqual((ushort)x, sprite.Position.X.Fraction, "Egg actor fraction X each frame");
                    AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)y)), sprite.WholePosition.Y, "Egg actor whole Y each frame");
                    AssertEqual((ushort)y, sprite.Position.Y.Fraction, "Egg actor fraction Y each frame");
                    checkedFrames++;
                }
                AssertTrue(!sprite.IsActive && !motion, "Egg actor reaches ground and deletes within its native curve");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.FragmentInitialPosition(6), "Fragment initial-position boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.FragmentY(41), "Fragment curve boundary after native overread");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.SlimeY(62, true), "Odd slime curve boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.SlimeY(69, false), "Even slime curve boundary");
        Console.WriteLine($"Egg motion definitions: twelve position words, 366 velocity words, and {checkedFrames} real actor frames match with all physical-table reads forbidden.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled intro egg motion")]
    private static byte CompiledIntroEggMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    private static CartridgeReadGuard NewEggMotionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8ba97c and < 0x8ba994 or
                >= 0x8ba9ea and < 0x8baaa6 or
                >= 0x8bab35 and < 0x8bad55,
            address => $"Runtime read compiled egg physical definition at {address:X6}.");
}
