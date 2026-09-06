using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Deterministic first-explosion/second-shot timing sweep through the production projectile system.</summary>
internal static class MissileReuseAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int eligible = 0;
        int reused = 0;
        int overlapping = 0;
        int followupFrames = 0;
        for (ushort family = 1; family <= 2; family++)
        for (int wallColumn = 7; wallColumn < 18; wallColumn++)
        for (int secondFrame = 20; secondFrame <= 80; secondFrame++)
        {
            ushort[] tiles = new ushort[32 * 16];
            for (int row = 0; row < 16; row++) tiles[row * 32 + wallColumn] = 0x8000;
            var level = new RoomLevelData(32, 16, tiles, new byte[tiles.Length], new ushort[tiles.Length], new byte[8]);
            var samus = new SamusState { Pose = 1, XPosition = 64, YPosition = 96,
                SelectedHudItem = family, Missiles = 99, SuperMissiles = 99 };
            samus.InitializeAnimation(bus);
            var projectiles = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            bool firstExploded = false;
            for (int frame = 0; frame <= secondFrame; frame++)
            {
                ushort input = frame == 0 || frame == secondFrame ? (ushort)SnesButton.X : (ushort)0;
                bombs.StepFrame(bus, level, samus, input, input);
                var result = projectiles.StepFrame(bus, level, samus, input, input, 0, 0, bombs);
                if (frame < secondFrame)
                    firstExploded |= projectiles.Slots.Any(slot => slot.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion));
                if (frame != secondFrame || !firstExploded) continue;
                if (result.FiredSlot is not int index)
                    throw new InvalidDataException($"Second shot rejected: family {family}, wall {wallColumn}, delay {secondFrame}.");
                var shot = projectiles.Slots[index];
                ushort expectedType = family == 1 ? (ushort)0x8100 : (ushort)0x8200;
                ushort data = Word(SamusProjectileRomData.NonBeam.DataPointers + family * 2);
                ushort flight = Word(SamusProjectileRomData.Banks.Projectile | (data + 2 + shot.PackedDirection.DirectionIndex * 2));
                ushort expectedSprite = Word(SamusProjectileRomData.Banks.Projectile | (flight + 2));
                if (shot.Type != expectedType || shot.SpritemapPointer != expectedSprite)
                    throw new InvalidDataException($"Fresh shot wrong art: family {family}, wall {wallColumn}, delay {secondFrame}, slot {index}, type {shot.Type:X4}, sprite {shot.SpritemapPointer:X4} expected {expectedSprite:X4}.");
                eligible++;
                if (index == 0) reused++;
                if (projectiles.Slots.Any(slot => slot.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion))) overlapping++;

                // Keep the original explosion and every live slot, but remove its wall.
                // The second missile now has twelve unequivocally collision-free frames;
                // an inherited explosion cannot be excused as a second terrain impact.
                for (int row = 0; row < 16; row++) level.SetForegroundEntry(row * 32 + wallColumn, 0);
                ushort expectedTimer = Word(SamusProjectileRomData.Banks.Projectile | flight);
                ushort expectedPointer = (ushort)(flight + 8);
                for (int later = 1; later <= 12; later++)
                {
                    bombs.StepFrame(bus, level, samus, 0, 0);
                    projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, bombs);
                    if (--expectedTimer == 0)
                    {
                        ushort record = Word(SamusProjectileRomData.Banks.Projectile | expectedPointer);
                        if (record == SamusProjectileRomData.Instructions.GoTo)
                        {
                            expectedPointer = Word(SamusProjectileRomData.Banks.Projectile | (expectedPointer + 2));
                            record = Word(SamusProjectileRomData.Banks.Projectile | expectedPointer);
                        }
                        if ((record & 0x8000) != 0) throw new InvalidDataException("Unexpected flight oracle instruction.");
                        expectedTimer = record;
                        expectedSprite = Word(SamusProjectileRomData.Banks.Projectile | (expectedPointer + 2));
                        expectedPointer += 8;
                    }
                    if (shot.Type != expectedType || shot.SpritemapPointer != expectedSprite ||
                        shot.InstructionTimer != expectedTimer || shot.InstructionPointer != expectedPointer)
                        throw new InvalidDataException($"Second-shot followup mismatch: family={family}, wall={wallColumn}, delay={secondFrame}, later={later}, type={shot.Type:X4}, sprite={shot.SpritemapPointer:X4}/{expectedSprite:X4}, timer={shot.InstructionTimer}/{expectedTimer}.");

                    var actualOam = new OamBuffer();
                    var expectedOam = new OamBuffer();
                    projectiles.DrawLiveProjectiles(bus, actualOam, 0, 0, (ushort)(frame + later));
                    // Missile families do not use beam flicker. Build their OAM in native
                    // descending slot order, substituting independently timed flight art.
                    foreach (var active in projectiles.Slots.Reverse())
                    {
                        if (!active.IsActive || active.PackedType.FamilyValue >= (ushort)SamusProjectileFamily.PowerBomb) continue;
                        expectedOam.AddProjectileSpritemap(bus,
                            ReferenceEquals(active, shot) ? expectedSprite : active.SpritemapPointer,
                            active.XPosition, active.YPosition);
                    }
                    if (actualOam.NextByteOffset != expectedOam.NextByteOffset ||
                        !actualOam.LowTable.SequenceEqual(expectedOam.LowTable) ||
                        !actualOam.HighTable.SequenceEqual(expectedOam.HighTable))
                        throw new InvalidDataException("Second-shot followup rendered OAM differs from flight records.");
                    followupFrames++;
                }
            }
        }
        if (eligible == 0 || reused == 0 || overlapping == 0) throw new InvalidDataException("Sweep failed to cover both reuse and simultaneous flight/explosion.");
        Console.WriteLine($"PASS: {eligible} post-impact second shots; {reused} reused slot zero; {overlapping} coexisted with an explosion. Every fresh type and spritemap matches the cartridge flight record.");
        Console.WriteLine($"PASS: {followupFrames} subsequent collision-free frames match ROM animation pointers/timers and rendered OAM.");
        Console.WriteLine("This isolated wall-impact sweep does not validate enemy/projectile interactions or the complete rendered scene.");
        return 0;
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}
