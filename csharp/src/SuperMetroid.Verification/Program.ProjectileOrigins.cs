using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyProjectileOrigins(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
        var bus = new ProjectileOriginReadGuard(rom);
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePosition", BindingFlags.NonPublic | BindingFlags.Static)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusState, SamusProjectileSlot>>();
        var samus = new SamusState();
        var slot = new SamusProjectileSlot(0);

        void Check(byte pose, ushort direction, ushort x, ushort y)
        {
            samus.Pose = pose; samus.XPosition = x; samus.YPosition = y; slot.Direction = direction;
            bool running = rom.ReadByte(0x91b629 + pose * 8 + 1) == 1 || pose is 0x75 or 0x76;
            int offset = (direction & 15) * 2;
            short dx = Word((running ? 0x90c22c : 0x90c204) + offset);
            short dy = Word((running ? 0x90c240 : 0x90c218) + offset);
            byte correction = rom.ReadByte(0x91b629 + pose * 8 + 4);
            initialize(bus, samus, slot);
            AssertEqual(unchecked((ushort)(x + dx)), slot.XPosition, "Physical muzzle X follows native direction/table selection");
            AssertEqual(unchecked((ushort)(y + dy - correction)), slot.YPosition, "Physical muzzle Y follows native signed origin and unsigned pose correction");
            AssertEqual(direction, slot.Direction, "Origin lookup does not rewrite lifecycle/direction bits");
            AssertEqual(x, samus.XPosition, "Origin setup leaves Samus X unchanged");
            AssertEqual(y, samus.YPosition, "Origin setup leaves Samus Y unchanged");
        }

        // All direction words, including lifecycle high bits and low-nibble overreads,
        // through standing, running, both special Moonwalk poses and ordinary Moonwalk.
        foreach (byte pose in new byte[] { 1, 9, 0x75, 0x76, 0x49 })
        for (int word = 0; word <= ushort.MaxValue; word++)
            Check(pose, (ushort)word, (ushort)word, unchecked((ushort)~word));
        foreach (ushort boundary in new ushort[] { 0, 127, 0x8000, ushort.MaxValue })
        for (int pose = 0; pose <= 0xfc; pose++)
        for (ushort direction = 0; direction < 16; direction++)
            Check((byte)pose, direction, boundary, boundary);

        // Ten authored directions cover every word. Extra nibble values intentionally
        // cross table boundaries; running Y reaches adjacent cooldown bytes unchanged.
        foreach (bool running in new[] { false, true })
        for (ushort direction = 0; direction < 16; direction++)
        {
            var actual = SamusProjectileOriginDefinitions.Read(bus, running, direction);
            AssertEqual((Word((running ? 0x90c22c : 0x90c204) + direction * 2),
                Word((running ? 0x90c240 : 0x90c218) + direction * 2)), actual,
                "Forty native origin words and cross-row/cooldown overreads");
        }
        Console.WriteLine("Projectile origins: forty native words and 343872 real position initializations cover all direction words, every pose, coordinate boundaries and adjacent cooldown reads with authored reads forbidden.");
    }

    private sealed class ProjectileOriginReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c204 and < 0x90c28f)
                throw new InvalidOperationException($"Compiled projectile origin read ROM ${address:X6}.");
            // Deliberately different presentation bytes cannot affect physical origins.
            if (address is >= 0x90c1a8 and < 0x90c204) return 0x5a;
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
