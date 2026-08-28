using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Norfair room $8F:A788. Its complete ten-record population has
/// three Norfair Rio parent/follower pairs and four already-translated Sova crawlers, so no
/// population decorator or synthetic actor placement is needed.
/// </summary>
internal static class NorfairRioAudit
{
    private const ushort DefinitionPointer = 0xd2ff;
    private const ushort RoomPointer = 0xa788;
    private const ushort ExpectedStatePointer = 0xa795;
    private const ushort ExpectedPopulationPointer = 0xb544;
    private const ushort CameraX = 0x0000;
    private const ushort CameraY = 0x0000;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(bus);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                BossBits: 0,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer ||
            room.State.EnemyTilesetPointer != 0x8a25 ||
            room.WidthInScreens != 3 || room.HeightInScreens != 2)
        {
            throw new InvalidDataException(
                $"Norfair Rio room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population/set=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}.");
        }

        VerifyAttackCycle(bus, room, assets);
        VerifyMirroredLaunch(bus, room, assets);
        VerifyFreezeMirroring(bus, room, assets);
        VerifyContactAndWeaponDamage(bus, room, assets);
        Console.WriteLine(
            "Norfair Rio untouched-room audit passed: all retail actors load, idle/takeoff/" +
            "dive/return animations run, movement collides with terrain, the follower tracks " +
            "parent animation offsets, mirrored attacks and freeze agree, contact damage, " +
            "beam death, follower cleanup, and power-bomb immunity agree.");
        return 0;
    }

    private static void VerifyAttackCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot parent = GetFirstParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        NorfairRioEnemyState parentState = GetState(loaded.Enemies, parent);
        NorfairRioEnemyState followerState = GetState(loaded.Enemies, follower);
        int norfairRioCount = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Count(slot => slot.EnemyDefinitionPointer == DefinitionPointer);
        if (loaded.Enemies.EnemyCount != 10 || norfairRioCount != 6 ||
            parent.XPosition != 0x00a1 || parent.YPosition != 0x0053 ||
            follower.XPosition != 0x00a1 || follower.YPosition != 0x0053 ||
            parent.Health != 120 || follower.Health != 120 ||
            parent.CurrentInstruction != 0xc0f1 || follower.CurrentInstruction != 0xc18f ||
            parentState.Function != NorfairRioEnemyFunction.WaitForAttackOpportunity ||
            followerState.Function != NorfairRioEnemyFunction.FollowParent ||
            parentState.InstalledInstructionList != 0xc0f1 ||
            followerState.InstalledInstructionList != 0xc18f ||
            parentState.AnimationSignal || parentState.FollowerYOffset != 0 ||
            parentState.IsFollower || !followerState.IsFollower)
        {
            throw new InvalidDataException(
                $"Norfair Rio initialization mismatch: count={loaded.Enemies.EnemyCount}/" +
                $"{norfairRioCount}, parent=({parent.XPosition:X4},{parent.YPosition:X4})/" +
                $"${parent.CurrentInstruction:X4}/${(ushort)parentState.Function:X4}, " +
                $"follower=({follower.XPosition:X4},{follower.YPosition:X4})/" +
                $"${follower.CurrentInstruction:X4}/${(ushort)followerState.Function:X4}.");
        }

        // Keep Samus outside the strict 192-pixel trigger for one frame. This proves the
        // follower hides specifically because the parent's installed list is the idle list.
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!follower.Properties.HasAny(EnemyProperties.Invisible))
            throw new InvalidDataException("Norfair Rio follower remained visible during parent idle.");

        loaded.Samus.XPosition = unchecked((ushort)(parent.XPosition + 32));
        loaded.Samus.YPosition = unchecked((ushort)(parent.YPosition + 96));
        ushort originX = parent.XPosition;
        ushort originY = parent.YPosition;
        var parentMaps = new HashSet<ushort>();
        var followerMaps = new HashSet<ushort>();
        var offsets = new HashSet<ushort>();
        bool sawTakeoff = false;
        bool sawDive = false;
        bool sawDiveSound = false;
        bool sawMovement = false;
        bool sawReturn = false;
        bool sawLateReturn = false;
        bool sawFollowerAbove = false;
        bool sawFollowerBelow = false;
        bool sawFinishLanding = false;
        bool sawWaitingAfterLanding = false;
        for (int frame = 0; frame < 1800; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
            parentMaps.Add(parent.SpritemapPointer);
            followerMaps.Add(follower.SpritemapPointer);
            offsets.Add(parentState.FollowerYOffset);
            sawTakeoff |= parentState.InstalledInstructionList == 0xc107;
            sawDive |= parentState.Function == NorfairRioEnemyFunction.Dive &&
                parentState.InstalledInstructionList == 0xc12f;
            sawDiveSound |= loaded.Enemies.LastNorfairRioSoundEffect == 0x0065;
            sawMovement |= parent.XPosition != originX || parent.YPosition != originY;
            sawReturn |= parentState.Function == NorfairRioEnemyFunction.ReturnToPerch &&
                parentState.InstalledInstructionList == 0xc145;
            sawLateReturn |= parentState.InstalledInstructionList == 0xc179;
            if (!follower.Properties.HasAny(EnemyProperties.Invisible))
            {
                ushort expectedY = unchecked((ushort)(
                    parent.YPosition + (short)parentState.FollowerYOffset));
                if (follower.XPosition != parent.XPosition || follower.YPosition != expectedY)
                {
                    throw new InvalidDataException(
                        $"Norfair Rio follower position mismatch: parent=" +
                        $"({parent.XPosition:X4},{parent.YPosition:X4}) offset=" +
                        $"${parentState.FollowerYOffset:X4}, follower=" +
                        $"({follower.XPosition:X4},{follower.YPosition:X4}).");
                }
                sawFollowerAbove |= (parentState.FollowerYOffset & 0x8000) != 0 &&
                    followerState.InstalledInstructionList == 0xc1a3;
                sawFollowerBelow |= (parentState.FollowerYOffset & 0x8000) == 0 &&
                    followerState.InstalledInstructionList == 0xc18f;
            }
            if (parentState.Function == NorfairRioEnemyFunction.FinishLanding)
            {
                sawFinishLanding = true;
                // Prevent a fresh random attack from immediately replacing the looping
                // $C179 post-landing list, so the one-frame function-six seam is observable.
                loaded.Samus.XPosition = 0x1000;
            }
            sawWaitingAfterLanding |= sawFinishLanding &&
                parentState.Function == NorfairRioEnemyFunction.WaitForAttackOpportunity &&
                parentState.InstalledInstructionList == 0xc179 &&
                !follower.Properties.HasAny(EnemyProperties.Invisible);
            if (sawWaitingAfterLanding)
                break;
        }

        if (!sawTakeoff || !sawDive || !sawDiveSound || !sawMovement || !sawReturn ||
            !sawLateReturn || !sawFollowerAbove || !sawFollowerBelow ||
            !sawFinishLanding || !sawWaitingAfterLanding || offsets.Count < 5 ||
            parentMaps.Count < 4 || followerMaps.Count < 2)
        {
            throw new InvalidDataException(
                $"Norfair Rio cycle mismatch: takeoff/dive/sound/move/return/late/" +
                $"above/below/land/wait={sawTakeoff}/{sawDive}/{sawDiveSound}/" +
                $"{sawMovement}/{sawReturn}/{sawLateReturn}/{sawFollowerAbove}/" +
                $"{sawFollowerBelow}/{sawFinishLanding}/{sawWaitingAfterLanding}, offsets/maps=" +
                $"{offsets.Count}/{parentMaps.Count}/{followerMaps.Count}, position=" +
                $"({parent.XPosition:X4},{parent.YPosition:X4}), velocity=" +
                $"({parentState.XVelocity:X4},{parentState.YVelocity:X4}), function/list=" +
                $"${(ushort)parentState.Function:X4}/${parentState.InstalledInstructionList:X4}.");
        }
    }

    private static void VerifyMirroredLaunch(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        ushort sourceXVelocity = ReadWord(bus, 0xa2c1c5);
        LoadedRoom right = LoadRoom(bus, room, assets);
        RoomEnemySlot rightParent = GetFirstParent(right.Enemies);
        right.Samus.XPosition = unchecked((ushort)(rightParent.XPosition + 32));
        StepUntilTakeoff(right, assets.LevelData);
        ushort rightVelocity = GetState(right.Enemies, rightParent).XVelocity;

        LoadedRoom left = LoadRoom(bus, room, assets);
        RoomEnemySlot leftParent = GetFirstParent(left.Enemies);
        left.Samus.XPosition = unchecked((ushort)(leftParent.XPosition - 32));
        StepUntilTakeoff(left, assets.LevelData);
        ushort leftVelocity = GetState(left.Enemies, leftParent).XVelocity;
        if (rightVelocity != sourceXVelocity ||
            leftVelocity != unchecked((ushort)-(short)sourceXVelocity))
        {
            throw new InvalidDataException(
                $"Norfair Rio launch mirror mismatch: ROM=${sourceXVelocity:X4}, " +
                $"right/left=${rightVelocity:X4}/${leftVelocity:X4}.");
        }
    }

    private static void StepUntilTakeoff(LoadedRoom loaded, RoomLevelData level)
    {
        RoomEnemySlot parent = GetFirstParent(loaded.Enemies);
        NorfairRioEnemyState state = GetState(loaded.Enemies, parent);
        for (int frame = 0; frame < 256; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: level);
            if (state.Function == NorfairRioEnemyFunction.WaitForTakeoffAnimation)
                return;
        }
        throw new InvalidDataException("Norfair Rio did not pass its random takeoff gate in 256 frames.");
    }

    private static void VerifyFreezeMirroring(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot parent = GetFirstParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        loaded.Samus.XPosition = unchecked((ushort)(parent.XPosition + 32));
        StepUntilTakeoff(loaded, assets.LevelData);
        parent.FrozenTimer = 10;
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (parent.FrozenTimer != 9 || follower.FrozenTimer != 9 ||
            !follower.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException(
                $"Norfair Rio freeze mirror mismatch: parent/follower=" +
                $"{parent.FrozenTimer}/{follower.FrozenTimer}, hidden=" +
                $"{follower.Properties.HasAny(EnemyProperties.Invisible)}.");
        }
    }

    private static void VerifyContactAndWeaponDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(bus, room, assets);
        RoomEnemySlot parent = GetFirstParent(loaded.Enemies);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.XPosition = parent.XPosition;
        loaded.Samus.YPosition = parent.YPosition;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, controllerInput: 0) ||
            loaded.Samus.Health != 939 || !loaded.Samus.KnockbackActive || parent.Health != 120)
        {
            throw new InvalidDataException(
                $"Norfair Rio contact mismatch: Samus health={loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, parent health={parent.Health}.");
        }

        loaded = LoadRoom(bus, room, assets);
        parent = GetFirstParent(loaded.Enemies);
        RoomEnemySlot follower = loaded.Enemies.Slots[parent.SlotIndex + 1];
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmLethalBeam(shots.Slots[0], parent);
        int beamHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (beamHits != 1 || parent.Health != 0 ||
            !parent.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Norfair Rio beam death mismatch: hits={beamHits}, health={parent.Health}, " +
                $"deleted={parent.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!follower.Properties.HasAny(EnemyProperties.Deleted))
            throw new InvalidDataException("Norfair Rio follower survived its dead parent.");

        loaded = LoadRoom(bus, room, assets);
        parent = GetFirstParent(loaded.Enemies);
        int powerBombReactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            parent.XPosition,
            parent.YPosition,
            explosionRadius: 32);
        if (powerBombReactions != 0 || parent.Health != 120 ||
            parent.InvincibilityTimer != 0 ||
            parent.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Norfair Rio power-bomb immunity mismatch: reactions={powerBombReactions}, " +
                $"health={parent.Health}, invincibility={parent.InvincibilityTimer}, " +
                $"deleted={parent.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static LoadedRoom LoadRoom(
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
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x1000,
            YPosition = 0x00b0,
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
            samus: samus,
            cameraX: CameraX,
            cameraY: CameraY);
        return new LoadedRoom(enemies, samus);
    }

    private static RoomEnemySlot GetFirstParent(RoomEnemySystem enemies) =>
        enemies.Slots
            .Take(enemies.EnemyCount)
            .First(slot => slot.EnemyDefinitionPointer == DefinitionPointer &&
                (slot.Parameter1 & 0x8000) == 0);

    private static NorfairRioEnemyState GetState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.NorfairRioStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Retail Norfair Rio slot {slot.SlotIndex} did not receive typed state.");

    private static void ArmLethalBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)SamusProjectileFamily.Beam;
        projectile.Damage = 1000;
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
        if (definition.TileDataSize != 0x0600 || definition.PalettePointer != 0xc0d1 ||
            definition.Health != 120 || definition.Damage != 60 ||
            definition.XRadius != 16 || definition.YRadius != 9 ||
            definition.Bank != 0xa2 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0036 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xc242 || definition.PartCount != 2 ||
            definition.MainAiPointer != 0xc277 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 2 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaed520 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf1f4 ||
            definition.VulnerabilityPointer != 0xee42 || definition.NamePointer != 0xde85)
        {
            throw new InvalidDataException("Retail Norfair Rio header words do not match $A0:D2FF.");
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedRoom(RoomEnemySystem Enemies, SamusState Samus);
}
