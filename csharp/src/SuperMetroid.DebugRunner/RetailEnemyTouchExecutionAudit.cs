using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes Samus-contact dispatch against every naturally interactable authored retail
/// enemy variant. Passive lifecycle coverage cannot prove this path because its long-running
/// fixture intentionally protects Samus from incidental damage and knockback.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    public static int RunTouchCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<TouchVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var unavailableDefinitions = new HashSet<ushort>();
        var failures = new List<TouchFailure>();
        int callbacks = 0;

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

                    var variant = new TouchVariant(
                        initialTarget.EnemyDefinitionPointer,
                        initialTarget.Parameter1,
                        initialTarget.Parameter2,
                        initialTarget.Properties,
                        initialTarget.ExtraProperties);
                    if (!testedVariants.Add(variant))
                        continue;

                    (ushort cameraX, ushort cameraY) = CameraFor(room, initialTarget);
                    int activationFrame = FindTouchActivationFrame(
                        bus,
                        room,
                        assets,
                        slotIndex,
                        variant.Definition,
                        cameraX,
                        cameraY);
                    if (activationFrame < 0)
                    {
                        // Deleted controllers, empty helper maps, and actors which retain
                        // property $0400 cannot enter the native Samus-collision index list.
                        unavailableDefinitions.Add(variant.Definition);
                        continue;
                    }

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
                        RoomEnemySystem.UsesExtendedSamusHitboxes(target) &&
                        RetailExtendedHitboxProbe.ReadTouchPoints(bus, target).Count == 0)
                    {
                        throw new InvalidDataException(
                            $"Definition ${variant.Definition:X4} was touch-interactable at " +
                            $"frame {activationFrame} in the probe load but not in its fresh replay.");
                    }

                    // The collision pass walks the entire frozen interactive list and does
                    // not re-read property $0400. Preserve companion records and typed
                    // ownership, but move every non-target collision origin outside the
                    // target probe so an overlapping wing/helper cannot consume this contact.
                    for (int otherIndex = 0;
                        otherIndex < loaded.Enemies.EnemyCount;
                        otherIndex++)
                    {
                        if (otherIndex == slotIndex)
                            continue;
                        RoomEnemySlot companion = loaded.Enemies.Slots[otherIndex];
                        companion.XPosition = unchecked((ushort)(target.XPosition + 0x4000));
                        companion.YPosition = unchecked((ushort)(target.YPosition + 0x4000));
                    }

                    if (!TryDispatchTouch(bus, loaded, target, assets.LevelData))
                    {
                        throw new InvalidDataException(
                            $"Interactable definition ${variant.Definition:X4} parameters " +
                            $"${variant.Parameter1:X4}/${variant.Parameter2:X4}, properties " +
                            $"${variant.Properties:X4}/${variant.ExtraProperties:X4}, and " +
                            $"touch callback ${target.Definition.Bank:X2}:" +
                            $"${target.Definition.TouchAiPointer:X4} accepted no authored " +
                            "hitbox midpoint or deterministic body probe.");
                    }

                    reachedDefinitions.Add(variant.Definition);
                    callbacks++;
                }
            }
            catch (Exception exception)
            {
                failures.Add(new TouchFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy touch audit found {failures.Count} failing room states:");
            foreach (IGrouping<string, TouchFailure> group in failures
                .GroupBy(failure =>
                    $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (TouchFailure failure in group.Take(12))
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
            $"Retail enemy touch audit passed: {testedVariants.Count} authored " +
            $"definition/parameter/property variants inspected, {callbacks} live contact " +
            $"callbacks executed across {reachedDefinitions.Count} definitions, and " +
            $"{unavailableDefinitions.Count} definitions remained naturally deleted, empty, " +
            $"or intangible through {ProjectileActivationFrameLimit} fresh-load frames.");
        return 0;
    }

    private static int FindTouchActivationFrame(
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
            loaded.Samus.InvincibilityTimer = ushort.MaxValue;
            loaded.Samus.KnockbackActive = false;
            loaded.Samus.KnockbackDirection = 0;
            loaded.Samus.KnockbackTimer = 0;
            loaded.Samus.Health = loaded.Samus.MaxHealth;
            loaded.Samus.Pose = SamusPoseIds.FacingRightNormalPose;
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
            if (loaded.Enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex) &&
                !target.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
                target.SpritemapPointer is not (0 or 0x804d) &&
                (!RoomEnemySystem.UsesExtendedSamusHitboxes(target) ||
                 RetailExtendedHitboxProbe.ReadTouchPoints(bus, target).Count != 0))
            {
                return frame;
            }
        }
        return -1;
    }

    private static bool TryDispatchTouch(
        SuperMetroidAddressSpace bus,
        LoadedRetailState loaded,
        RoomEnemySlot target,
        RoomLevelData level)
    {
        // Extended actors can place their body far from the population origin. Exercise
        // every current ROM-authored midpoint before using the ordinary body/grid probes.
        foreach (RetailExtendedHitboxShotPoint point in
            RetailExtendedHitboxProbe.ReadTouchPoints(bus, target))
        {
            if (TryDispatchTouchAt(bus, loaded, level, point.X, point.Y))
                return true;
        }

        bool usesWideProbe = target.ExtraProperties.HasAny(
                EnemyExtraProperties.UsesExtendedSpritemap) ||
            target.EnemyDefinitionPointer is
                0xe13f or // Ridley's custom body/tail composition
                0xe17f;
        ReadOnlySpan<short> offsets = usesWideProbe ? ExtendedHitboxProbeOffsets : [0];
        foreach (short yOffset in offsets)
        {
            foreach (short xOffset in offsets)
            {
                if (TryDispatchTouchAt(
                        bus,
                        loaded,
                        level,
                        unchecked((ushort)(target.XPosition + xOffset)),
                        unchecked((ushort)(target.YPosition + yOffset))))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryDispatchTouchAt(
        SuperMetroidAddressSpace bus,
        LoadedRetailState loaded,
        RoomLevelData level,
        ushort worldX,
        ushort worldY)
    {
        loaded.Samus.XPosition = worldX;
        loaded.Samus.YPosition = worldY;
        loaded.Samus.Health = loaded.Samus.MaxHealth;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.KnockbackDirection = 0;
        loaded.Samus.KnockbackTimer = 0;
        loaded.Samus.Pose = SamusPoseIds.FacingRightNormalPose;
        loaded.Samus.RefreshCollisionRadii(bus);
        loaded.Samus.InitializeAnimation(bus);

        // Ridley owns a separate composited body/tail solver. Every other translated enemy
        // reaches the common active-list walker, including private touch callbacks selected
        // from extended hitboxes.
        return loaded.Enemies.ResolveRidleySamusContact(loaded.Samus, controllerInput: 0) ||
            loaded.Enemies.ResolveOrdinarySamusContact(
                loaded.Samus,
                controllerInput: 0,
                level);
    }

    private readonly record struct TouchVariant(
        ushort Definition,
        ushort Parameter1,
        ushort Parameter2,
        ushort Properties,
        ushort ExtraProperties);

    private sealed record TouchFailure(
        RetailRoomState State,
        Exception Exception);
}
