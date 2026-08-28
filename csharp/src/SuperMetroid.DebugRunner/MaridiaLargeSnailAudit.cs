using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the three Oums at the start of room $8F:D913. Three unrelated
/// Scisers follow them, so the DebugRunner-only address-space decorator inserts a native
/// population terminator after the unchanged Oum prefix. Enemy code, instruction lists,
/// extended hitboxes, graphics, and collision terrain all remain private cartridge data.
/// </summary>
internal static class MaridiaLargeSnailAudit
{
    private const ushort DefinitionPointer = 0xd37f;
    private const ushort RoomPointer = 0xd913;
    private const ushort ExpectedStatePointer = 0xd920;
    private const ushort ExpectedPopulationPointer = 0xcf2d;
    private const ushort CameraX = 0x0200;
    private const ushort CameraY = 0x0180;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(retailBus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(retailBus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer)
        {
            throw new InvalidDataException(
                $"Oum room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}.");
        }

        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        VerifyInitialization(loaded);
        VerifyBounceRollAttackAndDrawing(loaded, assets.LevelData);
        VerifyProtectedAndDamagingContact(retailBus, room, assets);
        VerifyProtectedAndHandledShots(retailBus, room, assets);

        Console.WriteLine(
            "Maridia Large Snail audit passed: three retail Oums load; their ROM lists, " +
            "three-impact bounce, terrain movement, rotation-gated attack, extended " +
            "contact hitboxes, protected/damaging shot callbacks, sound tails, and OBJ " +
            "drawing all advance without synthetic enemy data.");
        return 0;
    }

    private static void VerifyInitialization(LoadedRoom loaded)
    {
        ushort[] expectedX = [0x0250, 0x02d0, 0x0370];
        if (loaded.Enemies.EnemyCount != 3)
            throw new InvalidDataException($"Expected three Oums, got {loaded.Enemies.EnemyCount}.");

        for (int index = 0; index < expectedX.Length; index++)
        {
            RoomEnemySlot slot = loaded.Enemies.Slots[index];
            MaridiaLargeSnailEnemyState state = GetState(loaded.Enemies, index);
            if (slot.EnemyDefinitionPointer != DefinitionPointer ||
                slot.XPosition != expectedX[index] || slot.YPosition != 0x0260 ||
                slot.Health != 300 ||
                slot.XRadius != 16 || slot.YRadius != 16 ||
                slot.CurrentInstruction != 0xca4b ||
                state.Function != MaridiaLargeSnailEnemyFunction.Idle ||
                state.BounceFunction != MaridiaLargeSnailBounceFunction.Falling ||
                state.RemainingBounces != 3 || state.YSpeedTableIndex != 0 ||
                state.AttackCooldown != 0x0080 || state.MovingLeft ||
                state.RequestedInstructionListIndex != 0 ||
                state.InstalledInstructionListIndex != 0)
            {
                throw new InvalidDataException(
                    $"Oum slot {index} initialization mismatch: position=" +
                    $"({slot.XPosition:X4},{slot.YPosition:X4}), health=" +
                    $"{slot.Health}, list=${slot.CurrentInstruction:X4}, " +
                    $"function/bounce=${(ushort)state.Function:X4}/" +
                    $"${(ushort)state.BounceFunction:X4}, remaining={state.RemainingBounces}.");
            }
        }
    }

    private static void VerifyBounceRollAttackAndDrawing(LoadedRoom loaded, RoomLevelData level)
    {
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        MaridiaLargeSnailEnemyState state = GetState(loaded.Enemies, 0);
        // The room places each actor directly on its floor, so its first downward probe can
        // consume the tiny initial bounce without changing an integer pixel. Lift only the
        // audited actor one tile after initialization to expose the complete quadratic
        // fall/rise path against the same untouched retail terrain.
        slot.YPosition = unchecked((ushort)(slot.YPosition - 16));
        ushort initialY = slot.YPosition;
        bool sawRising = false;
        bool sawVerticalMotion = false;
        bool settled = false;
        for (int frame = 0; frame < 1200; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: level);
            sawRising |= state.BounceFunction == MaridiaLargeSnailBounceFunction.Rising;
            sawVerticalMotion |= slot.YPosition != initialY;
            if (state.RemainingBounces == 0)
            {
                settled = true;
                break;
            }
        }
        if (!sawRising || !sawVerticalMotion || !settled)
        {
            throw new InvalidDataException(
                $"Oum bounce did not settle: rising={sawRising}, moved={sawVerticalMotion}, " +
                $"remaining={state.RemainingBounces}, Y=${slot.YPosition:X4}.");
        }

        // Strictly less than 24 pixels selects the rolling list. Keep Samus beside the
        // actor thereafter so the signed-underflow cooldown and ROM opcode $CCBE jointly
        // gate the attack; this verifies the animation is driving AI rather than a timer
        // invented by the host.
        loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + 16));
        loaded.Samus.YPosition = slot.YPosition;
        ushort rollingOriginX = slot.XPosition;
        bool sawRolling = false;
        bool sawHorizontalMotion = false;
        bool sawRotationWindow = false;
        bool sawAttack = false;
        bool sawAttackSound = false;
        bool returnedToRolling = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 2400; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: level);
            maps.Add(slot.SpritemapPointer);
            sawRolling |= state.Function == MaridiaLargeSnailEnemyFunction.Rolling;
            sawHorizontalMotion |= slot.XPosition != rollingOriginX;
            sawRotationWindow |= state.AttackAllowsRotation;
            sawAttackSound |= loaded.Enemies.LastMaridiaLargeSnailSoundEffect == 0x000e;
            if (state.Function == MaridiaLargeSnailEnemyFunction.Attacking)
                sawAttack = true;
            returnedToRolling |= sawAttack &&
                state.Function == MaridiaLargeSnailEnemyFunction.Rolling;

            // Follow the actor closely without pinning it to a fixed world coordinate.
            loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + 16));
            loaded.Samus.YPosition = slot.YPosition;
            if (returnedToRolling && sawAttackSound && maps.Count >= 4)
                break;
        }
        if (!sawRolling || !sawHorizontalMotion || !sawRotationWindow || !sawAttack ||
            !sawAttackSound || !returnedToRolling || maps.Count < 4)
        {
            throw new InvalidDataException(
                $"Oum cycle mismatch: roll/move/window/attack/sound/return=" +
                $"{sawRolling}/{sawHorizontalMotion}/{sawRotationWindow}/{sawAttack}/" +
                $"{sawAttackSound}/{returnedToRolling}, maps={maps.Count}, function/list=" +
                $"${(ushort)state.Function:X4}/{state.InstalledInstructionListIndex}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Oum's live ROM extended spritemap emitted no OBJ pieces.");
    }

    private static void VerifyProtectedAndDamagingContact(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);

        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackActive = false;
        ushort oldExtraX = loaded.Samus.Kinematics.ExtraXDisplacement;
        bool touched = FindAnyContact(loaded, slot, requireDamage: false);
        if (!touched || loaded.Samus.Health != 999 || loaded.Samus.KnockbackActive ||
            loaded.Samus.Kinematics.ExtraXDisplacement == oldExtraX)
        {
            throw new InvalidDataException(
                $"Oum protected-shell contact mismatch: touched={touched}, health=" +
                $"{loaded.Samus.Health}, knockback={loaded.Samus.KnockbackActive}, extraX=" +
                $"${oldExtraX:X4}->${loaded.Samus.Kinematics.ExtraXDisplacement:X4}.");
        }

        // Attack until an exposed frame's $D388 hitbox is selected, then probe contact on
        // each frame. Protected frames only shove; the exposed frame must eventually route
        // through common touch and consume the literal 100 damage in header $A0:D37F.
        MaridiaLargeSnailEnemyState state = GetState(loaded.Enemies, 0);
        loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + 16));
        loaded.Samus.YPosition = slot.YPosition;
        bool tookDamage = false;
        bool sawAttack = false;
        for (int frame = 0; frame < 2600 && !tookDamage; frame++)
        {
            loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
            sawAttack |= state.Function == MaridiaLargeSnailEnemyFunction.Attacking;
            tookDamage = FindAnyContact(loaded, slot, requireDamage: true);
            loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + 16));
            loaded.Samus.YPosition = slot.YPosition;
        }
        if (!tookDamage || loaded.Samus.Health > 899 || !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Oum exposed attack hitbox did not deal 100 damage: health=" +
                $"{loaded.Samus.Health}, knockback={loaded.Samus.KnockbackActive}, " +
                $"attack={sawAttack}, function=${(ushort)state.Function:X4}.");
        }
    }

    private static void VerifyProtectedAndHandledShots(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedRoom loaded = LoadRoom(retailBus, room, assets);
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(CameraX, CameraY, false, loaded.Samus, level: assets.LevelData);
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        int hits = FindAnyProjectileHit(
            retailBus,
            loaded,
            slot,
            shots,
            bombs,
            out ushort handledDirection);
        if (hits != 1 || slot.Health != 300 ||
            (handledDirection & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Oum shell-shot dispatch mismatch: hits={hits}, health={slot.Health}, " +
                $"direction=${handledDirection:X4}.");
        }

        // The current retail frame contains both protected and ordinary shot regions. Probe
        // across its width until $D3B4 is selected; the indestructible vulnerability keeps
        // health at 300, while Oum's private tail still publishes sound $57.
        bool handledShot = loaded.Enemies.LastMaridiaLargeSnailSoundEffect == 0x0057;
        for (int yOffset = -28; yOffset <= 28 && !handledShot; yOffset += 2)
        {
            for (int xOffset = -28; xOffset <= 28 && !handledShot; xOffset += 2)
            {
                shots = new SamusProjectileSystem();
                ArmBeam(
                    shots.Slots[0],
                    unchecked((ushort)(slot.XPosition + xOffset)),
                    unchecked((ushort)(slot.YPosition + yOffset)));
                loaded.Enemies.ResolveOrdinaryProjectileHits(retailBus, shots, bombs, loaded.Samus);
                handledShot = loaded.Enemies.LastMaridiaLargeSnailSoundEffect == 0x0057;
            }
        }
        if (!handledShot || slot.Health != 300)
        {
            throw new InvalidDataException(
                $"Oum $D3B4 shot tail mismatch: sound=" +
                $"{loaded.Enemies.LastMaridiaLargeSnailSoundEffect?.ToString("X4") ?? "none"}, " +
                $"health={slot.Health}.");
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
            retainedRecordCount: 3,
            deathQuota: 3);
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
            YPosition = 0x0260,
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

    private static MaridiaLargeSnailEnemyState GetState(RoomEnemySystem enemies, int index) =>
        enemies.MaridiaLargeSnailStates[index] ?? throw new InvalidDataException(
            $"Retail Oum slot {index} did not receive typed state.");

    /// <summary>
    /// Searches only the small physical extent of the current ROM extended spritemap. This
    /// avoids baking one frame's irregular multibox geometry into the audit while still
    /// requiring the production dispatcher to select a real hitbox and callback.
    /// </summary>
    private static bool FindAnyContact(
        LoadedRoom loaded,
        RoomEnemySlot slot,
        bool requireDamage)
    {
        for (int yOffset = -36; yOffset <= 36; yOffset += 2)
        {
            for (int xOffset = -36; xOffset <= 36; xOffset += 2)
            {
                loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + xOffset));
                loaded.Samus.YPosition = unchecked((ushort)(slot.YPosition + yOffset));
                loaded.Samus.InvincibilityTimer = 0;
                loaded.Samus.KnockbackActive = false;
                ushort healthBefore = loaded.Samus.Health;
                ushort extraXBefore = loaded.Samus.Kinematics.ExtraXDisplacement;
                bool touched = loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0);
                int damage = healthBefore - loaded.Samus.Health;
                if (requireDamage && damage == 100)
                    return true;
                if (!requireDamage && touched && damage == 0 &&
                    loaded.Samus.Kinematics.ExtraXDisplacement != extraXBefore)
                    return true;
            }
        }
        return false;
    }

    private static int FindAnyProjectileHit(
        ISnesAddressSpace bus,
        LoadedRoom loaded,
        RoomEnemySlot slot,
        SamusProjectileSystem shots,
        SamusBombProjectileSystem bombs,
        out ushort handledDirection)
    {
        for (int yOffset = -28; yOffset <= 28; yOffset += 2)
        {
            for (int xOffset = -28; xOffset <= 28; xOffset += 2)
            {
                shots = new SamusProjectileSystem();
                ArmBeam(
                    shots.Slots[0],
                    unchecked((ushort)(slot.XPosition + xOffset)),
                    unchecked((ushort)(slot.YPosition + yOffset)));
                int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
                    bus,
                    shots,
                    bombs,
                    loaded.Samus);
                if (hits != 0 && (shots.Slots[0].Direction & 0x0010) != 0)
                {
                    handledDirection = shots.Slots[0].Direction;
                    return hits;
                }
            }
        }
        handledDirection = 0;
        return 0;
    }

    private static void ArmBeam(SamusProjectileSlot projectile, ushort x, ushort y)
    {
        projectile.ClearFields();
        projectile.Type = (ushort)SamusProjectileFamily.Beam;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 1;
        projectile.YRadius = 1;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private readonly record struct LoadedRoom(RoomEnemySystem Enemies, SamusState Samus);
}
