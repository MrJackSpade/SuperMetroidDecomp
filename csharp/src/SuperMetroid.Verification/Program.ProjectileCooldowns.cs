using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyProjectileCooldowns()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bus = new ProjectileCooldownReadGuard(retail);
        for (int address = 0x90c250; address < 0x90c2b0; address++)
            AssertEqual(retail.ReadByte(address), SamusProjectileCooldownDefinitions.ReadByte(bus, address),
                "Compiled cooldown bytes and adjacent presentation fallback match native address identity");
        var room = CreateRoom(32, 16, new ushort[512], new byte[512]);
        var fire = typeof(SamusProjectileSystem).GetMethod("TryFireBeam", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        foreach (ushort edge in new ushort[] { 0, (ushort)SnesButton.X })
        {
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128, EquippedBeams = beam };
            var result = ((int? Slot, ushort Sound))fire.Invoke(projectiles,
                new object?[] { bus, room, samus, edge, shared, null, charged })!;
            AssertEqual((int?)0, result.Slot, "Cooldown check exercises the actual successful producer");
            int address = charged ? 0x90c264 + beam : edge == 0 ? 0x90c283 + beam : 0x90c254 + beam;
            AssertEqual((ushort)retail.ReadByte(address), shared.CooldownTimer,
                "Native charged/fresh/held selection installs exact shared delay");
        }
        foreach (ushort beam in new ushort[] { 0, 10 })
        {
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128, EquippedBeams = beam };
            var frames = new List<int>();
            for (int frame = 0; frame < 70; frame++)
            {
                // Actual shared cooldown owner advances before humanoid fire dispatch.
                shared.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                    frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                var result = projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                    frame == 0 ? (ushort)SnesButton.X : (ushort)0, 0, 0, shared);
                if (result.FiredSlot is not null) frames.Add(frame);
            }
            int firstDelay = beam == 10 ? 12 : 15;
            AssertTrue(frames.SequenceEqual(new[] { 0, firstDelay, firstDelay + 25, firstDelay + 50 }),
                "Held input fires at the native initial delay, then exact 25-frame auto-fire intervals");
        }
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                EquippedBeams = beam, SelectedHudItem = 3, PowerBombs = 10 };
            AssertTrue(projectiles.TryActivateCombo(bus, samus, shared, out _),
                "Each native special beam attack reaches its actual cooldown publisher");
            AssertEqual((ushort)retail.ReadByte(0x90c254 + (projectiles.Slots[0].Type & 0x3f)),
                shared.CooldownTimer, "Combo retains six-bit index into padding/non-beam neighbors");
        }
        Console.WriteLine("Projectile cooldowns: 59 native bytes, adjacent reads, 48 producer selections, four special attacks and two 70-frame held-fire sequences pass with cooldown ROM reads forbidden.");
    }

    private sealed class ProjectileCooldownReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c254 and < 0x90c28f)
                throw new InvalidOperationException($"Compiled cooldown read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
