using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledIntroEggMotion(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        void Compare(int address, int count, Func<int, (ushort Whole, ushort Fraction)> sample)
        {
            for (int index = 0; index < count; index++)
            {
                var actual = sample(index);
                AssertEqual(Word(address + index * 4), actual.Whole, "Egg motion native whole word");
                AssertEqual(Word(address + index * 4 + 2), actual.Fraction, "Egg motion native fraction word");
            }
        }
        Compare(0x8ba9ea, 6, IntroEggMotionDefinitions.FragmentX);
        Compare(0x8baa02, 41, IntroEggMotionDefinitions.FragmentY);
        Compare(0x8bab35, 5, IntroEggMotionDefinitions.SlimeX);
        Compare(0x8bab49, 62, frame => IntroEggMotionDefinitions.SlimeY(frame, true));
        Compare(0x8bac41, 69, frame => IntroEggMotionDefinitions.SlimeY(frame, false));
        var guarded = new EggMotionReadGuard(rom);
        uint Move(uint position, int address) => unchecked(position + ((uint)Word(address) << 16) + Word(address + 2));
        int checkedFrames = 0;
        foreach (ushort fraction in new ushort[] { 0, 1, 0x7fff, 0xffff })
        {
            for (byte index = 0; index < 10; index++)
            {
                bool fragment = index < 6;
                byte actorIndex = fragment ? index : (byte)(index - 6);
                object actor = fragment ? new IntroEggParticle(guarded, actorIndex) : new IntroEggSlimeDrop(128, 96, actorIndex);
                var sprite = (IntroDiscoverySprite)actor.GetType().GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(actor)!;
                sprite.XSubPosition = sprite.YSubPosition = fraction;
                uint x = ((uint)sprite.XPosition << 16) | fraction;
                uint y = ((uint)sprite.YPosition << 16) | fraction;
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
                    if (actor is IntroEggParticle particle) particle.Step(guarded);
                    else ((IntroEggSlimeDrop)actor).Step(guarded);
                    AssertEqual((ushort)(x >> 16), sprite.XPosition, "Egg actor whole X each frame");
                    AssertEqual((ushort)x, sprite.XSubPosition, "Egg actor fraction X each frame");
                    AssertEqual((ushort)(y >> 16), sprite.YPosition, "Egg actor whole Y each frame");
                    AssertEqual((ushort)y, sprite.YSubPosition, "Egg actor fraction Y each frame");
                    checkedFrames++;
                }
                AssertTrue(!sprite.IsActive && !motion, "Egg actor reaches ground and deletes within its native curve");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.FragmentY(41), "Fragment curve boundary after native overread");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.SlimeY(62, true), "Odd slime curve boundary");
        AssertThrows<ArgumentOutOfRangeException>(() => IntroEggMotionDefinitions.SlimeY(69, false), "Even slime curve boundary");
        Console.WriteLine($"Egg motion definitions: 366 native words and {checkedFrames} real actor frames match with velocity-table reads forbidden.");
    }

    private sealed class EggMotionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x8ba9ea and < 0x8baaa6 or >= 0x8bab35 and < 0x8bad55)
                throw new InvalidOperationException($"Runtime read compiled egg velocity at {address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
