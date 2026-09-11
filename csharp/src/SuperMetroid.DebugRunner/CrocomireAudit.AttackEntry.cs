using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

internal static partial class CrocomireAudit
{
    /// <summary>Reaches attack selection without writing Crocomire's fight function or instruction cursor.</summary>
    public static int RunNaturalAttackEntry(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.VramWrites.DrainTo(runtime.Vram, bus);
        runtime.LoadCartridgeRoomForDebug(RoomHeader, 1024, 0);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 1040;
        samus.YPosition = 139;
        samus.Health = samus.MaxHealth = 9999;
        samus.InputLocked = false;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var state = runtime.Enemies.Crocomire!;
        int hits = 0, spawned = 0;
        int lastEmissionFrame = -1, firstEmissionFrame = -1;
        ushort firstX = 0, firstY = 0;
        bool visibleFlight = false;
        int interval = Enumerable.Range(0, 5).Sum(i => ReadWord(bus, CrocomireAttackFixtureData.VolleyTimedRecords + i * 4));
        var previous = state.FightFunction;
        ushort counter = 0;
        for (int frame = 0; frame < 1500; frame++)
        {
            runtime.StepFrame(0);
            if (state.FightFunction != previous)
                Console.WriteLine($"frame={frame} fight={state.FightFunction} x={state.Body.XPosition}");
            previous = state.FightFunction;
            bool needsHit = hits == 0 && state.FightFunction == CrocomireFightFunction.WaitingForFirstDamage ||
                hits == 1 && state.FightFunction == CrocomireFightFunction.WaitingForSecondDamage;
            if (needsHit &&
                state.Body.SpritemapPointer >= 0x8000 && FindHitboxCenter(bus, state.Body, MouthShotCallback, out ushort x, out ushort y))
            {
                // Place one charged shot inside the current retail mouth hitbox.
                // Collision and reaction dispatch remain production-owned; this is
                // not an attempt to simulate aiming/controller travel to the mouth.
                var shots = new SamusProjectileSystem();
                var shot = shots.Slots[0];
                shot.Type = SamusProjectileTypeWord.CreateBeam(0, charged: true);
                shot.Damage = 20;
                shot.Direction = 2;
                shot.XPosition = x; shot.YPosition = y;
                shot.XRadius = shot.YRadius = 1;
                shot.InstructionPointer = CrocomireAttackFixtureData.SyntheticLiveInstruction;
                shot.InstructionTimer = 1;
                if (runtime.Enemies.ResolveOrdinaryProjectileHits(bus, shots, new SamusBombProjectileSystem(), samus) != 1)
                    throw new InvalidDataException("Current Crocomire mouth did not receive the injected shot.");
                hits++;
            }
            if (state.ProjectileCounter != counter)
            {
                if (state.ProjectileCounter > counter)
                {
                    spawned++;
                    var projectiles = runtime.Enemies.EnemyProjectiles.Where(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.CrocomireProjectile).ToArray();
                    Console.WriteLine($"frame={frame} projectileCounter={state.ProjectileCounter} active={projectiles.Length}");
                    if (projectiles.Length == 0) throw new InvalidDataException("Volley counter advanced without a live projectile.");
                    if (spawned == 1 && (projectiles[0].XVelocity != unchecked((ushort)-1024) || projectiles[0].YVelocity != 0))
                        throw new InvalidDataException($"First native zero-gradient projectile must travel left at 4 pixels/frame, not ({(short)projectiles[0].XVelocity},{(short)projectiles[0].YVelocity}).");
                    if (lastEmissionFrame >= 0 && frame - lastEmissionFrame != interval)
                        throw new InvalidDataException("Crocomire volley cadence differs from the five timed ROM records.");
                    lastEmissionFrame = frame;
                    if (spawned == 1)
                    {
                        firstEmissionFrame = frame;
                        firstX = projectiles[0].XPosition; firstY = projectiles[0].YPosition;
                    }
                }
                counter = state.ProjectileCounter;
            }
            if (spawned == 1)
            {
                var snapshot = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
                var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
                PngWriter.WriteRgba(Path.Combine(directory, $"first-volley-{frame:D4}.png"), 256, 224,
                    pixels);
                foreach (var p in runtime.Enemies.EnemyProjectiles.Where(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.CrocomireProjectile))
                {
                    if (p.XPosition != firstX - 4 * (frame - firstEmissionFrame) || p.YPosition != firstY)
                        throw new InvalidDataException("First Crocomire shot departed its native four-pixel horizontal trajectory.");
                    Console.WriteLine($"  {frame}: position={p.XPosition},{p.YPosition} velocity={p.XVelocity:X4},{p.YVelocity:X4} spritemap={p.SpritemapPointer:X4}");
                    if (frame - firstEmissionFrame == 24)
                    {
                        // This point is clear of both the body and Samus. Check visible
                        // yellow/orange fire pixels at the predicted projectile position,
                        // rather than treating allocation or offscreen OAM as visibility.
                        int sx = p.XPosition - runtime.Camera!.XPosition;
                        int sy = p.YPosition - runtime.Camera.YPosition;
                        visibleFlight = Enumerable.Range(-4, 8).Any(dy => Enumerable.Range(-4, 8).Any(dx =>
                            sx + dx is >= 0 and < 256 && sy + dy is >= 0 and < 224 &&
                            pixels[(sy + dy) * 256 + sx + dx] is { R: > 180, G: > 120, B: < 150 }));
                        if (!visibleFlight) throw new InvalidDataException("Projectile has no visible fire pixels at its flight position.");
                    }
                }
            }
            if (spawned >= 9)
            {
                if (!visibleFlight) throw new InvalidDataException("Volley completed without visible flight verification.");
                Console.WriteLine($"Natural attack selection reached nine emissions after {hits} mouth hits, at frame {frame}.");
                return 0;
            }
        }
        throw new InvalidDataException($"No complete natural volley: hits={hits}, spawned={spawned}, fight={state.FightFunction}.");
    }
}
