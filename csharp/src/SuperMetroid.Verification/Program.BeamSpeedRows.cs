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
            // Mechanics now use compiled native rows, not editable presentation data.
            var bus = new BeamSpeedRowAddressSpace(retail);
            bus.WriteByte(SamusMovementRomData.Poses.Definitions + 8 + 3, direction);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                EquippedBeams = combination };
            var projectiles = new SamusProjectileSystem();
            var result = projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                (ushort)SnesButton.X, 0, 0, new SamusBombProjectileSystem());
            AssertEqual((int?)0, result.FiredSlot, "speed-row fixture allocates projectile");
            var shot = projectiles.LastFiredProjectileSnapshot!.Value;
            int speed = direction is 1 or 3 or 6 or 8 ? 0x02ab : 0x0400;
            int x = direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0;
            int y = direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0;
            AssertEqual(x, shot.XVelocity, $"beam {combination} direction {direction} native speed-row X");
            AssertEqual(y, shot.YVelocity, $"beam {combination} direction {direction} native speed-row Y");
        }
        // Unlike the twelve identical authored rows, C..F overread different adjacent
        // words. Exercise the actual initializer to detect a dropped combination index.
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePowerBeamVelocity",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        ushort Word(int address) => (ushort)(retail.ReadByte(address) | retail.ReadByte(address + 1) << 8);
        var guarded = new BeamSpeedRowAddressSpace(retail);
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(guarded, address),
                "All 85 compiled motion words match the pinned ROM without authored reads");
        for (int address = 0x90c2b0; address < 0x90c3a0; address++)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(retail, address),
                "Exact-address dispatch preserves neighboring and unaligned word reads");
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectileSlot(0) { Type = combination, Direction = direction };
            initialize(guarded, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 +
                (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)),
                slot.XVelocity, "Actual initializer retains combination-dependent adjacent-table overreads X");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)),
                slot.YVelocity, "Actual initializer retains combination-dependent adjacent-table overreads Y");
        }
        for (ushort weapon = 1; weapon <= 2; weapon++)
        for (byte direction = 0; direction < 10; direction++)
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            bus.WriteByte(SamusMovementRomData.Poses.Definitions + 8 + 3, direction);
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                SelectedHudItem = weapon, Missiles = 10, SuperMissiles = 10 };
            var projectiles = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            int vx = direction is 1 or 2 or 3 ? 0x100 : direction is 6 or 7 or 8 ? -0x100 : 0;
            int vy = direction is 0 or 1 or 8 or 9 ? -0x100 : direction is 3 or 4 or 5 or 6 ? 0x100 : 0;
            uint x = 0, y = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                projectiles.StepFrame(bus, room, samus, input, input, 0, 0, bombs);
                if (frame == 0)
                {
                    var origin = projectiles.LastFiredProjectileSnapshot!.Value;
                    x = (uint)origin.XPosition << 16;
                    y = (uint)origin.YPosition << 16;
                }
                else
                {
                    int table = weapon == 1 ? 0x90c303 : 0x90c32b;
                    vx = unchecked((short)(vx + (short)Word(table + direction * 4) +
                        (weapon == 1 ? (short)Word(0x90c353 + direction * 2) : 0)));
                    vy = unchecked((short)(vy + (short)Word(table + direction * 4 + 2) +
                        (weapon == 1 ? (short)Word(0x90c367 + direction * 2) : 0)));
                }
                x = unchecked(x + (uint)(vx << 8));
                y = unchecked(y + (uint)(vy << 8));
                var owner = projectiles.Slots[0];
                AssertTrue(owner.IsActive, "Motion fixture retains its visible missile owner");
                AssertEqual((short)vx, owner.XVelocity, "Native missile acceleration X across ignition and movement");
                AssertEqual((short)vy, owner.YVelocity, "Native missile acceleration Y across ignition and movement");
                AssertEqual(x, ((uint)owner.XPosition << 16) | owner.XSubposition, "Missile X trajectory retains fixed-point carry");
                AssertEqual(y, ((uint)owner.YPosition << 16) | owner.YSubposition, "Missile Y trajectory retains fixed-point carry");
            }
        }
        Console.WriteLine("Projectile motion definitions: 85 native words, neighboring/unaligned reads, 120 beam launches, 160 indexed initializations and 120 missile trajectory frames pass with motion ROM reads forbidden.");
    }

    private sealed class BeamSpeedRowAddressSpace(ISnesAddressSpace source) : ISnesAddressSpace
    {
        private readonly Dictionary<int, byte> _overrides = new();
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c2d1 and < 0x90c37b)
                throw new InvalidOperationException($"Compiled projectile motion unexpectedly reads ROM ${address:X6}.");
            return _overrides.TryGetValue(address, out byte value) ? value : source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => _overrides[address] = value;
        public void SetWord(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, (byte)(value >> 8));
        }
    }
}
