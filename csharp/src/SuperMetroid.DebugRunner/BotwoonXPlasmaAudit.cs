using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class BotwoonAudit
{
    /// <summary>
    /// Exercises ordinary ItemSelect/Run input through three complete X-ray cycles.
    /// Only the initial stationary overlapping projectile is constructed; this is an
    /// integration prerequisite, not a normal-firing trajectory or native replay claim.
    /// </summary>
    public static int RunXPlasmaControls(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomPointer);
        runtime.InitializeDebugGroundedSamus(64, 166, 8);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.XrayScope;
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Plasma | SamusBeamFlags.Charge);
        samus.Missiles = samus.SuperMissiles = samus.PowerBombs = 0;
        runtime.StepFrame(0);
        runtime.StepFrame(runtime.ControllerBindings.ItemSelect);
        if (samus.SelectedHudItem != SamusXrayRomData.SelectedHudItem)
            throw new InvalidDataException("Controller ItemSelect did not select the equipped X-ray scope.");
        var head = runtime.Enemies.Slots[0];
        for (int frame = 0; frame < 360; frame++) runtime.StepFrame(0);
        ArmProjectile(runtime.Projectiles.Slots[0], head,
            SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, charged: true), 100);
        int activations = 0, releases = 0, hits = 0;
        for (int frame = 0; frame < 192; frame++)
        {
            bool frozenBefore = runtime.TimeIsFrozen;
            ushort healthBefore = head.Health, invincibilityBefore = head.InvincibilityTimer;
            ushort xBefore = head.XPosition, yBefore = head.YPosition, flashBefore = head.FlashTimer;
            ushort input = frame % 64 < 60 ? runtime.ControllerBindings.Dash : (ushort)0;
            runtime.StepFrame(input);
            if (!frozenBefore && runtime.TimeIsFrozen) activations++;
            if (frozenBefore && !runtime.TimeIsFrozen) releases++;
            if (head.Health != healthBefore)
            {
                if (healthBefore - head.Health != 100 || runtime.TimeIsFrozen)
                    throw new InvalidDataException($"Unexpected damage while scanning at frame {frame}.");
                hits++;
                Console.WriteLine($"Controller frame {frame}: Botwoon {healthBefore}->{head.Health}, timer={head.InvincibilityTimer}.");
            }
            if (frozenBefore && runtime.TimeIsFrozen &&
                (head.XPosition != xBefore || head.YPosition != yBefore || head.FlashTimer != flashBefore ||
                 head.InvincibilityTimer != Math.Max(0, invincibilityBefore - 1)))
                throw new InvalidDataException($"X-ray changed the wrong Botwoon state at frame {frame}.");
            if (runtime.Projectiles.Slots[0].PackedType.Family != SamusProjectileFamily.Beam)
                throw new InvalidDataException($"Retained Plasma was consumed at frame {frame}.");
        }
        if (activations != 3 || releases != 3 || hits != 4 || head.Health != 2600)
            throw new InvalidDataException($"X-ray cycles: activations={activations}, releases={releases}, hits={hits}, health={head.Health}.");
        Console.WriteLine("Three controller-driven X-ray cycles preserve the constructed overlapping beam and permit four Botwoon hits.");
        return 0;
    }

    /// <summary>
    /// Room-local combat prerequisite for #402. Freezing is supplied at EnemyMain's
    /// boundary; this deliberately does not claim to test controller X-ray admission.
    /// </summary>
    public static int RunXPlasma(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var room = CartridgeRoomHeader.Load(bus, RoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var loaded = Load(bus, room, assets, alreadyDefeated: false);
        AdvanceUntilShootable(loaded, assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head,
            type: SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, charged: true), damage: 100);
        int Hit() => loaded.Enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, loaded.Samus);
        if (Hit() != 1 || loaded.Head.Health != 2900 || loaded.Head.InvincibilityTimer != 16)
            throw new InvalidDataException($"Initial Plasma hit: health={loaded.Head.Health}, invincibility={loaded.Head.InvincibilityTimer}.");
        ushort x = loaded.Head.XPosition, y = loaded.Head.YPosition;
        ushort flash = loaded.Head.FlashTimer;
        for (int frame = 1; frame <= 14; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, timeIsFrozen: true, loaded.Samus, level: assets.LevelData);
            if (loaded.Head.InvincibilityTimer != 16 - frame || loaded.Head.Health != 2900 ||
                loaded.Head.XPosition != x || loaded.Head.YPosition != y || loaded.Head.FlashTimer != flash)
                throw new InvalidDataException($"Frozen Botwoon changed the wrong state at frame {frame}.");
        }
        // Runtime does not resolve beam hits during X-ray. The first ordinary enemy
        // pass on release republishes the interactive index list before beam collision.
        StepEnemies(loaded, assets.LevelData);
        if (loaded.Head.InvincibilityTimer != 1 || Hit() != 0 || loaded.Head.Health != 2900)
            throw new InvalidDataException("Plasma repeated its hit on the early release frame.");
        StepEnemies(loaded, assets.LevelData);
        if (Hit() != 1 || loaded.Head.Health != 2800 || loaded.Head.InvincibilityTimer != 16)
            throw new InvalidDataException($"Retained Plasma hit: health={loaded.Head.Health}, invincibility={loaded.Head.InvincibilityTimer}, direction={shots.Slots[0].Direction:X4}.");
        Console.WriteLine("Botwoon retained Plasma: health 3000->2900->2800; 15-frame rejection, 16-frame expiry, independent flash/position passed.");
        foreach (bool blocksPlasma in new[] { false, true })
        {
            ArmProjectile(shots.Slots[0], loaded.Head,
                SamusProjectileTypeWord.CreateBeam(blocksPlasma ? (ushort)SamusBeamFlags.Plasma : (ushort)0, charged: true), 100);
            if (!shots.TryStartEnemyImpact(bus, bombs, 0, blocksPlasma) ||
                shots.Slots[0].PackedType.Family != SamusProjectileFamily.BeamExplosion)
                throw new InvalidDataException("Nonpenetrating beam or Plasma-blocking target failed to consume the shot.");
        }
        Console.WriteLine("Controls: ordinary charged beam and Plasma-blocking target both produce beam impacts.");
        return 0;
    }
}
