using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes the header-selected Grapple reaction for every naturally interactive authored
/// retail enemy variant. Ordinary lifecycle coverage never sets handler bit zero, so it
/// cannot prove the seven bank-$A0 reaction functions or family-specific main-AI fallthrough.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    private const ushort GrappleNoInteraction = 0x8000;
    private const ushort GrappleAttach = 0x8005;
    private const ushort GrappleKill = 0x800a;
    private const ushort GrappleCancel = 0x800f;
    private const ushort GrappleAttachWithoutInvincibility = 0x8014;
    private const ushort GrappleAttachAndParalyze = 0x8019;
    private const ushort GrappleHurtSamus = 0x801e;

    public static int RunGrappleCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<GrappleVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var unavailableDefinitions = new HashSet<ushort>();
        var reactionCounts = new Dictionary<GrappleEnemyReaction, int>();
        var failures = new List<GrappleFailure>();
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

                    var variant = new GrappleVariant(
                        initialTarget.EnemyDefinitionPointer,
                        initialTarget.Parameter1,
                        initialTarget.Parameter2,
                        initialTarget.Properties,
                        initialTarget.ExtraProperties);
                    if (!testedVariants.Add(variant))
                        continue;

                    (ushort cameraX, ushort cameraY) = CameraFor(room, initialTarget);
                    int activationFrame = FindGrappleActivationFrame(
                        bus,
                        room,
                        assets,
                        slotIndex,
                        variant.Definition,
                        cameraX,
                        cameraY);
                    if (activationFrame < 0)
                    {
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
                        target.Properties.HasAny(EnemyProperties.Deleted) ||
                        !loaded.Enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex))
                    {
                        throw new InvalidDataException(
                            $"Definition ${variant.Definition:X4} was Grapple-interactive at " +
                            $"frame {activationFrame} in the probe load but not in its fresh replay.");
                    }

                    // The endpoint routine returns the first live index. Preserve every
                    // companion's geometry and typed state; a temporary invincibility word
                    // makes it skip non-target records without changing their next AI frame.
                    ushort[] savedInvincibility = loaded.Enemies.Slots
                        .Take(loaded.Enemies.EnemyCount)
                        .Select(slot => slot.InvincibilityTimer)
                        .ToArray();
                    for (int otherIndex = 0;
                        otherIndex < loaded.Enemies.EnemyCount;
                        otherIndex++)
                    {
                        loaded.Enemies.Slots[otherIndex].InvincibilityTimer =
                            otherIndex == slotIndex ? (ushort)0 : ushort.MaxValue;
                    }

                    GrappleEnemyReaction expectedReaction = ReactionFor(
                        target.Definition.GrappleAiPointer,
                        target);
                    GrappleEnemyCollision collision = loaded.Enemies.ResolveGrappleEndpoint(
                        target.XPosition,
                        target.YPosition);
                    for (int otherIndex = 0;
                        otherIndex < loaded.Enemies.EnemyCount;
                        otherIndex++)
                    {
                        if (otherIndex != slotIndex)
                        {
                            loaded.Enemies.Slots[otherIndex].InvincibilityTimer =
                                savedInvincibility[otherIndex];
                        }
                    }

                    if (!collision.Collided || collision.EnemyNativeIndex != target.NativeIndex ||
                        collision.Reaction != expectedReaction ||
                        collision.AnchorX != target.XPosition ||
                        collision.AnchorY != target.YPosition ||
                        collision.EnemyDamage != target.Definition.Damage ||
                        target.AiHandlerBits != 1)
                    {
                        throw new InvalidDataException(
                            $"Definition ${variant.Definition:X4} Grapple endpoint mismatch: " +
                            $"collided={collision.Collided}, native=${collision.EnemyNativeIndex:X4}/" +
                            $"${target.NativeIndex:X4}, reaction={collision.Reaction}/" +
                            $"{expectedReaction}, anchor=({collision.AnchorX},{collision.AnchorY})/" +
                            $"({target.XPosition},{target.YPosition}), damage=" +
                            $"{collision.EnemyDamage}/{target.Definition.Damage}, handler=" +
                            $"${target.AiHandlerBits:X4}.");
                    }

                    target.FrozenTimer = 0;
                    target.ShakeTimer = 7;
                    target.FlashTimer = 0;
                    ushort killedBefore = loaded.Enemies.EnemiesKilled;
                    loaded.Enemies.StepFrame(
                        cameraX,
                        cameraY,
                        timeIsFrozen: false,
                        loaded.Samus,
                        level: assets.LevelData,
                        samusProjectiles: loaded.SamusProjectiles,
                        nmiFrameCounter8: unchecked((byte)(activationFrame + 1)),
                        mode7Transform: loaded.Mode7Transform,
                        sharedProjectiles: loaded.SharedProjectiles);
                    VerifyGrappleReaction(
                        target,
                        expectedReaction,
                        killedBefore,
                        loaded.Enemies.EnemiesKilled);

                    reachedDefinitions.Add(variant.Definition);
                    reactionCounts[expectedReaction] = reactionCounts.GetValueOrDefault(
                        expectedReaction) + 1;
                    callbacks++;
                }
            }
            catch (Exception exception)
            {
                failures.Add(new GrappleFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy Grapple audit found {failures.Count} failing room states:");
            foreach (IGrouping<string, GrappleFailure> group in failures
                .GroupBy(failure =>
                    $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (GrappleFailure failure in group.Take(12))
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

        string reactions = string.Join(
            ", ",
            reactionCounts.OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={pair.Value}"));
        Console.WriteLine(
            $"Retail enemy Grapple audit passed: {testedVariants.Count} authored " +
            $"definition/parameter/property variants inspected, {callbacks} live reactions " +
            $"executed across {reachedDefinitions.Count} definitions ({reactions}), and " +
            $"{unavailableDefinitions.Count} definitions remained naturally deleted or " +
            $"outside the interactive list through {ProjectileActivationFrameLimit} " +
            "fresh-load frames.");
        return 0;
    }

    private static int FindGrappleActivationFrame(
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
            if (loaded.Enemies.InteractiveEnemyIndexes.Contains(target.NativeIndex) &&
                target.InvincibilityTimer == 0)
            {
                return frame;
            }
        }
        return -1;
    }

    private static GrappleEnemyReaction ReactionFor(
        ushort pointer,
        RoomEnemySlot target) => pointer switch
    {
        GrappleNoInteraction => GrappleEnemyReaction.None,
        GrappleAttach => GrappleEnemyReaction.Attach,
        GrappleKill => GrappleEnemyReaction.Kill,
        GrappleCancel => GrappleEnemyReaction.Cancel,
        GrappleAttachWithoutInvincibility => GrappleEnemyReaction.AttachWithoutInvincibility,
        GrappleAttachAndParalyze => GrappleEnemyReaction.AttachAndParalyze,
        GrappleHurtSamus => GrappleEnemyReaction.HurtSamus,
        _ => throw new NotSupportedException(
            $"Definition ${target.EnemyDefinitionPointer:X4} uses untranslated Grapple " +
            $"reaction ${target.Definition.Bank:X2}:{pointer:X4}."),
    };

    private static void VerifyGrappleReaction(
        RoomEnemySlot target,
        GrappleEnemyReaction reaction,
        ushort killedBefore,
        ushort killedAfter)
    {
        ushort hurtTime = target.HurtAiTime == 0 ? (ushort)4 : target.HurtAiTime;
        // EnemyMain installs the Grapple flash during AI, admits the actor to the draw
        // queue, then `$A0:9128` decrements that timer before StepFrame returns. Observe the
        // post-frame value without weakening the exact authored duration assertion.
        ushort visibleFlashTime = unchecked((ushort)(hurtTime - 1));
        bool valid = reaction switch
        {
            GrappleEnemyReaction.None =>
                target.AiHandlerBits == 0 && target.InvincibilityTimer == 0 &&
                target.FrozenTimer == 0 && target.ShakeTimer == 0,
            GrappleEnemyReaction.Attach =>
                target.AiHandlerBits == 0 && target.FlashTimer == visibleFlashTime,
            GrappleEnemyReaction.Kill =>
                target.Health == 0 && target.Properties.HasAny(EnemyProperties.Deleted) &&
                killedAfter == unchecked((ushort)(killedBefore + 1)),
            GrappleEnemyReaction.Cancel or GrappleEnemyReaction.HurtSamus =>
                target.AiHandlerBits == 4,
            GrappleEnemyReaction.AttachWithoutInvincibility =>
                target.AiHandlerBits == 0,
            GrappleEnemyReaction.AttachAndParalyze =>
                target.AiHandlerBits == 0 && target.FlashTimer == visibleFlashTime &&
                (target.ExtraProperties & 1) != 0,
            _ => false,
        };
        if (!valid)
        {
            throw new InvalidDataException(
                $"Definition ${target.EnemyDefinitionPointer:X4} Grapple {reaction} " +
                $"post-frame state diverged: handler=${target.AiHandlerBits:X4}, health=" +
                $"{target.Health}, properties=${target.Properties:X4}, flash=" +
                $"{target.FlashTimer}/{visibleFlashTime}, invinc={target.InvincibilityTimer}, " +
                $"frozen={target.FrozenTimer}, shake={target.ShakeTimer}, extra=" +
                $"${target.ExtraProperties:X4}, killed={killedBefore}->{killedAfter}.");
        }
    }

    private readonly record struct GrappleVariant(
        ushort Definition,
        ushort Parameter1,
        ushort Parameter2,
        ushort Properties,
        ushort ExtraProperties);

    private sealed record GrappleFailure(
        RetailRoomState State,
        Exception Exception);
}
