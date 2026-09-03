using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed end-to-end regression for Green Brinstar Beetoms. The unchanged room is an
/// unusually clean fixture: all four population records and the sole graphics-set entry are
/// Beetom, so no synthetic definition, animation, terrain, or vulnerability data is needed.
/// </summary>
internal static class BeetomAudit
{
    private const ushort GreenBrinstarBeetomsRoom = 0x9fe5;
    private const ushort BeetomDefinition = 0xe87f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, GreenBrinstarBeetomsRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        SamusState samus = CreateSamus(bus, 0x0100, 0x00b8);
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
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            controllerInput: 0x0080);

        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        ushort[] expectedX = [0x0050, 0x0070, 0x0090, 0x00b0];
        if (room.State.Pointer != 0x9ff2 ||
            room.State.EnemyPopulationPointer != 0x9735 ||
            room.State.EnemyTilesetPointer != 0x84d7 ||
            enemies.EnemyCount != 4 ||
            population.Any(slot => slot.EnemyDefinitionPointer != BeetomDefinition))
        {
            throw new InvalidDataException(
                $"Green Brinstar Beetoms load failed: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count={enemies.EnemyCount}.");
        }

        for (int index = 0; index < population.Length; index++)
        {
            RoomEnemySlot slot = population[index];
            BeetomEnemyState state = RequireState(enemies, slot);
            if (slot.XPosition != expectedX[index] || slot.YPosition != 0x00b8 ||
                slot.Properties != 0x2000 || slot.Health != 60 ||
                slot.Definition.Damage != 10 || slot.XRadius != 8 || slot.YRadius != 8 ||
                slot.Layer != 5 || slot.Definition.Bank != 0xa8 ||
                slot.Definition.InitializationAiPointer != 0xb776 ||
                slot.Definition.MainAiPointer != 0xb80d ||
                slot.Definition.TouchAiPointer != 0xbe2e ||
                slot.Definition.ShotAiPointer != 0xbeac ||
                state.Function != BeetomEnemyFunction.DecideAction ||
                state.ButtonCounter != 64 || state.PreviousController1Input != 0x0080 ||
                state.Falling || state.AttachedToSamus ||
                state.InstalledInstructionList != 0xb6f2)
            {
                throw new InvalidDataException(
                    $"Beetom {index} initialization failed: position=" +
                    $"({slot.XPosition:X4},{slot.YPosition:X4}), properties=" +
                    $"${slot.Properties:X4}, health/damage={slot.Health}/" +
                    $"{slot.Definition.Damage}, radii={slot.XRadius}/{slot.YRadius}, " +
                    $"layer={slot.Layer}, function={state.Function}, list=" +
                    $"${state.InstalledInstructionList:X4}, input=" +
                    $"${state.PreviousController1Input:X4}.");
            }
        }
        if (random.RandomNumber != 0x0017)
            throw new InvalidDataException($"Beetom initialization left RNG ${random.RandomNumber:X4}, expected $0017.");

        RoomEnemySlot actor = population[0];
        BeetomEnemyState actorState = RequireState(enemies, actor);
        VerifyNaturalMovement(enemies, assets, room, samus, actor, actorState);
        VerifyLatchDrainAndEscape(enemies, assets, room, samus, actor, actorState);
        VerifyShotDetach(bus, room, assets);

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Green Brinstar Beetoms emitted no ROM-authored OBJ.");

        Console.WriteLine(
            $"Beetom audit passed: unchanged Green Brinstar loaded four actors; ROM-derived " +
            $"jump indexes {actorState.InitialShortLeapYSpeedIndex}/" +
            $"{actorState.InitialLongLeapYSpeedIndex}/{actorState.InitialLungeYSpeedIndex}, " +
            $"natural crawl/hop/lunge movement, latch/drain, 64-change escape, freeze detach, " +
            $"ordinary damage, and {oam.LastFinalizedSpriteCount} OBJ pieces were verified.");
        return 0;
    }

    private static void VerifyNaturalMovement(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot actor,
        BeetomEnemyState state)
    {
        var functions = new HashSet<BeetomEnemyFunction>();
        var maps = new HashSet<ushort>();
        ushort startX = actor.XPosition;
        ushort minimumY = actor.YPosition;
        samus.XPosition = 0x0200;
        for (int frame = 0; frame < 1_024; frame++)
        {
            StepCentered(enemies, assets, room, samus, actor, controllerInput: 0);
            functions.Add(state.Function);
            if (actor.SpritemapPointer != 0)
                maps.Add(actor.SpritemapPointer);
            minimumY = Math.Min(minimumY, actor.YPosition);
        }

        bool sawHop = functions.Any(function => function is
            BeetomEnemyFunction.ShortHopLeft or BeetomEnemyFunction.ShortHopRight or
            BeetomEnemyFunction.LongHopLeft or BeetomEnemyFunction.LongHopRight);
        bool sawCrawl = functions.Any(function => function is
            BeetomEnemyFunction.CrawlingLeft or BeetomEnemyFunction.CrawlingRight);
        if (!sawHop || !sawCrawl || actor.XPosition == startX ||
            minimumY >= 0x00b8 || maps.Count < 5)
        {
            throw new InvalidDataException(
                $"Beetom natural movement failed: hop/crawl=" +
                $"{sawHop}/{sawCrawl}, X=${startX:X4}->${actor.XPosition:X4}, " +
                $"minimumY=${minimumY:X4}, maps={maps.Count}, states=" +
                $"{string.Join(',', functions)}.");
        }

        // Move only Samus and let the live state machine finish its current action. Its next
        // decision must select the cartridge's proximity lunge; no enemy function is injected.
        bool sawLunge = false;
        for (int frame = 0; frame < 512 && !sawLunge; frame++)
        {
            samus.XPosition = unchecked((ushort)(actor.XPosition + 48));
            samus.YPosition = actor.YPosition;
            StepCentered(enemies, assets, room, samus, actor, controllerInput: 0);
            sawLunge = state.Function is
                BeetomEnemyFunction.LungeLeft or BeetomEnemyFunction.LungeRight;
        }
        if (!sawLunge)
            throw new InvalidDataException($"Beetom never selected a natural proximity lunge; function={state.Function}.");
    }

    private static void VerifyLatchDrainAndEscape(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot actor,
        BeetomEnemyState state)
    {
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        StepCentered(enemies, assets, room, samus, actor, controllerInput: 0);
        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || !state.AttachedToSamus ||
            state.ButtonCounter != 64 || actor.Layer != 2)
        {
            throw new InvalidDataException(
                $"Beetom latch failed: attached={state.AttachedToSamus}, buttons=" +
                $"{state.ButtonCounter}, layer={actor.Layer}, function={state.Function}.");
        }

        ushort startingHealth = samus.Health;
        bool heardDrain = false;
        // The first post-touch frame installs the drain animation/function; the active
        // drain routine begins comparing input on the following frame.
        for (int frame = 0; frame < 65; frame++)
        {
            ushort input = (frame & 1) == 0 ? (ushort)1 : (ushort)2;
            StepCentered(enemies, assets, room, samus, actor, input);
            enemies.ResolveOrdinarySamusContact(samus, input);
            heardDrain |= enemies.LastBeetomSoundEffect == 0x002d;
        }
        if (state.ButtonCounter != 0 || samus.Health != startingHealth - 10 ||
            samus.InvincibilityTimer != 0 || samus.KnockbackTimer != 0 || !heardDrain)
        {
            throw new InvalidDataException(
                $"Beetom drain/escape counter failed: buttons={state.ButtonCounter}, " +
                $"health={startingHealth}->{samus.Health}, invincibility=" +
                $"{samus.InvincibilityTimer}, knockback={samus.KnockbackTimer}, sound={heardDrain}.");
        }

        StepCentered(enemies, assets, room, samus, actor, controllerInput: 2);
        if (state.AttachedToSamus || state.Function != BeetomEnemyFunction.StartBeingFlung)
        {
            throw new InvalidDataException(
                $"Beetom escape failed: attached={state.AttachedToSamus}, function={state.Function}.");
        }
    }

    private static void VerifyShotDetach(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        SamusState samus = CreateSamus(bus, 0x0050, 0x00b8);
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
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot actor = enemies.Slots[0];
        BeetomEnemyState state = RequireState(enemies, actor);
        StepCentered(enemies, assets, room, samus, actor, 0);
        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        enemies.ResolveOrdinarySamusContact(samus, 0);
        StepCentered(enemies, assets, room, samus, actor, 0);
        if (state.Function is not (BeetomEnemyFunction.DrainingLeft or BeetomEnemyFunction.DrainingRight))
            throw new InvalidDataException($"Beetom did not enter active drain before shot audit: {state.Function}.");

        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actor, projectileType: 0x0002, damage: 20);
        ushort startingHealth = actor.Health;
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus) != 1 ||
            state.AttachedToSamus || actor.FrozenTimer == 0 ||
            state.Function != BeetomEnemyFunction.StartDropping)
        {
            throw new InvalidDataException(
                $"Beetom freeze detach failed: health={startingHealth}->{actor.Health}, " +
                $"frozen={actor.FrozenTimer}, attached={state.AttachedToSamus}, " +
                $"function={state.Function}.");
        }


        // The remaining unchanged population records exercise ordinary nonlethal and lethal
        // common-shot paths through Beetom's private post-handler, proving the custom tail
        // does not replace normal vulnerability damage or deletion.
        RoomEnemySlot damageTarget = enemies.Slots[1];
        StepCentered(enemies, assets, room, samus, damageTarget, 0);
        projectiles = new SamusProjectileSystem();
        shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], damageTarget, projectileType: 0x0100, damage: 20);
        ushort damageTargetHealth = damageTarget.Health;
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus) != 1 ||
            damageTarget.Health >= damageTargetHealth || damageTarget.Health == 0 ||
            damageTarget.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Beetom nonlethal shot failed: health={damageTargetHealth}->" +
                $"{damageTarget.Health}, flash={damageTarget.FlashTimer}.");
        }

        RoomEnemySlot lethalTarget = enemies.Slots[2];
        StepCentered(enemies, assets, room, samus, lethalTarget, 0);
        projectiles = new SamusProjectileSystem();
        shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], lethalTarget, projectileType: 0x0200, damage: 1000);
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus) != 1 ||
            lethalTarget.Health != 0 ||
            !lethalTarget.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Beetom lethal shot failed: health={lethalTarget.Health}, " +
                $"properties=${lethalTarget.Properties:X4}.");
        }
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot target,
        ushort controllerInput)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        ushort cameraX = unchecked((ushort)Math.Clamp(target.XPosition - 128, 0, maximumX));
        enemies.StepFrame(
            cameraX,
            0,
            false,
            samus,
            level: assets.LevelData,
            controllerInput: controllerInput);
    }

    private static SamusState CreateSamus(SuperMetroidAddressSpace bus, int x, int y)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = unchecked((ushort)x),
            YPosition = unchecked((ushort)y),
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
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

    private static BeetomEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.BeetomStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Beetom slot {slot.SlotIndex} has no typed state.");
}
