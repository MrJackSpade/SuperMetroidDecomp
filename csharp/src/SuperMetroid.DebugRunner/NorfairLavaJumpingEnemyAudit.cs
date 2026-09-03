using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the parent/follower pair at the start of Norfair room $8F:AFA3.
/// The complete retail room also contains untranslated Lava Seahorses. A read-only address
/// decorator terminates the population after the first two unchanged $D2BF records; enemy
/// code, parameters, graphics, palette, room state, and terrain all remain retail bytes.
/// </summary>
internal static class NorfairLavaJumpingEnemyAudit
{
    private const ushort DefinitionPointer = 0xd2bf;
    private const ushort RoomPointer = 0xafa3;
    private const ushort ExpectedStatePointer = 0xafb0;
    private const ushort ExpectedPopulationPointer = 0xa6a8;
    private const ushort ExpectedTilesetPointer = 0x87c9;
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
                BossBits: BossBits.None,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(retailBus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer ||
            room.State.EnemyTilesetPointer != ExpectedTilesetPointer ||
            room.WidthInScreens != 5 || room.HeightInScreens != 1)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, tileset=" +
                $"${room.State.EnemyTilesetPointer:X4}, dimensions=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}.");
        }

        VerifyJumpAnimationAndFollower(retailBus, room, assets);
        VerifyFreezeMirroring(retailBus, room, assets);
        VerifyContactAndWeaponDamage(retailBus, room, assets);
        Console.WriteLine(
            "Norfair lava-jumping enemy audit passed: retail pair load, randomized jump, " +
            "ROM animation handshake, parent-slot following, freeze mirroring, lava reset, " +
            "contact damage, beam/missile/power-bomb immunity, super-missile death, and " +
            "follower cleanup agree.");
        return 0;
    }

    private static void VerifyJumpAnimationAndFollower(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(retailBus, room, assets);
        RoomEnemySlot parent = loaded.Parent;
        RoomEnemySlot follower = loaded.Follower;
        NorfairLavaJumpingEnemyState parentState = GetState(loaded.Enemies, parent);
        NorfairLavaJumpingEnemyState followerState = GetState(loaded.Enemies, follower);

        if (loaded.Enemies.EnemyCount != 2 ||
            parent.XPosition != 0x00e8 || parent.YPosition != 0x00f0 ||
            follower.XPosition != 0x00e8 || follower.YPosition != 0x00f0 ||
            parent.Health != 300 || follower.Health != 300 ||
            parent.CurrentInstruction != 0xbe3c || follower.CurrentInstruction != 0xbe62 ||
            parentState.Function != NorfairLavaJumpingEnemyFunction.BeginJump ||
            followerState.Function != NorfairLavaJumpingEnemyFunction.FollowParent ||
            parentState.SpawnX != 0x00e8 || parentState.SpawnY != 0x00f0 ||
            parentState.InstalledInstructionList != 0 || !followerState.IsFollower)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"parent=({parent.XPosition:X4},{parent.YPosition:X4})/" +
                $"${parent.CurrentInstruction:X4}/${(ushort)parentState.Function:X4}, " +
                $"follower=({follower.XPosition:X4},{follower.YPosition:X4})/" +
                $"${follower.CurrentInstruction:X4}/${(ushort)followerState.Function:X4}.");
        }

        ushort[] cartridgeVelocities = Enumerable.Range(0, 4)
            .Select(index => ReadWord(retailBus, 0xa2be86 + index * 2))
            .ToArray();
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!cartridgeVelocities.Contains(parentState.YVelocity) ||
            parentState.Function != NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch ||
            loaded.Enemies.LastNorfairLavaJumpingEnemySoundEffect != 0x000d ||
            !parent.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            follower.Properties.HasAny(EnemyProperties.Invisible) ||
            follower.YPosition != parent.YPosition || parent.SpritemapPointer == 0 ||
            follower.SpritemapPointer == 0)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping launch mismatch: velocity=${parentState.YVelocity:X4}, " +
                $"function=${(ushort)parentState.Function:X4}, sound=" +
                $"{loaded.Enemies.LastNorfairLavaJumpingEnemySoundEffect?.ToString("X4") ?? "none"}, " +
                $"offscreen={parent.Properties.HasAny(EnemyProperties.ProcessOffScreen)}, " +
                $"follower hidden={follower.Properties.HasAny(EnemyProperties.Invisible)}.");
        }

        var parentMaps = new HashSet<ushort> { parent.SpritemapPointer };
        var followerMaps = new HashSet<ushort> { follower.SpritemapPointer };
        bool sawJumpList = false;
        bool sawAnimationSignalConsumed = false;
        bool sawFollowerHiddenDuringDescent = false;
        bool sawLavaReset = false;
        for (int frame = 0; frame < 600; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
            parentMaps.Add(parent.SpritemapPointer);
            followerMaps.Add(follower.SpritemapPointer);
            sawJumpList |= parentState.InstalledInstructionList == 0xbe42;
            sawAnimationSignalConsumed |=
                parentState.Function == NorfairLavaJumpingEnemyFunction.FallBackIntoLava;
            sawFollowerHiddenDuringDescent |=
                (parentState.YVelocity & 0x8000) == 0 &&
                follower.Properties.HasAny(EnemyProperties.Invisible);
            sawLavaReset |= sawAnimationSignalConsumed &&
                parentState.Function == NorfairLavaJumpingEnemyFunction.BeginJump &&
                parent.XPosition == 0x00e8 && parent.YPosition == 0x00f0 &&
                parentState.InstalledInstructionList == 0xbe3c &&
                !parent.Properties.HasAny(EnemyProperties.ProcessOffScreen);
            if (sawLavaReset)
                break;
        }

        if (!sawJumpList || !sawAnimationSignalConsumed ||
            !sawFollowerHiddenDuringDescent || !sawLavaReset ||
            parentMaps.Count < 3 || followerMaps.Count < 2)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping cycle mismatch: list/signal/follower/reset=" +
                $"{sawJumpList}/{sawAnimationSignalConsumed}/" +
                $"{sawFollowerHiddenDuringDescent}/{sawLavaReset}, maps=" +
                $"{parentMaps.Count}/{followerMaps.Count}, position=" +
                $"({parent.XPosition:X4},{parent.YPosition:X4}), velocity=" +
                $"${parentState.YVelocity:X4}, function=${(ushort)parentState.Function:X4}, " +
                $"list=${parentState.InstalledInstructionList:X4}.");
        }
    }

    private static void VerifyFreezeMirroring(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(retailBus, room, assets);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        loaded.Parent.FrozenTimer = 10;
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);

        // The parent is processed first and decrements from ten to nine. The follower's
        // alias read then copies that live word and hides its cosmetic half in the same frame.
        if (loaded.Parent.FrozenTimer != 9 || loaded.Follower.FrozenTimer != 9 ||
            !loaded.Follower.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping freeze mirror mismatch: parent/follower=" +
                $"{loaded.Parent.FrozenTimer}/{loaded.Follower.FrozenTimer}, hidden=" +
                $"{loaded.Follower.Properties.HasAny(EnemyProperties.Invisible)}.");
        }
    }

    private static void VerifyContactAndWeaponDamage(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(retailBus, room, assets);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.XPosition = loaded.Parent.XPosition;
        loaded.Samus.YPosition = loaded.Parent.YPosition;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, controllerInput: 0) ||
            loaded.Samus.Health != 949 || !loaded.Samus.KnockbackActive ||
            loaded.Parent.Health != 300)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping contact mismatch: Samus health={loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, parent health={loaded.Parent.Health}.");
        }

        VerifyImmuneProjectile(
            retailBus,
            room,
            assets,
            SamusProjectileFamily.Beam,
            "beam");
        VerifyImmuneProjectile(
            retailBus,
            room,
            assets,
            SamusProjectileFamily.Missile,
            "missile");

        loaded = LoadPair(retailBus, room, assets);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Parent, SamusProjectileFamily.SuperMissile);
        int superMissileHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            retailBus,
            shots,
            bombs,
            loaded.Samus);
        if (superMissileHits != 1 ||
            loaded.Parent.Health != 0 ||
            !loaded.Parent.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping super-missile death mismatch: hits={superMissileHits}, " +
                $"health={loaded.Parent.Health}, map=${loaded.Parent.SpritemapPointer:X4}, " +
                $"deleted={loaded.Parent.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }

        // On the following enemy frame the dead parent's definition may already be cleared.
        // The follower must still observe the aliased zero-health word and delete itself.
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        if (!loaded.Follower.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                "Norfair lava-jumping follower survived its parent's zero-health cleanup frame.");
        }

        loaded = LoadPair(retailBus, room, assets);
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            retailBus,
            loaded.Parent.XPosition,
            loaded.Parent.YPosition,
            explosionRadius: 32);
        if (reactions != 0 || loaded.Parent.Health != 300 ||
            loaded.Parent.InvincibilityTimer != 0)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping power-bomb immunity mismatch: reactions={reactions}, " +
                $"health={loaded.Parent.Health}, invincibility={loaded.Parent.InvincibilityTimer}.");
        }
    }

    private static void VerifyImmuneProjectile(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SamusProjectileFamily family,
        string label)
    {
        LoadedPair loaded = LoadPair(retailBus, room, assets);
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Parent, family);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            retailBus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Parent.Health != 300 ||
            loaded.Parent.InvincibilityTimer != 0 ||
            loaded.Parent.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping {label} immunity mismatch: hits={hits}, " +
                $"health={loaded.Parent.Health}, invincibility=" +
                $"{loaded.Parent.InvincibilityTimer}, deleted=" +
                $"{loaded.Parent.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static LoadedPair LoadPair(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var pairBus = new PopulationPrefixAddressSpace(
            retailBus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 2,
            deathQuota: 1);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x00c0,
        };
        samus.RefreshCollisionRadii(retailBus);
        samus.InitializeAnimation(retailBus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
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
        return new LoadedPair(enemies, enemies.Slots[0], enemies.Slots[1], samus);
    }

    private static NorfairLavaJumpingEnemyState GetState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.NorfairLavaJumpingEnemyStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Retail Norfair lava-jumping slot {slot.SlotIndex} did not receive typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        SamusProjectileFamily family)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)family;
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
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0xbe1c ||
            definition.Health != 300 || definition.Damage != 50 ||
            definition.XRadius != 8 || definition.YRadius != 12 ||
            definition.Bank != 0xa2 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0036 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xbe99 || definition.PartCount != 2 ||
            definition.MainAiPointer != 0xbed2 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 2 ||
            definition.PowerBombReactionPointer != 0 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaebd20 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf35c ||
            definition.VulnerabilityPointer != 0xedea || definition.NamePointer != 0xe141)
        {
            throw new InvalidDataException(
                "Retail Norfair lava-jumping header words do not match $A0:D2BF.");
        }
    }

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedPair(
        RoomEnemySystem Enemies,
        RoomEnemySlot Parent,
        RoomEnemySlot Follower,
        SamusState Samus);
}
