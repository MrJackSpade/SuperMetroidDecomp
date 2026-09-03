using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed Kzan regression using the untouched post-Phantoon Spiky Death population.
/// That state consists of five complete top/bottom pairs, so the audit exercises native
/// population ordering without a terminator overlay or a host-authored actor record.
/// </summary>
internal static class KzanAudit
{
    private const ushort SpikyDeathRoom = 0xcb8b;
    private const ushort KzanTopDefinition = 0xdfff;
    private const ushort KzanBottomDefinition = 0xe03f;
    private const ushort KzanSpritemap = 0x8ce5;

    private static readonly ushort[] ExpectedXPositions =
    [
        0x0050,
        0x00a0,
        0x0100,
        0x0160,
        0x01b0,
    ];

    private static readonly ushort[] ExpectedParameters2 =
    [
        0x6810,
        0x680c,
        0x6811,
        0x680a,
        0x6814,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            SpikyDeathRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        LoadedKzans initialized = Load(bus, room, assets);
        VerifyUntouchedPopulation(bus, room, initialized);
        int objectPieces = VerifyInstructionAndDrawing(room, assets, initialized);
        MotionResult motion = VerifyCompleteCrushCycle(bus, room, assets);
        VerifyContactSolidityShotsPowerBombAndGrapple(bus, room, assets);

        Console.WriteLine(
            "Kzan audit passed: untouched five-pair Spiky Death population loaded; " +
            $"ROM animation emitted {objectPieces} OBJ pieces, falling covered " +
            $"{motion.FallingFrames} accelerated frames, the 64-frame landing pause and " +
            $"{motion.RisingFrames} half-pixel rise frames returned exactly to spawn; " +
            "the hidden bottoms tracked at +12px, rider displacement followed both " +
            "directions, solid/contact behavior dealt 200 damage, shots and power bombs " +
            "passed through the indestructible body, and grapple selected cancel.");
        return 0;
    }

    private static void VerifyUntouchedPopulation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        LoadedKzans loaded)
    {
        if (room.State.Pointer != 0xcbb7 || loaded.Enemies.EnemyCount != 10 ||
            loaded.Enemies.GraphicsSet.Count != 1)
        {
            throw new InvalidDataException(
                $"Spiky Death selected state ${room.State.Pointer:X4}, " +
                $"{loaded.Enemies.EnemyCount} actors, and " +
                $"{loaded.Enemies.GraphicsSet.Count} graphics records.");
        }

        ushort expectedInitialWhole = ReadWord(bus, 0xa08187 + 0x0200);
        ushort expectedInitialFraction = ReadWord(bus, 0xa08189 + 0x0200);
        for (int pairIndex = 0; pairIndex < ExpectedXPositions.Length; pairIndex++)
        {
            RoomEnemySlot top = loaded.Enemies.Slots[pairIndex * 2];
            RoomEnemySlot bottom = loaded.Enemies.Slots[pairIndex * 2 + 1];
            KzanEnemyState state = RequireState(loaded.Enemies, top);
            ushort expectedProperties = pairIndex % 2 == 0 ? (ushort)0xa800 : (ushort)0xa000;

            if (top.EnemyDefinitionPointer != KzanTopDefinition ||
                top.XPosition != ExpectedXPositions[pairIndex] || top.YPosition != 0x0060 ||
                top.Parameter1 != 0x0040 || top.Parameter2 != ExpectedParameters2[pairIndex] ||
                top.Properties != expectedProperties || top.Health != 500 ||
                top.Definition.Damage != 200 || top.XRadius != 16 || top.YRadius != 12 ||
                top.Definition.Bank != 0xa6 ||
                top.Definition.InitializationAiPointer != 0x8b2f ||
                top.Definition.MainAiPointer != 0x8bad ||
                top.Definition.GrappleAiPointer != 0x800f ||
                top.Definition.TouchAiPointer != 0x8023 ||
                top.Definition.ShotAiPointer != 0x804c ||
                top.Definition.VulnerabilityPointer != 0xeec6 ||
                top.Definition.PartCount != 2 || top.CurrentInstruction != 0x8b29 ||
                state.Function != KzanEnemyFunction.WaitingToFall ||
                state.FallWaitTimer != (ExpectedParameters2[pairIndex] & 0x00ff) ||
                state.FallWaitTimerResetValue != (ExpectedParameters2[pairIndex] & 0x00ff) ||
                state.RisingTargetYPosition != 0x0060 ||
                state.FallingTargetYPosition != 0x00c8 ||
                state.FallingYSpeedTableIndex != 0x0200 ||
                state.InitialFallingYSpeed != expectedInitialWhole ||
                state.InitialFallingYSubspeed != expectedInitialFraction)
            {
                throw new InvalidDataException(
                    $"Kzan top {pairIndex} initialization failed: definition=" +
                    $"${top.EnemyDefinitionPointer:X4}, position=" +
                    $"(${top.XPosition:X4},${top.YPosition:X4}), params=" +
                    $"${top.Parameter1:X4}/${top.Parameter2:X4}, properties=" +
                    $"${top.Properties:X4}, function=$A6:{(ushort)state.Function:X4}, " +
                    $"timers={state.FallWaitTimer}/{state.FallWaitTimerResetValue}, " +
                    $"targets=${state.RisingTargetYPosition:X4}/" +
                    $"${state.FallingTargetYPosition:X4}, speed=" +
                    $"{state.InitialFallingYSpeed:X4}.{state.InitialFallingYSubspeed:X4}/" +
                    $"index=${state.FallingYSpeedTableIndex:X4}.");
            }

            // Population Y is $68, but bottom initialization immediately replaces it with
            // the preceding top's Y + 12, yielding $6C. Its $0100 property suppresses both
            // art and ordinary touch while retaining the physical native slot.
            if (bottom.EnemyDefinitionPointer != KzanBottomDefinition ||
                bottom.XPosition != top.XPosition || bottom.YPosition != top.YPosition + 12 ||
                bottom.Properties != 0x0100 || bottom.Health != 500 ||
                bottom.Definition.Damage != 200 || bottom.XRadius != 16 ||
                bottom.YRadius != 2 || bottom.Definition.Bank != 0xa6 ||
                bottom.Definition.InitializationAiPointer != 0x8b85 ||
                bottom.Definition.MainAiPointer != 0x8b99 ||
                bottom.Definition.PartCount != 1 || bottom.SpritemapPointer != 0 ||
                loaded.Enemies.KzanStates[bottom.SlotIndex] is not null)
            {
                throw new InvalidDataException(
                    $"Kzan bottom {pairIndex} initialization failed: definition=" +
                    $"${bottom.EnemyDefinitionPointer:X4}, position=" +
                    $"(${bottom.XPosition:X4},${bottom.YPosition:X4}), properties=" +
                    $"${bottom.Properties:X4}, map=${bottom.SpritemapPointer:X4}.");
            }
        }
    }

    private static int VerifyInstructionAndDrawing(
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        LoadedKzans loaded)
    {
        RoomEnemySlot firstTop = loaded.Enemies.Slots[0];
        StepCentered(loaded, room, assets, firstTop);
        if (firstTop.SpritemapPointer != KzanSpritemap)
        {
            throw new InvalidDataException(
                $"Kzan instruction list emitted ${firstTop.SpritemapPointer:X4}, " +
                $"expected ROM map $A6:{KzanSpritemap:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 4)
        {
            throw new InvalidDataException(
                $"Kzan four-piece ROM spritemap emitted only " +
                $"{oam.LastFinalizedSpriteCount} OBJ pieces.");
        }
        return oam.LastFinalizedSpriteCount;
    }

    private static MotionResult VerifyCompleteCrushCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedKzans loaded = Load(bus, room, assets);
        RoomEnemySlot top = loaded.Enemies.Slots[0];
        RoomEnemySlot bottom = loaded.Enemies.Slots[1];
        KzanEnemyState state = RequireState(loaded.Enemies, top);

        // Force only the final waiting frame. DEC turns one into zero and changes the
        // function, but the indirect dispatcher does not execute Falling until next frame.
        state.FallWaitTimer = 1;
        StepCentered(loaded, room, assets, top);
        if (state.Function != KzanEnemyFunction.Falling || top.YPosition != 0x0060 ||
            state.FallingYSpeedTableIndex != 0x0200)
        {
            throw new InvalidDataException(
                $"Kzan fall transition failed: function=$A6:{(ushort)state.Function:X4}, " +
                $"Y=${top.YPosition:X4}, speed index=${state.FallingYSpeedTableIndex:X4}.");
        }

        // Put Samus exactly on the top surface according to collision radii. Kzan's
        // five-pixel shifted private rider check must add its actual accepted whole delta
        // to ExtraYDisplacement on the first accelerated frame.
        PlaceSamusOnTop(loaded.Samus, top);
        loaded.Samus.Kinematics.ExtraYDisplacement = 0;
        ushort fallingStartY = top.YPosition;
        ushort expectedWhole = ReadWord(bus, 0xa08187 + 0x0200);
        ushort expectedFraction = ReadWord(bus, 0xa08189 + 0x0200);
        StepCentered(loaded, room, assets, top);
        (ushort expectedY, ushort expectedSubY) = AddFixed(
            fallingStartY,
            0,
            expectedWhole,
            expectedFraction);
        ushort expectedDelta = unchecked((ushort)(expectedY - fallingStartY));
        if (top.YPosition != expectedY || top.YSubposition != expectedSubY ||
            bottom.XPosition != top.XPosition || bottom.YPosition != top.YPosition + 12 ||
            loaded.Samus.Kinematics.ExtraYDisplacement != expectedDelta ||
            loaded.Samus.Kinematics.ExtraYSubdisplacement != 0)
        {
            throw new InvalidDataException(
                $"Kzan first falling frame failed: Y=" +
                $"${top.YPosition:X4}.{top.YSubposition:X4}, expected " +
                $"${expectedY:X4}.{expectedSubY:X4}; bottom=" +
                $"${bottom.XPosition:X4},${bottom.YPosition:X4}; rider=" +
                $"${loaded.Samus.Kinematics.ExtraYDisplacement:X4}." +
                $"{loaded.Samus.Kinematics.ExtraYSubdisplacement:X4}.");
        }

        loaded.Samus.XPosition = 0xffff;
        loaded.Samus.YPosition = 0xffff;
        int fallingFrames = 1;
        bool landed = false;
        for (; fallingFrames < 256; fallingFrames++)
        {
            StepCentered(loaded, room, assets, top);
            if (state.Function != KzanEnemyFunction.WaitingToRise)
                continue;

            landed = true;
            break;
        }
        if (!landed || top.YPosition != 0x00c8 || bottom.YPosition != 0x00d4 ||
            state.RiseWaitTimer != 0x0040 || loaded.Enemies.LastKzanSoundEffect != 0x001b ||
            state.FallingYSpeedTableIndex != 0x0200)
        {
            throw new InvalidDataException(
                $"Kzan landing failed after {fallingFrames} frames: function=" +
                $"$A6:{(ushort)state.Function:X4}, Y=${top.YPosition:X4}/" +
                $"${bottom.YPosition:X4}, wait={state.RiseWaitTimer}, sound=" +
                $"{loaded.Enemies.LastKzanSoundEffect?.ToString("X4") ?? "none"}, " +
                $"speed index=${state.FallingYSpeedTableIndex:X4}.");
        }

        // Sixty-three decrements leave one. The sixty-fourth selects Rising but performs no
        // movement because the caller invoked only the old indirect target this frame.
        for (int frame = 0; frame < 63; frame++)
            StepCentered(loaded, room, assets, top);
        if (state.RiseWaitTimer != 1 || state.Function != KzanEnemyFunction.WaitingToRise)
            throw new InvalidDataException("Kzan landing pause did not retain its 64-frame countdown.");
        StepCentered(loaded, room, assets, top);
        if (state.Function != KzanEnemyFunction.Rising || top.YPosition != 0x00c8)
            throw new InvalidDataException("Kzan landing-pause expiry moved during the dispatch frame.");

        // The first NTSC rise subtracts 0.8000h, borrowing one whole pixel; the next
        // subtracts the remaining half without changing the whole word. These alternating
        // whole deltas are exactly what the rider producer publishes.
        PlaceSamusOnTop(loaded.Samus, top);
        loaded.Samus.Kinematics.ExtraYDisplacement = 0;
        StepCentered(loaded, room, assets, top);
        if (top.YPosition != 0x00c7 || top.YSubposition != 0x8000 ||
            bottom.YPosition != 0x00d3 ||
            loaded.Samus.Kinematics.ExtraYDisplacement != 0xffff)
        {
            throw new InvalidDataException(
                $"Kzan first rise half-pixel failed: Y=" +
                $"${top.YPosition:X4}.{top.YSubposition:X4}, bottom=" +
                $"${bottom.YPosition:X4}, rider=" +
                $"${loaded.Samus.Kinematics.ExtraYDisplacement:X4}.");
        }
        PlaceSamusOnTop(loaded.Samus, top);
        loaded.Samus.Kinematics.ExtraYDisplacement = 0;
        StepCentered(loaded, room, assets, top);
        if (top.YPosition != 0x00c7 || top.YSubposition != 0 ||
            loaded.Samus.Kinematics.ExtraYDisplacement != 0)
        {
            throw new InvalidDataException(
                $"Kzan second rise half-pixel failed: Y=" +
                $"${top.YPosition:X4}.{top.YSubposition:X4}, rider=" +
                $"${loaded.Samus.Kinematics.ExtraYDisplacement:X4}.");
        }

        loaded.Samus.XPosition = 0xffff;
        loaded.Samus.YPosition = 0xffff;
        int risingFrames = 2;
        for (; risingFrames < 300 && state.Function == KzanEnemyFunction.Rising; risingFrames++)
            StepCentered(loaded, room, assets, top);
        if (state.Function != KzanEnemyFunction.WaitingToFall ||
            top.YPosition != 0x0060 || bottom.YPosition != 0x006c ||
            state.FallWaitTimer != state.FallWaitTimerResetValue)
        {
            throw new InvalidDataException(
                $"Kzan rise return failed after {risingFrames} frames: function=" +
                $"$A6:{(ushort)state.Function:X4}, Y=${top.YPosition:X4}/" +
                $"${bottom.YPosition:X4}, fall timer={state.FallWaitTimer}/" +
                $"{state.FallWaitTimerResetValue}.");
        }

        return new MotionResult(fallingFrames, risingFrames);
    }

    private static void VerifyContactSolidityShotsPowerBombAndGrapple(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedKzans loaded = Load(bus, room, assets);
        RoomEnemySlot top = loaded.Enemies.Slots[0];
        RoomEnemySlot bottom = loaded.Enemies.Slots[1];
        StepCentered(loaded, room, assets, top);

        SolidEnemyCollisionBody topBody = loaded.Enemies.InteractiveCollisionBodies.Single(
            body => body.Index == top.NativeIndex);
        SolidEnemyCollisionBody bottomBody = loaded.Enemies.InteractiveCollisionBodies.Single(
            body => body.Index == bottom.NativeIndex);
        if ((topBody.Properties & 0x8000) == 0 || (bottomBody.Properties & 0x8000) != 0)
            throw new InvalidDataException("Kzan solid flag was not confined to the visible top half.");

        loaded.Samus.XPosition = top.XPosition;
        loaded.Samus.YPosition = top.YPosition;
        loaded.Samus.InvincibilityTimer = 0;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health - 200 || !loaded.Samus.KnockbackActive ||
            loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Kzan contact failed: health={health}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, " +
                $"invincibility={loaded.Samus.InvincibilityTimer}.");
        }

        var projectiles = new SamusProjectileSystem();
        SamusProjectileSlot shot = projectiles.Slots[0];
        ArmProjectile(shot, top);
        int shotHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (shotHits != 0 || top.Health != 500 || !shot.IsActive ||
            shot.Direction != 2 || shot.InstructionPointer != 0x9000)
        {
            throw new InvalidDataException(
                $"Kzan no-op shot AI failed: hits={shotHits}, health={top.Health}, " +
                $"active={shot.IsActive}, direction=${shot.Direction:X4}, " +
                $"list=${shot.InstructionPointer:X4}.");
        }

        int powerBombHits = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            top.XPosition,
            top.YPosition,
            explosionRadius: 64);
        if (powerBombHits != 0 || top.Health != 500)
        {
            throw new InvalidDataException(
                $"Indestructible Kzan accepted {powerBombHits} power bombs; " +
                $"health={top.Health}.");
        }

        LoadedKzans grappleLoad = Load(bus, room, assets);
        RoomEnemySlot grappleTop = grappleLoad.Enemies.Slots[0];
        StepCentered(grappleLoad, room, assets, grappleTop);
        GrappleEnemyCollision grapple = grappleLoad.Enemies.ResolveGrappleEndpoint(
            grappleTop.XPosition,
            grappleTop.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel ||
            grapple.EnemyNativeIndex != grappleTop.NativeIndex ||
            grapple.EnemyDamage != 200 || grappleTop.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"Kzan grapple failed: collided={grapple.Collided}, reaction=" +
                $"{grapple.Reaction}, index=${grapple.EnemyNativeIndex:X4}, " +
                $"damage={grapple.EnemyDamage}, handler=${grappleTop.AiHandlerBits:X4}.");
        }
    }

    private static LoadedKzans Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
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
            isAreaBossDefeated: () => true);
        return new LoadedKzans(enemies, samus);
    }

    private static void StepCentered(
        LoadedKzans loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static void PlaceSamusOnTop(SamusState samus, RoomEnemySlot top)
    {
        samus.XPosition = top.XPosition;
        samus.YPosition = unchecked((ushort)(
            top.YPosition - top.YRadius - samus.Kinematics.YRadius));
    }

    private static (ushort Whole, ushort Fraction) AddFixed(
        ushort position,
        ushort subposition,
        ushort wholeSpeed,
        ushort fractionalSpeed)
    {
        uint fractionalSum = (uint)subposition + fractionalSpeed;
        return (
            unchecked((ushort)(position + wholeSpeed +
                (fractionalSum > ushort.MaxValue ? 1 : 0))),
            unchecked((ushort)fractionalSum));
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0x0100;
        projectile.Damage = 20;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static KzanEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.KzanStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Kzan top slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedKzans(RoomEnemySystem Enemies, SamusState Samus);
    private readonly record struct MotionResult(int FallingFrames, int RisingFrames);
}
