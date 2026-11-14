using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Samus.Projectiles.SamusProjectilesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks compiled beam speed rows and missile acceleration through production projectile launch and motion.</summary>
internal static class BeamSpeedRowsVerification
{
    /// <summary>Compares native motion words, combination-dependent overreads, and six-frame missile trajectories.</summary>
    internal static void VerifyBeamSpeedRows()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SamusPoseId AimPose(byte direction) => SamusPoseNumbering.FromNumber(Enumerable.Range(0, 253).Where(SamusPoseNumbering.NamesPose).First(pose =>
            retail.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value + pose * 8 + 3) == direction));
        var room = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid());
        for (ushort combination = 0; combination < 12; combination++)
        for (byte direction = 0; direction < 10; direction++)
        {
            // Mechanics now use compiled native rows, not editable presentation data.
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = AimPose(direction), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = SamusEquipmentWordCodec.FromBeamWord(combination) };
            var projectiles = CreateProjectileFixture();
            projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.X }, new(0, 0), CreateBombFixture());
            AssertTrue(projectiles.Slots[0].IsActive && projectiles.Slots.Skip(1).All(slot => !slot.IsActive),
                "speed-row fixture allocates projectile");
            var shot = projectiles.Slots[0];
            int speed = direction is 1 or 3 or 6 or 8 ? 0x02ab : 0x0400;
            int x = direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0;
            int y = direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0;
            // The fire frame's beam pre-instruction adds one direction-indexed acceleration
            // word to the speed-row velocity before the slot is observable.
            EightEightVector<RoomPixels> acceleration = SamusProjectileAccelerations.For(SamusProjectileWordCodec.DecodeDirection(direction)).Beam;
            int ax = acceleration.X.InTwoHundredFiftySixthsOfPixel;
            int ay = acceleration.Y.InTwoHundredFiftySixthsOfPixel;
            AssertEqual(unchecked((short)(x + ax)), shot.Velocity.X.InTwoHundredFiftySixthsOfPixel, $"beam {combination} direction {direction} native speed-row X");
            AssertEqual(unchecked((short)(y + ay)), shot.Velocity.Y.InTwoHundredFiftySixthsOfPixel, $"beam {combination} direction {direction} native speed-row Y");
        }
        // Unlike the twelve identical authored rows, the Spazer and Plasma combinations C..F read different words past
        // the speed table, which the installed service holds (#1500). Each pair is the two words at its four-byte row.
        UnauthoredBeamFiring<SamusBeamCombination> unauthored = InstalledFixtures.SamusOverreads.Projectiles.UnauthoredBeamFiring;
        foreach (SamusBeamCombination beams in Enum.GetValues<SamusBeamCombination>())
        {
            int row = 0x90c2d1 + beams.TableIndex * 4;
            if (beams.IsRetail)
            {
                AssertThrows<ArgumentOutOfRangeException>(() => unauthored.Speed(beams, diagonal: false), $"{beams}'s authored speeds are not read past the table");
                continue;
            }
            AssertEqual(unchecked((short)retail.ReadCartridgeWord(row)), unauthored.Speed(beams, diagonal: false), $"{beams} cardinal speed read past the table");
            AssertEqual(unchecked((short)retail.ReadCartridgeWord(row + 2)), unauthored.Speed(beams, diagonal: true), $"{beams} diagonal speed read past the table");
        }
        // Exercise the actual initializer to detect a dropped combination index.
        var initialize = new Action<ProjectileLaunchInheritance, SamusProjectile>((records, launched) =>
            BeamFiring.InitializePowerBeamVelocity(records, launched, unauthored));
        var stationary = new ProjectileLaunchInheritance();
        // Each firing direction's beam, missile and Super Missile accelerations are the words the motion routines read
        // for that direction: sixty words in all.
        for (ushort direction = 0; direction < SamusSourceDefinitionsConstants.FiringDirectionCount; direction++)
        {
            SamusDirectionAccelerations accelerations = SamusProjectileAccelerations.For(SamusProjectileWordCodec.DecodeDirection(direction));
            short RomWord(int address) => unchecked((short)retail.ReadCartridgeWord(address));
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.BeamXAccelerations + direction * 2), accelerations.Beam.X.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} beam X acceleration matches the pinned ROM");
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.BeamYAccelerations + direction * 2), accelerations.Beam.Y.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} beam Y acceleration matches the pinned ROM");
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.MissileAccelerations + direction * 4), accelerations.Missile.X.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} missile X acceleration matches the pinned ROM");
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.MissileAccelerations + direction * 4 + 2), accelerations.Missile.Y.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} missile Y acceleration matches the pinned ROM");
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.SuperMissileAccelerations + direction * 4), accelerations.SuperMissile.X.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} Super Missile X acceleration matches the pinned ROM");
            AssertEqual(RomWord(SamusSourceDefinitionsConstants.SuperMissileAccelerations + direction * 4 + 2), accelerations.SuperMissile.Y.InTwoHundredFiftySixthsOfPixel,
                $"direction {direction} Super Missile Y acceleration matches the pinned ROM");
        }
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectile(0) { Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(combination), Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction) };
            initialize(stationary, slot);
            int speed = unchecked((short)retail.ReadCartridgeWord((0x90c2d1 + combination * 4 +
                (direction is 1 or 3 or 6 or 8 ? 2 : 0))));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)),
                slot.Velocity.X.InTwoHundredFiftySixthsOfPixel, "Actual initializer retains combination-dependent adjacent-table overreads X");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)),
                slot.Velocity.Y.InTwoHundredFiftySixthsOfPixel, "Actual initializer retains combination-dependent adjacent-table overreads Y");
        }
        for (ushort weapon = 1; weapon <= 2; weapon++)
        for (byte direction = 0; direction < 10; direction++)
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = AimPose(direction), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                SelectedHudItem = SuperMetroid.Diagnostics.Saves.SaveSlotNumbering.SavedHudItem(weapon), Missiles = 10, SuperMissiles = 10 };
            var projectiles = CreateProjectileFixture();
            var bombs = CreateBombFixture();
            int vx = direction is 1 or 2 or 3 ? 0x100 : direction is 6 or 7 or 8 ? -0x100 : 0;
            int vy = direction is 0 or 1 or 8 or 9 ? -0x100 : direction is 3 or 4 or 5 or 6 ? 0x100 : 0;
            uint x = 0, y = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                ControllerButton input = frame == 0 ? ControllerButton.X : 0;
                projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(0, 0), bombs);
                if (frame == 0)
                {
                    // Anchor the trajectory at the spawn origin: the fire-frame position less
                    // that frame's unaccelerated ignition step, which the update below re-adds.
                    var fired = projectiles.Slots[0];
                    x = unchecked((new FixedCoordinate(fired.Position.X.Whole, fired.Position.X.Fraction).RawValue) - (uint)SixteenSixteen.FromEightEight(vx));
                    y = unchecked((new FixedCoordinate(fired.Position.Y.Whole, fired.Position.Y.Fraction).RawValue) - (uint)SixteenSixteen.FromEightEight(vy));
                }
                else
                {
                    int table = weapon == 1 ? 0x90c303 : 0x90c32b;
                    vx = unchecked((short)(vx + (short)retail.ReadCartridgeWord((table + direction * 4)) +
                        (weapon == 1 ? (short)retail.ReadCartridgeWord((0x90c353 + direction * 2)) : 0)));
                    vy = unchecked((short)(vy + (short)retail.ReadCartridgeWord((table + direction * 4 + 2)) +
                        (weapon == 1 ? (short)retail.ReadCartridgeWord((0x90c367 + direction * 2)) : 0)));
                }
                x = unchecked(x + (uint)SixteenSixteen.FromEightEight(vx));
                y = unchecked(y + (uint)SixteenSixteen.FromEightEight(vy));
                var owner = projectiles.Slots[0];
                AssertTrue(owner.IsActive, "Motion fixture retains its visible missile owner");
                AssertEqual((short)vx, owner.Velocity.X.InTwoHundredFiftySixthsOfPixel, "Native missile acceleration X across ignition and movement");
                AssertEqual((short)vy, owner.Velocity.Y.InTwoHundredFiftySixthsOfPixel, "Native missile acceleration Y across ignition and movement");
                AssertEqual(x, new FixedCoordinate(owner.Position.X.Whole, owner.Position.X.Fraction).RawValue, "Missile X trajectory retains fixed-point carry");
                AssertEqual(y, new FixedCoordinate(owner.Position.Y.Whole, owner.Position.Y.Fraction).RawValue, "Missile Y trajectory retains fixed-point carry");
            }
        }
        Console.WriteLine("Projectile motion definitions: 60 native acceleration words by direction, the Spazer/Plasma launch speeds, 120 beam launches, 160 indexed initializations and 120 missile trajectory frames pass with motion ROM reads forbidden.");
    }

    /// <summary>Forwards fixture writes while rejecting native projectile-mechanics source reads.</summary>
    internal sealed class BeamSpeedRowAddressSpace(ISnesAddressSpace source) : ISnesMemoryBus, IImportCartridgeSource
    {
        /// <summary>Routes WRAM reads through the guarded address-space fixture.</summary>
        public byte ReadWorkRamByte(int address) => ReadByte(address);
        /// <summary>Routes cartridge reads through the guard that rejects migrated motion tables.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Fixture writes overlaid on the retail source without modifying its bytes.</summary>
        private readonly Dictionary<int, byte> _overrides = new();
        /// <summary>Rejects compiled mechanics ranges before selecting an overlay byte or source byte.</summary>
        public byte ReadByte(int address)
        {
            if (address is >= 0x90c2d1 and < 0x90c37b or
                >= SamusBeamFrameBehaviorCodes.UnchargedTable and < SamusBeamFrameBehaviorCodes.UnchargedTable + 32 or
                >= SamusBeamFrameBehaviorCodes.ChargedTable and < SamusBeamFrameBehaviorCodes.ChargedTable + 32)
                throw new InvalidOperationException($"Compiled projectile mechanics unexpectedly read ROM ${address:X6}.");
            return _overrides.TryGetValue(address, out byte value) ? value : CpuBusRegionRead.Read(source, address);
        }
        /// <summary>Records a fixture-owned bus byte in the overlay.</summary>
        public void WriteByte(int address, byte value) => _overrides[address] = value;
        /// <summary>Writes a little-endian fixture word into consecutive overlay bytes.</summary>
        public void SetWord(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, LittleEndianBytes.HighByte(value));
        }
    }

    /// <summary>Runs the compiled projectile-motion comparison against the retail ROM.</summary>
    [VerificationCommand("--projectile-motion")]
    internal static int ProjectileMotionCommand()
    {
        Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
        return 0;
    }

    /// <summary>Runs beam speed and production projectile checks together.</summary>
    [VerificationCommand("--beam-speed-rows")]
    internal static int BeamSpeedRowsCommand()
    {
        Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
        Suite(nameof(VerifySamusPowerBeamProjectiles), () => VerifySamusPowerBeamProjectiles());
        return 0;
    }
}
