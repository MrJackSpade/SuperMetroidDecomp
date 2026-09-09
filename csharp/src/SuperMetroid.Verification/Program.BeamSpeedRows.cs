using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBeamSpeedRows()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = new RoomLevelData(16, 16, new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        for (ushort combination = 0; combination < 12; combination++)
        for (byte direction = 0; direction < 10; direction++)
        {
            // Retail's twelve rows happen to be identical. Distinct fixture rows expose
            // a missing index that ordinary cartridge-only tests cannot distinguish.
            var bus = new BeamSpeedRowAddressSpace(retail);
            for (int row = 0; row < 12; row++)
            {
                bus.SetWord(0x90c2d1 + row * 4, (ushort)(0x0400 + row * 16));
                bus.SetWord(0x90c2d3 + row * 4, (ushort)(0x0200 + row * 16));
            }
            bus.WriteByte(SamusMovementRomData.Poses.Definitions + 8 + 3, direction);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                EquippedBeams = combination };
            var projectiles = new SamusProjectileSystem();
            var result = projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                (ushort)SnesButton.X, 0, 0, new SamusBombProjectileSystem());
            AssertEqual((int?)0, result.FiredSlot, "speed-row fixture allocates projectile");
            var shot = projectiles.LastFiredProjectileSnapshot!.Value;
            int speed = (direction is 1 or 3 or 6 or 8 ? 0x0200 : 0x0400) + combination * 16;
            int x = direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0;
            int y = direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0;
            AssertEqual(x, shot.XVelocity, $"beam {combination} direction {direction} native speed-row X");
            AssertEqual(y, shot.YVelocity, $"beam {combination} direction {direction} native speed-row Y");
        }
        Console.WriteLine("Beam speed rows: 120 production firing cases select combination and direction independently.");
    }

    private sealed class BeamSpeedRowAddressSpace(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private readonly Dictionary<int, byte> _overrides = new();
        public byte ReadByte(int address) => _overrides.TryGetValue(address, out byte value) ? value : source.ReadByte(address);
        public void WriteByte(int address, byte value) => _overrides[address] = value;
        public void SetWord(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, (byte)(value >> 8));
        }
    }
}
