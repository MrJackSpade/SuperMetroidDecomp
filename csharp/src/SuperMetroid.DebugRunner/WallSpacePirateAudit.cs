using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

/// <summary>
/// ROM-backed end-to-end audit for all six wall Space Pirate headers, the Climb's untouched
/// eleven-Pirate population, and the Pit's real fast-jump record. No actor/list/map fixture is
/// synthesized: the private retail cartridge remains authoritative for every audited word.
/// </summary>
internal static class WallSpacePirateAudit
{
    private const ushort ClimbRoomPointer = 0x96ba;
    private const ushort PitRoomPointer = 0x975c;
    private const ushort FastPillarsRoomPointer = 0xb3a5;

    private static readonly ushort[] Definitions =
        [0xf353, 0xf393, 0xf3d3, 0xf413, 0xf453, 0xf493];
    private static readonly ushort[] Health = [20, 90, 200, 900, 300, 500];
    private static readonly ushort[] Damage = [15, 20, 80, 200, 160, 15];
    private static readonly ushort[] PowerBombAi = [0x8767, 0, 0, 0, 0, 0];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);

        // Event zero is the post-Ceres awakening event. The Climb's second state is the
        // ordinary Zebes state containing wall Pirates; the default state is empty/ruined.
        var awakeEvents = new byte[] { 1 };
        CartridgeRoomHeader climb = CartridgeRoomHeader.Load(
            bus,
            ClimbRoomPointer,
            new RoomStateSelectionContext(awakeEvents, 0, false, false));
        CartridgeRoomAssets climbAssets = CartridgeRoomAssets.Load(bus, climb);

        VerifyDefinitionsAndRomLists(bus, climb);
        VerifyUntouchedClimbPopulation(bus, climb, climbAssets);
        ClimbResult left = VerifyClimbDirectionFamily(
            bus, climb, climbAssets, actorIndex: 0, onRightWall: false);
        ClimbResult right = VerifyClimbDirectionFamily(
            bus, climb, climbAssets, actorIndex: 1, onRightWall: true);
        AttackResult slowAttack = VerifySlowAttackJumpAndLasers(bus, climb, climbAssets);
        VerifyFastRetailBranch(bus);
        VerifyCombat(bus, climb, climbAssets);

        Console.WriteLine(
            "Wall Space Pirate audit passed: all six retail headers share exact " +
            "$B2:EF9F/$F02D AI; the Climb loaded all eleven untouched actors; left/right " +
            $"wall climbing covered {left.MapCount + right.MapCount} ROM maps and both " +
            $"collision/random directions across Y ${Math.Min(left.MinimumY, right.MinimumY):X4}-" +
            $"${Math.Max(left.MaximumY, right.MaximumY):X4}; both firing/jump arcs, landing " +
            $"lists, $66/$67 sounds, {slowAttack.SpawnedLasers} lasers, native A050 " +
            "immediate motion, slow 2-px and real Pit-record fast 4-px branches passed; " +
            "body/laser contact, frozen touch, beam, normal-bomb callback/immunity, " +
            "power-bomb damage/vulnerability, " +
            "grapple, and extended " +
            "spritemap rendering passed.");
        return 0;
    }

    private static void VerifyDefinitionsAndRomLists(
        ISnesAddressSpace bus,
        CartridgeRoomHeader climb)
    {
        if (climb.State.Pointer != 0x96eb || climb.WidthInScreens != 3 ||
            climb.HeightInScreens != 9)
        {
            throw new InvalidDataException(
                $"Climb state mismatch: state=${climb.State.Pointer:X4}, " +
                $"size={climb.WidthInScreens}x{climb.HeightInScreens}.");
        }

        for (int index = 0; index < Definitions.Length; index++)
        {
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definitions[index]);
            if (definition.TileDataSize != 0x0c00 || definition.Health != Health[index] ||
                definition.Damage != Damage[index] || definition.XRadius != 0x0010 ||
                definition.YRadius != 0x0018 || definition.Bank != 0xb2 ||
                definition.HurtAiTime != 0 || definition.InitializationAiPointer != 0xef9f ||
                definition.PartCount != 1 || definition.MainAiPointer != 0xf02d ||
                definition.GrappleAiPointer != 0x800f || definition.HurtAiPointer != 0x804c ||
                definition.FrozenAiPointer != 0x8041 || definition.DeathAnimation != 4 ||
                definition.PowerBombReactionPointer != PowerBombAi[index] ||
                definition.TouchAiPointer != 0x876c || definition.ShotAiPointer != 0x8779 ||
                definition.TileDataAddress == 0 || definition.Layer != 5 ||
                definition.ItemDropChancesPointer == 0 ||
                definition.VulnerabilityPointer == 0 || definition.NamePointer == 0)
            {
                throw new InvalidDataException(
                    $"Wall Space Pirate header $A0:{Definitions[index]:X4} does not match " +
                    "its retail family contract.");
            }
        }

        // These sentinels prove the audit is attached to the expected instruction encoding,
        // not merely to labels remembered from a disassembly. They span attack, all four
        // climb directions, collision, random selection, arc preparation, fire, and dispatch.
        VerifyWords(bus, 0xb2ecc0, [0xef83, 0xf0e3, 0x0009]);
        VerifyWords(bus, 0xb2ecec, [0xef83, 0xf034, 0x8123, 0x0004]);
        VerifyWords(bus, 0xb2ed36, [0xef83, 0xf034, 0x8123, 0x0004]);
        VerifyWords(bus, 0xb2ed80, [0xef83, 0xf04f, 0x0009]);
        VerifyWords(bus, 0xb2edac, [0xef83, 0xf0c8, 0x8123, 0x0004]);
        VerifyWords(bus, 0xb2edf6, [0xef83, 0xf0c8, 0x8123, 0x0004]);
        VerifyWords(bus, 0xb2ecf8, [0xee40, 0xfffd, 0x0008]);
        VerifyWords(bus, 0xb2eccc, [0xef2a, 0x813a, 0x0020]);
        VerifyWords(bus, 0xb2ed92, [0xeed4, 0xef83, 0xf050]);
        VerifyWords(bus, 0x86a17b,
            [0xa009, 0xa05c, 0x9f41, 0x0410, 0x100a, 0x0000, 0x84fc]);
    }

    private static void VerifyUntouchedClimbPopulation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedWallPirates loaded = Load(bus, room, assets, 0, 0);
        if (loaded.Enemies.EnemyCount != 11 || loaded.Pirates.Count != 11)
            throw new InvalidDataException("The Climb did not load its eleven wall Pirates.");

        ushort[] rawX =
            [0x0130, 0x01d0, 0x0130, 0x0130, 0x01d0, 0x0130,
             0x01d0, 0x012d, 0x01d0, 0x01d0, 0x0130];
        ushort[] expectedY =
            [0x00d8, 0x0128, 0x01d8, 0x0338, 0x03d8, 0x04b8,
             0x05a8, 0x0698, 0x0278, 0x0708, 0x07c8];

        for (int index = 0; index < loaded.Pirates.Count; index++)
        {
            RoomEnemySlot actor = loaded.Pirates[index];
            WallSpacePirateEnemyState state = State(loaded.Enemies, actor);
            bool startsRight = (actor.Parameter1 & 1) != 0;
            ushort snappedX = (rawX[index] & 0x000f) < 11
                ? unchecked((ushort)(rawX[index] & 0xfff8))
                : unchecked((ushort)((rawX[index] & 0xfff0) + 16));
            if (actor.XPosition != snappedX || actor.YPosition != expectedY[index] ||
                actor.Parameter2 != 0x00a0 || actor.Properties != 0x2000 ||
                actor.ExtraProperties != 0x0004 || actor.Health != 20 ||
                actor.CurrentInstruction != (startsRight ? 0xedac : 0xed36) ||
                state.Function != (startsRight
                    ? WallSpacePirateFunction.ClimbingRightWall
                    : WallSpacePirateFunction.ClimbingLeftWall) ||
                state.ClimbDirection != WallSpacePirateClimbDirection.Down ||
                state.RightJumpTargetAngle != 190 || state.LeftJumpTargetAngle != 66 ||
                state.JumpAngleDelta != 2 || state.SpawnedLaserCount != 0)
            {
                throw new InvalidDataException(
                    $"Climb wall Pirate {index} initialization mismatch at " +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), list=" +
                    $"${actor.CurrentInstruction:X4}, function=${(ushort)state.Function:X4}.");
            }
        }
    }

    private static ClimbResult VerifyClimbDirectionFamily(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int actorIndex,
        bool onRightWall)
    {
        LoadedWallPirates loaded = Load(bus, room, assets, 0, 0);
        RoomEnemySlot actor = KeepOnly(loaded, actorIndex);
        var maps = new HashSet<ushort>();
        var directions = new HashSet<WallSpacePirateClimbDirection>();
        ushort minimumY = actor.YPosition;
        ushort maximumY = actor.YPosition;

        for (int frame = 0; frame < 1800; frame++)
        {
            // Keep Samus exactly 256 pixels away so no firing branch can replace climbing,
            // and follow the actor with the camera so off-screen processing never masks AI.
            loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 0x0100));
            ushort cameraY = actor.YPosition > 0x0080
                ? unchecked((ushort)(actor.YPosition - 0x0080))
                : (ushort)0;
            loaded.Enemies.StepFrame(
                cameraX: 0x0100,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData);
            maps.Add(actor.SpritemapPointer);
            directions.Add(State(loaded.Enemies, actor).ClimbDirection);
            minimumY = Math.Min(minimumY, actor.YPosition);
            maximumY = Math.Max(maximumY, actor.YPosition);
        }

        int[] frameAddresses = onRightWall
            ? [0xb2edb4, 0xb2edbc, 0xb2edc4, 0xb2edcc, 0xb2edd4, 0xb2eddc,
               0xb2ede4, 0xb2edec, 0xb2edfe, 0xb2ee06, 0xb2ee0e, 0xb2ee16,
               0xb2ee1e, 0xb2ee26, 0xb2ee2e, 0xb2ee36]
            : [0xb2ecf4, 0xb2ecfc, 0xb2ed04, 0xb2ed0c, 0xb2ed14, 0xb2ed1c,
               0xb2ed24, 0xb2ed2c, 0xb2ed3e, 0xb2ed46, 0xb2ed4e, 0xb2ed56,
               0xb2ed5e, 0xb2ed66, 0xb2ed6e, 0xb2ed76];
        HashSet<ushort> expectedMaps = ReadMapPointers(bus, frameAddresses);
        if (!expectedMaps.IsSubsetOf(maps) || minimumY == maximumY ||
            !directions.SetEquals(
                [WallSpacePirateClimbDirection.Down, WallSpacePirateClimbDirection.Up]))
        {
            throw new InvalidDataException(
                $"Wall Pirate {(onRightWall ? "right" : "left")} climbing failed: " +
                $"Y=${minimumY:X4}-${maximumY:X4}, directions=" +
                $"{string.Join(',', directions)}, missing maps=" +
                $"{string.Join(',', expectedMaps.Except(maps).Order())}.");
        }

        return new ClimbResult(minimumY, maximumY, maps.Count);
    }

    private static AttackResult VerifySlowAttackJumpAndLasers(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedWallPirates loaded = Load(bus, room, assets, 0, 0);
        RoomEnemySlot actor = KeepOnly(loaded, 0);
        var functions = new HashSet<WallSpacePirateFunction>();
        var maps = new HashSet<ushort>();
        var directions = new HashSet<ushort>();
        bool heardLaser = false;
        bool heardJump = false;
        bool sawImmediateTwoPixelMove = false;
        bool sawSteadyTwoPixelMove = false;
        ushort minimumX = actor.XPosition;
        ushort maximumX = actor.XPosition;

        for (int frame = 0; frame < 900; frame++)
        {
            loaded.Samus.XPosition = actor.XPosition;
            loaded.Samus.YPosition = actor.YPosition;
            loaded.Enemies.StepFrame(
                cameraX: 0x0100,
                cameraY: 0x0050,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            functions.Add(State(loaded.Enemies, actor).Function);
            maps.Add(actor.SpritemapPointer);
            minimumX = Math.Min(minimumX, actor.XPosition);
            maximumX = Math.Max(maximumX, actor.XPosition);
            heardLaser |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0067;
            heardJump |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0066;

            var before = loaded.Enemies.EnemyProjectiles.ToDictionary(
                projectile => projectile.SlotIndex,
                projectile => (projectile.Kind, projectile.XPosition, projectile.PreInstruction));
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser)
                    directions.Add(projectile.DirectionParameter);
            }

            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                loaded.Samus,
                cameraX: 0x0100,
                cameraY: 0x0050);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (!projectile.IsActive ||
                    projectile.Kind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                    continue;
                (RoomEnemyProjectileKind oldKind, ushort oldX, ushort oldPre) =
                    before[projectile.SlotIndex];
                if (oldKind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                    continue;
                int delta = Math.Abs(unchecked((short)(projectile.XPosition - oldX)));
                sawImmediateTwoPixelMove |= oldPre == 0xa05b && delta == 2;
                sawSteadyTwoPixelMove |= oldPre is 0xa05c or 0xa07a && delta == 2;
            }
        }

        HashSet<ushort> expectedMaps = ReadMapPointers(
            bus,
            [0xb2ecc4, 0xb2ecc8, 0xb2ecda, 0xb2ecde, 0xb2ece8,
             0xb2ed84, 0xb2ed88, 0xb2ed9a, 0xb2ed9e, 0xb2eda8]);
        WallSpacePirateEnemyState state = State(loaded.Enemies, actor);
        if (!expectedMaps.IsSubsetOf(maps) ||
            !functions.Contains(WallSpacePirateFunction.WallJumpingRight) ||
            !functions.Contains(WallSpacePirateFunction.WallJumpingLeft) ||
            !directions.SetEquals([(ushort)0, (ushort)1]) ||
            state.SpawnedLaserCount < 2 || !heardLaser || !heardJump ||
            !sawImmediateTwoPixelMove || !sawSteadyTwoPixelMove ||
            minimumX == maximumX)
        {
            throw new InvalidDataException(
                $"Wall Pirate slow attack failed: X=${minimumX:X4}-${maximumX:X4}, " +
                $"functions={string.Join(',', functions)}, directions=" +
                $"{string.Join(',', directions)}, lasers={state.SpawnedLaserCount}, " +
                $"sounds={heardLaser}/{heardJump}, motion=" +
                $"{sawImmediateTwoPixelMove}/{sawSteadyTwoPixelMove}, missing maps=" +
                $"{string.Join(',', expectedMaps.Except(maps).Order())}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, 0x0100, 0x0050, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Wall Pirate extended map emitted no OBJ pieces.");

        return new AttackResult(state.SpawnedLaserCount);
    }

    private static void VerifyFastRetailBranch(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader pit = CartridgeRoomHeader.Load(
            bus,
            PitRoomPointer,
            new RoomStateSelectionContext(default, 0, true, false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, pit);
        LoadedWallPirates loaded = Load(bus, pit, assets, 0, 0);
        RoomEnemySlot actor = loaded.Pirates.Single();
        foreach (RoomEnemySlot other in loaded.Enemies.Slots.Where(
            slot => slot.EnemyDefinitionPointer != 0 && !ReferenceEquals(slot, actor)))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);

        WallSpacePirateEnemyState state = State(loaded.Enemies, actor);
        if (pit.State.Pointer != 0x9787 || actor.XPosition != 0x02d0 ||
            actor.Parameter1 != 0x0001 || actor.Parameter2 != 0x0020 ||
            state.Function != WallSpacePirateFunction.ClimbingRightWall ||
            state.RightJumpTargetAngle != 192 || state.LeftJumpTargetAngle != 64 ||
            state.JumpAngleDelta != 4)
        {
            throw new InvalidDataException(
                $"Pit fast wall Pirate initialization mismatch: state=${pit.State.Pointer:X4}, " +
                $"position=${actor.XPosition:X4}, params=${actor.Parameter1:X4}/" +
                $"${actor.Parameter2:X4}, targets={state.RightJumpTargetAngle}/" +
                $"{state.LeftJumpTargetAngle}/{state.JumpAngleDelta}.");
        }

        bool sawFourPixelMotion = false;
        for (int frame = 0; frame < 420 && !sawFourPixelMotion; frame++)
        {
            loaded.Samus.XPosition = actor.XPosition;
            loaded.Samus.YPosition = actor.YPosition;
            loaded.Enemies.StepFrame(
                0x0250, 0, false, loaded.Samus, level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            var before = loaded.Enemies.EnemyProjectiles.ToDictionary(
                projectile => projectile.SlotIndex,
                projectile => (projectile.Kind, projectile.XPosition));
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData, null, cameraX: 0x0250, cameraY: 0);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                (RoomEnemyProjectileKind kind, ushort x) = before[projectile.SlotIndex];
                if (projectile.IsActive && kind == RoomEnemyProjectileKind.PirateMotherBrainLaser &&
                    Math.Abs(unchecked((short)(projectile.XPosition - x))) == 4)
                    sawFourPixelMotion = true;
            }
        }
        if (!sawFourPixelMotion)
            throw new InvalidDataException("Pit wall Pirate never exercised 4-pixel laser motion.");
    }

    private static void VerifyCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedWallPirates body = Load(bus, room, assets, 0, 0);
        RoomEnemySlot bodyActor = KeepOnly(body, 0);
        Prime(body, assets, bodyActor);
        body.Samus.XPosition = bodyActor.XPosition;
        body.Samus.YPosition = bodyActor.YPosition;
        body.Samus.InvincibilityTimer = 0;
        if (!body.Enemies.ResolveOrdinarySamusContact(body.Samus, 0) ||
            body.Samus.Health != 984 || !body.Samus.KnockbackActive)
            throw new InvalidDataException("Grey wall Pirate body contact did not deal 15 damage.");

        LoadedWallPirates frozen = Load(bus, room, assets, 0, 0);
        RoomEnemySlot frozenActor = KeepOnly(frozen, 0);
        // Freeze only after the initialization cursor has installed a real bank-$B2 map.
        // `$804F` is a common-bank empty-map sentinel during allocation, not a Wall Pirate
        // frame that native gameplay can collide while the family is already frozen.
        Prime(frozen, assets, frozenActor);
        frozenActor.FrozenTimer = 2;
        frozen.Samus.XPosition = frozenActor.XPosition;
        frozen.Samus.YPosition = frozenActor.YPosition;
        frozen.Samus.InvincibilityTimer = 0;
        if (!frozen.Enemies.ResolveOrdinarySamusContact(frozen.Samus, 0) ||
            frozen.Samus.Health != 999)
            throw new InvalidDataException("Frozen wall Pirate incorrectly damaged Samus.");

        LoadedWallPirates shot = Load(bus, room, assets, 0, 0);
        RoomEnemySlot shotActor = KeepOnly(shot, 0);
        Prime(shot, assets, shotActor);
        // Grey wall Pirates have exactly 20 HP and the retail uncharged Power Beam deals
        // exactly 20 damage. Use those literal cartridge values here: the old audit called
        // a synthetic 300-damage missile a “beam”, masking both the legitimate one-shot
        // result and any stale HUD-family bug in the real producer.
        ArmPowerBeam(shot.Projectiles.Slots[0], shotActor.XPosition, shotActor.YPosition);
        if (shot.Enemies.ResolveOrdinaryProjectileHits(
                bus, shot.Projectiles, shot.SharedProjectiles, shot.Samus) != 1 ||
            shotActor.EnemyDefinitionPointer != 0 || shotActor.Health != 0 ||
            shotActor.VariableB != 0)
            throw new InvalidDataException("Wall Pirate lethal projectile damage failed.");
        VerifyDeathExplosionPickupAndSound(shot, assets);

        LoadedWallPirates bomb = Load(bus, room, assets, 0, 0);
        RoomEnemySlot bombActor = KeepOnly(bomb, 0);
        Prime(bomb, assets, bombActor);
        SamusBombProjectileSlot normalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bomb.SharedProjectiles,
                bombActor.XPosition,
                bombActor.YPosition,
                damage: 1000);
        if (bomb.Enemies.ResolveOrdinaryBombHits(
                bomb.SharedProjectiles, bomb.Projectiles, bomb.Samus) != 1 ||
            (normalBomb.Direction & 0x0010) == 0 || bombActor.Health != 20 ||
            bombActor.InvincibilityTimer != 0 || bombActor.FlashTimer != 0 ||
            bombActor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                "Wall Pirate normal bomb did not run its selected bank-$B2 hitbox " +
                "callback through the grey variant's zero-vulnerability common path.");
        }

        LoadedWallPirates powerBomb = Load(bus, room, assets, 0, 0);
        RoomEnemySlot powerBombActor = KeepOnly(powerBomb, 0);
        int powerBombHits = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
            bus, powerBombActor.XPosition, powerBombActor.YPosition, 64);
        if (powerBombHits != 1 || powerBombActor.EnemyDefinitionPointer != 0 ||
            powerBombActor.Health != 0 ||
            !powerBombActor.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            // The header's `$8767` reaction enters common power-bomb AI. Byte fifteen in
            // the grey vulnerability record is nonzero, so its 20 health is exhausted and
            // bank $A0 also leaves the native process-off-screen bit on the deleted actor.
            throw new InvalidDataException(
                $"Grey wall Pirate power-bomb damage failed: hits={powerBombHits}, " +
                $"health={powerBombActor.Health}, properties=${(ushort)powerBombActor.Properties:X4}.");
        }
        VerifyGoldPowerBombDamage(bus);

        LoadedWallPirates grapple = Load(bus, room, assets, 0, 0);
        RoomEnemySlot grappleActor = KeepOnly(grapple, 0);
        Prime(grapple, assets, grappleActor);
        GrappleEnemyCollision grappleResult = grapple.Enemies.ResolveGrappleEndpoint(
            grappleActor.XPosition, grappleActor.YPosition);
        if (!grappleResult.Collided || grappleResult.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Wall Pirate grapple cancel failed.");

        LoadedWallPirates laser = Load(bus, room, assets, 0, 0);
        RoomEnemySlot laserSource = KeepOnly(laser, 0);
        RoomEnemyProjectileSlot? laserActor = null;
        for (int frame = 0; frame < 180 && laserActor is null; frame++)
        {
            laser.Samus.XPosition = laserSource.XPosition;
            laser.Samus.YPosition = laserSource.YPosition;
            laser.Enemies.StepFrame(
                0x0100, 0x0050, false, laser.Samus, level: assets.LevelData,
                samusProjectiles: laser.Projectiles);
            laserActor = laser.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser);
        }
        if (laserActor is null)
            throw new InvalidDataException("Wall Pirate never spawned a contact-test laser.");
        foreach (RoomEnemyProjectileSlot other in laser.Enemies.EnemyProjectiles)
        {
            if (!ReferenceEquals(other, laserActor))
                other.Clear();
        }
        laser.Samus.XPosition = laserActor.XPosition;
        laser.Samus.YPosition = laserActor.YPosition;
        laser.Samus.InvincibilityTimer = 0;

        // Advance the ROM list to a real map and prove the complete room-gfx-zero path
        // resolves to non-black pixels through standard OBJ VRAM and the authored palette.
        for (int frame = 0;
             frame < 8 && laserActor.SpritemapPointer is 0 or 0x8000;
             frame++)
        {
            laser.Enemies.StepEnemyProjectiles(
                assets.LevelData, samus: null, cameraX: 0x0100, cameraY: 0x0050);
        }
        AssertProjectileRendersVisibleColor(
            laser,
            RoomEnemyProjectileKind.PirateMotherBrainLaser,
            "Wall Pirate laser");

        laser.Enemies.StepEnemyProjectiles(
            assets.LevelData, laser.Samus, cameraX: 0x0100, cameraY: 0x0050);
        if (laser.Samus.Health != 984 || laserActor.IsActive)
            throw new InvalidDataException("Wall Pirate laser contact did not deal 15 damage.");
    }

    private static void VerifyGoldPowerBombDamage(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, FastPillarsRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedWallPirates loaded = Load(bus, room, assets, 0, 0);
        RoomEnemySlot actor = KeepOnly(loaded, 0);
        if (room.State.Pointer != 0xb3b2 || actor.EnemyDefinitionPointer != 0xf413 ||
            actor.Health != 900)
        {
            throw new InvalidDataException(
                $"Fast Pillars gold wall Pirate mismatch: state=${room.State.Pointer:X4}, " +
                $"definition=${actor.EnemyDefinitionPointer:X4}, health={actor.Health}.");
        }

        // Gold's header has a zero reaction pointer but vulnerability byte two. The generic
        // power-bomb path is the literal behavior: 100 * 2 damage and standard timers.
        if (loaded.Enemies.ResolveOrdinaryPowerBombHits(
                bus, actor.XPosition, actor.YPosition, 64) != 1 ||
            actor.Health != 700 || actor.InvincibilityTimer != 48 || actor.FlashTimer != 12 ||
            !actor.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Gold wall Pirate power-bomb damage failed.");
        }

        // WriteEnemyOams chooses temporary OBJ palette zero on the shared counter's
        // `& 2` phase, then restores the graphics-set palette on the other phase. Exercise
        // that common draw path with a surviving Pirate so a black/deleted explosion cannot
        // masquerade as a successful hurt flash.
        ushort cameraX = actor.XPosition > 0x0080
            ? unchecked((ushort)(actor.XPosition - 0x0080))
            : (ushort)0;
        ushort cameraY = actor.YPosition > 0x0070
            ? unchecked((ushort)(actor.YPosition - 0x0070))
            : (ushort)0;
        bool sawPaletteZeroFlash = false;
        bool sawNormalPalette = false;
        for (int frame = 0; frame < 6; frame++)
        {
            loaded.Samus.XPosition = unchecked((ushort)(actor.XPosition + 0x0100));
            loaded.Samus.YPosition = actor.YPosition;
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            var oam = new OamBuffer();
            oam.BeginFrame();
            loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
            oam.FinalizeFrame();
            for (int sprite = 0; sprite < oam.LastFinalizedSpriteCount; sprite++)
            {
                byte palette = unchecked((byte)oam.GetEntry(sprite).Palette);
                sawPaletteZeroFlash |= palette == 0;
                sawNormalPalette |= palette == unchecked((byte)(actor.PaletteIndex >> 9));
            }
        }
        if (!sawPaletteZeroFlash || !sawNormalPalette)
        {
            throw new InvalidDataException(
                $"Gold wall Pirate did not alternate common hurt palettes: " +
                $"zero/normal={sawPaletteZeroFlash}/{sawNormalPalette}.");
        }
    }

    private static LoadedWallPirates Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX,
        ushort samusY)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();

        // RoomEnemySystem intentionally owns only the room-selected enemy sheets. The
        // laser, common death explosion, and eventual pickup use graphics index zero and
        // therefore address the standard OBJ sheet installed by gameplay setup. Seed both
        // immutable gameplay resources before loading the room so visual assertions below
        // exercise the same complete PPU state as the desktop runtime.
        cgram.LoadFromBus(bus, 0x9a8000);
        cgram.LoadFromBus(bus, 0x9afc00, colorCount: 16, destinationIndex: 128);
        vram.LoadBytes(
            destinationByteOffset: 0xc000,
            RomDataReader.ReadFixedBank(bus, 0x9ad200, 0x2e00));
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            XPosition = samusX,
            YPosition = samusY,
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
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        List<RoomEnemySlot> pirates = enemies.Slots
            .Where(slot => Definitions.Contains(slot.EnemyDefinitionPointer))
            .ToList();
        return new LoadedWallPirates(
            enemies,
            samus,
            pirates,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            vram,
            cgram);
    }

    private static RoomEnemySlot KeepOnly(LoadedWallPirates loaded, int actorIndex)
    {
        RoomEnemySlot actor = loaded.Pirates[actorIndex];
        foreach (RoomEnemySlot other in loaded.Enemies.Slots.Where(
            slot => slot.EnemyDefinitionPointer != 0 && !ReferenceEquals(slot, actor)))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);
        return actor;
    }

    private static void Prime(
        LoadedWallPirates loaded,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        loaded.Samus.YPosition = unchecked((ushort)(actor.YPosition + 0x0100));
        loaded.Enemies.StepFrame(
            0x0100,
            actor.YPosition > 0x0080 ? unchecked((ushort)(actor.YPosition - 0x0080)) : (ushort)0,
            false,
            loaded.Samus,
            level: assets.LevelData,
            samusProjectiles: loaded.Projectiles);
    }

    private static WallSpacePirateEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.WallSpacePirateStates[actor.SlotIndex] ??
        throw new InvalidDataException(
            $"Wall Pirate slot {actor.SlotIndex} did not initialize typed state.");

    private static void ArmPowerBeam(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y)
    {
        projectile.ClearFields();
        projectile.Type = 0x0000;
        projectile.Damage = 20;
        projectile.Direction = 2;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void VerifyDeathExplosionPickupAndSound(
        LoadedWallPirates loaded,
        CartridgeRoomAssets assets)
    {
        bool renderedExplosion = false;
        bool sawDeathSoundOpcode = false;
        bool sawPublishedDeathSound = false;
        var maps = new HashSet<ushort>();
        var preInstructions = new HashSet<ushort>();

        for (int frame = 0; frame < 180; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                loaded.Samus,
                cameraX: 0x0100,
                cameraY: 0x0050);
            sawDeathSoundOpcode |= loaded.Enemies.LastEnemyDeathSoundEffectLibrary2.HasValue;

            // Big Pirate death variant four is composed primarily from the separate
            // bank-$B4 sprite-object pool. DrawEnemyProjectiles deliberately emits that
            // pool first, so test the resulting pixels rather than assuming the carrier
            // eproj itself must expose a timed spritemap on every frame.
            renderedExplosion |= ProjectileLayerHasVisibleColor(loaded);

            RoomEnemyProjectileSlot? actor = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.IsActive &&
                    projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
            if (actor is not null && actor.SpritemapPointer is not 0 and not 0x8000)
            {
                maps.Add(actor.SpritemapPointer);
                preInstructions.Add(actor.PreInstruction);
                AssertProjectileRendersVisibleColor(
                    loaded,
                    RoomEnemyProjectileKind.EnemyDeathExplosion,
                    "Wall Pirate death explosion/pickup");
                renderedExplosion = true;
            }

            // EnemyMain publishes bank-$86's preceding-frame death opcode through the
            // same append-only queue consumed by the desktop audio bridge.
            loaded.Enemies.StepFrame(
                0x0100,
                0x0050,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            sawPublishedDeathSound |= loaded.Enemies.SoundRequests.Any(request =>
                request.Library == SoundEffectLibrary.Library2 && request.MaximumQueued == 1 &&
                request.SoundId is 9 or 0x24 or 0x0b);

            if (!loaded.Enemies.EnemyProjectiles.Any(projectile => projectile.IsActive))
                break;
        }

        bool renderedPickup = VerifyDirectPickupRendering(loaded, assets);
        if (!renderedExplosion || !renderedPickup ||
            !sawDeathSoundOpcode || !sawPublishedDeathSound)
        {
            throw new InvalidDataException(
                $"Wall Pirate death integration failed: visible explosion/pickup=" +
                $"{renderedExplosion}/{renderedPickup}, death sound opcode/published=" +
                $"{sawDeathSoundOpcode}/{sawPublishedDeathSound}, maps=" +
                $"[{string.Join(',', maps.Select(value => $"${value:X4}"))}], pre=" +
                $"[{string.Join(',', preInstructions.Select(value => $"${value:X4}"))}].");
        }
    }

    private static bool VerifyDirectPickupRendering(
        LoadedWallPirates loaded,
        CartridgeRoomAssets assets)
    {
        // Force the cartridge's low-energy branch so the grey Pirate's real chance table
        // has an enabled pickup family. A fully stocked Samus can legitimately renormalize
        // the same random samples into NoDrop, which is not a rendering failure.
        loaded.Samus.Health = 1;
        for (int attempt = 0; attempt < 18; attempt++)
        {
            RoomEnemyProjectileSlot? pickup = loaded.Enemies.SpawnEnemyDropFromEnemyHeader(
                x: 0x0130,
                y: 0x00d8,
                enemyHeaderPointer: Definitions[0]);
            if (pickup is null)
                break;
            if (pickup.PreInstruction != 0xefe0)
                continue;

            for (int frame = 0;
                 frame < 8 && pickup.SpritemapPointer is 0 or 0x8000;
                 frame++)
            {
                loaded.Enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    loaded.Samus,
                    cameraX: 0x0100,
                    cameraY: 0x0050);
            }
            AssertProjectileRendersVisibleColor(
                loaded,
                RoomEnemyProjectileKind.EnemyDeathPickup,
                "Wall Pirate pickup");
            return true;
        }

        return false;
    }

    private static bool ProjectileLayerHasVisibleColor(LoadedWallPirates loaded)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX: 0x0100, cameraY: 0x0050);
        oam.FinalizeFrame();
        return SnesObjRenderer.Render(oam, loaded.Vram, loaded.Cgram, obsel: 0x03)
            .Any(pixel => pixel.A != 0 && (pixel.R != 0 || pixel.G != 0 || pixel.B != 0));
    }

    private static void AssertProjectileRendersVisibleColor(
        LoadedWallPirates loaded,
        RoomEnemyProjectileKind kind,
        string description)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX: 0x0100, cameraY: 0x0050);
        oam.FinalizeFrame();
        Rgba32[] pixels = SnesObjRenderer.Render(
            oam,
            loaded.Vram,
            loaded.Cgram,
            obsel: 0x03);
        int opaque = pixels.Count(pixel => pixel.A != 0);
        int colored = pixels.Count(pixel =>
            pixel.A != 0 && (pixel.R != 0 || pixel.G != 0 || pixel.B != 0));
        if (opaque == 0 || colored == 0)
        {
            RoomEnemyProjectileSlot? projectile = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                candidate => candidate.IsActive && candidate.Kind == kind);
            throw new InvalidDataException(
                $"{description} rendered {opaque} opaque/{colored} colored pixels " +
                $"(map=${projectile?.SpritemapPointer ?? 0:X4}, " +
                $"gfx=${projectile?.GraphicsIndex ?? 0:X4}, OAM={oam.LastFinalizedSpriteCount}).");
        }
    }

    private static HashSet<ushort> ReadMapPointers(
        ISnesAddressSpace bus,
        IEnumerable<int> frameAddresses) =>
        frameAddresses.Select(address => ReadWord(bus, address + 2)).ToHashSet();

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

    private readonly record struct LoadedWallPirates(
        RoomEnemySystem Enemies,
        SamusState Samus,
        IReadOnlyList<RoomEnemySlot> Pirates,
        SamusProjectileSystem Projectiles,
        SamusBombProjectileSystem SharedProjectiles,
        SnesVram Vram,
        SnesCgram Cgram);

    private readonly record struct ClimbResult(
        ushort MinimumY,
        ushort MaximumY,
        int MapCount);

    private readonly record struct AttackResult(int SpawnedLasers);
}
