using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>#553: actual wall collision must reach the displayed background shake.</summary>
internal static class MissileImpactShakeAudit
{
    public static int Run(string rom)
    {
        foreach (var (selection, shouldShake) in new[] { ((ushort)1, false), ((ushort)2, true) })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
            var samus = runtime.Samus!;
            samus.SelectedHudItem = selection;
            samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 10;
            var level = runtime.LevelData!;
            for (int row = 0; row < 16; row++)
            {
                int block = row * level.WidthInBlocks + 18;
                level.SetForegroundEntry(block, 0x8000);
                level.SetBehavior(block, 0);
            }
            int impactFrame = -1, displayedShakeFrames = 0;
            for (int frame = 0; frame < 100; frame++)
            {
                runtime.StepFrame(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                if (impactFrame < 0 && runtime.Projectiles.Slots.Any(s => s.IsActive && s.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion)))
                {
                    impactFrame = frame;
                    Console.WriteLine($"selection={selection} impact={frame}, projectile quake={runtime.Projectiles.EarthquakeType}/{runtime.Projectiles.EarthquakeTimer}, room quake={runtime.Enemies.EarthquakeType}/{runtime.Enemies.EarthquakeTimer}.");
                }
                var shake = runtime.DisplayedGameplayPpu.RoomShake;
                if (shake.Applied)
                {
                    if (displayedShakeFrames == 0 && frame != impactFrame + 1)
                        throw new InvalidDataException("Impact shake was not published on the next displayed frame.");
                    int timer = SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration - displayedShakeFrames;
                    short expected = (short)((timer & RoomFxRomData.Earthquake.AlternatingDirectionTimerMask) == 0 ? 1 : -1);
                    if (shake.Bg1X != expected || shake.Bg1Y != expected || shake.Bg2X != expected ||
                        shake.Bg2Y != expected || !shake.ShakesEnemies)
                        throw new InvalidDataException("Impact displacement differs from native type twenty's one-pixel diagonal table entry.");
                    displayedShakeFrames++;
                    var packet = GameplayDisplayCapture.CaptureOrdinaryBase(runtime);
                    var registers = ((OrdinaryGameplayRenderLayer)packet.Layers[0]).Registers;
                    if (registers.Bg1X != unchecked((ushort)(runtime.DisplayedGameplayPpu.Bg1HorizontalScroll + shake.Bg1X)) ||
                        registers.Bg1Y != unchecked((ushort)(runtime.DisplayedGameplayPpu.Bg1VerticalScroll + shake.Bg1Y)))
                        throw new InvalidDataException("Displayed background registers omitted the impact displacement.");
                    var layer = (OrdinaryGameplayRenderLayer)packet.Layers[0];
                    var neutral = new OrdinaryGameplayRenderLayer(registers with {
                        Bg1X = runtime.DisplayedGameplayPpu.Bg1HorizontalScroll,
                        Bg1Y = runtime.DisplayedGameplayPpu.Bg1VerticalScroll,
                        Bg2X = unchecked((ushort)(registers.Bg2X - shake.Bg2X)),
                        Bg2Y = unchecked((ushort)(registers.Bg2Y - shake.Bg2Y)) },
                        layer.HorizontalScrolls.ToArray().Select(x => unchecked((ushort)(x - shake.Bg2X))).ToArray(),
                        layer.VerticalScrolls.ToArray().Select(y => unchecked((ushort)(y - shake.Bg2Y))).ToArray());
                    var movedPixels = SoftwareLayeredSnapshotRenderer.Render(packet);
                    var neutralPixels = SoftwareLayeredSnapshotRenderer.Render(new LayeredRenderSnapshot(
                        packet.Memory, [neutral], packet.ObjectSelection, packet.Brightness));
                    if (movedPixels.AsSpan().SequenceEqual(neutralPixels))
                        throw new InvalidDataException("Impact registers changed but rendered background pixels did not move.");
                    int hudPixels = SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.GameplayHudHeightPixels;
                    if (!movedPixels.AsSpan(0, hudPixels).SequenceEqual(neutralPixels.AsSpan(0, hudPixels)))
                        throw new InvalidDataException("Impact BG displacement moved the fixed HUD.");
                }
            }
            if (impactFrame < 0) throw new InvalidDataException("Missile fixture did not collide with the wall.");
            if (displayedShakeFrames != (shouldShake ? 30 : 0))
                throw new InvalidDataException($"selection={selection}: displayed shake lasted {displayedShakeFrames} frames, expected {(shouldShake ? 30 : 0)}.");
        }
        VerifyRequestOrdering(rom);
        MissileExplosionAnimationAudit.Verify(rom);
        MissileEnemyImpactAudit.Verify(rom);
        Console.WriteLine("Missile control and Super Missile impact shake reach the displayed backgrounds with exact lifetimes.");
        return 0;
    }

    private static void VerifyRequestOrdering(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false, wideRunway: true);
        runtime.Samus!.SelectedHudItem = 2;
        runtime.Samus.SuperMissiles = runtime.Samus.MaxSuperMissiles = 10;
        runtime.StepFrame((ushort)SnesButton.X);
        int slot = Array.FindIndex(runtime.Projectiles.Slots.ToArray(),
            p => p.IsActive && p.PackedType.IsFamily(SamusProjectileFamily.SuperMissile));
        if (slot < 0) throw new InvalidDataException("No live Super Missile for enemy-prelude ordering test.");
        runtime.Enemies.EarthquakeType = 0;
        runtime.Enemies.EarthquakeTimer = 8;
        runtime.Projectiles.ApplyEnemyCollisionPrelude(slot, markCollisionState: false);
        if (runtime.Enemies.EarthquakeType != SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType ||
            runtime.Enemies.EarthquakeTimer != SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration)
            throw new InvalidDataException("Enemy collision prelude did not publish synchronously over the earlier request.");
        // A later native producer wins. A frame-end copy of stale projectile words
        // would overwrite this and continually restart the impact's thirty-frame timer.
        runtime.Enemies.EarthquakeType = 0;
        runtime.Enemies.EarthquakeTimer = 8;
        runtime.StepFrame(0);
        if (runtime.Enemies.EarthquakeType != 0 || runtime.Enemies.EarthquakeTimer != 7)
            throw new InvalidDataException("Projectile request overwrote a later room quake or restarted its timer.");
    }
}
