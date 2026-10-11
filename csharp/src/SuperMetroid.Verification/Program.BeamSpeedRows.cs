using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for beam speed rows, pose aim directions and missile acceleration words")]
    private static void VerifyBeamSpeedRows()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusPoseId AimPose(byte direction) => (SamusPoseId)Enumerable.Range(0, 253).First(pose =>
            retail.ReadCartridgeByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction);
        var room = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        for (ushort combination = 0; combination < 12; combination++)
        for (byte direction = 0; direction < 10; direction++)
        {
            // Mechanics now use compiled native rows, not editable presentation data.
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState { Pose = AimPose(direction), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = combination };
            var projectiles = CreateProjectileFixture();
            projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                (ushort)SnesButton.X, new(0, 0), CreateBombFixture());
            AssertTrue(projectiles.Slots[0].IsActive && projectiles.Slots.Skip(1).All(slot => !slot.IsActive),
                "speed-row fixture allocates projectile");
            var shot = projectiles.Slots[0];
            int speed = direction is 1 or 3 or 6 or 8 ? 0x02ab : 0x0400;
            int x = direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0;
            int y = direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0;
            // The fire frame's beam pre-instruction adds one direction-indexed acceleration
            // word to the speed-row velocity before the slot is observable.
            short ax = unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.XAccelerations + direction * 2));
            short ay = unchecked((short)SamusProjectileMotionDefinitions.ReadWord(
                SamusProjectileRomData.Beams.YAccelerations + direction * 2));
            AssertEqual(unchecked((short)(x + ax)), shot.Velocity.X.SignedRawValue, $"beam {combination} direction {direction} native speed-row X");
            AssertEqual(unchecked((short)(y + ay)), shot.Velocity.Y.SignedRawValue, $"beam {combination} direction {direction} native speed-row Y");
        }
        // Unlike the twelve identical authored rows, C..F overread different adjacent
        // words. Exercise the actual initializer to detect a dropped combination index.
        var initialize = typeof(SamusProjectileSystem).GetMethod("InitializePowerBeamVelocity",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        ushort Word(int address) => (ushort)(retail.ReadCartridgeByte(address) | retail.ReadCartridgeByte(address + 1) << 8);
        var guarded = new BeamSpeedRowAddressSpace(retail);
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(Word(address), SamusProjectileMotionDefinitions.ReadWord(address),
                "All 85 compiled motion words match the pinned ROM without authored reads");
        foreach (int address in new[] { 0x908000, 0x90c2d0, 0x90c2d2, 0x90c37b, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => SamusProjectileMotionDefinitions.ReadWord(address),
                "Unknown or unaligned projectile motion address fails instead of reading adjacent ROM");
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectileSlot(0) { Type = combination, Direction = direction };
            initialize(guarded, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 +
                (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)),
                slot.Velocity.X.SignedRawValue, "Actual initializer retains combination-dependent adjacent-table overreads X");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)),
                slot.Velocity.Y.SignedRawValue, "Actual initializer retains combination-dependent adjacent-table overreads Y");
        }
        for (ushort weapon = 1; weapon <= 2; weapon++)
        for (byte direction = 0; direction < 10; direction++)
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState { Pose = AimPose(direction), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                SelectedHudItem = weapon, Missiles = 10, SuperMissiles = 10 };
            var projectiles = CreateProjectileFixture();
            var bombs = CreateBombFixture();
            int vx = direction is 1 or 2 or 3 ? 0x100 : direction is 6 or 7 or 8 ? -0x100 : 0;
            int vy = direction is 0 or 1 or 8 or 9 ? -0x100 : direction is 3 or 4 or 5 or 6 ? 0x100 : 0;
            uint x = 0, y = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                projectiles.StepFrame(bus, room, samus, input, input, new(0, 0), bombs);
                if (frame == 0)
                {
                    // Anchor the trajectory at the spawn origin: the fire-frame position less
                    // that frame's unaccelerated ignition step, which the update below re-adds.
                    var fired = projectiles.Slots[0];
                    x = unchecked((((uint)fired.Position.X.Whole << 16) | fired.Position.X.Fraction) - (uint)(vx << 8));
                    y = unchecked((((uint)fired.Position.Y.Whole << 16) | fired.Position.Y.Fraction) - (uint)(vy << 8));
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
                AssertEqual((short)vx, owner.Velocity.X.SignedRawValue, "Native missile acceleration X across ignition and movement");
                AssertEqual((short)vy, owner.Velocity.Y.SignedRawValue, "Native missile acceleration Y across ignition and movement");
                AssertEqual(x, ((uint)owner.Position.X.Whole << 16) | owner.Position.X.Fraction, "Missile X trajectory retains fixed-point carry");
                AssertEqual(y, ((uint)owner.Position.Y.Whole << 16) | owner.Position.Y.Fraction, "Missile Y trajectory retains fixed-point carry");
            }
        }
        Console.WriteLine("Projectile motion definitions: 85 native words, loud non-catalog rejection, 120 beam launches, 160 indexed initializations and 120 missile trajectory frames pass with motion ROM reads forbidden.");
    }

    [CpuBusBoundary("exact bus test: write-overlay fixture over the retail bus that rejects compiled projectile-mechanics reads")]
    private sealed class BeamSpeedRowAddressSpace(ISnesAddressSpace source) : ISnesMemoryBus, IImportCartridgeSource
    {
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        public byte ReadSaveRamByte(int address) => ReadByte(address);
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        private readonly Dictionary<int, byte> _overrides = new();
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c2d1 and < 0x90c37b or
                >= SamusBeamPreInstructionCodesConstants.UnchargedTable and < SamusBeamPreInstructionCodesConstants.UnchargedTable + 32 or
                >= SamusBeamPreInstructionCodesConstants.ChargedTable and < SamusBeamPreInstructionCodesConstants.ChargedTable + 32)
                throw new InvalidOperationException($"Compiled projectile mechanics unexpectedly read ROM ${address:X6}.");
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
