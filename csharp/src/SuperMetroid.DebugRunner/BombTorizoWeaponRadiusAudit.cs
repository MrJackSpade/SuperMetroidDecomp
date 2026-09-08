using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class BombTorizoAudit
{
    private static HashSet<(ushort X, ushort Y)> VerifyLiveWeaponRadii(SuperMetroidAddressSpace bus)
    {
        var radii = new HashSet<(ushort X, ushort Y)>();
        var air = new RoomLevelData(16, 16, new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        int checkedFrames = 0;
        foreach (byte pose in new[] { SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose })
        foreach (ushort item in new ushort[] { 0, 1, 2 })
        {
            var samus = new SamusState { Pose = pose, XPosition = 128, YPosition = 96,
                SelectedHudItem = item, Missiles = 10, MaxMissiles = 10,
                SuperMissiles = 10, MaxSuperMissiles = 10 };
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            var fired = shots.StepFrame(bus, air, samus, (ushort)SnesButton.X,
                (ushort)SnesButton.X, 0, 0, bombs);
            if (fired.FiredSlot is not int index)
                throw new InvalidDataException($"Retail weapon {item}, pose {pose}: no shot fired.");
            var shot = shots.Slots[index];
            int table = item == 0 ? SamusProjectileRomData.Beams.UnchargedDataPointers
                : SamusProjectileRomData.NonBeam.DataPointers;
            int data = SamusProjectileRomData.Banks.Projectile | ReadWord(bus, table + item * 2);
            ushort cursor = ReadWord(bus, data + 2 + shot.PackedDirection.DirectionIndex * 2);
            ushort timer = 1, expectedX = 0, expectedY = 0;
            for (int frame = 0; frame < 72; frame++)
            {
                // Independent scalar reading of $93:81E9: only a timer expiry
                // installs new radii, and goto is followed before the record is read.
                if (--timer == 0)
                {
                    while (ReadWord(bus, SamusProjectileRomData.Banks.Projectile | cursor) ==
                        SamusProjectileRomData.Instructions.GoTo)
                        cursor = ReadWord(bus, SamusProjectileRomData.Banks.Projectile | (cursor + 2));
                    int record = SamusProjectileRomData.Banks.Projectile | cursor;
                    timer = ReadWord(bus, record);
                    if (timer >= 32768)
                        throw new InvalidDataException("Unexpected non-flight opcode in retail weapon audit.");
                    expectedX = bus.ReadByte(record + 4);
                    expectedY = bus.ReadByte(record + 5);
                    cursor += 8;
                }
                if (!shot.IsActive || shot.XRadius != expectedX || shot.YRadius != expectedY ||
                    shot.InstructionTimer != timer || shot.InstructionPointer != cursor)
                    throw new InvalidDataException($"Retail weapon {item}, pose {pose}, frame {frame}: " +
                        $"radius {shot.XRadius}x{shot.YRadius}, expected {expectedX}x{expectedY}; " +
                        $"timer {shot.InstructionTimer}/{timer}, cursor {shot.InstructionPointer:X4}/{cursor:X4}.");
                radii.Add((shot.XRadius, shot.YRadius));
                checkedFrames++;
                // Retain a real flight animation without letting off-screen culling or
                // terrain end this geometry fixture. No projectile state is synthesized.
                shot.XPosition = 128;
                shot.YPosition = 96;
                shots.StepFrame(bus, air, samus, 0, 0, 0, 0, bombs, projectileProducerEnabled: false);
            }
        }
        Console.WriteLine($"Retail beam/missile/Super radii: {checkedFrames} live flight frames, " +
            $"both facings, values [{string.Join(", ", radii.Select(r => $"{r.X}x{r.Y}"))}] match bank-$93 records and timers.");
        return radii;
    }
}
