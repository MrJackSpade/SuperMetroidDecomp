using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Focused coverage for Bomb Torizo's phase-only visual projectiles. These actors are not
/// selected by the normal attack loop, so the broad placement sweep correctly classifies
/// them as residuals. A real nonfatal shot enters the authored gut-break phase and a real
/// fatal shot enters the death list; only the player weapon stimuli are constructed here.
/// </summary>
internal static partial class BombTorizoAudit
{
    private const ushort BombTorizoActiveFunction = 0xc6ff;
    private const ushort BombTorizoLowHealthThreshold = 350;

    private static void VerifyLowHealthAndDeathProjectileLifecycles(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            _ => false,
            () => { });
        AdvancePhaseProbeToActive(loaded, assets.LevelData);
        DamageBombTorizoBelowGutThreshold(bus, loaded);

        var lowHealthExplosion = new PhaseProjectileTracker(
            bus,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion);
        var continuousDrool = new PhaseProjectileTracker(
            bus,
            RoomEnemyProjectileKind.BombTorizoLowHealthDrool);
        var initialDrool = new PhaseProjectileTracker(
            bus,
            RoomEnemyProjectileKind.BombTorizoInitialDrool);

        // $AA:B0E5 first emits six simultaneous core explosions, then $AA:B11D emits
        // six continuous-drool actors. Ordinary low-health animation opcodes and the
        // deterministic random check can subsequently emit the initial-drool definition.
        for (int frame = 0; frame < 12000; frame++)
        {
            StepPhaseProbe(loaded, assets.LevelData, frame);
            lowHealthExplosion.Observe(loaded.Enemies);
            continuousDrool.Observe(loaded.Enemies);
            initialDrool.Observe(loaded.Enemies);
            if (lowHealthExplosion.MaximumConcurrent >= 6 &&
                continuousDrool.MaximumConcurrent >= 6 && initialDrool.SawActor &&
                lowHealthExplosion.SawMovement && lowHealthExplosion.VisibleMaps.Count >= 5 &&
                continuousDrool.SawMovement && continuousDrool.VisibleMaps.Count >= 2 &&
                initialDrool.SawMovement && initialDrool.VisibleMaps.Count >= 2)
            {
                break;
            }
        }

        if (lowHealthExplosion.MaximumConcurrent < 6 ||
            continuousDrool.MaximumConcurrent < 6 || !initialDrool.SawActor)
        {
            throw new InvalidDataException(
                "Bomb Torizo low-health producer mismatch: maximum core/drool actors=" +
                $"{lowHealthExplosion.MaximumConcurrent}/" +
                $"{continuousDrool.MaximumConcurrent}, initial drool={initialDrool.SawActor}.");
        }

        // End the repeating low-health producer through the ordinary shot callback. This
        // also enters the real $AA:B1C8 death stream whose $C32F opcode owns $86:A9AF.
        KillBombTorizoThroughOrdinaryShot(bus, loaded);
        var deathExplosion = new PhaseProjectileTracker(
            bus,
            RoomEnemyProjectileKind.BombTorizoDeathExplosion);
        for (int frame = 0; frame < 12000; frame++)
        {
            StepPhaseProbe(loaded, assets.LevelData, frame);
            lowHealthExplosion.Observe(loaded.Enemies);
            continuousDrool.Observe(loaded.Enemies);
            initialDrool.Observe(loaded.Enemies);
            deathExplosion.Observe(loaded.Enemies);
            if (loaded.Head.Properties.HasAny(EnemyProperties.Deleted) &&
                deathExplosion.SawActor && deathExplosion.SawMovement &&
                deathExplosion.VisibleMaps.Count >= 4 &&
                !loaded.Enemies.EnemyProjectiles.Any(projectile => projectile.IsActive &&
                    projectile.Kind is
                        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
                        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
                        RoomEnemyProjectileKind.BombTorizoInitialDrool or
                        RoomEnemyProjectileKind.BombTorizoDeathExplosion))
            {
                break;
            }
        }

        lowHealthExplosion.RequireLifecycle(
            minimumVisibleMaps: 5,
            requireMovement: true,
            requireDeletion: true);
        continuousDrool.RequireLifecycle(
            minimumVisibleMaps: 2,
            requireMovement: true,
            requireDeletion: true);
        initialDrool.RequireLifecycle(
            minimumVisibleMaps: 2,
            requireMovement: true,
            requireDeletion: true);
        deathExplosion.RequireLifecycle(
            minimumVisibleMaps: 4,
            requireMovement: true,
            requireDeletion: true);
    }

    private static void AdvancePhaseProbeToActive(
        LoadedBombTorizo loaded,
        RoomLevelData level)
    {
        for (int frame = 0; frame < 12000; frame++)
        {
            StepPhaseProbe(loaded, level, frame);
            if (loaded.State.Function == BombTorizoActiveFunction)
                return;
        }

        throw new InvalidDataException(
            "Bomb Torizo did not reach its active function for the phase-projectile probe.");
    }

    /// <summary>
    /// Computes a Super Missile stimulus from the cartridge vulnerability byte so the hit
    /// crosses health $015E without killing the 800-HP actor. This is deliberately not a
    /// direct health assignment: the normal shot callback, flash state, and threshold
    /// interrupt all participate.
    /// </summary>
    private static void DamageBombTorizoBelowGutThreshold(
        ISnesAddressSpace bus,
        LoadedBombTorizo loaded)
    {
        ushort vulnerability = loaded.Head.Definition.VulnerabilityPointer;
        byte superMultiplier = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerability + 13)));
        if (superMultiplier is 0 or 0xff)
        {
            throw new InvalidDataException(
                $"Bomb Torizo Super Missile vulnerability ${superMultiplier:X2} cannot " +
                "enter the nonfatal gut-break phase.");
        }

        const ushort targetHealth = 300;
        int desiredDamage = loaded.Head.Health - targetHealth;
        ushort projectileDamage = checked((ushort)(
            ((desiredDamage + superMultiplier - 1) / superMultiplier) * 2));
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        loaded.Head.FlashTimer = 0;
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, projectileDamage);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health == 0 ||
            loaded.Head.Health >= BombTorizoLowHealthThreshold || loaded.State.DeathStarted)
        {
            throw new InvalidDataException(
                $"Bomb Torizo nonfatal phase shot mismatch: hits={hits}, " +
                $"health={loaded.Head.Health}, death={loaded.State.DeathStarted}, " +
                $"multiplier/raw=${superMultiplier:X2}/{projectileDamage}.");
        }
    }

    private static void KillBombTorizoThroughOrdinaryShot(
        ISnesAddressSpace bus,
        LoadedBombTorizo loaded)
    {
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        loaded.Head.FlashTimer = 0;
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, damage: 4000);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != 0 || !loaded.State.DeathStarted)
        {
            throw new InvalidDataException(
                $"Bomb Torizo phase-probe fatal shot mismatch: hits={hits}, " +
                $"health={loaded.Head.Health}, death={loaded.State.DeathStarted}.");
        }
    }

    private static void StepPhaseProbe(
        LoadedBombTorizo loaded,
        RoomLevelData level,
        int frame)
    {
        byte nmi = unchecked((byte)frame);
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: level,
            nmiFrameCounter8: nmi);
        // These four definitions are visual-only and keep property $2000 throughout their
        // lives. Null Samus makes that invariant explicit while retaining terrain movement.
        loaded.Enemies.StepEnemyProjectiles(
            level,
            samus: null,
            cameraX: CameraX,
            cameraY: CameraY,
            nmiFrameCounter8: nmi);
    }

    /// <summary>
    /// Aggregates lifecycle evidence across every physical slot used by one definition.
    /// Bomb Torizo deliberately spawns six siblings at once, so following only slot zero
    /// would make animation/deletion coverage depend on allocator order and wall impacts.
    /// </summary>
    private sealed class PhaseProjectileTracker
    {
        private readonly ISnesAddressSpace _bus;
        private readonly RoomEnemyProjectileKind _kind;
        private readonly Dictionary<int, (ushort X, ushort Y)> _origins = [];
        private readonly HashSet<int> _activeSlots = [];

        public PhaseProjectileTracker(
            ISnesAddressSpace bus,
            RoomEnemyProjectileKind kind)
        {
            _bus = bus;
            _kind = kind;
        }

        public bool SawActor { get; private set; }
        public bool SawMovement { get; private set; }
        public bool SawDeletion { get; private set; }
        public int MaximumConcurrent { get; private set; }
        public HashSet<ushort> VisibleMaps { get; } = [];

        public void Observe(RoomEnemySystem enemies)
        {
            RoomEnemyProjectileSlot[] current = enemies.EnemyProjectiles
                .Where(projectile => projectile.IsActive && projectile.Kind == _kind)
                .ToArray();
            var currentSlots = current.Select(projectile => projectile.SlotIndex).ToHashSet();
            SawDeletion |= _activeSlots.Any(slot => !currentSlots.Contains(slot));
            _activeSlots.Clear();
            foreach (int slot in currentSlots)
                _activeSlots.Add(slot);
            MaximumConcurrent = Math.Max(MaximumConcurrent, current.Length);

            foreach (RoomEnemyProjectileSlot projectile in current)
            {
                SawActor = true;
                ValidateDefinitionAndIntangibility(projectile);
                if (!_origins.TryAdd(
                        projectile.SlotIndex,
                        (projectile.XPosition, projectile.YPosition)) &&
                    _origins[projectile.SlotIndex] !=
                        (projectile.XPosition, projectile.YPosition))
                {
                    SawMovement = true;
                }
                if (projectile.SpritemapPointer is not 0 and not 0x8000)
                    VisibleMaps.Add(projectile.SpritemapPointer);
            }
        }

        public void RequireLifecycle(
            int minimumVisibleMaps,
            bool requireMovement,
            bool requireDeletion)
        {
            if (!SawActor || VisibleMaps.Count < minimumVisibleMaps ||
                (requireMovement && !SawMovement) || (requireDeletion && !SawDeletion))
            {
                throw new InvalidDataException(
                    $"Bomb Torizo phase actor {_kind} lifecycle incomplete: " +
                    $"spawned={SawActor}, maps={VisibleMaps.Count}/{minimumVisibleMaps}, " +
                    $"movement={SawMovement}/{requireMovement}, " +
                    $"deletion={SawDeletion}/{requireDeletion}, max={MaximumConcurrent}.");
            }
        }

        private void ValidateDefinitionAndIntangibility(RoomEnemyProjectileSlot projectile)
        {
            ushort radii = ReadWord(
                _bus,
                0x860000 | unchecked((ushort)((ushort)_kind + 6)));
            ushort properties = ReadWord(
                _bus,
                0x860000 | unchecked((ushort)((ushort)_kind + 8)));
            if (projectile.Damage != (properties & 0x0fff) ||
                projectile.XRadius != unchecked((byte)radii) ||
                projectile.YRadius != unchecked((byte)(radii >> 8)) ||
                projectile.CanDamageSamus || projectile.BlocksSamusProjectiles)
            {
                throw new InvalidDataException(
                    $"Bomb Torizo phase actor {_kind} disagrees with definition " +
                    $"${(ushort)_kind:X4}: damage/radii/contact/block=" +
                    $"{projectile.Damage}/{projectile.XRadius}x{projectile.YRadius}/" +
                    $"{projectile.CanDamageSamus}/{projectile.BlocksSamusProjectiles}, " +
                    $"ROM=${properties:X4}/${radii:X4}.");
            }
        }
    }
}
