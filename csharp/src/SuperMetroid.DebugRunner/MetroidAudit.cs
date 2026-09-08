using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for ordinary Metroid $DD7F in the pre-boss Tourian Metroid room. The
/// four untouched actors exercise finite sprite-object allocation while isolated reloads
/// keep contact, ice, missile, and power-bomb assertions independent.
/// </summary>
internal static partial class MetroidAudit
{
    private const ushort MetroidRoomHeader = 0xdae1;
    private const ushort MetroidRoomState = 0xdaf3;
    private const ushort MetroidPopulation = 0xe1d8;
    private const ushort MetroidDefinition = 0xdd7f;
    private const ushort RinkaDefinition = 0xd23f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, MetroidRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyDefinition(bus);

        LoadedMetroids loaded = Load(bus, room, assets);
        VerifyLoadAndAnimation(bus, room, assets, loaded);
        VerifyMovementAndAttachment(bus, room, assets);
        VerifySuitDrainCadence(bus, room, assets);
        VerifyIceMissilesAndDeath(bus, room, assets);
        VerifyPowerBombReaction(bus, room, assets);
        VerifyGrappleDamageRules(bus, room, assets);

        Console.WriteLine(
            "Ordinary Metroid audit passed: four retail actors and eight composited sprite " +
            "objects loaded; ROM animation, homing/closing/attachment/escape movement, " +
            "Power/Varia/Gravity drain, ice freezing, frozen missile damage, special drops, " +
            "hurt palette ownership, bomb-slot detachment, and power-bomb admission were verified.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, MetroidDefinition);
        if (definition.Bank != 0xa3 ||
            definition.InitializationAiPointer != 0xea4f ||
            definition.MainAiPointer != 0xeb98 ||
            definition.HurtAiPointer != 0xeb33 ||
            definition.FrozenAiPointer != 0xeae6 ||
            definition.TouchAiPointer != 0xedeb ||
            definition.ShotAiPointer != 0xef07 ||
            definition.PowerBombReactionPointer != 0xf042 ||
            definition.Health != 500 || definition.Damage != 120 ||
            definition.NamePointer != 0xdfab)
        {
            throw new InvalidDataException(
                "Metroid $DD7F header disagrees with the translated dispatcher: " +
                $"bank=${definition.Bank:X2}, init=${definition.InitializationAiPointer:X4}, " +
                $"main=${definition.MainAiPointer:X4}, hurt=${definition.HurtAiPointer:X4}, " +
                $"frozen=${definition.FrozenAiPointer:X4}, touch=${definition.TouchAiPointer:X4}, " +
                $"shot=${definition.ShotAiPointer:X4}, PB=${definition.PowerBombReactionPointer:X4}, " +
                $"health/damage={definition.Health}/{definition.Damage}.");
        }
    }

    private static void VerifyLoadAndAnimation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LoadedMetroids loaded)
    {
        if (room.State.Pointer != MetroidRoomState ||
            room.State.EnemyPopulationPointer != MetroidPopulation ||
            loaded.Enemies.EnemyCount != 8 ||
            loaded.Enemies.Slots.Take(4).Any(slot =>
                slot.EnemyDefinitionPointer != MetroidDefinition || slot.Parameter2 != 5) ||
            loaded.Enemies.Slots.Skip(4).Take(4).Any(slot =>
                slot.EnemyDefinitionPointer != RinkaDefinition) ||
            loaded.Enemies.MetroidStates.Take(4).Any(state => state is null))
        {
            throw new InvalidDataException(
                $"Metroid room selected state ${room.State.Pointer:X4}/population " +
                $"${room.State.EnemyPopulationPointer:X4} with an unexpected " +
                $"{loaded.Enemies.EnemyCount}-actor layout.");
        }

        RoomSpriteObjectSlot[] outerBodies = loaded.Enemies.RoomSpriteObjects
            .Where(sprite => sprite.IsActive && sprite.Kind is
                RoomSpriteObjectKind.MetroidOuterBodyA or
                RoomSpriteObjectKind.MetroidOuterBodyB)
            .ToArray();
        if (outerBodies.Length != 8 ||
            outerBodies.Count(sprite => sprite.Kind == RoomSpriteObjectKind.MetroidOuterBodyA) != 4 ||
            outerBodies.Count(sprite => sprite.Kind == RoomSpriteObjectKind.MetroidOuterBodyB) != 4)
        {
            throw new InvalidDataException(
                $"Four retail Metroids allocated {outerBodies.Length} outer sprite objects, expected eight.");
        }

        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        MetroidEnemyState state = RequireState(loaded.Enemies, actor);
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 80; frame++)
        {
            StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
            maps.Add(actor.SpritemapPointer);
            if (state.OuterBodyA.XPosition != actor.XPosition ||
                state.OuterBodyA.YPosition != actor.YPosition ||
                state.OuterBodyB.XPosition != actor.XPosition ||
                state.OuterBodyB.YPosition != actor.YPosition)
            {
                throw new InvalidDataException("Metroid outer sprite objects stopped following the body.");
            }
        }
        if (maps.Count < 2)
            throw new InvalidDataException($"Metroid idle list produced only {maps.Count} ROM map(s).");

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(
            oam,
            CameraFor(room, actor),
            0);
        loaded.Enemies.DrawLayers(
            oam,
            CameraFor(room, actor),
            0,
            0,
            7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 3)
        {
            throw new InvalidDataException(
                $"Metroid composite emitted only {oam.LastFinalizedSpriteCount} OBJ pieces.");
        }
    }

    private static void VerifyMovementAndAttachment(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedMetroids loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        MetroidEnemyState state = RequireState(loaded.Enemies, actor);
        loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 12));
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 8));
        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            state.Function != MetroidAiFunction.ClosingOnSamus)
        {
            throw new InvalidDataException(
                $"Metroid near-contact selected {state.Function}, expected ClosingOnSamus.");
        }

        ushort closingX = actor.XPosition;
        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        if (actor.XPosition == closingX || Math.Abs(state.XVelocity) != 3)
        {
            throw new InvalidDataException(
                $"Metroid fixed-speed closing failed: X={closingX}->{actor.XPosition}, " +
                $"velocity={state.XVelocity}:{state.XSubvelocity:X4}.");
        }

        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 8));
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            state.Function != MetroidAiFunction.AttachedToSamus ||
            loaded.Samus.SpecialSuperPaletteFlags != 1)
        {
            throw new InvalidDataException(
                $"Metroid attachment failed: function={state.Function}, " +
                $"palette=${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }

        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        if (actor.XPosition != loaded.Samus.XPosition ||
            actor.YPosition != unchecked((ushort)(loaded.Samus.YPosition - 8)))
        {
            throw new InvalidDataException("Attached Metroid did not lock to Samus's $Y-8 origin.");
        }

        var cgram = new SnesCgram();
        if (!SamusSpecialSuperPalette.Update(bus, cgram, loaded.Samus) ||
            loaded.Samus.SpecialSuperPaletteFlags != 2 ||
            !SamusSpecialSuperPalette.Update(bus, cgram, loaded.Samus) ||
            loaded.Samus.SpecialSuperPaletteFlags != 3)
        {
            throw new InvalidDataException(
                $"Metroid special palette did not alternate/increment: " +
                $"${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }

        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        SamusBombProjectileSlot bomb = shared.Slots[0];
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 100;
        bomb.XPosition = actor.XPosition;
        bomb.YPosition = actor.YPosition;
        bomb.XRadius = 16;
        bomb.YRadius = 16;
        bomb.BombTimer = 0;
        if (loaded.Enemies.ResolveMetroidBombHits(
                shared,
                projectiles,
                loaded.Samus) != 1 ||
            state.Function != MetroidAiFunction.PowerBombEscape ||
            state.EscapeTimer != 4 || loaded.Samus.SpecialSuperPaletteFlags != 0 ||
            (bomb.Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Attached Metroid power-bomb detach failed: function={state.Function}, " +
                $"timer={state.EscapeTimer}, direction=${bomb.Direction:X4}, " +
                $"palette=${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }

        for (int frame = 0; frame < 4; frame++)
            StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        if (state.Function != MetroidAiFunction.Homing)
            throw new InvalidDataException($"Metroid escape ended in {state.Function}, expected Homing.");
    }

    private static void VerifySuitDrainCadence(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        VerifyDrain(bus, room, assets, equippedItems: 0, expectedDamage: 12, "Power Suit");
        VerifyDrain(bus, room, assets, equippedItems: 1, expectedDamage: 6, "Varia Suit");
        VerifyDrain(bus, room, assets, equippedItems: 0x20, expectedDamage: 3, "Gravity Suit");
    }

    private static void VerifyDrain(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort equippedItems,
        int expectedDamage,
        string suitName)
    {
        LoadedMetroids loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        loaded.Samus.EquippedItems = equippedItems;
        loaded.Samus.Health = 999;
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 8));
        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        for (int contact = 0; contact < 16; contact++)
        {
            if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0))
                throw new InvalidDataException($"{suitName} Metroid contact {contact} missed.");
        }

        int actualDamage = 999 - loaded.Samus.Health;
        if (actualDamage != expectedDamage || loaded.Samus.InvincibilityTimer != 0 ||
            loaded.Samus.KnockbackTimer != 0)
        {
            throw new InvalidDataException(
                $"{suitName} Metroid drain dealt {actualDamage}, expected {expectedDamage}; " +
                $"invincibility/knockback={loaded.Samus.InvincibilityTimer}/" +
                $"{loaded.Samus.KnockbackTimer}.");
        }
    }

    private static void VerifyIceMissilesAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedMetroids loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        MetroidEnemyState state = RequireState(loaded.Enemies, actor);
        // Common frozen AI checks the live equipment word after every timer decrement.
        // This synthetic projectile sequence must retain the Ice bit just as the real
        // bank-$90 producer does after firing an Ice beam.
        loaded.Samus.EquippedBeams = (ushort)SamusBeamFlags.Ice;
        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();

        for (int iceHit = 0; iceHit < 3; iceHit++)
        {
            ArmProjectile(projectiles.Slots[0], actor, 0x0002, 2);
            if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                    bus,
                    projectiles,
                    shared,
                    loaded.Samus) != 1)
            {
                throw new InvalidDataException($"Metroid ice hit {iceHit} did not resolve.");
            }
        }
        if (actor.Health != 500 || actor.Parameter2 != 0 || actor.FrozenTimer != 400 ||
            (actor.AiHandlerBits & 4) == 0 ||
            loaded.Enemies.LastMetroidSoundEffectLibrary3 != 0x000a)
        {
            throw new InvalidDataException(
                $"Metroid ice accumulation failed: health={actor.Health}, parameter2={actor.Parameter2}, " +
                $"frozen={actor.FrozenTimer}, AI=${actor.AiHandlerBits:X4}, " +
                $"sound={loaded.Enemies.LastMetroidSoundEffectLibrary3:X4}.");
        }

        StepCentered(loaded.Enemies, assets, room, loaded.Samus, actor);
        if (state.OuterBodyA.DisableFlags != 1 || state.OuterBodyB.DisableFlags != 1 ||
            state.OuterBodyA.GraphicsIndex != 0x0c00 ||
            state.OuterBodyB.GraphicsIndex != 0x0c00 ||
            state.OuterBodyA.InstructionPointer != 0xc3ba ||
            state.OuterBodyB.InstructionPointer != 0xc4b6)
        {
            throw new InvalidDataException("Metroid frozen AI did not replace/disable both outer layers.");
        }

        ushort healthBeforeBeam = actor.Health;
        ArmProjectile(projectiles.Slots[0], actor, 0x0000, 1000);
        loaded.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, loaded.Samus);
        if (actor.Health != healthBeforeBeam)
            throw new InvalidDataException("Frozen Metroid incorrectly accepted beam health damage.");

        RoomEnemyDefinition definition = actor.Definition;
        byte missileVulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(definition.VulnerabilityPointer + 12)));
        int expectedDamage = (100 >> 1) * (missileVulnerability & 0x7f);
        ArmProjectile(projectiles.Slots[0], actor, 0x0100, 100);
        loaded.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, loaded.Samus);
        if (actor.Health != Math.Max(0, healthBeforeBeam - expectedDamage))
        {
            throw new InvalidDataException(
                $"Frozen Metroid missile damage was {healthBeforeBeam - actor.Health}, " +
                $"expected {expectedDamage} from vulnerability ${missileVulnerability:X2}.");
        }

        actor.Health = 1;
        actor.InvincibilityTimer = 0;
        actor.FlashTimer = 0;
        actor.AiHandlerBits = 4;
        actor.FrozenTimer = 200;
        // Native death clears the enemy slot. Drop origins must be compared with
        // the last live coordinates, not the cleared X/Y words afterward.
        ushort deathX = actor.XPosition, deathY = actor.YPosition;
        ArmProjectile(projectiles.Slots[0], actor, 0x0200, 1000);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                shared,
                loaded.Samus) != 1 ||
            actor.Health != 0 || actor.EnemyDefinitionPointer != 0 || loaded.Enemies.EnemiesKilled != 1 ||
            state.OuterBodyA.IsActive || state.OuterBodyB.IsActive ||
            loaded.Enemies.MetroidDropRequests.Count != 5 ||
            loaded.Samus.SpecialSuperPaletteFlags != 0)
        {
            throw new InvalidDataException(
                $"Frozen Metroid death failed: health={actor.Health}, properties=${actor.Properties:X4}, " +
                $"outer={state.OuterBodyA.IsActive}/{state.OuterBodyB.IsActive}, " +
                $"drops={loaded.Enemies.MetroidDropRequests.Count}, " +
                $"palette=${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }
        if (loaded.Enemies.MetroidDropRequests.Any(drop =>
                Math.Abs(unchecked((short)(drop.XPosition - deathX))) > 16 ||
                Math.Abs(unchecked((short)(drop.YPosition - deathY))) > 16 ||
                drop.EnemyDefinitionPointer != MetroidDefinition ||
                drop.SourceSpriteObjectIndex != state.OuterBodyB.NativeIndex))
        {
            throw new InvalidDataException("Metroid's five RNG drop requests left the native scatter contract.");
        }
    }

    private static void VerifyPowerBombReaction(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedMetroids loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        MetroidEnemyState state = RequireState(loaded.Enemies, actor);
        actor.Health = 1;
        loaded.Samus.SpecialSuperPaletteFlags = 1;
        byte powerBombVulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(actor.Definition.VulnerabilityPointer + 15)));
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 64,
            loaded.Samus);
        if ((powerBombVulnerability & 0x7f) == 0)
        {
            // Retail Metroid vulnerability rejects the expansion-radius pass before its
            // installed callback. Detachment instead comes from the overlapping family-$05
            // bomb actor audited above. Keep the otherwise-real callback translated for
            // debug-edited tables, but prove the shipped admission gate here.
            if (reactions != 0 || actor.Health != 1 ||
                !state.OuterBodyA.IsActive || !state.OuterBodyB.IsActive ||
                loaded.Samus.SpecialSuperPaletteFlags != 1)
            {
                throw new InvalidDataException(
                    $"Power-bomb-immune Metroid changed state: reactions={reactions}, " +
                    $"health={actor.Health}, outer={state.OuterBodyA.IsActive}/" +
                    $"{state.OuterBodyB.IsActive}, palette=" +
                    $"${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
            }
            return;
        }

        if (reactions == 0 || actor.Health != 0 ||
            actor.EnemyDefinitionPointer != 0 || loaded.Enemies.EnemiesKilled != 1 ||
            state.OuterBodyA.IsActive || state.OuterBodyB.IsActive ||
            loaded.Samus.SpecialSuperPaletteFlags != 0)
        {
            throw new InvalidDataException(
                $"Metroid power-bomb reaction failed for vulnerability " +
                $"${powerBombVulnerability:X2}: reactions={reactions}, health={actor.Health}, " +
                $"properties=${actor.Properties:X4}, outer={state.OuterBodyA.IsActive}/" +
                $"{state.OuterBodyB.IsActive}, palette=${loaded.Samus.SpecialSuperPaletteFlags:X4}.");
        }
    }

    private static LoadedMetroids Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0100,
            YPosition = 0x0080,
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
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedMetroids(enemies, samus);
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot actor)
    {
        enemies.StepFrame(
            CameraFor(room, actor),
            0,
            false,
            samus,
            level: assets.LevelData);
    }

    private static ushort CameraFor(CartridgeRoomHeader room, RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        return unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX));
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static MetroidEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.MetroidStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Metroid slot {slot.SlotIndex} has no typed state.");

    private readonly record struct LoadedMetroids(RoomEnemySystem Enemies, SamusState Samus);
}
