using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static MagicNumberAuditVerification;
using static SamusProjectilesVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using static RetailRuntimeFixtureVerification;

/// <summary>Checks compiled beam speed rows and missile acceleration through production projectile launch and motion.</summary>
internal static class BeamSpeedRowsVerification
{
    /// <summary>Compares native motion words, combination-dependent overreads, and six-frame missile trajectories.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for beam speed rows, pose aim directions and missile acceleration words")]
    internal static void VerifyBeamSpeedRows()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusPoseId AimPose(byte direction) => (SamusPoseId)Enumerable.Range(0, 253).First(pose =>
            retail.ReadCartridgeByte(SamusMovementRomData.Poses.Definitions + pose * 8 + 3) == direction);
        var room = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        for (ushort combination = 0; combination < 12; combination++)
        for (byte direction = 0; direction < 10; direction++)
        {
            // Mechanics now use compiled native rows, not editable presentation data.
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState { Pose = AimPose(direction), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = combination };
            var projectiles = CreateProjectileFixture();
            projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), CreateBombFixture());
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
        var initialize = typeof(BeamFiring).GetMethod("InitializePowerBeamVelocity",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        var guarded = new BeamSpeedRowAddressSpace(retail);
        for (int address = 0x90c2d1; address < 0x90c37b; address += 2)
            AssertEqual(retail.ReadCartridgeWord(address), SamusProjectileMotionDefinitions.ReadWord(address),
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
            int speed = unchecked((short)retail.ReadCartridgeWord((0x90c2d1 + combination * 4 +
                (direction is 1 or 3 or 6 or 8 ? 2 : 0))));
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
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(0, 0), bombs);
                if (frame == 0)
                {
                    // Anchor the trajectory at the spawn origin: the fire-frame position less
                    // that frame's unaccelerated ignition step, which the update below re-adds.
                    var fired = projectiles.Slots[0];
                    x = unchecked((new SnesFixedPosition(fired.Position.X.Whole, fired.Position.X.Fraction).RawValue) - (uint)SnesSixteenSixteen.FromEightEight(vx));
                    y = unchecked((new SnesFixedPosition(fired.Position.Y.Whole, fired.Position.Y.Fraction).RawValue) - (uint)SnesSixteenSixteen.FromEightEight(vy));
                }
                else
                {
                    int table = weapon == 1 ? 0x90c303 : 0x90c32b;
                    vx = unchecked((short)(vx + (short)retail.ReadCartridgeWord((table + direction * 4)) +
                        (weapon == 1 ? (short)retail.ReadCartridgeWord((0x90c353 + direction * 2)) : 0)));
                    vy = unchecked((short)(vy + (short)retail.ReadCartridgeWord((table + direction * 4 + 2)) +
                        (weapon == 1 ? (short)retail.ReadCartridgeWord((0x90c367 + direction * 2)) : 0)));
                }
                x = unchecked(x + (uint)SnesSixteenSixteen.FromEightEight(vx));
                y = unchecked(y + (uint)SnesSixteenSixteen.FromEightEight(vy));
                var owner = projectiles.Slots[0];
                AssertTrue(owner.IsActive, "Motion fixture retains its visible missile owner");
                AssertEqual((short)vx, owner.Velocity.X.SignedRawValue, "Native missile acceleration X across ignition and movement");
                AssertEqual((short)vy, owner.Velocity.Y.SignedRawValue, "Native missile acceleration Y across ignition and movement");
                AssertEqual(x, new SnesFixedPosition(owner.Position.X.Whole, owner.Position.X.Fraction).RawValue, "Missile X trajectory retains fixed-point carry");
                AssertEqual(y, new SnesFixedPosition(owner.Position.Y.Whole, owner.Position.Y.Fraction).RawValue, "Missile Y trajectory retains fixed-point carry");
            }
        }
        Console.WriteLine("Projectile motion definitions: 85 native words, loud non-catalog rejection, 120 beam launches, 160 indexed initializations and 120 missile trajectory frames pass with motion ROM reads forbidden.");
    }

    /// <summary>Forwards fixture writes while rejecting native projectile-mechanics source reads.</summary>
    [CpuBusBoundary("exact bus test: write-overlay fixture over the retail bus that rejects compiled projectile-mechanics reads")]
    internal sealed class BeamSpeedRowAddressSpace(ISnesAddressSpace source) : ISnesMemoryBus, IImportCartridgeSource
    {
        /// <summary>Routes WRAM reads through the guarded address-space fixture.</summary>
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        /// <summary>Routes save-RAM reads through the guarded address-space fixture.</summary>
        public byte ReadSaveRamByte(int address) => ReadByte(address);
        /// <summary>Routes cartridge reads through the guard that rejects migrated motion tables.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Fixture writes overlaid on the retail source without modifying its bytes.</summary>
        private readonly Dictionary<int, byte> _overrides = new();
        /// <summary>Rejects compiled mechanics ranges before selecting an overlay byte or source byte.</summary>
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c2d1 and < 0x90c37b or
                >= SamusBeamPreInstructionCodesConstants.UnchargedTable and < SamusBeamPreInstructionCodesConstants.UnchargedTable + 32 or
                >= SamusBeamPreInstructionCodesConstants.ChargedTable and < SamusBeamPreInstructionCodesConstants.ChargedTable + 32)
                throw new InvalidOperationException($"Compiled projectile mechanics unexpectedly read ROM ${address:X6}.");
            return _overrides.TryGetValue(address, out byte value) ? value : CpuBusRegionRead.Read(source, address);
        }
        /// <summary>Records a fixture-owned bus byte in the overlay.</summary>
        public void WriteByte(int address, byte value) => _overrides[address] = value;
        /// <summary>Writes a little-endian fixture word into consecutive overlay bytes.</summary>
        public void SetWord(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, SnesLittleEndian.HighByte(value));
        }
    }

    /// <summary>Runs the compiled projectile-motion comparison against the retail ROM.</summary>
    [VerificationCommand("--projectile-motion")]
    internal static int ProjectileMotionCommand()
    {
        Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
        return 0;
    }

    /// <summary>Runs beam speed, production projectile, and magic-number checks together.</summary>
    [VerificationCommand("--beam-speed-rows")]
    internal static int BeamSpeedRowsCommand()
    {
        Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
        Suite(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles());
        Suite(nameof(VerifyProductionMagicNumberAudit), () => VerifyProductionMagicNumberAudit());
        return 0;
    }
}
