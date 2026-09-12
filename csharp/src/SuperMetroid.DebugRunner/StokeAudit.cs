using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed end-to-end audit for Stoke, Nintendo's shipped but normal-room-unused
/// mini-Crocomire. The actor record at <c>$B4:E9D5</c>, enemy header, speed table, bytecode,
/// palette, tiles, spritemaps, projectile definition, and projectile maps all remain direct
/// cartridge reads. Only the test floor is synthetic because no retail room population ever
/// instantiates this otherwise complete enemy.
/// </summary>
internal static class StokeAudit
{
    private const ushort DefinitionPointer = 0xceff;
    private const ushort DebugPopulationPointer = 0xe9d5;
    private const ushort DebugEnemyTilesetPointer = 0x8047;
    private const ushort CameraX = 0x0bc0;
    private const ushort CameraY = 0x0140;

    private static readonly ushort[] LeftWalkingMaps = [0x8aca, 0x8ad6, 0x8ae7, 0x8af3];
    private static readonly ushort[] RightWalkingMaps = [0x8b15, 0x8b21, 0x8b32, 0x8b3e];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(retailBus);
        VerifyLeftWalkingAnimationAndExactFixedMotion(retailBus);
        VerifyWallTurnRightAnimationAndProjectile(retailBus);
        VerifyLeftProjectileDamageAndLifetime(retailBus);
        VerifyBodyCombat(retailBus);

        Console.WriteLine(
            "Stoke audit passed: cartridge debug actor/graphics loaded; both four-map walks, " +
            "16.16 motion, wall turn, both attack maps/directions, two-frame projectile " +
            "animation, inclusive viewport deletion, projectile/body damage, Grapple cancel, " +
            "beam hurt flash, and ordinary death all matched ROM data.");
        return 0;
    }

    private static void VerifyLeftWalkingAnimationAndExactFixedMotion(
        SuperMetroidAddressSpace retailBus)
    {
        LoadedStoke loaded = Load(retailBus, CreateFlatFloorLevel(includeImmediateLeftWall: false));
        RoomEnemySlot actor = loaded.Actor;
        StokeEnemyState state = State(loaded);

        ushort expectedRightWhole = ReadWord(retailBus, 0xa0818f);
        ushort expectedRightFraction = ReadWord(retailBus, 0xa08191);
        ushort expectedLeftWhole = ReadWord(retailBus, 0xa08193);
        ushort expectedLeftFraction = ReadWord(retailBus, 0xa08195);
        if (loaded.Enemies.EnemyCount != 1 || loaded.Enemies.GraphicsSet.Count != 1 ||
            loaded.Enemies.GraphicsSet[0].DefinitionPointer != DefinitionPointer ||
            actor.XPosition != 0x0c30 || actor.YPosition != 0x01f8 ||
            actor.Parameter1 != 0 || actor.Parameter2 != 1 || actor.Properties != 0x2000 ||
            actor.Health != 20 || actor.PaletteIndex != 0x0e00 ||
            actor.CurrentInstruction != 0x8932 ||
            actor.SpritemapPointer != 0x804d || state.Direction != StokeDirection.Left ||
            state.Function != StokeAiFunction.MovingLeft ||
            state.RightVelocity != expectedRightWhole ||
            state.RightSubvelocity != expectedRightFraction ||
            state.LeftVelocity != expectedLeftWhole ||
            state.LeftSubvelocity != expectedLeftFraction)
        {
            throw new InvalidDataException(
                "Stoke did not load the exact $B4:E9D5 population record, $B4:8047 " +
                "graphics record, and parameter-one speed-table words.");
        }

        int leftDisplacement = ((int)(short)state.LeftVelocity << 16) | state.LeftSubvelocity;
        uint expectedFixedX = PackFixed(actor.XPosition, actor.XSubposition);
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 48; frame++)
        {
            expectedFixedX = unchecked(expectedFixedX + (uint)leftDisplacement);
            StepActor(loaded);
            maps.Add(actor.SpritemapPointer);
            if (PackFixed(actor.XPosition, actor.XSubposition) != expectedFixedX ||
                actor.YPosition != 0x01f8 || actor.YSubposition != 0)
            {
                throw new InvalidDataException(
                    $"Stoke left 16.16 motion diverged on frame {frame + 1}: " +
                    $"expected ${expectedFixedX:X8}, got " +
                    $"${PackFixed(actor.XPosition, actor.XSubposition):X8}; " +
                    $"Y=${actor.YPosition:X4}.${actor.YSubposition:X4}.");
            }
        }

        if (!LeftWalkingMaps.All(maps.Contains) || state.Function != StokeAiFunction.MovingLeft)
        {
            throw new InvalidDataException(
                "Stoke's left walking bytecode did not expose all four ROM spritemaps: " +
                string.Join(',', maps.Order()));
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount is < 2 or > 3)
        {
            throw new InvalidDataException(
                $"Stoke walking map emitted {oam.LastFinalizedSpriteCount} OBJ pieces, expected 2-3.");
        }
    }

    private static void VerifyWallTurnRightAnimationAndProjectile(
        SuperMetroidAddressSpace retailBus)
    {
        RoomLevelData level = CreateFlatFloorLevel(includeImmediateLeftWall: true);
        LoadedStoke loaded = Load(retailBus, level);
        RoomEnemySlot actor = loaded.Actor;
        StokeEnemyState state = State(loaded);

        // The debug actor begins partially inside block 194. Native horizontal collision
        // refuses to apply the $0C38 alignment because that would move right while Stoke
        // requested left; it retains $0C30, turns, and executes $899D in this same phase.
        StepActor(loaded);
        if (actor.XPosition != 0x0c30 || actor.XSubposition != 0 ||
            state.Direction != StokeDirection.Right ||
            state.Function != StokeAiFunction.MovingRight ||
            actor.SpritemapPointer != RightWalkingMaps[0])
        {
            throw new InvalidDataException(
                $"Stoke wall turn failed: X=${actor.XPosition:X4}.${actor.XSubposition:X4}, " +
                $"direction={state.Direction}, function=$A2:{(ushort)state.Function:X4}, " +
                $"map=$A2:{actor.SpritemapPointer:X4}.");
        }

        // Remove only the audit wall; the complete solid floor remains. Forty-eight frames
        // cover one whole right-facing animation period while random $80 cannot select an
        // attack at the actor's current frame-counter range.
        level.SetForegroundEntry(level.GetBlockIndex(194, 31), 0);
        int rightDisplacement = ((int)(short)state.RightVelocity << 16) | state.RightSubvelocity;
        uint expectedFixedX = PackFixed(actor.XPosition, actor.XSubposition);
        var maps = new HashSet<ushort> { actor.SpritemapPointer };
        for (int frame = 0; frame < 48; frame++)
        {
            expectedFixedX = unchecked(expectedFixedX + (uint)rightDisplacement);
            StepActor(loaded);
            maps.Add(actor.SpritemapPointer);
        }
        if (PackFixed(actor.XPosition, actor.XSubposition) != expectedFixedX ||
            !RightWalkingMaps.All(maps.Contains))
        {
            throw new InvalidDataException(
                "Stoke's right movement or four-map animation diverged after its wall turn.");
        }

        TriggerAttackOnNextFrame(loaded);
        if (state.Function != StokeAiFunction.IdleDuringAttack ||
            actor.SpritemapPointer != 0x8b32)
        {
            throw new InvalidDataException(
                $"Right Stoke attack did not stop movement on map $8B32: function=" +
                $"$A2:{(ushort)state.Function:X4}, map=$A2:{actor.SpritemapPointer:X4}.");
        }

        ushort spawnX = actor.XPosition;
        ushort spawnY = actor.YPosition;
        for (int frame = 0; frame < 16; frame++)
        {
            StepActor(loaded);
            loaded.Enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
        }

        RoomEnemyProjectileSlot projectile = SingleProjectile(loaded.Enemies);
        ushort expectedGraphics = unchecked((ushort)(actor.PaletteIndex | actor.VramTilesIndex));
        if (actor.SpritemapPointer != 0x8b4a ||
            projectile.Kind != RoomEnemyProjectileKind.StokeProjectile ||
            projectile.DirectionParameter != 1 || projectile.Variable0 != 0xdb8c ||
            projectile.PreInstruction != 0xdb5b || projectile.InstructionPointer != 0xdb10 ||
            projectile.SpritemapPointer != 0xa94e || projectile.XPosition != spawnX + 1 ||
            projectile.YPosition != spawnY + 2 || projectile.XVelocity != 0x0100 ||
            projectile.YVelocity != 0xff00 || projectile.XRadius != 2 ||
            projectile.YRadius != 2 || projectile.Damage != 5 ||
            projectile.InvincibilityFrames != 96 || !projectile.CanDamageSamus ||
            projectile.GraphicsIndex != expectedGraphics)
        {
            throw new InvalidDataException(
                "Right Stoke projectile initializer, first movement, or first ROM map diverged.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount != 1)
            throw new InvalidDataException("Stoke projectile did not emit its one-piece bank-$8D map.");

        for (int frame = 0; frame < 16; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
        }
        if (projectile.SpritemapPointer != 0xa955 || projectile.XPosition != spawnX + 17)
        {
            throw new InvalidDataException(
                $"Stoke projectile second animation frame/motion mismatch: " +
                $"map=$8D:{projectile.SpritemapPointer:X4}, X=${projectile.XPosition:X4}.");
        }

        // Put the left viewport boundary two pixels ahead. The right-moving preinstruction
        // advances one pixel, then the signed comparison observes X < camera and deletes it.
        loaded.Enemies.StepEnemyProjectiles(
            level,
            samus: null,
            cameraX: unchecked((ushort)(projectile.XPosition + 2)),
            cameraY: CameraY);
        if (loaded.Enemies.ActiveEnemyProjectileCount != 0)
            throw new InvalidDataException("Off-screen Stoke projectile did not delete itself.");
    }

    private static void VerifyLeftProjectileDamageAndLifetime(
        SuperMetroidAddressSpace retailBus)
    {
        RoomLevelData level = CreateFlatFloorLevel(includeImmediateLeftWall: false);
        LoadedStoke loaded = Load(retailBus, level);
        RoomEnemySlot actor = loaded.Actor;
        TriggerAttackOnNextFrame(loaded);
        ushort spawnX = actor.XPosition;

        for (int frame = 0; frame < 16; frame++)
        {
            StepActor(loaded);
            loaded.Enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
        }

        RoomEnemyProjectileSlot projectile = SingleProjectile(loaded.Enemies);
        if (actor.SpritemapPointer != 0x8aff || projectile.DirectionParameter != 0 ||
            projectile.Variable0 != 0xdb62 || projectile.XPosition != spawnX - 1)
        {
            throw new InvalidDataException(
                "Left Stoke attack map or the cartridge's Y-velocity-driven X movement diverged.");
        }

        SamusState samus = CreateSamus(retailBus);
        samus.Health = 100;
        samus.XPosition = unchecked((ushort)(projectile.XPosition - 1));
        samus.YPosition = projectile.YPosition;
        var beforeHit = EnemyContactAuditAssertions.Capture(samus);
        loaded.Enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: CameraX,
            cameraY: CameraY);
        if (samus.Health != 95 || samus.InvincibilityTimer != 96 ||
            loaded.Enemies.ActiveEnemyProjectileCount != 0)
        {
            throw new InvalidDataException(
                $"Stoke projectile contact failed: health={samus.Health}, " +
                $"invincibility={samus.InvincibilityTimer}, knockback={samus.KnockbackActive}, " +
                $"active projectiles={loaded.Enemies.ActiveEnemyProjectileCount}.");
        }
        EnemyContactAuditAssertions.VerifyStandingAirHit(retailBus, samus, beforeHit, 5, 1, "Stoke projectile");
    }

    private static void VerifyBodyCombat(SuperMetroidAddressSpace retailBus)
    {
        RoomLevelData level = CreateFlatFloorLevel(includeImmediateLeftWall: false);
        LoadedStoke contactLoad = Load(retailBus, level);
        StepActor(contactLoad);
        SamusState samus = contactLoad.Samus;
        samus.Health = 999;
        samus.XPosition = contactLoad.Actor.XPosition;
        samus.YPosition = contactLoad.Actor.YPosition;
        var beforeHit = EnemyContactAuditAssertions.Capture(samus);
        if (!contactLoad.Enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 959)
        {
            throw new InvalidDataException(
                $"Stoke's 40-point body contact failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }
        EnemyContactAuditAssertions.VerifyStandingAirHit(retailBus, samus, beforeHit, 40, 1, "Stoke body");

        LoadedStoke shotLoad = Load(retailBus, level);
        StepActor(shotLoad);
        var shots = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], shotLoad.Actor, type: 0x0000, damage: 5);
        if (shotLoad.Enemies.ResolveOrdinaryProjectileHits(
                shotLoad.Bus,
                shots,
                shared,
                shotLoad.Samus) != 1 || shotLoad.Actor.Health != 16 ||
            shotLoad.Actor.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Stoke's default beam vulnerability failed: health={shotLoad.Actor.Health}, " +
                $"flash={shotLoad.Actor.FlashTimer}.");
        }

        GrappleEnemyCollision grapple = shotLoad.Enemies.ResolveGrappleEndpoint(
            shotLoad.Actor.XPosition,
            shotLoad.Actor.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel ||
            grapple.EnemyNativeIndex != shotLoad.Actor.NativeIndex)
        {
            throw new InvalidDataException("Stoke did not execute common Grapple-cancel AI.");
        }

        LoadedStoke deathLoad = Load(retailBus, level);
        StepActor(deathLoad);
        var lethal = new SamusProjectileSystem();
        var beforeDeath = EnemyDeathAuditAssertions.Capture(deathLoad.Enemies, deathLoad.Actor);
        ArmProjectile(lethal.Slots[0], deathLoad.Actor, type: 0x0000, damage: 20);
        if (deathLoad.Enemies.ResolveOrdinaryProjectileHits(
                deathLoad.Bus,
                lethal,
                new SamusBombProjectileSystem(),
                deathLoad.Samus) != 1 || deathLoad.Actor.Health != 0 ||
            deathLoad.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Stoke ordinary death failed: health={deathLoad.Actor.Health}, " +
                $"properties=${deathLoad.Actor.Properties:X4}, " +
                $"killed={deathLoad.Enemies.EnemiesKilled}.");
        }
        EnemyDeathAuditAssertions.Verify(deathLoad.Enemies, deathLoad.Actor, beforeDeath, "Stoke beam");
    }

    private static LoadedStoke Load(SuperMetroidAddressSpace retailBus, RoomLevelData level)
    {
        var bus = new StokeDebugPopulationAddressSpace(retailBus);
        var random = new ControlledRandom { Value = 0x0080 };
        SamusState samus = CreateSamus(retailBus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            DebugPopulationPointer,
            DebugEnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            random.Next,
            random.Set,
            random.Read,
            level,
            samus);

        RoomEnemySlot actor = enemies.Slots[0];
        if (actor.EnemyDefinitionPointer != DefinitionPointer ||
            enemies.StokeStates[actor.SlotIndex] is null)
        {
            throw new InvalidDataException("Cartridge debug population did not initialize Stoke.");
        }
        return new LoadedStoke(bus, enemies, actor, samus, level, random);
    }

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static RoomLevelData CreateFlatFloorLevel(bool includeImmediateLeftWall)
    {
        const int width = 0xff;
        const int height = 40;
        var foreground = new ushort[width * height];
        var behavior = new byte[foreground.Length];
        var background = new ushort[foreground.Length];

        // Stoke's debug Y=$01F8 and radius eight put its bottom at $01FF. A type-$8 row at
        // block Y=32 begins at $0200 and is therefore found by the actor's +2-pixel probe.
        for (int blockX = 0; blockX < width; blockX++)
            foreground[32 * width + blockX] = 0x8000;
        if (includeImmediateLeftWall)
            foreground[31 * width + 194] = 0x8000;

        return new RoomLevelData(
            width,
            height,
            foreground,
            behavior,
            background,
            ReadOnlySpan<byte>.Empty);
    }

    private static void StepActor(LoadedStoke loaded) =>
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: loaded.Level);

    private static void TriggerAttackOnNextFrame(LoadedStoke loaded)
    {
        // The native condition is byte(frame_counter + random) < 2. Choose the additive
        // inverse of the current low frame byte so the deterministic result is exactly zero.
        loaded.Random.Value = unchecked((byte)(0 - (byte)loaded.Actor.FrameCounter));
        StepActor(loaded);
        loaded.Random.Value = 0x0080;
    }

    private static StokeEnemyState State(LoadedStoke loaded) =>
        loaded.Enemies.StokeStates[loaded.Actor.SlotIndex] ??
        throw new InvalidDataException("Stoke's typed state disappeared after initialization.");

    private static RoomEnemyProjectileSlot SingleProjectile(RoomEnemySystem enemies)
    {
        RoomEnemyProjectileSlot[] active = enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .ToArray();
        if (active.Length != 1)
        {
            throw new InvalidDataException(
                $"Expected exactly one live Stoke projectile, found {active.Length}.");
        }
        return active[0];
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
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x8912 ||
            definition.Health != 20 || definition.Damage != 40 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.HurtSoundEffect != 0x0053 ||
            definition.InitializationAiPointer != 0x89ad || definition.PartCount != 1 ||
            definition.MainAiPointer != 0x89f0 || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.DeathAnimation != 0 || definition.PowerBombReactionPointer != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xacd000 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf3b6 ||
            definition.VulnerabilityPointer != 0xec1c || definition.NamePointer != 0xe15d)
        {
            throw new InvalidDataException("Retail Stoke header words do not match $A0:CEFF.");
        }
    }

    private static uint PackFixed(ushort position, ushort subposition) =>
        ((uint)position << 16) | subposition;

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed record LoadedStoke(
        ISnesAddressSpace Bus,
        RoomEnemySystem Enemies,
        RoomEnemySlot Actor,
        SamusState Samus,
        RoomLevelData Level,
        ControlledRandom Random);

    /// <summary>Controllable stand-in for WRAM random_number used only to select audit frames.</summary>
    private sealed class ControlledRandom
    {
        public ushort Value { get; set; }
        public ushort Next() => Value;
        public ushort Read() => Value;
        public void Set(ushort value) => Value = value;
    }

    /// <summary>
    /// Exposes Nintendo's bank-$B4 debug population through the bank-$A1 window consumed by
    /// the production room loader. All nineteen bytes (one record plus <c>$FFFF,quota</c>)
    /// are unchanged; writes remain delegated to the underlying cartridge address space.
    /// </summary>
    private sealed class StokeDebugPopulationAddressSpace : ISnesAddressSpace
    {
        private const int LoaderPopulationAddress = 0xa1e9d5;
        private const int DebugPopulationAddress = 0xb4e9d5;
        private const int PopulationByteLength = 19;
        private readonly ISnesAddressSpace _inner;

        public StokeDebugPopulationAddressSpace(ISnesAddressSpace inner) =>
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public byte ReadByte(int address)
        {
            int offset = address - LoaderPopulationAddress;
            return (uint)offset < PopulationByteLength
                ? _inner.ReadByte(DebugPopulationAddress + offset)
                : _inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => _inner.WriteByte(address, value);
    }
}
