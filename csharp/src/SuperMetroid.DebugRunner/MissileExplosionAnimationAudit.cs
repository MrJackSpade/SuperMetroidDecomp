using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

/// <summary>Retail explosion duration and ROM-authored OBJ records, sampled on every frame.</summary>
internal static class MissileExplosionAnimationAudit
{
    public static void Verify(string rom)
    {
        foreach (ushort selection in new ushort[] { 1, 2 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var samus = runtime.Samus!;
            samus.SelectedHudItem = selection;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            for (int row = 0; row < 16; row++)
            {
                int block = row * runtime.LevelData!.WidthInBlocks + 18;
                runtime.LevelData.SetForegroundEntry(block, 0x8000);
                runtime.LevelData.SetBehavior(block, 0);
            }
            ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            int table = selection == 1 ? SamusProjectileRomData.NonBeam.MissileExplosionInstructionPointer
                : SamusProjectileRomData.NonBeam.SuperMissileExplosionInstructionPointer;
            int pointer = SamusProjectileRomData.Banks.Projectile | Word(table);
            var expectedMaps = new List<ushort>();
            while (Word(pointer) < 0x8000)
            {
                expectedMaps.AddRange(Enumerable.Repeat(Word(pointer + 2), Word(pointer)));
                pointer += 8;
            }
            if (Word(pointer) != SamusProjectileRomData.Instructions.Delete)
                throw new InvalidDataException("Explosion oracle expected a straight-line list ending in deletion.");
            SamusProjectileSlot? explosion = null;
            int age = 0;
            ushort impactX = 0, impactY = 0;
            var images = new HashSet<string>();
            for (int frame = 0; frame < 100; frame++)
            {
                runtime.StepFrame(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                if (explosion is null)
                {
                    explosion = runtime.Projectiles.Slots.FirstOrDefault(s => s.IsActive &&
                        s.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion));
                    if (explosion is null) continue;
                    impactX = explosion.XPosition; impactY = explosion.YPosition;
                }
                if (!explosion.IsActive) break;
                if (age >= expectedMaps.Count || explosion.SpritemapPointer != expectedMaps[age] ||
                    explosion.XPosition != impactX || explosion.YPosition != impactY)
                    throw new InvalidDataException($"Missile {selection} explosion frame {age} has incorrect art, duration or world position.");
                var oam = new OamBuffer();
                oam.BeginFrame();
                // Normalize the impact to the center for an OBJ-only visual comparison;
                // do not change the projectile's world position to manufacture visibility.
                runtime.Projectiles.DrawExplosions(bus, oam,
                    unchecked((ushort)(impactX - 128)), unchecked((ushort)(impactY - 128)));
                oam.FinalizeFrame();
                int map = SamusProjectileRomData.Banks.Projectile | expectedMaps[age];
                if (oam.LastFinalizedSpriteCount != Word(map))
                    throw new InvalidDataException("Explosion emitted an incorrect OBJ count.");
                for (int i = 0; i < Word(map); i++)
                {
                    int entry = map + 2 + 5 * i;
                    ushort x = Word(entry), attributes = Word(entry + 3);
                    var expected = new OamEntry((128 + x) & 511,
                        unchecked((byte)(128 + bus.ReadByte(entry + 2))), attributes & 511,
                        (attributes >> 9) & 7, (attributes >> 12) & 3,
                        (attributes & 0x4000) != 0, (attributes & 0x8000) != 0, (x & 0x8000) != 0);
                    if (oam.GetEntry(i) != expected)
                        throw new InvalidDataException($"Explosion OBJ {i} disagrees with ROM geometry/attributes.");
                }
                var pixels = SnesObjRenderer.Render(oam, runtime.Vram, runtime.Cgram, 3);
                if (!pixels.Any(p => p.A != 0)) throw new InvalidDataException("Explosion art is entirely transparent.");
                images.Add(Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pixels.AsSpan()))));
                age++;
            }
            if (explosion is null || explosion.IsActive || age != expectedMaps.Count || images.Count != 6)
                throw new InvalidDataException($"Missile {selection}: {age}/{expectedMaps.Count} explosion frames, {images.Count}/6 distinct images.");
            Console.WriteLine($"Missile {selection}: {age} stationary explosion frames, six visible ROM-authored images, then deletion.");
        }
    }
}
