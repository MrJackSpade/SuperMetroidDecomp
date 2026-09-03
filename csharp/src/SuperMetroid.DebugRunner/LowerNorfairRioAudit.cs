using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the six Lower Norfair Rio pairs at the start of room $8F:B482.
/// Three unrelated records follow them, so a read-only decorator writes only the native
/// terminator after the unchanged twelve-record prefix. Code, lists, graphics, and terrain
/// remain cartridge data.
/// </summary>
internal static class LowerNorfairRioAudit
{
    private const ushort DefinitionPointer = 0xd33f;
    private const ushort RoomPointer = 0xb482;
    private const ushort ExpectedStatePointer = 0xb48f;
    private const ushort ExpectedPopulationPointer = 0xaa8d;
    private const ushort AuditedParentX = 0x0068;
    private const ushort CameraX = 0x0000;
    private const ushort CameraY = 0x0000;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(retailBus);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            retailBus,
            RoomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                BossBits: 0,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(retailBus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer ||
            room.State.EnemyTilesetPointer != 0x885f ||
            room.WidthInScreens != 3 || room.HeightInScreens != 1)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population/set=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}.");
        }

        VerifyAttackCycle(retailBus, room, assets);
        VerifyMirroredLaunch(retailBus, room, assets);
        VerifyFreezeMirroring(retailBus, room, assets);
        VerifyContactAndWeaponDamage(retailBus, room, assets);
        Console.WriteLine(
            "Lower Norfair Rio audit passed: six retail pairs load, idle/takeoff/dive/" +
            "return/landing animations run, movement collides with terrain, the follower " +
            "tracks ROM visibility, mirrored attacks, freeze, contact damage, beam death, " +
            "paired cleanup, and power-bomb immunity agree.");
        return 0;
    }

    private static void VerifyAttackCycle(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        RoomEnemySlot parent = GetAuditedParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        LowerNorfairRioEnemyState parentState = GetState(loaded.Enemies, parent);
        LowerNorfairRioEnemyState followerState = GetState(loaded.Enemies, follower);
        if (loaded.Enemies.EnemyCount != 12 ||
            loaded.Enemies.Slots.Take(12).Any(slot => slot.EnemyDefinitionPointer != DefinitionPointer) ||
            parent.YPosition != 0x0058 || follower.XPosition != AuditedParentX ||
            follower.YPosition != 0x0058 || parent.Health != 900 || follower.Health != 900 ||
            parent.CurrentInstruction != 0xc61a || follower.CurrentInstruction != 0xc6b0 ||
            parentState.Function != LowerNorfairRioEnemyFunction.WaitForAttackOpportunity ||
            followerState.Function != LowerNorfairRioEnemyFunction.FollowParent ||
            parentState.InstalledInstructionList != 0xc61a ||
            followerState.InstalledInstructionList != 0xc6b0 ||
            parentState.AnimationSignal || parentState.FollowerVisible ||
            parentState.IsFollower || !followerState.IsFollower)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"parent=({parent.XPosition:X4},{parent.YPosition:X4})/" +
                $"${parent.CurrentInstruction:X4}/${(ushort)parentState.Function:X4}, " +
                $"follower=({follower.XPosition:X4},{follower.YPosition:X4})/" +
                $"${follower.CurrentInstruction:X4}/${(ushort)followerState.Function:X4}.");
        }

        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!follower.Properties.HasAny(EnemyProperties.Invisible))
            throw new InvalidDataException("Lower Norfair Rio follower remained visible while its flag was clear.");

        loaded.Samus.XPosition = unchecked((ushort)(parent.XPosition + 32));
        loaded.Samus.YPosition = unchecked((ushort)(parent.YPosition + 96));
        ushort originX = parent.XPosition;
        ushort originY = parent.YPosition;
        var parentMaps = new HashSet<ushort>();
        var followerMaps = new HashSet<ushort>();
        bool sawTakeoff = false;
        bool sawDive = false;
        bool sawMovement = false;
        bool sawReturn = false;
        bool sawReturnSound = false;
        bool sawLateReturn = false;
        bool sawFollowerVisible = false;
        bool sawFollowerHiddenAgain = false;
        bool sawLandingWait = false;
        bool sawIdleAgain = false;
        for (int frame = 0; frame < 1800; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
            parentMaps.Add(parent.SpritemapPointer);
            followerMaps.Add(follower.SpritemapPointer);
            sawTakeoff |= parentState.InstalledInstructionList == 0xc630;
            sawDive |= parentState.Function == LowerNorfairRioEnemyFunction.Dive &&
                parentState.InstalledInstructionList == 0xc65a;
            sawMovement |= parent.XPosition != originX || parent.YPosition != originY;
            sawReturn |= parentState.Function == LowerNorfairRioEnemyFunction.ReturnToPerch &&
                parentState.InstalledInstructionList == 0xc662;
            sawReturnSound |= loaded.Enemies.LastLowerNorfairRioSoundEffect == 0x0064;
            sawLateReturn |= parentState.InstalledInstructionList == 0xc674;
            if (!follower.Properties.HasAny(EnemyProperties.Invisible))
            {
                if (follower.XPosition != parent.XPosition ||
                    follower.YPosition != unchecked((ushort)(parent.YPosition + 12)))
                {
                    throw new InvalidDataException(
                        $"Lower Norfair Rio follower position mismatch: parent=" +
                        $"({parent.XPosition:X4},{parent.YPosition:X4}), follower=" +
                        $"({follower.XPosition:X4},{follower.YPosition:X4}).");
                }
                sawFollowerVisible = true;
            }
            sawFollowerHiddenAgain |= sawFollowerVisible && !parentState.FollowerVisible &&
                follower.Properties.HasAny(EnemyProperties.Invisible);
            if (parentState.Function == LowerNorfairRioEnemyFunction.WaitForLandingAnimation &&
                parentState.InstalledInstructionList == 0xc686)
            {
                sawLandingWait = true;
                loaded.Samus.XPosition = 0x1000;
            }
            sawIdleAgain |= sawLandingWait &&
                parentState.Function == LowerNorfairRioEnemyFunction.WaitForAttackOpportunity &&
                parentState.InstalledInstructionList == 0xc61a;
            if (sawIdleAgain)
                break;
        }

        if (!sawTakeoff || !sawDive || !sawMovement || !sawReturn || !sawReturnSound ||
            !sawLateReturn || !sawFollowerVisible || !sawFollowerHiddenAgain ||
            !sawLandingWait || !sawIdleAgain || parentMaps.Count < 4 || followerMaps.Count < 2)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio cycle mismatch: takeoff/dive/move/return/sound/late/" +
                $"visible/hidden/land/idle={sawTakeoff}/{sawDive}/{sawMovement}/" +
                $"{sawReturn}/{sawReturnSound}/{sawLateReturn}/{sawFollowerVisible}/" +
                $"{sawFollowerHiddenAgain}/{sawLandingWait}/{sawIdleAgain}, maps=" +
                $"{parentMaps.Count}/{followerMaps.Count}, position=" +
                $"({parent.XPosition:X4},{parent.YPosition:X4}), velocity=" +
                $"({parentState.XVelocity:X4},{parentState.YVelocity:X4}), function/list=" +
                $"${(ushort)parentState.Function:X4}/${parentState.InstalledInstructionList:X4}.");
        }
    }

    private static void VerifyMirroredLaunch(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        ushort sourceXVelocity = ReadWord(retailBus, 0xa2c6ce);
        LoadedRoom right = LoadRoom(retailBus, room, assets);
        RoomEnemySlot rightParent = GetAuditedParent(right.Enemies);
        right.Samus.XPosition = unchecked((ushort)(rightParent.XPosition + 32));
        StepUntilTakeoff(right, assets.LevelData);
        ushort rightVelocity = GetState(right.Enemies, rightParent).XVelocity;

        LoadedRoom left = LoadRoom(retailBus, room, assets);
        RoomEnemySlot leftParent = GetAuditedParent(left.Enemies);
        left.Samus.XPosition = unchecked((ushort)(leftParent.XPosition - 32));
        StepUntilTakeoff(left, assets.LevelData);
        ushort leftVelocity = GetState(left.Enemies, leftParent).XVelocity;
        if (rightVelocity != sourceXVelocity ||
            leftVelocity != unchecked((ushort)-(short)sourceXVelocity))
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio launch mirror mismatch: ROM=${sourceXVelocity:X4}, " +
                $"right/left=${rightVelocity:X4}/${leftVelocity:X4}.");
        }
    }

    private static void StepUntilTakeoff(LoadedRoom loaded, RoomLevelData level)
    {
        RoomEnemySlot parent = GetAuditedParent(loaded.Enemies);
        LowerNorfairRioEnemyState state = GetState(loaded.Enemies, parent);
        for (int frame = 0; frame < 256; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: level);
            if (state.Function == LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation)
                return;
        }
        throw new InvalidDataException("Lower Norfair Rio did not pass its random takeoff gate.");
    }

    private static void VerifyFreezeMirroring(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        RoomEnemySlot parent = GetAuditedParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        loaded.Samus.XPosition = unchecked((ushort)(parent.XPosition + 32));
        StepUntilTakeoff(loaded, assets.LevelData);
        parent.FrozenTimer = 10;
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (parent.FrozenTimer != 9 || follower.FrozenTimer != 9 ||
            !follower.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio freeze mirror mismatch: parent/follower=" +
                $"{parent.FrozenTimer}/{follower.FrozenTimer}, hidden=" +
                $"{follower.Properties.HasAny(EnemyProperties.Invisible)}.");
        }
    }

    private static void VerifyContactAndWeaponDamage(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        RoomEnemySlot parent = GetAuditedParent(loaded.Enemies);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.XPosition = parent.XPosition;
        loaded.Samus.YPosition = parent.YPosition;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, controllerInput: 0) ||
            loaded.Samus.Health != 879 || !loaded.Samus.KnockbackActive || parent.Health != 900)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio contact mismatch: Samus health={loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, parent health={parent.Health}.");
        }

        loaded = LoadRoom(retailBus, room, assets);
        parent = GetAuditedParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmLethalBeam(shots.Slots[0], parent);
        int beamHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            retailBus,
            shots,
            bombs,
            loaded.Samus);
        if (beamHits != 1 || parent.Health != 0 ||
            !parent.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio beam death mismatch: hits={beamHits}, health={parent.Health}, " +
                $"deleted={parent.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!follower.Properties.HasAny(EnemyProperties.Deleted))
            throw new InvalidDataException("Lower Norfair Rio follower survived its dead parent.");

        loaded = LoadRoom(retailBus, room, assets);
        parent = GetAuditedParent(loaded.Enemies);
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            retailBus,
            parent.XPosition,
            parent.YPosition,
            explosionRadius: 32);
        if (reactions != 0 || parent.Health != 900 || parent.InvincibilityTimer != 0 ||
            parent.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio power-bomb immunity mismatch: reactions={reactions}, " +
                $"health={parent.Health}, invincibility={parent.InvincibilityTimer}, " +
                $"deleted={parent.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static LoadedRoom LoadRoom(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            retailBus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 12,
            deathQuota: 6);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x1000,
            YPosition = 0x00b0,
        };
        samus.RefreshCollisionRadii(retailBus);
        samus.InitializeAnimation(retailBus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: CameraX,
            cameraY: CameraY);
        return new LoadedRoom(enemies, samus);
    }

    private static RoomEnemySlot GetAuditedParent(RoomEnemySystem enemies) =>
        enemies.Slots
            .Take(enemies.EnemyCount)
            .Single(slot => slot.EnemyDefinitionPointer == DefinitionPointer &&
                slot.XPosition == AuditedParentX && (slot.Parameter1 & 0x8000) == 0);

    private static LowerNorfairRioEnemyState GetState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.LowerNorfairRioStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Retail Lower Norfair Rio slot {slot.SlotIndex} did not receive typed state.");

    private static void ArmLethalBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)SamusProjectileFamily.Beam;
        projectile.Damage = 2000;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0800 || definition.PalettePointer != 0xc5fa ||
            definition.Health != 900 || definition.Damage != 120 ||
            definition.XRadius != 16 || definition.YRadius != 10 ||
            definition.Bank != 0xa2 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x005f || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xc6f3 || definition.PartCount != 2 ||
            definition.MainAiPointer != 0xc724 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 2 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaed920 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf356 ||
            definition.VulnerabilityPointer != 0xee58 || definition.NamePointer != 0xded9)
        {
            throw new InvalidDataException(
                "Retail Lower Norfair Rio header words do not match $A0:D33F.");
        }
    }

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedRoom(RoomEnemySystem Enemies, SamusState Samus);
}
