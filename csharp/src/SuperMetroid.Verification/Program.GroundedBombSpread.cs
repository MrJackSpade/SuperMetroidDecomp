using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyGroundedBombSpread()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(128, 128, new ushort[16384], new byte[16384], new ushort[16384], new byte[8]);
        foreach (int hold in new[] { 0, 1, 63, 64, 127, 128, 191, 192 })
        foreach (bool keepDown in hold == 192 ? new[] { false, true } : new[] { false })
        {
            var samus = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose,
                EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
                XPosition = 512, YPosition = 512, ProjectileFlareCounter = SamusBombSpreadRomData.RequiredChargeFrames };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.YAcceleration = 0;
            samus.Kinematics.YSubacceleration = 0x1c00;
            var bombs = new SamusBombProjectileSystem();
            ushort shootDown = (ushort)(SnesButton.X | SnesButton.Down);
            for (int tick = 0; tick < hold; tick++)
            {
                var charging = bombs.StepFrame(bus, level, samus, shootDown, 0);
                AssertTrue(!charging.BombSpreadStarted && bombs.BombCounter == 0, "Down+Shoot postpones grounded spread");
                AssertEqual(SamusBombSpreadRomData.RequiredChargeFrames, samus.ProjectileFlareCounter, "retained spread charge is not consumed while holding Down");
                AssertEqual((ushort)(tick + 1), samus.BombSpreadChargeTimeoutCounter, "grounded spread hold counter advances once per tick");
            }
            var released = bombs.StepFrame(bus, level, samus, keepDown ? shootDown : (ushort)SnesButton.X, 0);
            AssertTrue(released.BombSpreadStarted && released.BeamChargeConsumed, "release or timeout creates spread and consumes charge");
            AssertEqual((ushort)5, bombs.BombCounter, "spread fills all five bomb slots");
            AssertEqual((ushort)0, samus.ProjectileFlareCounter, "spawn clears actual flare counter");
            AssertEqual((ushort)0, samus.BombSpreadChargeTimeoutCounter, "spawn clears hold counter");
            // Independent closed-form free-flight expectations from native D849/D8F7:
            // acceleration precedes integration, including the spawning alpha call.
            for (int tick = 1; tick <= 60; tick++)
            {
                if (tick != 1) bombs.StepFrame(bus, level, samus, (ushort)SnesButton.X, 0);
                for (int slotIndex = 0; slotIndex < bombs.Slots.Count; slotIndex++)
                {
                    var slot = bombs.Slots[slotIndex];
                    ushort Read(int address) => RomDataReader.ReadWordFixedBank(bus, address + slotIndex * 2);
                    ushort encodedX = Read(SamusBombSpreadRomData.XVelocities);
                    long initialY = -(Read(SamusBombSpreadRomData.YSpeeds) + (hold >> 6 & 3)) * 65536L + Read(SamusBombSpreadRomData.YSubspeeds);
                    long expectedX = 512L * 65536 + tick * (encodedX & 0x7fff) * 256L * ((encodedX & 0x8000) == 0 ? 1 : -1);
                    long expectedY = 512L * 65536 + tick * initialY + 0x1c00L * tick * (tick + 1) / 2;
                    string context = $"hold {hold}, down {keepDown}, tick {tick}, slot {slotIndex}";
                    AssertEqual((ushort)(expectedX >> 16), slot.XPosition, $"spread X: {context}");
                    AssertEqual((ushort)expectedX, slot.XSubposition, $"spread X fraction: {context}");
                    AssertEqual((ushort)(expectedY >> 16), slot.YPosition, $"spread Y: {context}");
                    AssertEqual((ushort)expectedY, slot.YSubposition, $"spread Y fraction: {context}");
                    AssertEqual((ushort)(Read(SamusBombSpreadRomData.FuseTimers) - tick), slot.BombTimer, $"individual spread fuse: {context}");
                }
            }
        }
        Console.WriteLine("Grounded Bomb Spread: retained charge, release/timeout boundaries and all five 16.16 free-flight trajectories pass.");
    }
}
