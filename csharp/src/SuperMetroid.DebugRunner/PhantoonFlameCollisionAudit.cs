using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Constructed contacts using production Phantoon flame initialization, collision and bytecode.</summary>
internal static class PhantoonFlameCollisionAudit
{
    public static int Run(string rom)
    {
        int cases = 0;
        foreach (ushort parameter in new ushort[] { 0, 0x0200, 0x0400, 0x0600 })
        {
            foreach (bool overlaps in new[] { false, true })
            {
                var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
                var flame = Spawn(runtime, parameter);
                var samus = runtime.Samus!;
                samus.Health = 500;
                samus.EquippedItems = 0;
                samus.InvincibilityTimer = 0;
                samus.KnockbackTimer = 0;
                samus.HorizontalSpeed.ContactDamageIndex = 0;
                flame.XPosition = (ushort)(samus.XPosition + samus.Kinematics.XRadius + flame.XRadius - (overlaps ? 1 : 0));
                flame.YPosition = samus.YPosition;
                runtime.Enemies.ResolveEnemyProjectileSamusHits(samus);
                bool hurts = parameter != 0 && overlaps;
                if (samus.Health != (hurts ? 460 : 500) || flame.IsActive == hurts ||
                    samus.InvincibilityTimer != (hurts ? 96 : 0) || samus.KnockbackTimer != (hurts ? 5 : 0))
                    throw new InvalidDataException($"Flame {parameter:X4}: touch edge {overlaps} differs from native damage/deletion/timers.");
                cases++;
            }
            foreach (bool sameCell in new[] { false, true })
            {
                var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
                var flame = Spawn(runtime, parameter);
                flame.XPosition = flame.YPosition = 128;
                var samus = runtime.Samus!;
                samus.SelectedHudItem = 1;
                samus.Missiles = samus.MaxMissiles = 10;
                var fired = runtime.Projectiles.TryFireMissile(runtime.AddressSpace, samus,
                    (ushort)SnesButton.X, 0, runtime.BombProjectiles);
                if (fired.Slot is not { } index) throw new InvalidDataException("Could not initialize collision fixture missile.");
                var shot = runtime.Projectiles.Slots[index];
                // Far corner of the same cell hits; a neighboring pixel across the
                // cell boundary misses. Native $A0:99B9 uses no radius test here.
                shot.XPosition = (ushort)(sameCell ? 159 : 127);
                shot.YPosition = (ushort)(sameCell ? 159 : 128);
                int hits = runtime.Enemies.ResolveEnemyProjectileSamusProjectileHits(runtime.AddressSpace,
                    runtime.Projectiles, runtime.BombProjectiles);
                bool hit = parameter != 0 && sameCell;
                if (hits != (hit ? 1 : 0)) throw new InvalidDataException($"Flame {parameter:X4}: cell collision {sameCell} returned {hits}.");
                if (hit)
                {
                    ushort Word(int address) => (ushort)(runtime.AddressSpace.ReadByte(address) | runtime.AddressSpace.ReadByte(address + 1) << 8);
                    ushort list = Word(0x869c35);
                    if (flame.InstructionPointer != list || flame.InstructionTimer != 1 || flame.BlocksSamusProjectiles)
                        throw new InvalidDataException("Shot failed to install native dying instruction list and clear shootable property.");
                    int duration = 0;
                    for (int offset = 0; offset < 16; offset += 4) duration += Word(0x860000 | (list + offset));
                    int dropCount = runtime.Enemies.PhantoonFlameDropRequests.Count;
                    for (int frame = 0; frame <= duration; frame++)
                    {
                        runtime.Enemies.StepEnemyProjectileInstructions(runtime.LevelData!, samus);
                        if (frame < duration)
                        {
                            int remaining = frame;
                            int offset = 0;
                            while (remaining >= Word(0x860000 | (list + offset)))
                            { remaining -= Word(0x860000 | (list + offset)); offset += 4; }
                            ushort expectedMap = Word(0x860000 | (list + offset + 2));
                            if (!flame.IsActive || flame.SpritemapPointer != expectedMap || flame.XPosition != 128 || flame.YPosition != 128)
                                throw new InvalidDataException($"Shot flame animation frame {frame}: wrong map, lifetime or moving explosion.");
                        }
                    }
                    if (runtime.Enemies.PhantoonFlameDropRequests.Count != dropCount + 1 ||
                        flame.IsActive && flame.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame)
                        throw new InvalidDataException("Shot flame failed to request exactly one drop and delete after its ROM animation.");
                }
                cases++;
            }
        }
        Console.WriteLine($"Phantoon flames: {cases} touch/shot cases pass; rain/rage/spiral shot deaths hold four ROM sprite maps for 20 frames then request one drop.");
        VerifyExpansionLifetime(rom);
        VerifyRainLifetime(rom);
        return 0;
    }

    private static void VerifyRainLifetime(string rom)
    {
        var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
        var flame = Spawn(runtime, 0x0410);
        int impact = -1;
        for (int frame = 0; frame < 200; frame++)
        {
            runtime.Enemies.StepEnemyProjectileInstructions(runtime.LevelData!, null);
            if (impact < 0 && flame.PreInstruction == EnemyProjectileCodePointers.RTS_869A44)
                impact = frame;
            if (impact < 0) continue;
            // $97AC holds the impact image for eight frames; $979A then holds
            // four dying images for five each. Unlike shot death, no drop opcode.
            int age = frame - impact;
            if (age < 28)
            {
                int address = age < 8 ? 0x8697ae : 0x86979c + ((age - 8) / 5) * 4;
                ushort map = (ushort)(runtime.AddressSpace.ReadByte(address) | runtime.AddressSpace.ReadByte(address + 1) << 8);
                if (!flame.IsActive || flame.SpritemapPointer != map)
                    throw new InvalidDataException($"Rain impact age {age}: incorrect native animation image/lifetime.");
            }
            else
            {
                if (flame.IsActive || runtime.Enemies.PhantoonFlameDropRequests.Count != 0)
                    throw new InvalidDataException("Natural rain impact must delete without a shot drop.");
                Console.WriteLine($"Rain terrain impact: frame {impact}, native 8+20-frame image sequence, then deletion without a drop.");
                return;
            }
        }
        throw new InvalidDataException("Rain never completed its terrain impact sequence.");
    }

    private static void VerifyExpansionLifetime(string rom)
    {
        int trajectories = 0;
        int shortest = int.MaxValue, longest = 0;
        foreach (bool spiral in new[] { false, true })
        foreach (ushort bodyY in new ushort[] { 32, 112 })
        for (int direction = 0; direction < (spiral ? 8 : 16); direction++)
        {
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var body = runtime.Enemies.Phantoon!.Body;
            body.XPosition = 128;
            body.YPosition = bodyY;
            var flame = Spawn(runtime, (ushort)((spiral ? 0x0600 : 0x0200) | direction));
            // Coordinate arithmetic has a separate exhaustive original-CPU audit.
            // Here invoke that helper only to predict the first boundary crossing;
            // exercise removal through the real pre-instruction/bytecode frame pass.
            var prediction = runtime.Enemies.EnemyProjectiles.First(p => !p.IsActive);
            var position = typeof(RoomEnemySystem).GetMethod("PositionPhantoonFlameAroundBody", BindingFlags.NonPublic | BindingFlags.Instance)!;
            int angle = flame.Variable0;
            int delta = spiral ? 2 : (short)flame.XVelocity;
            int step = spiral ? 2 : 4;
            int expectedDeath = 0;
            for (int frame = 1; frame < 256; frame++)
            {
                position.Invoke(runtime.Enemies, [prediction, body, (ushort)((angle + delta * frame) & 255), (ushort)((step * frame) & 255)]);
                bool outside = prediction.XPosition >= 256 || prediction.YPosition >= 256;
                runtime.Enemies.StepEnemyProjectileInstructions(runtime.LevelData!, null);
                if (flame.IsActive == outside)
                    throw new InvalidDataException($"Flame {(spiral ? "spiral" : "rage")} direction {direction}, body Y {bodyY}, frame {frame}: removal did not match first boundary crossing.");
                if (!outside) continue;
                expectedDeath = frame;
                break;
            }
            if (expectedDeath == 0 || runtime.Enemies.PhantoonFlameDropRequests.Count != 0)
                throw new InvalidDataException("Off-room flame persisted or incorrectly generated an item drop.");
            trajectories++;
            shortest = Math.Min(shortest, expectedDeath);
            longest = Math.Max(longest, expectedDeath);
        }
        Console.WriteLine($"Expansion lifetime: {trajectories} trajectories delete on their first boundary crossing ({shortest}..{longest} frames), without drops.");
    }

    private static RoomEnemyProjectileSlot Spawn(SuperMetroidRuntime runtime, ushort parameter)
    {
        bool spawned = (bool)typeof(RoomEnemySystem).GetMethod("SpawnPhantoonDestroyableFlame", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(runtime.Enemies, [runtime.Enemies.Phantoon!.Body, parameter])!;
        if (!spawned) throw new InvalidDataException("Fixture flame allocation failed.");
        return runtime.Enemies.EnemyProjectiles.Single(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);
    }
}
