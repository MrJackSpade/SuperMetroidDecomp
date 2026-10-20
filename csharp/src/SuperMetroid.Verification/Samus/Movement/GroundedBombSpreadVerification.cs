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
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks grounded Bomb Spread charge, spawn, and five-slot free-flight behavior.</summary>
internal static class GroundedBombSpreadVerification
{
    /// <summary>Compares release and timeout boundaries plus each 16.16 trajectory and fuse with native tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the five native bomb-spread velocity and fuse tables")]
    internal static void VerifyGroundedBombSpread()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(128, 128), new RoomLevelPlanes(new ushort[16384], new byte[16384], new ushort[16384]), new byte[8]);
        foreach (int hold in new[] { 0, 1, 63, 64, 127, 128, 191, 192 })
        foreach (bool keepDown in hold == 192 ? new[] { false, true } : new[] { false })
        {
            var samus = new SamusState { Pose = SamusPoseId.MorphBallGroundRightPose,
                EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)), ProjectileFlareCounter = SamusBombSpreadRomData.RequiredChargeFrames };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.Kinematics.YAcceleration = new SnesUnsignedSixteenSixteen(0, 0x1c00);
            var bombs = CreateBombFixture();
            SnesButton shootDown = (SnesButton.X | SnesButton.Down);
            for (int tick = 0; tick < hold; tick++)
            {
                bombs.StepFrame(new SamusTerrain { Level = level }, samus, shootDown, 0);
                AssertTrue(bombs.Slots.All(slot => !slot.IsActive) && bombs.BombCounter == 0, "Down+Shoot postpones grounded spread");
                AssertEqual(SamusBombSpreadRomData.RequiredChargeFrames, samus.ProjectileFlareCounter, "retained spread charge is not consumed while holding Down");
                AssertEqual((ushort)(tick + 1), samus.BombSpreadChargeTimeoutCounter, "grounded spread hold counter advances once per tick");
            }
            var released = bombs.StepFrame(new SamusTerrain { Level = level }, samus, keepDown ? shootDown : SnesButton.X, 0);
            AssertTrue(bombs.Slots.All(slot => slot.IsActive && slot.IsBombSpread) && released.BeamChargeConsumed, "release or timeout creates spread and consumes charge");
            AssertEqual(5, bombs.BombCounter, "spread fills all five bomb slots");
            AssertEqual(0, samus.ProjectileFlareCounter, "spawn clears actual flare counter");
            AssertEqual(0, samus.BombSpreadChargeTimeoutCounter, "spawn clears hold counter");
            // Independent closed-form free-flight expectations from native D849/D8F7:
            // acceleration precedes integration, including the spawning alpha call.
            for (int tick = 1; tick <= 60; tick++)
            {
                if (tick != 1) bombs.StepFrame(new SamusTerrain { Level = level }, samus, SnesButton.X, 0);
                for (int slotIndex = 0; slotIndex < bombs.Slots.Count; slotIndex++)
                {
                    var slot = bombs.Slots[slotIndex];
                    ushort Read(int address) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address + slotIndex * 2);
                    ushort encodedX = Read(SamusBombSpreadRomData.XVelocities);
                    long initialY = -(Read(SamusBombSpreadRomData.YSpeeds) + (hold >> 6 & 3)) * 65536L + Read(SamusBombSpreadRomData.YSubspeeds);
                    long expectedX = 512L * 65536 + tick * (encodedX & 0x7fff) * 256L * ((encodedX & 0x8000) == 0 ? 1 : -1);
                    long expectedY = 512L * 65536 + tick * initialY + 0x1c00L * tick * (tick + 1) / 2;
                    string context = $"hold {hold}, down {keepDown}, tick {tick}, slot {slotIndex}";
                    AssertEqual((ushort)SnesSixteenSixteen.WholeOf(unchecked((int)expectedX)), slot.Position.X.Whole, $"spread X: {context}");
                    AssertEqual((ushort)expectedX, slot.Position.X.Fraction, $"spread X fraction: {context}");
                    AssertEqual((ushort)SnesSixteenSixteen.WholeOf(unchecked((int)expectedY)), slot.Position.Y.Whole, $"spread Y: {context}");
                    AssertEqual((ushort)expectedY, slot.Position.Y.Fraction, $"spread Y fraction: {context}");
                    AssertEqual((ushort)(Read(SamusBombSpreadRomData.FuseTimers) - tick), slot.BombTimer.RawValue, $"individual spread fuse: {context}");
                }
            }
        }
        Console.WriteLine("Grounded Bomb Spread: retained charge, release/timeout boundaries and all five 16.16 free-flight trajectories pass.");
    }

    /// <summary>Runs the focused grounded Bomb Spread timing and motion check.</summary>
    [VerificationCommand("--grounded-bomb-spread")]
    internal static int GroundedBombSpreadCommand()
    {
        Suite(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread());
        return 0;
    }
}
