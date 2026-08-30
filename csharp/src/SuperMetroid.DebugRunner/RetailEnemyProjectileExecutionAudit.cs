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

    public static int RunProjectileCombat(string romPath, ushort? definitionFilter = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<ProjectileVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var unavailableDefinitions = new HashSet<ushort>();
        var failures = new List<ProjectileFailure>();
        int weaponDispatches = 0;
        long postHitFrames = 0;

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
                if (definitionFilter is ushort requestedDefinition &&
                    !PopulationContainsDefinition(
                        bus,
                        exactState.EnemyPopulationPointer,
                        requestedDefinition))
                {
                    continue;
                }
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
                LoadedRetailState initial = LoadState(bus, room, assets);

                for (int slotIndex = 0; slotIndex < initial.Enemies.EnemyCount; slotIndex++)
                {
                    RoomEnemySlot initialTarget = initial.Enemies.Slots[slotIndex];
                    if (initialTarget.EnemyDefinitionPointer is 0 or 0xffff)
                        continue;
                    if (definitionFilter is ushort targetDefinitionFilter &&
                        initialTarget.EnemyDefinitionPointer != targetDefinitionFilter)
                    {
                        continue;
                    }

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
                    string lastTargetDiagnostic = "no fresh weapon load completed";
                    foreach (ProjectileWeapon weapon in ProjectileWeapons)
                    {
                        LoadedRetailState loaded = LoadState(bus, room, assets);
                        RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
                        AdvanceToProjectileFrame(
                            bus,
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
                            target.SpritemapPointer is 0 or 0x804d ||
                            !loaded.Enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex) ||
                            RoomEnemySystem.UsesExtendedProjectileHitboxes(target) &&
                            RetailExtendedHitboxProbe.ReadShotPoints(bus, target).Count == 0)
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
                            postHitFrames += AdvancePostProjectileLifecycle(
                                bus,
                                loaded,
                                target,
                                assets.LevelData,
                                cameraX,
                                cameraY,
                                activationFrame);
                        }
                        lastTargetDiagnostic =
                            $"bank=${target.Definition.Bank:X2}, shot=" +
                            $"${target.Definition.ShotAiPointer:X4}/opcode " +
                            $"${bus.ReadByte((target.Definition.Bank << 16) | target.Definition.ShotAiPointer):X2}, " +
                            $"position=(${target.XPosition:X4},${target.YPosition:X4}), " +
                            $"radii={target.XRadius}x{target.YRadius}, map=" +
                            $"${target.SpritemapPointer:X4}, live properties=" +
                            $"${target.Properties:X4}/${target.ExtraProperties:X4}, " +
                            $"native=${target.NativeIndex:X4}, active=[" +
                            $"{string.Join(',', loaded.Enemies.ActiveEnemyIndexes.Select(index => $"{index:X4}"))}], " +
                            $"interactive=[{string.Join(',', loaded.Enemies.InteractiveEnemyIndexes.Select(index => $"{index:X4}"))}], " +
                            $"extended hitboxes=[{string.Join(',',
                                RetailExtendedHitboxProbe.ReadShotPoints(bus, target)
                                    .Select(point =>
                                        $"{point.X:X4}/{point.Y:X4}:${point.Callback:X4}"))}]";
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
                            "dispatch any beam, ice, missile, or super-missile callback; " +
                            lastTargetDiagnostic + ".");
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
                string? stackTrace = group.First().Exception.StackTrace;
                if (!string.IsNullOrWhiteSpace(stackTrace))
                {
                    foreach (string line in stackTrace.Split(Environment.NewLine))
                        Console.Error.WriteLine($"      {line.Trim()}");
                }
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
            $"weapon callbacks and {postHitFrames} hurt/freeze/death frames executed across " +
            $"{reachedDefinitions.Count} definitions, and " +
            $"{unavailableDefinitions.Count} definitions remained naturally deleted, empty, " +
            $"or intangible through {ProjectileActivationFrameLimit} fresh-load frames.");
        return 0;
    }

    private static bool PopulationContainsDefinition(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort requestedDefinition)
    {
        int cursor = populationPointer;
        for (int record = 0; record < RoomEnemySystem.MaximumEnemyCount; record++)
        {
            ushort definition = ReadWord(bus, 0xa10000 | unchecked((ushort)cursor));
            if (definition == 0xffff)
                return false;
            if (definition == requestedDefinition)
                return true;
            cursor = unchecked((ushort)(cursor + 16));
        }

        return ReadWord(bus, 0xa10000 | unchecked((ushort)cursor)) == 0xffff
            ? false
            : throw new InvalidDataException(
                $"Enemy population $A1:{populationPointer:X4} has no terminator within " +
                $"{RoomEnemySystem.MaximumEnemyCount} records.");
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
            // Long boss windups may sweep this deliberately target-relative stimulus with
            // body parts or enemy projectiles before a shot-bearing map appears. Preserve
            // the positional trigger while preventing incidental Samus hurt/knockback from
            // becoming the thing this enemy-projectile audit is measuring.
            loaded.Samus.InvincibilityTimer = ushort.MaxValue;
            loaded.Samus.KnockbackActive = false;
            loaded.Samus.KnockbackDirection = 0;
            loaded.Samus.KnockbackTimer = 0;
            loaded.Samus.Health = loaded.Samus.MaxHealth;
            loaded.Samus.Pose = SamusState.FacingRightNormalPose;
            loaded.Samus.RefreshCollisionRadii(bus);
            loaded.Samus.InitializeAnimation(bus);
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
            // The collision walker consumes the activity scan's frozen interactive-index
            // list, not merely the live property word after AI/instruction processing. A
            // callback may clear `$0400` during this frame but cannot receive a projectile
            // until the following scan admits its native slot.
            if (loaded.Enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex) &&
                !target.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
                target.SpritemapPointer is not (0 or 0x804d) &&
                (!RoomEnemySystem.UsesExtendedProjectileHitboxes(target) ||
                 RetailExtendedHitboxProbe.ReadShotPoints(bus, target).Count != 0))
            {
                return frame;
            }
        }
        return -1;
    }

    private static void AdvanceToProjectileFrame(
        SuperMetroidAddressSpace bus,
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
            loaded.Samus.InvincibilityTimer = ushort.MaxValue;
            loaded.Samus.KnockbackActive = false;
            loaded.Samus.KnockbackDirection = 0;
            loaded.Samus.KnockbackTimer = 0;
            loaded.Samus.Health = loaded.Samus.MaxHealth;
            loaded.Samus.Pose = SamusState.FacingRightNormalPose;
            loaded.Samus.RefreshCollisionRadii(bus);
            loaded.Samus.InitializeAnimation(bus);
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
        if (target.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
        {
            // Exercise the cartridge's exact current component rectangles before falling
            // back to a spatial grid. Crocomire's tongue, long boss limbs, and composited
            // actors can place a legitimate hitbox far outside the population origin.
            foreach (RetailExtendedHitboxShotPoint point in
                RetailExtendedHitboxProbe.ReadShotPoints(bus, target))
            {
                if (TryDispatchProjectileAt(
                        bus,
                        loaded,
                        target,
                        weapon,
                        point.X,
                        point.Y))
                {
                    return true;
                }
            }
        }

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
                if (TryDispatchProjectileAt(
                        bus,
                        loaded,
                        target,
                        weapon,
                        unchecked((ushort)(target.XPosition + xOffset)),
                        unchecked((ushort)(target.YPosition + yOffset))))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryDispatchProjectileAt(
        ISnesAddressSpace bus,
        LoadedRetailState loaded,
        RoomEnemySlot target,
        ProjectileWeapon weapon,
        ushort worldX,
        ushort worldY)
    {
        SamusProjectileSlot projectile = loaded.SamusProjectiles.Slots[0];
        ArmAuditProjectile(projectile, weapon, worldX, worldY);
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
        return hits != 0;
    }

    /// <summary>
    /// Follows one accepted weapon callback through the state it actually installed. Normal
    /// hits need enough frames to clear hurt/invincibility dispatch; Ice needs its entire
    /// cartridge-authored freeze clock plus two resumed frames. This turns the exhaustive
    /// collision inventory into evidence for the enemy's resulting animation and movement,
    /// rather than stopping immediately after health/frozen words were written.
    /// </summary>
    private static int AdvancePostProjectileLifecycle(
        SuperMetroidAddressSpace bus,
        LoadedRetailState loaded,
        RoomEnemySlot target,
        RoomLevelData level,
        ushort cameraX,
        ushort cameraY,
        int activationFrame)
    {
        const int MinimumPostHitFrames = 64;
        const int MaximumExpectedFreezeFrames = 2048;
        int frozenFrames = target.FrozenTimer;
        if (frozenFrames > MaximumExpectedFreezeFrames)
        {
            throw new InvalidDataException(
                $"Definition ${target.EnemyDefinitionPointer:X4} installed implausible " +
                $"{frozenFrames}-frame freeze state after a retail weapon callback.");
        }
        int frameCount = Math.Max(MinimumPostHitFrames, frozenFrames + 2);

        for (int postFrame = 0; postFrame < frameCount; postFrame++)
        {
            // Keep the same target-relative stimulus used to activate the authored state,
            // while immunizing Samus from incidental room projectiles. Enemy attacks still
            // spawn, animate, collide with terrain, and expire through their real systems.
            loaded.Samus.XPosition = target.XPosition;
            loaded.Samus.YPosition = target.YPosition;
            loaded.Samus.InvincibilityTimer = ushort.MaxValue;
            loaded.Samus.KnockbackActive = false;
            loaded.Samus.KnockbackDirection = 0;
            loaded.Samus.KnockbackTimer = 0;
            loaded.Samus.Health = loaded.Samus.MaxHealth;
            loaded.Samus.Pose = SamusState.FacingRightNormalPose;
            loaded.Samus.RefreshCollisionRadii(bus);
            loaded.Samus.InitializeAnimation(bus);
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: level,
                samusProjectiles: loaded.SamusProjectiles,
                nmiFrameCounter8: unchecked((byte)(activationFrame + postFrame + 1)),
                mode7Transform: loaded.Mode7Transform,
                sharedProjectiles: loaded.SharedProjectiles);
            loaded.Enemies.StepEnemyProjectiles(
                level,
                loaded.Samus,
                cameraX,
                cameraY);
        }

        if (frozenFrames != 0 && target.FrozenTimer != 0)
        {
            throw new InvalidDataException(
                $"Definition ${target.EnemyDefinitionPointer:X4} did not leave its " +
                $"{frozenFrames}-frame frozen state after {frameCount} live enemy frames; " +
                $"remaining={target.FrozenTimer}, handler=${target.AiHandlerBits:X4}.");
        }
        return frameCount;
    }

    private static void ArmAuditProjectile(
        SamusProjectileSlot projectile,
        ProjectileWeapon weapon,
        ushort worldX,
        ushort worldY)
    {
        projectile.ClearFields();
        projectile.Type = weapon.Type;
        projectile.Damage = weapon.Damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = worldX;
        projectile.YPosition = worldY;
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
