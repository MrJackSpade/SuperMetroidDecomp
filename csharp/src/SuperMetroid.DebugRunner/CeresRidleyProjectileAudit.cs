using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Exhaustive focused proof for Ceres Ridley's fireball and six afterburn definitions.
/// The older battle check accepted any non-fireball actor as sufficient, which could not
/// detect a missing directional branch or incorrect common property initialization.
/// </summary>
internal static class CeresRidleyProjectileAudit
{
    private const ushort CameraX = 0;
    private const ushort CameraY = 0;
    private const ushort FireballGraphicsIndex = 0x0a00;

    private static readonly RoomEnemyProjectileKind[] ProjectileKinds =
    [
        RoomEnemyProjectileKind.CeresRidleyFireball,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight,
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp,
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown,
    ];

    // The same fixed random word used by the full battle audit selects Ridley's first
    // authored fireball route. Different target points make that route strike floor or wall
    // without changing the boss state, projectile velocity, or room collision data.
    private static readonly (ushort X, ushort Y)[] SamusTargets =
    [
        (0x0080, 0x0064),
        (0x0080, 0x00d0),
        (0x0020, 0x0064),
        (0x00e0, 0x0064),
        (0x0080, 0x0020),
    ];

    public static void Verify(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        foreach (RoomEnemyProjectileKind kind in ProjectileKinds)
        {
            VerifyNaturalLifecycle(bus, room, assets, kind);

            // Contact mutates a persistent actor's timer/list state, so use an independent
            // encounter rather than weakening the lifecycle assertion above.
            (LoadedCeresProjectileProbe contactLoaded,
                RoomEnemyProjectileSlot contactProjectile,
                int contactFrame) = FindNaturalProjectile(
                    bus,
                    room,
                    assets,
                    kind,
                    requireDamageEnabled: true);
            VerifyDefinition(bus, contactProjectile);
            EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
                bus,
                contactLoaded.Enemies,
                contactLoaded.Samus,
                new SamusBombProjectileSystem(),
                assets.LevelData,
                contactProjectile,
                CameraX,
                CameraY,
                unchecked((byte)contactFrame));
        }
    }

    private static void VerifyNaturalLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemyProjectileKind kind)
    {
        (LoadedCeresProjectileProbe loaded, RoomEnemyProjectileSlot projectile, int frame) =
            FindNaturalProjectile(bus, room, assets, kind);
        VerifyDefinition(bus, projectile);
        ushort originX = projectile.XPosition;
        ushort originY = projectile.YPosition;
        var maps = new HashSet<ushort>();
        bool center = kind is
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter or
            RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter;

        // A center's $95BA/$95ED instruction allocates both directional children below its
        // physical slot. Bank $86 scans $22 toward $00, so each child receives its first
        // fourteen-pixel movement tick before FindNaturalProjectile can observe it. In the
        // compact Ceres collision probes that first tick can already start the stationary
        // impact list. Seed the movement proof from the exact child/center displacement;
        // requiring a second movement tick would once again assert the old ascending pass.
        bool moved = !center && HasCompletedNativeFirstDirectionalStep(
            loaded.Enemies,
            projectile);
        for (int lifetimeFrame = 0; lifetimeFrame < 1024 &&
            projectile.IsActive && projectile.Kind == kind; lifetimeFrame++)
        {
            if (projectile.SpritemapPointer is not 0 and not 0x8000)
                maps.Add(projectile.SpritemapPointer);
            moved |= projectile.XPosition != originX || projectile.YPosition != originY;
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)(frame + lifetimeFrame + 1)));
        }

        if (projectile.IsActive && projectile.Kind == kind || maps.Count < 4 ||
            (!center && !moved) || (center && moved))
        {
            throw new InvalidDataException(
                $"Ceres Ridley {kind} lifecycle mismatch: live=" +
                $"{projectile.IsActive && projectile.Kind == kind}, maps={maps.Count}/4, " +
                $"moved={moved}, stationaryCenter={center}, " +
                $"origin/end=(${originX:X4},${originY:X4})/" +
                $"(${projectile.XPosition:X4},${projectile.YPosition:X4}).");
        }
    }

    private static bool HasCompletedNativeFirstDirectionalStep(
        RoomEnemySystem enemies,
        RoomEnemyProjectileSlot child)
    {
        (RoomEnemyProjectileKind centerKind, int expectedXDelta, int expectedYDelta) =
            child.Kind switch
            {
                RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight =>
                    (RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter, 14, 0),
                RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft =>
                    (RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter, -14, 0),
                RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp =>
                    (RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter, 0, -14),
                RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown =>
                    (RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter, 0, 14),
                _ => (RoomEnemyProjectileKind.None, 0, 0),
            };
        if (centerKind == RoomEnemyProjectileKind.None)
            return false;

        RoomEnemyProjectileSlot? center = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
            projectile.IsActive && projectile.Kind == centerKind &&
            child.XPosition == unchecked((ushort)(projectile.XPosition + expectedXDelta)) &&
            child.YPosition == unchecked((ushort)(projectile.YPosition + expectedYDelta)));
        return center is not null;
    }

    private static void VerifyDefinition(
        ISnesAddressSpace bus,
        RoomEnemyProjectileSlot projectile)
    {
        RoomEnemyProjectileKind kind = projectile.Kind;
        bool directionalAfterburn = kind is
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight or
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft or
            RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp or
            RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown;
        bool enteredNativeFinalAnimation = directionalAfterburn &&
            !projectile.CanDamageSamus &&
            projectile.XVelocity == 0 &&
            projectile.YVelocity == 0;
        ushort radii = ReadWord(
            bus,
            0x860000 | unchecked((ushort)((ushort)kind + 6)));
        ushort properties = ReadWord(
            bus,
            0x860000 | unchecked((ushort)((ushort)kind + 8)));
        if (projectile.Damage != (properties & 0x0fff) ||
            projectile.XRadius != unchecked((byte)radii) ||
            projectile.YRadius != unchecked((byte)(radii >> 8)) ||
            projectile.CanDamageSamus == enteredNativeFinalAnimation ||
            !projectile.PersistsOnSamusContact ||
            projectile.BlocksSamusProjectiles ||
            projectile.GraphicsIndex != FireballGraphicsIndex)
        {
            throw new InvalidDataException(
                $"Natural Ceres Ridley {kind} disagrees with definition " +
                $"$86:{(ushort)kind:X4}: damage/radii/contact/persist/block/gfx=" +
                $"{projectile.Damage}/{projectile.XRadius}x{projectile.YRadius}/" +
                $"{projectile.CanDamageSamus}/{projectile.PersistsOnSamusContact}/" +
                $"{projectile.BlocksSamusProjectiles}/${projectile.GraphicsIndex:X4}, " +
                $"ROM properties/radii=${properties:X4}/${radii:X4}.");
        }
    }

    private static (LoadedCeresProjectileProbe Loaded,
        RoomEnemyProjectileSlot Projectile,
        int Frame) FindNaturalProjectile(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets,
            RoomEnemyProjectileKind kind,
            bool requireDamageEnabled = false)
    {
        foreach ((ushort samusX, ushort samusY) in SamusTargets)
        {
            LoadedCeresProjectileProbe loaded = Load(
                bus,
                room,
                assets,
                samusX,
                samusY);
            for (int frame = 0; frame < 4096; frame++)
            {
                byte nmi = unchecked((byte)frame);
                loaded.Enemies.StepFrame(
                    CameraX,
                    CameraY,
                    timeIsFrozen: false,
                    loaded.Samus,
                    level: assets.LevelData,
                    nmiFrameCounter8: nmi);
                RoomEnemyProjectileSlot? candidate = loaded.Enemies.EnemyProjectiles
                    .FirstOrDefault(projectile => projectile.IsActive &&
                        projectile.Kind == kind &&
                        (!requireDamageEnabled || projectile.CanDamageSamus));
                if (candidate is not null)
                    return (loaded, candidate, frame);

                loaded.Enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    samus: null,
                    cameraX: CameraX,
                    cameraY: CameraY,
                    nmiFrameCounter8: nmi);
                candidate = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                    projectile => projectile.IsActive &&
                        projectile.Kind == kind &&
                        (!requireDamageEnabled || projectile.CanDamageSamus));
                if (candidate is not null)
                    return (loaded, candidate, frame);
            }
        }

        throw new InvalidDataException(
            $"Ceres Ridley never naturally produced {kind} across " +
            $"{SamusTargets.Length} retail target routes.");
    }

    private static LoadedCeresProjectileProbe Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX,
        ushort samusY)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = samusX,
            YPosition = samusY,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            nextRandom: () => 0x1230,
            level: assets.LevelData,
            samus: samus);
        return new LoadedCeresProjectileProbe(enemies, samus);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedCeresProjectileProbe(
        RoomEnemySystem Enemies,
        SamusState Samus);
}
