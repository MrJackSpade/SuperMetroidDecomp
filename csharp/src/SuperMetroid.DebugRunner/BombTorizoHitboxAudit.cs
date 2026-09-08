using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class BombTorizoAudit
{
    /// <summary>
    /// Exercises the actual projectile dispatcher at the authored rectangle edges.
    /// Unlike the encounter's midpoint shots, these probes include exact tangencies
    /// and one-pixel misses. Deliberately constructed projectile radii isolate geometry.
    /// </summary>
    private static void VerifyHitboxBoundaries(SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        var projectileRadii = VerifyLiveWeaponRadii(bus);
        foreach (ushort radius in new ushort[] { 1, 4, 8 }) projectileRadii.Add((radius, radius));
        var animated = Load(bus, room, assets, false, _ => false, () => { });
        var probe = Load(bus, room, assets, false, _ => false, () => { });
        // Collision consumes the frame-built interactive enemy list, not Load's
        // population alone. Enter the ordinary enemy frame before injecting probes.
        Step(probe, assets.LevelData, stepProjectiles: false);
        var seen = new HashSet<ushort>();
        int cases = 0, damaging = 0, misses = 0;
        for (int frame = 0; frame < 2400; frame++)
        {
            animated.Samus.XPosition = (ushort)(frame % 400 < 200 ? 48 : 208);
            if (frame == 1200) animated.Head.Health = 300;
            Step(animated, assets.LevelData, stepProjectiles: true);
            ushort map = animated.Head.SpritemapPointer;
            if (!seen.Add(map)) continue;
            probe.Head.SpritemapPointer = map;
            probe.Head.XPosition = 128;
            probe.Head.YPosition = 160;
            var boxes = ReadBoxes();
            foreach (var radius in projectileRadii)
            foreach (var box in boxes)
            {
                int midX = (box.Left + box.Right) / 2;
                int midY = (box.Top + box.Bottom) / 2;
                foreach (int delta in new[] { -1, 0, 1 })
                {
                    Check(box.Left - radius.X + delta, midY, radius);
                    Check(box.Right + radius.X + delta, midY, radius);
                    Check(midX, box.Top - radius.Y + delta, radius);
                    Check(midX, box.Bottom + radius.Y + delta, radius);
                }
            }

            List<(int Left, int Top, int Right, int Bottom, ushort Callback)> ReadBoxes()
            {
                var result = new List<(int, int, int, int, ushort)>();
                int bank = probe.Head.Definition.Bank << 16;
                int start = bank | map;
                for (int part = 0; part < bus.ReadByte(start); part++)
                {
                    int component = start + 2 + part * 8;
                    int x = probe.Head.XPosition + (short)ReadWord(bus, component);
                    int y = probe.Head.YPosition + (short)ReadWord(bus, component + 2);
                    int list = bank | ReadWord(bus, component + 6);
                    for (int i = 0; i < ReadWord(bus, list); i++)
                    {
                        int entry = list + 2 + i * 12;
                        result.Add((x + (short)ReadWord(bus, entry),
                            y + (short)ReadWord(bus, entry + 2),
                            x + (short)ReadWord(bus, entry + 4),
                            y + (short)ReadWord(bus, entry + 6), ReadWord(bus, entry + 10)));
                    }
                }
                return result;
            }

            void Check(int x, int y, (ushort X, ushort Y) radius)
            {
                // Native shot collision takes the first intersecting authored box.
                // Top/left tangency counts; bottom/right tangency does not.
                var selected = boxes.FirstOrDefault(b => x + radius.X >= b.Left &&
                    x - radius.X < b.Right && y + radius.Y >= b.Top && y - radius.Y < b.Bottom);
                bool shouldDamage = selected.Callback == MainShotCallback;
                probe.Head.Health = 800;
                probe.Head.Properties = 0;
                probe.Head.FlashTimer = 0;
                probe.Head.InvincibilityTimer = 0;
                probe.State.ShotGuard = 0;
                var shots = new SamusProjectileSystem();
                ArmProjectile(shots.Slots[0], probe.Head, (ushort)SamusProjectileFamily.Missile, 10);
                var shot = shots.Slots[0];
                shot.XPosition = unchecked((ushort)x);
                shot.YPosition = unchecked((ushort)y);
                shot.XRadius = radius.X;
                shot.YRadius = radius.Y;
                int hits = probe.Enemies.ResolveOrdinaryProjectileHits(bus, shots,
                    new SamusBombProjectileSystem(), probe.Samus);
                bool shouldHit = selected.Callback != 0;
                if ((hits != 0) != shouldHit || (probe.Head.Health < 800) != shouldDamage)
                    throw new InvalidDataException($"Torizo map {map:X4}, point ({x},{y}), radius {radius}: " +
                        $"expected callback {selected.Callback:X4}, hits={hits}, health={probe.Head.Health}.");
                cases++;
                if (shouldDamage) damaging++;
                if (!shouldHit) misses++;
            }
        }
        if (seen.Count < 10 || damaging == 0 || misses == 0)
            throw new InvalidDataException("Torizo boundary audit did not cover enough animation maps, hits and misses.");
        Console.WriteLine($"Bomb Torizo boundary audit: {cases} probes, {seen.Count} live maps, " +
            $"{damaging} damaging intersections, {misses} misses; ROM rectangle/callback selection agrees.");
    }
}
