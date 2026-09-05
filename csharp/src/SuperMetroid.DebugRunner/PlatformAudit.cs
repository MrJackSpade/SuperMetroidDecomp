using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed end-to-end regression for Tripper and Kamer's shared platform engine. East
/// Ocean proves the now-complete thirteen-actor population and suspensor riding path;
/// Ice Beam Acid and Spiky Platforms Tunnel cover Tripper's two directions, motion art,
/// private frozen maps, ordinary damage, solidity, and no-op touch routine.
/// </summary>
internal static class PlatformAudit
{
    private const ushort ChootDefinition = 0xd3bf;
    private const ushort SkulteraDefinition = 0xd6ff;
    private const ushort TripperDefinition = 0xd7ff;
    private const ushort KamerDefinition = 0xd83f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyRoom021eRisingPlatform(bus);
        KamerResult kamer = VerifyEastOceanKamer(bus);
        TripperResult tripper = VerifyIceBeamAcidTrippers(bus);
        VerifyRightFacingTripperFreeze(bus);
        VerifyRuntimeRiding(bus);

        Console.WriteLine(
            "Platform audit passed: unchanged East Ocean loaded 5 Choots, 5 Skulteras, " +
            $"and 3 Kamers; suspensor travel produced {kamer.CarryPixels} vertical carry " +
            $"pixels and {kamer.ObjPieces} OBJ pieces. Ice Beam Acid loaded three Trippers, " +
            $"crossed X {tripper.MinimumX:X4}-{tripper.MaximumX:X4}, animated " +
            $"{tripper.MapCount} retail maps, produced signed X/Y rider displacement, " +
            "dealt no touch damage, accepted normal/freeze/lethal projectile semantics in " +
            "both directions, and the production runtime carried Samus through bank-$90 " +
            "collision-aware movement.");
        return 0;
    }

    private static KamerResult VerifyEastOceanKamer(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x94fd);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = LoadEnemies(bus, room, assets);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        RoomEnemySlot[] platforms = population
            .Where(slot => slot.EnemyDefinitionPointer == KamerDefinition)
            .ToArray();

        // All preceding actors are translated now, so this must use the untouched population
        // from `$A1:8002`; a prefix adapter would hide future cross-family scheduler faults.
        if (room.State.Pointer != 0x950a ||
            room.State.EnemyPopulationPointer != 0x8002 ||
            enemies.EnemyCount != 13 || platforms.Length != 3 ||
            population.Take(5).Any(slot => slot.EnemyDefinitionPointer != ChootDefinition) ||
            population.Skip(5).Take(5).Any(slot =>
                slot.EnemyDefinitionPointer != SkulteraDefinition) ||
            population.Skip(10).Any(slot => slot.EnemyDefinitionPointer != KamerDefinition) ||
            platforms.Any(slot => enemies.PlatformStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"East Ocean state/population failed: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, count={enemies.EnemyCount}, " +
                $"Choot={population.Count(x => x.EnemyDefinitionPointer == ChootDefinition)}, " +
                $"Skultera={population.Count(x => x.EnemyDefinitionPointer == SkulteraDefinition)}, " +
                $"Kamer={platforms.Length}.");
        }

        RoomEnemySlot platform = platforms[0];
        PlatformEnemyState state = RequireState(enemies, platform);
        if (platform.XPosition != 0x04c0 || platform.YPosition != 0x04f0 ||
            platform.Parameter1 != 0 || platform.Parameter2 != 0x2800 ||
            platform.Properties != 0xa000 || platform.Health != 20 ||
            platform.Definition.Damage != 40 || platform.XRadius != 16 ||
            platform.YRadius != 8 || platform.Layer != 5 ||
            platform.CurrentInstruction != 0x9be7 || !state.IsSuspensorPlatform ||
            state.XMovement != PlatformHorizontalMovement.Left ||
            state.YMovement != PlatformVerticalMovement.Rising ||
            state.TargetYPosition != 0x04f1 || state.MaximumYSpeedTableIndex != 0x28 ||
            state.LeftDisplacement != 0 || state.RightDisplacement != 0)
        {
            throw new InvalidDataException(
                $"East Ocean Kamer init failed: position=({platform.XPosition:X4}," +
                $"{platform.YPosition:X4}), params=${platform.Parameter1:X4}/" +
                $"${platform.Parameter2:X4}, properties=${platform.Properties:X4}, " +
                $"health/damage={platform.Health}/{platform.Definition.Damage}, radii=" +
                $"{platform.XRadius}/{platform.YRadius}, layer={platform.Layer}, list=" +
                $"${platform.CurrentInstruction:X4}, movement={state.XMovement}/" +
                $"{state.YMovement}, target=${state.TargetYPosition:X4}, max=" +
                $"{state.MaximumYSpeedTableIndex}, speeds={state.LeftDisplacement:X8}/" +
                $"{state.RightDisplacement:X8}.");
        }

        SamusState samus = CreateSamus(bus, 0, 0);
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 42; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            maps.Add(platform.SpritemapPointer);
        }
        if (maps.Count != 4 || platform.YPosition != 0x04f0 ||
            !state.VerticallyStillArtInstalled)
        {
            throw new InvalidDataException(
                $"Idle Kamer animation failed: maps={maps.Count}, Y=${platform.YPosition:X4}, " +
                $"stillArt={state.VerticallyStillArtInstalled}.");
        }

        // `$A3:9F07` is not ordinary contact damage. Deliberately overlap centers so the
        // collision dispatcher certainly invokes it, then prove it leaves every hurt field
        // untouched instead of silently excluding the enemy from interaction altogether.
        samus.XPosition = platform.XPosition;
        samus.YPosition = platform.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 999 ||
            samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Kamer no-op touch failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

        // The indestructible vulnerability table still accepts projectile impact and turns
        // the beam into its explosion; it simply applies zero health/flash damage.
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], platform, projectileType: 0, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || platform.Health != 20 || platform.FlashTimer != 0)
        {
            throw new InvalidDataException(
                $"Kamer indestructible beam response failed: health={platform.Health}, " +
                $"flash={platform.FlashTimer}.");
        }

        // Place Samus with her feet exactly at the platform top. The enemy writes only
        // whole external-displacement words, so this subsystem audit applies accepted whole
        // deltas to retain contact; the production collision-aware consumer is tested below.
        PlaceSamusOnPlatform(samus, platform);
        ushort startY = platform.YPosition;
        int carryPixels = 0;
        ushort maximumY = startY;
        for (int frame = 0; frame < 192; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            short xCarry = unchecked((short)samus.Kinematics.ExtraXDisplacement);
            short yCarry = unchecked((short)samus.Kinematics.ExtraYDisplacement);
            if (xCarry != 0 || samus.Kinematics.ExtraXSubdisplacement != 0 ||
                samus.Kinematics.ExtraYSubdisplacement != 0)
            {
                throw new InvalidDataException(
                    $"Stationary Kamer emitted non-native displacement: X={xCarry}, " +
                    $"Xsub=${samus.Kinematics.ExtraXSubdisplacement:X4}, " +
                    $"Ysub=${samus.Kinematics.ExtraYSubdisplacement:X4}.");
            }
            samus.YPosition = unchecked((ushort)(samus.YPosition + yCarry));
            carryPixels += yCarry;
            maximumY = Math.Max(maximumY, platform.YPosition);
        }
        if (maximumY <= startY || carryPixels <= 0 ||
            state.YMovement != PlatformVerticalMovement.Sinking)
        {
            throw new InvalidDataException(
                $"Kamer sinking/carry failed: Y=${startY:X4}-${maximumY:X4}, " +
                $"carry={carryPixels}, movement={state.YMovement}.");
        }

        // Once Samus leaves the asymmetric rider box, the same actor accelerates upward and
        // eventually reinstalls its vertically-still list near the authored target.
        samus.XPosition = 0;
        samus.YPosition = 0;
        ushort releasedY = platform.YPosition;
        for (int frame = 0; frame < 256; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
        }
        if (platform.YPosition >= releasedY || platform.YPosition >= state.TargetYPosition ||
            state.YMovement != PlatformVerticalMovement.Rising ||
            state.YSpeedTableIndex != 0 || !state.VerticallyStillArtInstalled)
        {
            throw new InvalidDataException(
                $"Kamer release/rise failed: Y=${releasedY:X4}->${platform.YPosition:X4}, " +
                $"target=${state.TargetYPosition:X4}, movement={state.YMovement}, " +
                $"speedIndex={state.YSpeedTableIndex}, stillArt=" +
                $"{state.VerticallyStillArtInstalled}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, platform);
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("East Ocean Kamers emitted no live ROM OBJ.");

        return new KamerResult(carryPixels, oam.LastFinalizedSpriteCount);
    }

    private static TripperResult VerifyIceBeamAcidTrippers(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xa75d);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = LoadEnemies(bus, room, assets);
        RoomEnemySlot[] trippers = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != 0xa76a || enemies.EnemyCount != 3 ||
            trippers.Any(slot => slot.EnemyDefinitionPointer != TripperDefinition) ||
            trippers.Any(slot => enemies.PlatformStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Ice Beam Acid state/population failed: state=${room.State.Pointer:X4}, " +
                $"count={enemies.EnemyCount}, Tripper=" +
                $"{trippers.Count(x => x.EnemyDefinitionPointer == TripperDefinition)}.");
        }

        RoomEnemySlot platform = trippers[0];
        PlatformEnemyState state = RequireState(enemies, platform);
        if (platform.XPosition != 0x0190 || platform.YPosition != 0x00a8 ||
            platform.Parameter1 != 0 || platform.Parameter2 != 0x1010 ||
            platform.CurrentInstruction != 0x9c3f || state.IsSuspensorPlatform ||
            state.XMovement != PlatformHorizontalMovement.Left ||
            state.LeftVelocity != -1 || state.LeftSubvelocity != 0 ||
            state.RightVelocity != 1 || state.RightSubvelocity != 0 ||
            state.MaximumYSpeedTableIndex != 0x10)
        {
            throw new InvalidDataException(
                $"Ice Beam Acid Tripper init failed: position=({platform.XPosition:X4}," +
                $"{platform.YPosition:X4}), params=${platform.Parameter1:X4}/" +
                $"${platform.Parameter2:X4}, list=${platform.CurrentInstruction:X4}, " +
                $"suspensor={state.IsSuspensorPlatform}, movement={state.XMovement}, " +
                $"speed={state.LeftVelocity}:{state.LeftSubvelocity:X4}/" +
                $"{state.RightVelocity}:{state.RightSubvelocity:X4}, max=" +
                $"{state.MaximumYSpeedTableIndex}.");
        }

        SamusState samus = CreateSamus(bus, 0, 0);
        var maps = new HashSet<ushort>();
        var directions = new HashSet<PlatformHorizontalMovement>();
        ushort minimumX = platform.XPosition;
        ushort maximumX = platform.XPosition;

        // First let the retail actor traverse naturally with no rider. This covers both
        // horizontal directions and direction commands without conflating them with carry.
        for (int frame = 0; frame < 1_024 && directions.Count < 2; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            maps.Add(platform.SpritemapPointer);
            directions.Add(state.XMovement);
            minimumX = Math.Min(minimumX, platform.XPosition);
            maximumX = Math.Max(maximumX, platform.XPosition);
        }
        if (directions.Count != 2 || minimumX == maximumX)
        {
            throw new InvalidDataException(
                $"Tripper horizontal reversal failed: directions=" +
                $"{string.Join(',', directions)}, X=${minimumX:X4}-${maximumX:X4}.");
        }

        // Riding a horizontally moving Tripper must publish its signed accepted X delta as
        // well as the sinking Y delta. Apply those whole words to keep Samus on the platform.
        PlaceSamusOnPlatform(samus, platform);
        bool sawNegativeXCarry = false;
        bool sawPositiveXCarry = false;
        bool sawYCarry = false;
        for (int frame = 0; frame < 1_024; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            short xCarry = unchecked((short)samus.Kinematics.ExtraXDisplacement);
            short yCarry = unchecked((short)samus.Kinematics.ExtraYDisplacement);
            samus.XPosition = unchecked((ushort)(samus.XPosition + xCarry));
            samus.YPosition = unchecked((ushort)(samus.YPosition + yCarry));
            sawNegativeXCarry |= xCarry < 0;
            sawPositiveXCarry |= xCarry > 0;
            sawYCarry |= yCarry > 0;
            maps.Add(platform.SpritemapPointer);
            directions.Add(state.XMovement);
            minimumX = Math.Min(minimumX, platform.XPosition);
            maximumX = Math.Max(maximumX, platform.XPosition);
            if (sawNegativeXCarry && sawPositiveXCarry && sawYCarry)
                break;
        }
        if (!sawNegativeXCarry || !sawPositiveXCarry || !sawYCarry)
        {
            throw new InvalidDataException(
                $"Tripper rider displacement failed: negativeX={sawNegativeXCarry}, " +
                $"positiveX={sawPositiveXCarry}, Y={sawYCarry}.");
        }

        // Release the platform long enough to select moving art while it rises. Combined
        // with both natural still facings above, this exercises instruction-driven maps.
        samus.XPosition = 0;
        samus.YPosition = 0;
        for (int frame = 0; frame < 256; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            maps.Add(platform.SpritemapPointer);
        }

        // The first release naturally selects whichever horizontal direction was current at
        // the end of the long ride. Wait for the next rightward wall reversal, depress the
        // same retail actor again, then release it while it still has ample horizontal room.
        // That forces the fourth and final instruction-list family without writing an AI
        // index or an instruction pointer from the audit.
        bool sawLeftBeforeRight = state.XMovement == PlatformHorizontalMovement.Left;
        bool sawFreshRightReversal = false;
        for (int frame = 0; frame < 384 && !sawFreshRightReversal; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            maps.Add(platform.SpritemapPointer);
            sawLeftBeforeRight |= state.XMovement == PlatformHorizontalMovement.Left;
            sawFreshRightReversal = sawLeftBeforeRight &&
                state.XMovement == PlatformHorizontalMovement.Right;
        }
        if (!sawFreshRightReversal)
            throw new InvalidDataException("Tripper did not naturally reverse right for its second ride.");

        PlaceSamusOnPlatform(samus, platform);
        for (int frame = 0; frame < 32; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            samus.XPosition = unchecked((ushort)(samus.XPosition +
                unchecked((short)samus.Kinematics.ExtraXDisplacement)));
            samus.YPosition = unchecked((ushort)(samus.YPosition +
                unchecked((short)samus.Kinematics.ExtraYDisplacement)));
            maps.Add(platform.SpritemapPointer);
        }
        samus.XPosition = 0;
        samus.YPosition = 0;
        for (int frame = 0; frame < 48; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(enemies, assets, room, samus, platform);
            maps.Add(platform.SpritemapPointer);
        }
        if (maps.Count != 12)
        {
            throw new InvalidDataException(
                $"Tripper instruction animation reached {maps.Count} of 12 retail maps: " +
                $"{string.Join(',', maps.Order().Select(map => $"${map:X4}"))}.");
        }

        // Rebuild interaction over the audited actor and prove the literal RTL touch AI.
        StepCentered(enemies, assets, room, samus, platform);
        samus.XPosition = platform.XPosition;
        samus.YPosition = platform.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 999 ||
            samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Tripper no-op touch failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

        // Ice-beam type two selects `$FF` in Tripper's actual vulnerability table. Its
        // private shot tail must retain direction with `$A009/$A015`, not leave an animated
        // map frozen mid-cycle.
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], platform, projectileType: 0x0002, damage: 20);
        ushort expectedFrozenMap = state.XMovement == PlatformHorizontalMovement.Left
            ? (ushort)0xa009
            : (ushort)0xa015;
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || platform.FrozenTimer != 400 ||
            platform.SpritemapPointer != expectedFrozenMap || platform.Health != 20)
        {
            throw new InvalidDataException(
                $"Tripper freeze failed: timer={platform.FrozenTimer}, map=" +
                $"${platform.SpritemapPointer:X4}/${expectedFrozenMap:X4}, " +
                $"health={platform.Health}.");
        }

        // A different retail actor receives a super-missile-family hit. Offset thirteen is
        // multiplier two, so the common handler must kill the 20-health body and mark it for
        // deletion; this verifies real damage as distinct from freeze admission.
        RoomEnemySlot lethalTarget = trippers[1];
        StepCentered(enemies, assets, room, samus, lethalTarget);
        projectiles = new SamusProjectileSystem();
        sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], lethalTarget, projectileType: 0x0200, damage: 300);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || lethalTarget.Health != 0 ||
            lethalTarget.EnemyDefinitionPointer != 0 || lethalTarget.Properties != 0)
        {
            throw new InvalidDataException(
                $"Tripper lethal damage failed: health={lethalTarget.Health}, " +
                $"properties=${lethalTarget.Properties:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, platform);
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Ice Beam Acid Trippers emitted no live ROM OBJ.");

        return new TripperResult(minimumX, maximumX, maps.Count);
    }

    private static void VerifyRoom021eRisingPlatform(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            PlatformAuditRomData.Room021eHeaderPointer,
            cameraX: 0,
            cameraY: 0);
        SamusState samus = runtime.Samus ?? throw new InvalidDataException("Room $02/$1E omitted Samus.");
        int platformSlot = PlatformAuditRomData.Room021eRisingKamerSlot;
        RoomEnemySlot platform = runtime.Enemies.Slots[platformSlot];
        VerticalShutterEnemyState state = runtime.Enemies.VerticalShutterStates[platformSlot] ??
            throw new InvalidDataException("Room $02/$1E slot three omitted vertical-platform state.");
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = platform.XPosition;
        samus.YPosition = unchecked((ushort)(
            platform.YPosition - platform.YRadius - samus.Kinematics.YRadius));
        ushort platformStartY = platform.YPosition;
        ushort samusStartY = samus.YPosition;
        int largestSupportGap = 0;
        for (int frame = 0; frame < 48; frame++)
        {
            runtime.StepFrame(0);
            int supportGap = platform.YPosition - platform.YRadius -
                (samus.YPosition + samus.Kinematics.YRadius);
            largestSupportGap = Math.Max(largestSupportGap, Math.Abs(supportGap));
            if (samus.ReadMovementType(bus) != SamusMovementType.Standing)
            {
                throw new InvalidDataException(
                    $"Room $02/$1E rising platform changed Samus to pose ${samus.Pose:X2}/" +
                    $"{samus.ReadMovementType(bus)} on frame {frame}; platform/Samus Y=" +
                    $"${platform.YPosition:X4}/${samus.YPosition:X4}, gap={supportGap}.");
            }
        }

        short platformTravel = unchecked((short)(platform.YPosition - platformStartY));
        short samusTravel = unchecked((short)(samus.YPosition - samusStartY));
        if (state.Function != VerticalShutterFunction.MovingUp || platformTravel >= 0 ||
            samusTravel != platformTravel || largestSupportGap != 0)
        {
            throw new InvalidDataException(
                $"Room $02/$1E rising-platform attachment failed: function={state.Function}, " +
                $"platform={platformTravel}, Samus={samusTravel}, max-gap={largestSupportGap}.");
        }

        runtime.StepFrame(runtime.ControllerBindings.Jump);
        if (samus.ReadMovementType(bus) != SamusMovementType.NormalJumping)
        {
            throw new InvalidDataException(
                $"Room $02/$1E rising platform rejected Jump: pose=${samus.Pose:X2}/" +
                $"{samus.ReadMovementType(bus)}.");
        }
    }

    private static void VerifyRightFacingTripperFreeze(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xae07);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        // Spiky Platforms Tunnel begins with two Trippers and only then reaches the
        // untranslated shutter. Retaining that exact prefix preserves both genuine records;
        // the first is the shipped parameter-one=1/right-facing variant needed here.
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 2,
            deathQuota: 2);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot right = enemies.Slots[0];
        PlatformEnemyState state = RequireState(enemies, right);
        if (room.State.Pointer != 0xae14 || enemies.EnemyCount != 2 ||
            right.EnemyDefinitionPointer != TripperDefinition || right.Parameter1 != 1 ||
            right.Parameter2 != 0x0018 ||
            state.XMovement != PlatformHorizontalMovement.Right ||
            state.RightVelocity != 1 || state.RightSubvelocity != 0x8000 ||
            right.CurrentInstruction != 0x9c55)
        {
            throw new InvalidDataException(
                $"Right-facing Tripper init failed: state=${room.State.Pointer:X4}, " +
                $"count={enemies.EnemyCount}, definition=${right.EnemyDefinitionPointer:X4}, " +
                $"params=${right.Parameter1:X4}/${right.Parameter2:X4}, movement=" +
                $"{state.XMovement}, speed={state.RightVelocity}:" +
                $"{state.RightSubvelocity:X4}, list=${right.CurrentInstruction:X4}.");
        }

        SamusState samus = CreateSamus(prefixBus, 0, 0);
        StepCentered(enemies, assets, room, samus, right);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], right, projectileType: 0x0002, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                prefixBus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || right.FrozenTimer != 400 ||
            right.SpritemapPointer != 0xa015)
        {
            throw new InvalidDataException(
                $"Right-facing Tripper freeze failed: timer={right.FrozenTimer}, " +
                $"map=${right.SpritemapPointer:X4}.");
        }
    }

    private static void VerifyRuntimeRiding(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0x94fd, cameraX: 0x0400, cameraY: 0x0400);

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Runtime East Ocean load omitted Samus.");
        RoomEnemySlot platform = runtime.Enemies.Slots[10];
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InputLocked = false;
        samus.Health = 999;
        PlaceSamusOnPlatform(samus, platform);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        ushort platformStartY = platform.YPosition;
        ushort samusStartY = samus.YPosition;
        bool sawExternalY = false;
        for (int frame = 0; frame < 96; frame++)
        {
            runtime.StepFrame(0);
            sawExternalY |= samus.Kinematics.ExtraYDisplacement != 0;
        }

        short platformTravel = unchecked((short)(platform.YPosition - platformStartY));
        short samusTravel = unchecked((short)(samus.YPosition - samusStartY));
        // The platform's own rider predicate deliberately evaluates Samus Y + 3. Native
        // solid-enemy movement can therefore settle her as much as three pixels into that
        // asymmetric support window while still retaining the ride; a tighter host bound
        // would reject the cartridge's actual steady-state geometry.
        if (!sawExternalY || platformTravel <= 0 || samusTravel <= 0 ||
            Math.Abs(samusTravel - platformTravel) > 3)
        {
            throw new InvalidDataException(
                $"Runtime platform carry failed: platform={platformTravel}, Samus=" +
                $"{samusTravel}, externalY={sawExternalY}, final words=" +
                $"${samus.Kinematics.ExtraYDisplacement:X4}." );
        }

        // Move off the platform for one production frame. The pre-enemy clear must remove
        // the prior producer value rather than letting stale downward carry persist.
        samus.XPosition = 0x0010;
        samus.YPosition = 0x0010;
        runtime.StepFrame(0);
        if (samus.Kinematics.ExtraXFixed != 0 || samus.Kinematics.ExtraYFixed != 0)
        {
            throw new InvalidDataException(
                $"Runtime retained stale platform displacement: X=" +
                $"{samus.Kinematics.ExtraXFixed:X8}, Y={samus.Kinematics.ExtraYFixed:X8}.");
        }
    }

    private static RoomEnemySystem LoadEnemies(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);
        return enemies;
    }

    private static SamusState CreateSamus(
        ISnesAddressSpace bus,
        ushort xPosition,
        ushort yPosition)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = xPosition,
            YPosition = yPosition,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void PlaceSamusOnPlatform(SamusState samus, RoomEnemySlot platform)
    {
        samus.XPosition = platform.XPosition;
        samus.YPosition = unchecked((ushort)(
            platform.YPosition - platform.YRadius - samus.Kinematics.YRadius));
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
    }

    private static void ClearExternalDisplacement(SamusState samus)
    {
        samus.Kinematics.ExtraXSubdisplacement = 0;
        samus.Kinematics.ExtraXDisplacement = 0;
        samus.Kinematics.ExtraYSubdisplacement = 0;
        samus.Kinematics.ExtraYDisplacement = 0;
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot target)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, target);
        enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            samus,
            level: assets.LevelData);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort projectileType,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = projectileType;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static PlatformEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.PlatformStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Platform slot {slot.SlotIndex} has no typed state.");

    private readonly record struct KamerResult(int CarryPixels, int ObjPieces);

    private readonly record struct TripperResult(
        ushort MinimumX,
        ushort MaximumX,
        int MapCount);
}
