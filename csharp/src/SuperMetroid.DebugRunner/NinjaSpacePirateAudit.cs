using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit of the complete ninja Space Pirate family. The Metal Pirates room is
/// especially useful evidence: its untouched population contains exactly the two retail gold
/// ninjas, with opposite starting posts, and no unrelated enemy can accidentally satisfy a
/// movement or projectile assertion.
/// </summary>
internal static class NinjaSpacePirateAudit
{
    private const ushort MetalPiratesRoomPointer = 0xb62b;

    private static readonly ushort[] Definitions =
        [0xf4d3, 0xf513, 0xf553, 0xf593, 0xf5d3, 0xf613];
    private static readonly ushort[] Health = [20, 90, 200, 1800, 300, 500];
    private static readonly ushort[] Damage = [15, 20, 80, 100, 160, 15];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, MetalPiratesRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyDefinitionsAndBytecode(bus);
        VerifyUntouchedPopulation(bus, room, assets);
        VerifyIdleLoopReactivatesBothPirates(bus, room, assets);
        MotionResult leftToRight = VerifyMotionFamily(bus, room, assets, actorIndex: 0);
        MotionResult rightToLeft = VerifyMotionFamily(bus, room, assets, actorIndex: 1);
        ClawResult claw = VerifyClawAttack(bus, room, assets);
        VerifyKickAndFlinch(bus, room, assets);
        VerifyCombatAndRendering(bus, room, assets);

        Console.WriteLine(
            "Ninja Space Pirate audit passed: all six retail headers, the untouched two-gold-" +
            "Pirate room population, derived post geometry, both spin-jump arcs, both " +
            "divekicks and return walks, landing dust, kick/flinch priority, palette/sound " +
            $"opcodes, {claw.SpawnedCount} left/right returning claws ({claw.MapCount} ROM " +
            $"maps), per-frame extended hitboxes, vulnerable/reflective gold armor, contact " +
            $"damage, shot/normal-bomb/power-bomb/grapple handling, and extended OBJ " +
            $"rendering passed across {leftToRight.FunctionCount + rightToLeft.FunctionCount} " +
            "observed movement functions.");
        return 0;
    }

    /// <summary>Both native facing lists must return to active AI after their idle animation.</summary>
    private static void VerifyIdleLoopReactivatesBothPirates(
        SuperMetroidAddressSpace bus, CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        for (int index = 0; index < 2; index++)
        {
            LoadedNinjas loaded = Load(bus, room, assets);
            RoomEnemySlot actor = KeepOnly(loaded, index);
            NinjaSpacePirateEnemyState state = State(loaded.Enemies, actor);
            // Outside the kick and midpoint gates, but inside initial activation range.
            // Unlike the old claw audit, do not synchronize entry with the claw timer:
            // this forces an idle cycle before the periodic attack becomes eligible.
            loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + (index == 0 ? -96 : 96)));
            loaded.Samus.YPosition = state.SpawnY;
            for (int frame = 0; frame < 400; frame++)
            {
                loaded.Enemies.StepFrame(0, 0, false, loaded.Samus,
                    level: assets.LevelData, samusProjectiles: loaded.Projectiles);
                loaded.Enemies.StepEnemyProjectiles(assets.LevelData, null, cameraX: 0, cameraY: 0);
            }
            if (state.SpawnedClawCount < 4)
                throw new InvalidDataException($"Pirate {index} stalled after idle: only {state.SpawnedClawCount} claws.");
        }
        Console.WriteLine("Both pirate facing lists resume repeated attacks after idle cycles.");
    }

    private static void VerifyDefinitionsAndBytecode(ISnesAddressSpace bus)
    {
        for (int index = 0; index < Definitions.Length; index++)
        {
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definitions[index]);
            if (definition.TileDataSize != 0x1800 || definition.Health != Health[index] ||
                definition.Damage != Damage[index] || definition.XRadius != 0x0010 ||
                definition.YRadius != 0x0020 || definition.Bank != 0xb2 ||
                definition.InitializationAiPointer != 0xf5de || definition.PartCount != 1 ||
                definition.MainAiPointer != 0xf6a2 || definition.GrappleAiPointer != 0x800f ||
                definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
                definition.DeathAnimation != 4 || definition.PowerBombReactionPointer != 0x8767 ||
                definition.TouchAiPointer != 0x876c || definition.ShotAiPointer != 0x8779 ||
                definition.Layer != 5 || definition.TileDataAddress == 0 ||
                definition.VulnerabilityPointer == 0 || definition.NamePointer == 0)
            {
                throw new InvalidDataException(
                    $"Ninja Pirate header $A0:{Definitions[index]:X4} does not match retail data.");
            }
        }

        // These words span every family-specific opcode plus the shared function setter.
        // Checking encoded data keeps the audit coupled to the actual cartridge lists rather
        // than allowing a host implementation and a host fixture to agree with each other.
        VerifyWords(bus, 0xb2f15c, [0xef83, 0x804b, 0x0005]);
        VerifyWords(bus, 0xb2f178, [0xf564, 0x0000, 0xffe0, 0xfff8]);
        VerifyWords(bus, 0xb2f1a8, [0xf564, 0x0000, 0xfff0, 0x0008]);
        VerifyWords(bus, 0xb2f366, [0xf564, 0x0001, 0x0020, 0xfff8]);
        VerifyWords(bus, 0xb2f3e6, [0xf5d6, 0xef83, 0x804b]);
        VerifyWords(bus, 0xb2f47c, [0xf536, 0x0200, 0x0004]);
        VerifyWords(bus, 0xb2f522, [0xf546, 0x0066]);
        VerifyWords(bus, 0x86a189, [0xa098, 0xa05b, 0x0000, 0x0808, 0x1014, 0x0000, 0x84fc]);
    }

    private static void VerifyUntouchedPopulation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedNinjas loaded = Load(bus, room, assets);
        if (loaded.Enemies.EnemyCount != 2 || loaded.Ninjas.Count != 2 ||
            loaded.Ninjas.Any(actor => actor.EnemyDefinitionPointer != 0xf593))
        {
            throw new InvalidDataException(
                $"Metal Pirates loaded {loaded.Enemies.EnemyCount} enemies and " +
                $"{loaded.Ninjas.Count} ninja Pirates instead of two gold ninjas.");
        }

        ushort[] expectedX = [0x00e7, 0x0219];
        ushort[] expectedMidpoint = [0x0148, 0x01b8];
        ushort[] expectedLeft = [0x00e7, 0x0157];
        ushort[] expectedRight = [0x01a9, 0x0219];
        ushort[] expectedLists = [0xf4cc, 0xf2da];
        for (int index = 0; index < loaded.Ninjas.Count; index++)
        {
            RoomEnemySlot actor = loaded.Ninjas[index];
            NinjaSpacePirateEnemyState state = State(loaded.Enemies, actor);
            if (actor.XPosition != expectedX[index] || actor.YPosition != 0x00b0 ||
                actor.Parameter1 != (index == 0 ? 1 : 0) || actor.Parameter2 != 0x00c0 ||
                actor.Properties != 0x2800 || actor.ExtraProperties != 0x0004 ||
                actor.Health != 1800 || actor.CurrentInstruction != expectedLists[index] ||
                state.ActiveInstruction != expectedLists[index] ||
                state.PostsMidpointX != expectedMidpoint[index] ||
                state.LeftPostX != expectedLeft[index] ||
                state.RightPostX != expectedRight[index] ||
                state.CalculatedSpinJumpSpeed != 0x04e0 ||
                state.Function != NinjaSpacePirateFunction.NoOperation ||
                state.SpawnY != 0x00b0)
            {
                throw new InvalidDataException(
                    $"Metal Pirate {index} initialization mismatch: X=${actor.XPosition:X4}, " +
                    $"posts=${state.LeftPostX:X4}/${state.PostsMidpointX:X4}/" +
                    $"${state.RightPostX:X4}, speed=${state.CalculatedSpinJumpSpeed:X4}, " +
                    $"list=${actor.CurrentInstruction:X4}, function=${(ushort)state.Function:X4}.");
            }
        }
    }

    private static MotionResult VerifyMotionFamily(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int actorIndex)
    {
        LoadedNinjas loaded = Load(bus, room, assets);
        RoomEnemySlot actor = KeepOnly(loaded, actorIndex);
        NinjaSpacePirateEnemyState state = State(loaded.Enemies, actor);
        loaded.Samus.XPosition = state.PostsMidpointX;
        loaded.Samus.YPosition = state.SpawnY;

        var functions = new HashSet<NinjaSpacePirateFunction>();
        var maps = new HashSet<ushort>();
        bool sawNormalPalette = false;
        bool sawFlashPalette = false;
        bool heardSpin = false;
        bool heardDive = false;
        ushort minimumX = actor.XPosition;
        ushort maximumX = actor.XPosition;
        ushort minimumY = actor.YPosition;

        for (int frame = 0; frame < 1800; frame++)
        {
            loaded.Enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            functions.Add(state.Function);
            maps.Add(actor.SpritemapPointer);
            minimumX = Math.Min(minimumX, actor.XPosition);
            maximumX = Math.Max(maximumX, actor.XPosition);
            minimumY = Math.Min(minimumY, actor.YPosition);
            sawNormalPalette |= actor.PaletteIndex == 0x0200;
            sawFlashPalette |= actor.PaletteIndex == 0x0e00;
            heardSpin |= loaded.Enemies.LastSpacePirateSoundEffect == 0x003f;
            heardDive |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0066;
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData, null, cameraX: 0, cameraY: 0);
        }

        NinjaSpacePirateFunction[] required = actorIndex == 0
            ? [NinjaSpacePirateFunction.Initial, NinjaSpacePirateFunction.Active,
               NinjaSpacePirateFunction.SpinJumpRightRising,
               NinjaSpacePirateFunction.SpinJumpRightFalling,
               NinjaSpacePirateFunction.ReadyToDivekick,
               NinjaSpacePirateFunction.DivekickRightJump,
               NinjaSpacePirateFunction.DivekickRightDive,
               NinjaSpacePirateFunction.DivekickRightWalkToPost]
            : [NinjaSpacePirateFunction.Initial, NinjaSpacePirateFunction.Active,
               NinjaSpacePirateFunction.SpinJumpLeftRising,
               NinjaSpacePirateFunction.SpinJumpLeftFalling,
               NinjaSpacePirateFunction.ReadyToDivekick,
               NinjaSpacePirateFunction.DivekickLeftJump,
               NinjaSpacePirateFunction.DivekickLeftDive,
               NinjaSpacePirateFunction.DivekickLeftWalkToPost];
        if (required.Any(function => !functions.Contains(function)) ||
            minimumX == maximumX || minimumY >= state.SpawnY || maps.Count < 12 ||
            state.LandingDustCount < 4 || !sawNormalPalette || !sawFlashPalette ||
            !heardSpin || !heardDive)
        {
            throw new InvalidDataException(
                $"Ninja Pirate {actorIndex} motion failed: X=${minimumX:X4}-${maximumX:X4}, " +
                $"Ymin=${minimumY:X4}, maps={maps.Count}, dust={state.LandingDustCount}, " +
                $"palettes={sawNormalPalette}/{sawFlashPalette}, sounds={heardSpin}/{heardDive}, " +
                $"missing={string.Join(',', required.Except(functions))}.");
        }

        return new MotionResult(functions.Count);
    }

    private static ClawResult VerifyClawAttack(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedNinjas loaded = Load(bus, room, assets);
        RoomEnemySlot actor = KeepOnly(loaded, 0);
        NinjaSpacePirateEnemyState state = State(loaded.Enemies, actor);
        // Keep the initial watcher outside its 128-pixel activation radius until the native
        // frame counter is 63. Activating on that frame makes the first Active call observe
        // counter 64, which is the retail periodic-claw gate rather than a forced host state.
        loaded.Samus.XPosition = unchecked((ushort)(state.LeftPostX - 256));
        loaded.Samus.YPosition = state.SpawnY;

        var directions = new HashSet<ushort>();
        var maps = new HashSet<ushort>();
        bool sawOutboundDeceleration = false;
        bool sawInboundAcceleration = false;
        bool heardThrow = false;
        for (int frame = 0; frame < 900; frame++)
        {
            if (actor.FrameCounter == 63)
                loaded.Samus.XPosition = unchecked((ushort)(state.LeftPostX - 96));
            loaded.Enemies.StepFrame(
                0, 0, false, loaded.Samus, level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            heardThrow |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0066;
            var before = loaded.Enemies.EnemyProjectiles.ToDictionary(
                projectile => projectile.SlotIndex,
                projectile => (projectile.Kind, projectile.Variable0, projectile.Variable1));
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PirateClaw)
                {
                    directions.Add(projectile.DirectionParameter);
                    maps.Add(projectile.SpritemapPointer);
                }
            }
            // Follow one live claw with the audit camera. Its full 288-pixel outbound arc is
            // wider than one SNES viewport, so a stationary camera correctly culls it before
            // reversal; tracking is required to prove the otherwise reachable inbound branch.
            RoomEnemyProjectileSlot? followedClaw = loaded.Enemies.EnemyProjectiles
                .FirstOrDefault(projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
            ushort projectileCameraX = followedClaw is null
                ? (ushort)0
                : unchecked((ushort)(followedClaw.XPosition - 128));
            ushort projectileCameraY = followedClaw is null
                ? (ushort)0
                : unchecked((ushort)(followedClaw.YPosition - 128));
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                null,
                cameraX: projectileCameraX,
                cameraY: projectileCameraY);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (!projectile.IsActive || projectile.Kind != RoomEnemyProjectileKind.PirateClaw)
                    continue;
                (RoomEnemyProjectileKind kind, ushort speed, ushort outbound) =
                    before[projectile.SlotIndex];
                if (kind != RoomEnemyProjectileKind.PirateClaw)
                    continue;
                sawOutboundDeceleration |= outbound != 0 && projectile.Variable0 < speed;
                sawInboundAcceleration |= outbound == 0 && projectile.Variable0 > speed;
            }
        }

        // Exercise the opposite throw list without inventing a population: the second retail
        // actor begins at the right post and naturally throws right when Samus is to its right.
        LoadedNinjas opposite = Load(bus, room, assets);
        RoomEnemySlot rightActor = KeepOnly(opposite, 1);
        NinjaSpacePirateEnemyState rightState = State(opposite.Enemies, rightActor);
        opposite.Samus.XPosition = unchecked((ushort)(rightState.RightPostX + 256));
        opposite.Samus.YPosition = rightState.SpawnY;
        for (int frame = 0; frame < 500; frame++)
        {
            if (rightActor.FrameCounter == 63)
                opposite.Samus.XPosition = unchecked((ushort)(rightState.RightPostX + 96));
            opposite.Enemies.StepFrame(
                0x0100, 0, false, opposite.Samus, level: assets.LevelData,
                samusProjectiles: opposite.Projectiles);
            foreach (RoomEnemyProjectileSlot projectile in opposite.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PirateClaw)
                    directions.Add(projectile.DirectionParameter);
            }
            opposite.Enemies.StepEnemyProjectiles(
                assets.LevelData, null, cameraX: 0x0100, cameraY: 0);
        }

        if (!directions.SetEquals([(ushort)0, (ushort)1]) || maps.Count < 4 ||
            state.SpawnedClawCount < 2 || !sawOutboundDeceleration ||
            !sawInboundAcceleration || !heardThrow)
        {
            throw new InvalidDataException(
                $"Ninja claw attack failed: directions={string.Join(',', directions)}, " +
                $"maps={maps.Count}, count={state.SpawnedClawCount}, motion=" +
                $"{sawOutboundDeceleration}/{sawInboundAcceleration}, sound={heardThrow}.");
        }

        return new ClawResult(state.SpawnedClawCount, maps.Count);
    }

    private static void VerifyKickAndFlinch(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedNinjas kick = Load(bus, room, assets);
        RoomEnemySlot kickActor = KeepOnly(kick, 0);
        PrimeActive(kick, assets, kickActor);
        kick.Samus.XPosition = unchecked((ushort)(kickActor.XPosition + 20));
        kick.Samus.YPosition = kickActor.YPosition;
        kick.Enemies.StepFrame(0, 0, false, kick.Samus, level: assets.LevelData,
            samusProjectiles: kick.Projectiles);
        // EnemyMain immediately consumes the kick list's function opcode and first timed
        // record, leaving the cursor at $F522 after selecting list $F51A.
        if (kickActor.CurrentInstruction != 0xf522 ||
            State(kick.Enemies, kickActor).Function != NinjaSpacePirateFunction.NoOperation)
            throw new InvalidDataException("Ninja Pirate did not select the right-facing kick.");

        LoadedNinjas flinch = Load(bus, room, assets);
        RoomEnemySlot flinchActor = KeepOnly(flinch, 0);
        PrimeActive(flinch, assets, flinchActor);
        flinch.Samus.XPosition = unchecked((ushort)(flinchActor.XPosition - 80));
        flinch.Samus.YPosition = flinchActor.YPosition;
        ArmProjectile(
            flinch.Projectiles.Slots[4],
            flinchActor.XPosition,
            flinchActor.YPosition);
        flinch.Enemies.StepFrame(0, 0, false, flinch.Samus, level: assets.LevelData,
            samusProjectiles: flinch.Projectiles);
        if (flinchActor.CurrentInstruction != 0xf278 ||
            State(flinch.Enemies, flinchActor).Function != NinjaSpacePirateFunction.NoOperation)
            throw new InvalidDataException("Ninja Pirate did not prioritize projectile flinch.");
    }

    private static void VerifyCombatAndRendering(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedNinjas body = Load(bus, room, assets);
        RoomEnemySlot bodyActor = KeepOnly(body, 0);
        PrimeActive(body, assets, bodyActor);
        body.Samus.XPosition = bodyActor.XPosition;
        body.Samus.YPosition = bodyActor.YPosition;
        body.Samus.InvincibilityTimer = 0;
        if (!body.Enemies.ResolveOrdinarySamusContact(body.Samus, 0) ||
            body.Samus.Health != 899)
            throw new InvalidDataException("Gold ninja body contact did not deal 100 damage.");

        LoadedNinjas frozen = Load(bus, room, assets);
        RoomEnemySlot frozenActor = KeepOnly(frozen, 0);
        PrimeActive(frozen, assets, frozenActor);
        frozenActor.FrozenTimer = 2;
        frozen.Samus.XPosition = frozenActor.XPosition;
        frozen.Samus.YPosition = frozenActor.YPosition;
        frozen.Samus.InvincibilityTimer = 0;
        if (!frozen.Enemies.ResolveOrdinarySamusContact(frozen.Samus, 0) ||
            frozen.Samus.Health != 999)
            throw new InvalidDataException("Frozen ninja Pirate incorrectly damaged Samus.");

        LoadedNinjas shot = Load(bus, room, assets);
        RoomEnemySlot shotActor = KeepOnly(shot, 0);
        PrimeActive(shot, assets, shotActor);
        // `$B2:89C4` puts the shared `$87C8` vulnerable callback on its first component.
        // Aim inside that component instead of the actor origin: the origin belongs to the
        // overlapping armored body in several idle frames and must reflect rather than hurt.
        shotActor.SpritemapPointer = 0x89c4;
        ArmProjectile(
            shot.Projectiles.Slots[0],
            unchecked((ushort)(shotActor.XPosition - 10)),
            unchecked((ushort)(shotActor.YPosition - 10)));
        ushort healthBefore = shotActor.Health;
        if (shot.Enemies.ResolveOrdinaryProjectileHits(
                bus, shot.Projectiles, shot.SharedProjectiles, shot.Samus) != 1 ||
            shotActor.Health >= healthBefore || shotActor.FlashTimer == 0)
            throw new InvalidDataException("Gold ninja accepted shot did not apply ROM damage.");

        LoadedNinjas armored = Load(bus, room, assets);
        RoomEnemySlot armoredActor = KeepOnly(armored, 0);
        PrimeActive(armored, assets, armoredActor);
        // Idle extended map zero's first component points at `$883E`. A rightward missile
        // must become the native up-left direction, reload bank-$93 art/damage, enter the
        // `$00F0` reflected acceleration state, and leave Pirate health untouched.
        armoredActor.SpritemapPointer = 0x8a54;
        SamusProjectileSlot reflectedMissile = armored.Projectiles.Slots[0];
        ArmProjectile(
            reflectedMissile,
            armoredActor.XPosition,
            armoredActor.YPosition,
            type: 0x0100,
            variable: 0x0300);
        ushort armoredHealth = armoredActor.Health;
        if (armored.Enemies.ResolveOrdinaryProjectileHits(
                bus, armored.Projectiles, armored.SharedProjectiles, armored.Samus) != 1 ||
            armoredActor.Health != armoredHealth || armoredActor.InvincibilityTimer != 10 ||
            reflectedMissile.Direction != (ushort)SamusProjectileDirection.UpLeft ||
            reflectedMissile.Variable != 240 ||
            reflectedMissile.PreInstruction != SamusProjectilePreInstruction.Missile ||
            reflectedMissile.InstructionPointer == 0x9000 || reflectedMissile.Damage == 300 ||
            armored.Enemies.LastSpacePirateSoundEffect != 0x0066)
        {
            throw new InvalidDataException(
                "Gold ninja armored hitbox did not perform native missile reflection.");
        }

        LoadedNinjas immuneBeam = Load(bus, room, assets);
        RoomEnemySlot immuneBeamActor = KeepOnly(immuneBeam, 0);
        PrimeActive(immuneBeam, assets, immuneBeamActor);
        immuneBeamActor.SpritemapPointer = 0x89c4;
        SamusProjectileSlot reflectedPowerBeam = immuneBeam.Projectiles.Slots[0];
        ArmProjectile(
            reflectedPowerBeam,
            unchecked((ushort)(immuneBeamActor.XPosition - 10)),
            unchecked((ushort)(immuneBeamActor.YPosition - 10)),
            type: 0x0000);
        if (immuneBeam.Enemies.ResolveOrdinaryProjectileHits(
                bus, immuneBeam.Projectiles, immuneBeam.SharedProjectiles,
                immuneBeam.Samus) != 1 ||
            reflectedPowerBeam.Direction != (ushort)SamusProjectileDirection.UpLeft ||
            reflectedPowerBeam.PreInstruction != SamusProjectilePreInstruction.NoWaveBeam ||
            immuneBeamActor.Health != 1800)
        {
            throw new InvalidDataException(
                "Gold ninja vulnerable callback did not redirect an immune power beam to reflection.");
        }

        LoadedNinjas freshSuper = Load(bus, room, assets);
        RoomEnemySlot freshSuperActor = KeepOnly(freshSuper, 0);
        PrimeActive(freshSuper, assets, freshSuperActor);
        freshSuperActor.SpritemapPointer = 0x8a54;
        SamusProjectileSlot ignoredSuper = freshSuper.Projectiles.Slots[0];
        ArmProjectile(
            ignoredSuper,
            freshSuperActor.XPosition,
            freshSuperActor.YPosition,
            type: 0x0200,
            variable: 0);
        if (freshSuper.Enemies.ResolveOrdinaryProjectileHits(
                bus, freshSuper.Projectiles, freshSuper.SharedProjectiles,
                freshSuper.Samus) != 1 ||
            ignoredSuper.Direction !=
                ((ushort)SamusProjectileDirection.Right | 0x0010) ||
            ignoredSuper.InstructionPointer != 0x9000 || freshSuperActor.InvincibilityTimer != 0 ||
            freshSuperActor.Health != 1800 || freshSuper.Projectiles.EarthquakeType != 20 ||
            freshSuper.Projectiles.EarthquakeTimer != 30)
        {
            throw new InvalidDataException(
                "Gold ninja armor did not ignore a pre-link Super Missile exactly once.");
        }

        VerifyIgnoredNormalBomb(
            bus,
            room,
            assets,
            spritemapPointer: 0x89c4,
            xOffset: -10,
            yOffset: -10,
            callbackName: "$87C8 vulnerable");
        VerifyIgnoredNormalBomb(
            bus,
            room,
            assets,
            spritemapPointer: 0x8a54,
            xOffset: 0,
            yOffset: 0,
            callbackName: "$883E armored");

        LoadedNinjas powerBomb = Load(bus, room, assets);
        RoomEnemySlot powerBombActor = KeepOnly(powerBomb, 0);
        ushort powerBombHealth = powerBombActor.Health;
        int powerBombHits = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
            bus, powerBombActor.XPosition, powerBombActor.YPosition, 64);
        if (powerBombHits != 0 || powerBombActor.Health != powerBombHealth)
        {
            // Gold ninja vulnerability byte fourteen is zero. Its nonzero $8767 header
            // pointer is therefore never reached because the outer power-bomb pass rejects
            // the actor first—an important distinction from gold wall/walking Pirates.
            throw new InvalidDataException("Gold ninja power-bomb immunity was not preserved.");
        }

        LoadedNinjas grapple = Load(bus, room, assets);
        RoomEnemySlot grappleActor = KeepOnly(grapple, 0);
        PrimeActive(grapple, assets, grappleActor);
        GrappleEnemyCollision grappleResult = grapple.Enemies.ResolveGrappleEndpoint(
            grappleActor.XPosition, grappleActor.YPosition);
        if (!grappleResult.Collided || grappleResult.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Ninja Pirate grapple cancel failed.");

        var oam = new OamBuffer();
        oam.BeginFrame();
        grapple.Enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 2)
            throw new InvalidDataException("Ninja Pirate extended map emitted no OBJ body.");

        LoadedNinjas clawContact = Load(bus, room, assets);
        RoomEnemySlot clawSource = KeepOnly(clawContact, 0);
        NinjaSpacePirateEnemyState clawState = State(clawContact.Enemies, clawSource);
        clawContact.Samus.XPosition = unchecked((ushort)(clawState.LeftPostX - 256));
        clawContact.Samus.YPosition = clawState.SpawnY;
        RoomEnemyProjectileSlot? claw = null;
        for (int frame = 0; frame < 500 && claw is null; frame++)
        {
            if (clawSource.FrameCounter == 63)
                clawContact.Samus.XPosition = unchecked((ushort)(clawState.LeftPostX - 96));
            clawContact.Enemies.StepFrame(
                0, 0, false, clawContact.Samus, level: assets.LevelData,
                samusProjectiles: clawContact.Projectiles);
            claw = clawContact.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.PirateClaw);
        }
        if (claw is null)
            throw new InvalidDataException("Ninja Pirate did not spawn a contact-test claw.");
        foreach (RoomEnemyProjectileSlot other in clawContact.Enemies.EnemyProjectiles)
        {
            if (!ReferenceEquals(other, claw))
                other.Clear();
        }
        clawContact.Samus.XPosition = claw.XPosition;
        clawContact.Samus.YPosition = claw.YPosition;
        clawContact.Samus.InvincibilityTimer = 0;
        clawContact.Enemies.StepEnemyProjectiles(assets.LevelData, clawContact.Samus, 0, 0, 0);
        if (clawContact.Samus.Health != 979 || claw.IsActive)
            throw new InvalidDataException("Pirate claw contact did not deal 20 damage and delete.");
    }

    private static void VerifyIgnoredNormalBomb(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort spritemapPointer,
        short xOffset,
        short yOffset,
        string callbackName)
    {
        LoadedNinjas loaded = Load(bus, room, assets);
        RoomEnemySlot actor = KeepOnly(loaded, 0);
        PrimeActive(loaded, assets, actor);
        actor.SpritemapPointer = spritemapPointer;
        ushort healthBefore = actor.Health;
        SamusBombProjectileSlot bomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                loaded.SharedProjectiles,
                unchecked((ushort)(actor.XPosition + xOffset)),
                unchecked((ushort)(actor.YPosition + yOffset)),
                damage: 4000);

        int hits = loaded.Enemies.ResolveOrdinaryBombHits(
            loaded.SharedProjectiles,
            loaded.Projectiles,
            loaded.Samus);
        if (hits != 1 || (bomb.Direction & 0x0010) == 0 || actor.Health != healthBefore ||
            actor.InvincibilityTimer != 0 || actor.FlashTimer != 0 ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Gold ninja {callbackName} normal-bomb dispatch mismatch: hits={hits}, " +
                $"direction=${bomb.Direction:X4}, health={healthBefore}->{actor.Health}, " +
                $"invincibility/flash={actor.InvincibilityTimer}/{actor.FlashTimer}, " +
                $"deleted={actor.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static LoadedNinjas Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            XPosition = 0,
            YPosition = 0,
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            () => random.RandomNumber,
            assets.LevelData,
            samus);
        List<RoomEnemySlot> ninjas = enemies.Slots
            .Where(slot => Definitions.Contains(slot.EnemyDefinitionPointer))
            .ToList();
        return new LoadedNinjas(
            enemies,
            samus,
            ninjas,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem());
    }

    private static RoomEnemySlot KeepOnly(LoadedNinjas loaded, int actorIndex)
    {
        RoomEnemySlot actor = loaded.Ninjas[actorIndex];
        foreach (RoomEnemySlot other in loaded.Enemies.Slots.Where(
            slot => slot.EnemyDefinitionPointer != 0 && !ReferenceEquals(slot, actor)))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);
        return actor;
    }

    private static void PrimeActive(
        LoadedNinjas loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        NinjaSpacePirateEnemyState state = State(loaded.Enemies, actor);
        // Eighty pixels to the outside is inside the 128-pixel activation band but remains
        // well outside both the 40-pixel kick box and the midpoint's 32-pixel jump trigger.
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition - 80));
        loaded.Samus.YPosition = actor.YPosition;
        for (int frame = 0; frame < 3; frame++)
        {
            loaded.Enemies.StepFrame(
                0, 0, false, loaded.Samus, level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
        }
        if (state.Function != NinjaSpacePirateFunction.Active)
            throw new InvalidDataException("Ninja Pirate did not enter active AI while priming.");
    }

    private static NinjaSpacePirateEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.NinjaSpacePirateStates[actor.SlotIndex] ??
        throw new InvalidDataException(
            $"Ninja Pirate slot {actor.SlotIndex} did not initialize typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y,
        ushort type = 0x0200,
        ushort variable = 0)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = 300;
        projectile.Direction = 2;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
        projectile.Variable = variable;
    }

    private static void VerifyWords(
        ISnesAddressSpace bus,
        int address,
        IReadOnlyList<ushort> expected)
    {
        for (int index = 0; index < expected.Count; index++)
        {
            ushort actual = ReadWord(bus, address + index * 2);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"ROM word ${address + index * 2:X6} was ${actual:X4}; expected " +
                    $"${expected[index]:X4}.");
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedNinjas(
        RoomEnemySystem Enemies,
        SamusState Samus,
        IReadOnlyList<RoomEnemySlot> Ninjas,
        SamusProjectileSystem Projectiles,
        SamusBombProjectileSystem SharedProjectiles);

    private readonly record struct MotionResult(int FunctionCount);
    private readonly record struct ClawResult(int SpawnedCount, int MapCount);
}
