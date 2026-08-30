using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused bank-$86 coverage for every Work Robot laser definition. The ordinary long-run
/// audit proves that untouched cartridge walking lists reach both facings and several shot
/// branches. This companion enters each of those authored branch words on a fresh retail
/// robot so cooldown timing cannot make one direction stand in for another.
/// </summary>
internal static partial class WorkRobotAudit
{
    private const ushort LaserPreInstruction = 0xd3bf;
    private const ushort LaserInstructionList = 0xd2ec;

    // These are addresses of real custom-opcode words inside the left/right walking lists,
    // not synthetic projectile constructors. StepFrame must parse the opcode, pass through
    // Work Robot AI, allocate a physical bank-$86 slot, and run the cartridge initializer.
    private static readonly LaserRoute[] LaserRoutes =
    [
        new("up-left", RoomEnemyProjectileKind.WorkRobotLaserUpLeft,
            FiringCursor: 0xc6ff, FacingVelocity: 0xfe00,
            ExpectedXVelocity: 0xfe00, ExpectedYVelocity: 0xff80),
        new("left", RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
            FiringCursor: 0xc6f5, FacingVelocity: 0xfe00,
            ExpectedXVelocity: 0xfe00, ExpectedYVelocity: 0x0000),
        new("down-left", RoomEnemyProjectileKind.WorkRobotLaserDownLeft,
            FiringCursor: 0xc735, FacingVelocity: 0xfe00,
            ExpectedXVelocity: 0xfe00, ExpectedYVelocity: 0x0080),
        new("down-right", RoomEnemyProjectileKind.WorkRobotLaserDownRight,
            FiringCursor: 0xc935, FacingVelocity: 0x0200,
            ExpectedXVelocity: 0x0200, ExpectedYVelocity: 0x0080),
        new("right", RoomEnemyProjectileKind.WorkRobotLaserHorizontal,
            FiringCursor: 0xc945, FacingVelocity: 0x0200,
            ExpectedXVelocity: 0x0200, ExpectedYVelocity: 0x0000),
        new("up-right", RoomEnemyProjectileKind.WorkRobotLaserUpRight,
            FiringCursor: 0xc94f, FacingVelocity: 0x0200,
            ExpectedXVelocity: 0x0200, ExpectedYVelocity: 0xff80),
    ];

    private static LaserResult VerifyLaserLifecycles(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var definitions = new HashSet<RoomEnemyProjectileKind>();
        foreach (LaserRoute route in LaserRoutes)
        {
            VerifyLaserMovementAnimationAndTerrainDisposal(bus, room, assets, route);
            VerifyLaserInteractions(bus, room, assets, route);
            definitions.Add(route.Kind);
        }

        if (definitions.Count != 5)
        {
            throw new InvalidDataException(
                $"Work Robot route table covered {definitions.Count} distinct definitions, not five.");
        }

        return new LaserResult(LaserRoutes.Length, definitions.Count);
    }

    /// <summary>
    /// Proves definition loading, signed 8.8 movement, the six-map cartridge animation, and
    /// eventual deletion by the real room-collision pre-instruction. Each route gets a fresh
    /// encounter so a prior laser cannot free/reuse the physical slot being observed.
    /// </summary>
    private static void VerifyLaserMovementAnimationAndTerrainDisposal(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LaserRoute route)
    {
        (LoadedRobots loaded, RoomEnemySlot robot, RoomEnemyProjectileSlot laser) =
            SpawnLaserThroughAuthoredRoute(bus, room, assets, route);
        VerifyLaserInitialization(bus, robot, laser, route);

        ushort initialX = laser.XPosition;
        ushort initialXSubposition = laser.XSubposition;
        ushort initialY = laser.YPosition;
        ushort initialYSubposition = laser.YSubposition;
        (ushort expectedX, ushort expectedXSubposition) = AdvanceEightBitVelocity(
            initialX,
            initialXSubposition,
            route.ExpectedXVelocity);
        (ushort expectedY, ushort expectedYSubposition) = AdvanceEightBitVelocity(
            initialY,
            initialYSubposition,
            route.ExpectedYVelocity);

        // Common pre-instruction $D3BF owns the first motion before the instruction list
        // selects its first visible map. A collision here would mean the route was not given
        // enough unobstructed retail room space to prove the velocity it claims to cover.
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: 0,
            cameraY: 0,
            nmiFrameCounter8: 0);
        if (!laser.IsActive || laser.XPosition != expectedX ||
            laser.XSubposition != expectedXSubposition || laser.YPosition != expectedY ||
            laser.YSubposition != expectedYSubposition || laser.GraphicsIndex != 0)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} laser first motion/graphics failed: active=" +
                $"{laser.IsActive}, X=${initialX:X4}:{initialXSubposition:X4}->" +
                $"${laser.XPosition:X4}:{laser.XSubposition:X4} expected " +
                $"${expectedX:X4}:{expectedXSubposition:X4}, Y=" +
                $"${initialY:X4}:{initialYSubposition:X4}->" +
                $"${laser.YPosition:X4}:{laser.YSubposition:X4} expected " +
                $"${expectedY:X4}:{expectedYSubposition:X4}, graphics=${laser.GraphicsIndex:X4}.");
        }

        var maps = new HashSet<ushort>();
        if (laser.SpritemapPointer is not 0 and not 0x8000)
            maps.Add(laser.SpritemapPointer);
        for (int frame = 1; frame < 1024 && laser.IsActive; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            if (laser.IsActive && laser.SpritemapPointer is not 0 and not 0x8000)
                maps.Add(laser.SpritemapPointer);
        }

        // The startup contributes maps 0..2, then the permanent loop contributes 3..5.
        // A small room can delete a laser before all six are reached, but three distinct
        // cartridge maps are the minimum evidence that this was an animated attack rather
        // than a moving sentinel. Terminal inactivity must still come from room collision.
        if (laser.IsActive || maps.Count < 3)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} laser terrain lifecycle failed: live=" +
                $"{laser.IsActive}, distinct maps={maps.Count}.");
        }
    }

    /// <summary>
    /// Uses a second fresh actor to prove exact common Samus contact and the absence of
    /// property-$8000 shot blocking. The contact helper additionally requires native
    /// invincibility, knockback, and deletion because these definitions lack property $4000.
    /// </summary>
    private static void VerifyLaserInteractions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LaserRoute route)
    {
        (LoadedRobots loaded, RoomEnemySlot robot, RoomEnemyProjectileSlot laser) =
            SpawnLaserThroughAuthoredRoute(bus, room, assets, route);
        VerifyLaserInitialization(bus, robot, laser, route);

        var samusProjectiles = new SamusProjectileSystem();
        SamusProjectileSlot shot = samusProjectiles.Slots[0];
        shot.Type = 0x0001;
        shot.Damage = 20;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = laser.XPosition;
        shot.YPosition = laser.YPosition;
        shot.XRadius = 4;
        shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        int shotHits = loaded.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            samusProjectiles,
            new SamusBombProjectileSystem());
        if (shotHits != 0 || shot.InstructionPointer != 0x9000 || !laser.IsActive)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} nonblocking laser intercepted a Samus shot: " +
                $"hits={shotHits}, shot=${shot.InstructionPointer:X4}, live={laser.IsActive}.");
        }

        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            loaded.Enemies,
            loaded.Samus,
            new SamusBombProjectileSystem(),
            assets.LevelData,
            laser,
            cameraX: 0,
            cameraY: 0);
    }

    private static (LoadedRobots Loaded, RoomEnemySlot Robot, RoomEnemyProjectileSlot Laser)
        SpawnLaserThroughAuthoredRoute(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets,
            LaserRoute route)
    {
        LoadedRobots loaded = Load(bus, room, assets, bossDefeated: true);
        // The basement's first robot starts beside the left wall and its second starts near
        // the right side. Use the actor whose muzzle has open room in the route's direction;
        // otherwise correct $D3BF collision would delete the laser before one movement tick.
        RoomEnemySlot robot = unchecked((short)route.FacingVelocity) < 0
            ? loaded.Enemies.Slots[1]
            : loaded.Enemies.Slots[0];
        WorkRobotEnemyState state = RequireState(loaded.Enemies, robot);

        // Powered robots are placed above their eventual support surface. While airborne,
        // main AI deliberately increments the instruction timer to cancel the interpreter's
        // decrement, so forcing a one-frame branch immediately after Load would never run.
        // Wait until the initial $20-frame list has installed its first timed map; that is
        // authoritative evidence that gravity found terrain and instruction time is live.
        for (int frame = 0; frame < 128 && robot.CurrentInstruction != 0xc6e9; frame++)
            Step(loaded, assets, robot, room);
        if (robot.CurrentInstruction != 0xc6e9)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} route never reached grounded instruction time; " +
                $"list=${robot.CurrentInstruction:X4}, timer={robot.InstructionTimer}, " +
                $"Y=${robot.YPosition:X4}.");
        }

        // The branch word normally occurs after a timed walk frame has established facing.
        // Seed precisely those two owner words, leave every projectile slot empty, and let
        // the ordinary enemy-frame interpreter perform allocation and initialization.
        state.LaserXVelocity = route.FacingVelocity;
        state.LaserCooldown = 0;
        robot.CurrentInstruction = route.FiringCursor;
        robot.InstructionTimer = 1;
        loaded.Samus.XPosition = 0x0200;
        loaded.Samus.YPosition = 0x0040;
        Step(loaded, assets, robot, room);

        RoomEnemyProjectileSlot[] matches = loaded.Enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == route.Kind)
            .ToArray();
        if (matches.Length != 1)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} route $A8:{route.FiringCursor:X4} produced " +
                $"{matches.Length} {route.Kind} actors.");
        }

        return (loaded, robot, matches[0]);
    }

    private static void VerifyLaserInitialization(
        ISnesAddressSpace bus,
        RoomEnemySlot robot,
        RoomEnemyProjectileSlot laser,
        LaserRoute route)
    {
        int definition = 0x860000 | (ushort)route.Kind;
        ushort packedRadii = ReadLaserAuditWord(bus, definition + 6);
        ushort properties = ReadLaserAuditWord(bus, definition + 8);
        ushort expectedGraphics = route.Kind is
            RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
            RoomEnemyProjectileKind.WorkRobotLaserDownRight
                ? (ushort)0
                : unchecked((ushort)(robot.VramTilesIndex | robot.PaletteIndex));
        int muzzleOffset = unchecked((short)route.FacingVelocity) < 0 ? -4 : 4;
        if (laser.PreInstruction != ReadLaserAuditWord(bus, definition + 2) ||
            laser.PreInstruction != LaserPreInstruction ||
            laser.InstructionPointer != ReadLaserAuditWord(bus, definition + 4) ||
            laser.InstructionPointer != LaserInstructionList || laser.InstructionTimer != 1 ||
            laser.SpritemapPointer != 0x8000 ||
            laser.XRadius != unchecked((byte)packedRadii) ||
            laser.YRadius != unchecked((byte)(packedRadii >> 8)) ||
            laser.Damage != (properties & 0x0fff) || laser.InvincibilityFrames != 96 ||
            !laser.CanDamageSamus || laser.PersistsOnSamusContact ||
            laser.BlocksSamusProjectiles || laser.CollisionOption != 0 ||
            laser.XVelocity != route.ExpectedXVelocity ||
            laser.YVelocity != route.ExpectedYVelocity ||
            laser.XPosition != unchecked((ushort)(robot.XPosition + muzzleOffset)) ||
            laser.YPosition != unchecked((ushort)(robot.YPosition - 16)) ||
            laser.XSubposition != 0 || laser.YSubposition != 0 ||
            laser.GraphicsIndex != expectedGraphics)
        {
            throw new InvalidDataException(
                $"Work Robot {route.Name} laser initialization diverged from definition " +
                $"$86:{(ushort)route.Kind:X4}: position=(${laser.XPosition:X4}," +
                $"${laser.YPosition:X4}), velocity=(${laser.XVelocity:X4}," +
                $"${laser.YVelocity:X4}), list/pre/map=${laser.InstructionPointer:X4}/" +
                $"${laser.PreInstruction:X4}/${laser.SpritemapPointer:X4}, radii=" +
                $"{laser.XRadius}/{laser.YRadius}, damage={laser.Damage}, graphics=" +
                $"${laser.GraphicsIndex:X4}, properties damage/persist/block=" +
                $"{laser.CanDamageSamus}/{laser.PersistsOnSamusContact}/" +
                $"{laser.BlocksSamusProjectiles}.");
        }
    }

    private static (ushort Position, ushort Subposition) AdvanceEightBitVelocity(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    private static ushort ReadLaserAuditWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LaserRoute(
        string Name,
        RoomEnemyProjectileKind Kind,
        ushort FiringCursor,
        ushort FacingVelocity,
        ushort ExpectedXVelocity,
        ushort ExpectedYVelocity);

    private readonly record struct LaserResult(int FiringRoutes, int Definitions);
}
