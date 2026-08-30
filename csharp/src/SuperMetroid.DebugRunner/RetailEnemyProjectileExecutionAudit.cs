using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes beam, ice, missile, and super-missile dispatch against every naturally
/// interactable authored enemy variant. This complements passive AI and power-bomb audits:
/// the five ordinary projectile slots use different admission, hitbox, vulnerability,
/// freeze, reflection, and private boss paths.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    private const int ProjectileActivationFrameLimit = 2048;

    private static readonly ProjectileWeapon[] ProjectileWeapons =
    [
        new("power beam", 0x8000, 20),
        new("ice beam", 0x8002, 20),
        new("missile", 0x8100, 20),
        new("super missile", 0x8200, 300),
    ];

    // Extended maps and BG2 bosses can place their live rectangle far from the population
    // origin. The grid is intentionally symmetric and deterministic; a successful callback
    // still comes from the cartridge's current hitbox list, never a fabricated radius.
    private static readonly short[] ExtendedHitboxProbeOffsets =
    [
        -128, -112, -96, -80, -64, -48, -32, -16,
        0,
        16, 32, 48, 64, 80, 96, 112, 128,
    ];

    public static int RunProjectileCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<ProjectileVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var unavailableDefinitions = new HashSet<ushort>();
        var failures = new List<ProjectileFailure>();
        int weaponDispatches = 0;

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
                LoadedRetailState initial = LoadState(bus, room, assets);

                for (int slotIndex = 0; slotIndex < initial.Enemies.EnemyCount; slotIndex++)
                {
                    RoomEnemySlot initialTarget = initial.Enemies.Slots[slotIndex];
                    if (initialTarget.EnemyDefinitionPointer is 0 or 0xffff)
                        continue;

                    var variant = new ProjectileVariant(
                        initialTarget.EnemyDefinitionPointer,
                        initialTarget.Parameter1,
                        initialTarget.Parameter2,
                        initialTarget.Properties,
                        initialTarget.ExtraProperties);
                    if (!testedVariants.Add(variant))
                        continue;

                    (ushort cameraX, ushort cameraY) = CameraFor(room, initialTarget);
                    int activationFrame = FindProjectileActivationFrame(
                        bus,
                        room,
                        assets,
                        slotIndex,
                        variant.Definition,
                        cameraX,
                        cameraY);
                    if (activationFrame < 0)
                    {
                        // Deleted controllers, empty helper maps, and permanently intangible
                        // components cannot enter the native five-slot collision list. Keep
                        // them visible in the report instead of claiming callback execution.
                        unavailableDefinitions.Add(variant.Definition);
                        continue;
                    }

                    int successfulWeapons = 0;
                    foreach (ProjectileWeapon weapon in ProjectileWeapons)
                    {
                        LoadedRetailState loaded = LoadState(bus, room, assets);
                        RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
                        AdvanceToProjectileFrame(
                            loaded,
                            assets.LevelData,
                            slotIndex,
                            cameraX,
                            cameraY,
                            activationFrame);
                        if (target.EnemyDefinitionPointer != variant.Definition ||
                            target.Properties.HasAny(
                                EnemyProperties.Deleted |
                                EnemyProperties.IgnoreSamusCollision) ||
                            target.SpritemapPointer is 0 or 0x804d)
                        {
                            throw new InvalidDataException(
                                $"Definition ${variant.Definition:X4} was interactable at " +
                                $"frame {activationFrame} in the probe load but not in the " +
                                $"fresh {weapon.Name} load.");
                        }

                        // Ordinary collision walks every active slot. Retain companion state
                        // for parent/child callbacks but give non-targets property $0400 so a
                        // coincident helper cannot consume the audit projectile first.
                        for (int otherIndex = 0;
                            otherIndex < loaded.Enemies.EnemyCount;
                            otherIndex++)
                        {
                            if (otherIndex == slotIndex)
                                continue;
                            RoomEnemySlot companion = loaded.Enemies.Slots[otherIndex];
                            companion.Properties = companion.Properties.With(
                                EnemyProperties.IgnoreSamusCollision);
                        }

                        bool usesWideProbe = target.ExtraProperties.HasAny(
                                EnemyExtraProperties.UsesExtendedSpritemap) ||
                            target.EnemyDefinitionPointer is
                                0xe13f or // Ceres Ridley custom extended body
                                0xe2bf or // Kraid BG2 contour
                                0xe4bf;   // Phantoon custom extended body
                        if (TryDispatchProjectile(
                                bus,
                                loaded,
                                target,
                                weapon,
                                usesWideProbe))
                        {
                            successfulWeapons++;
                            weaponDispatches++;
                        }
                    }

                    // Some private callbacks intentionally reject a particular family, but
                    // an interactable enemy with four zero-result families has no executed
                    // evidence at all. Report that exact variant instead of silently counting
                    // the loader and spritemap as combat coverage.
                    if (successfulWeapons == 0)
                    {
                        throw new InvalidDataException(
                            $"Interactable definition ${variant.Definition:X4} parameters " +
                            $"${variant.Parameter1:X4}/${variant.Parameter2:X4} and properties " +
                            $"${variant.Properties:X4}/${variant.ExtraProperties:X4} did not " +
                            "dispatch any beam, ice, missile, or super-missile callback.");
                    }
                    reachedDefinitions.Add(variant.Definition);
                }
            }
            catch (Exception exception)
            {
                failures.Add(new ProjectileFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy projectile audit found {failures.Count} failing room states:");
            foreach (IGrouping<string, ProjectileFailure> group in failures
                .GroupBy(failure =>
                    $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (ProjectileFailure failure in group.Take(12))
                {
                    Console.Error.WriteLine(
                        $"    room/state $8F:{failure.State.RoomPointer:X4}/" +
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}");
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more states");
            }
            return 1;
        }

        Console.WriteLine(
            $"Retail enemy projectile audit passed: {testedVariants.Count} authored " +
            $"definition/parameter/property variants inspected, {weaponDispatches} live " +
            $"weapon callbacks executed across {reachedDefinitions.Count} definitions, and " +
            $"{unavailableDefinitions.Count} definitions remained naturally deleted, empty, " +
            $"or intangible through {ProjectileActivationFrameLimit} fresh-load frames.");
        return 0;
    }

    private static int FindProjectileActivationFrame(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int slotIndex,
        ushort expectedDefinition,
        ushort cameraX,
        ushort cameraY)
    {
        LoadedRetailState loaded = LoadState(bus, room, assets);
        RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
        for (int frame = 0; frame < ProjectileActivationFrameLimit; frame++)
        {
            loaded.Samus.XPosition = target.XPosition;
            loaded.Samus.YPosition = target.YPosition;
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.SamusProjectiles,
                nmiFrameCounter8: unchecked((byte)frame),
                mode7Transform: loaded.Mode7Transform,
                sharedProjectiles: loaded.SharedProjectiles);
            if (target.EnemyDefinitionPointer != expectedDefinition ||
                target.Properties.HasAny(EnemyProperties.Deleted))
            {
                return -1;
            }
            if (!target.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
                target.SpritemapPointer is not (0 or 0x804d))
            {
                return frame;
            }
        }
        return -1;
    }

    private static void AdvanceToProjectileFrame(
        LoadedRetailState loaded,
        RoomLevelData level,
        int slotIndex,
        ushort cameraX,
        ushort cameraY,
        int activationFrame)
    {
        RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
        for (int frame = 0; frame <= activationFrame; frame++)
        {
            // Match the activation probe's live target-relative Samus placement exactly so
            // directional and proximity state cannot diverge between weapon families.
            loaded.Samus.XPosition = target.XPosition;
            loaded.Samus.YPosition = target.YPosition;
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: level,
                samusProjectiles: loaded.SamusProjectiles,
                nmiFrameCounter8: unchecked((byte)frame),
                mode7Transform: loaded.Mode7Transform,
                sharedProjectiles: loaded.SharedProjectiles);
        }
    }

    private static bool TryDispatchProjectile(
        ISnesAddressSpace bus,
        LoadedRetailState loaded,
        RoomEnemySlot target,
        ProjectileWeapon weapon,
        bool usesWideProbe)
    {
        ReadOnlySpan<short> xOffsets = usesWideProbe
            ? ExtendedHitboxProbeOffsets
            : [0];
        ReadOnlySpan<short> yOffsets = usesWideProbe
            ? ExtendedHitboxProbeOffsets
            : [0];
        foreach (short yOffset in yOffsets)
        {
            foreach (short xOffset in xOffsets)
            {
                SamusProjectileSlot projectile = loaded.SamusProjectiles.Slots[0];
                ArmAuditProjectile(projectile, target, weapon, xOffset, yOffset);
                target.InvincibilityTimer = 0;
                int hits = 0;
                hits += loaded.Enemies.ResolveCeresRidleyProjectileHits(
                    bus,
                    loaded.SamusProjectiles,
                    loaded.SharedProjectiles);
                hits += loaded.Enemies.ResolveKraidProjectileHits(
                    bus,
                    loaded.SamusProjectiles,
                    loaded.SharedProjectiles);
                hits += loaded.Enemies.ResolvePhantoonProjectileHits(
                    bus,
                    loaded.SamusProjectiles,
                    loaded.SharedProjectiles);
                hits += loaded.Enemies.ResolveOrdinaryProjectileHits(
                    bus,
                    loaded.SamusProjectiles,
                    loaded.SharedProjectiles,
                    loaded.Samus);
                if (hits != 0)
                    return true;
            }
        }
        return false;
    }

    private static void ArmAuditProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ProjectileWeapon weapon,
        short xOffset,
        short yOffset)
    {
        projectile.ClearFields();
        projectile.Type = weapon.Type;
        projectile.Damage = weapon.Damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = unchecked((ushort)(target.XPosition + xOffset));
        projectile.YPosition = unchecked((ushort)(target.YPosition + yOffset));
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private readonly record struct ProjectileVariant(
        ushort Definition,
        ushort Parameter1,
        ushort Parameter2,
        ushort Properties,
        ushort ExtraProperties);

    private readonly record struct ProjectileWeapon(
        string Name,
        ushort Type,
        ushort Damage);

    private sealed record ProjectileFailure(
        RetailRoomState State,
        Exception Exception);
}
