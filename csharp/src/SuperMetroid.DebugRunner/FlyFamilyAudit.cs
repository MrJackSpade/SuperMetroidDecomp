using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// End-to-end ROM-backed proof for all three enemies that share bank-$A2 fly AI:
/// Mellow ($D0FF), Mella ($D13F), and Memu ($D17F). The former inline Flyway probe only
/// exercised Mellow; keeping the complete family here makes the shared implementation and
/// each definition's distinct health, damage, radius, palette, tiles, and vulnerability
/// independently inspectable.
/// </summary>
internal static class FlyFamilyAudit
{
    private const ushort FlyInstructionList = 0xb013;
    private const int SignedSineTable = 0xa0b3c3;
    private const int AttackHorizontalRange = 0x70;

    // These are actual room headers, selected state records, and pure-family population
    // sizes. Mellow uses Flyway; Mella uses the Maridia room state at $A815; Memu uses the
    // pre-Spring-Ball room. Nothing in the audit synthesizes an enemy record.
    private static readonly FlyProfile[] Profiles =
    [
        new(
            Name: "Mellow",
            DefinitionPointer: RoomEnemySystem.MellowDefinition,
            RoomPointer: 0x9879,
            StatePointer: 0x9890,
            ExpectedEnemyCount: 12,
            ExpectedFlyCount: 12,
            PalettePointer: 0xaff3,
            Health: 9,
            Damage: 8,
            XRadius: 8,
            YRadius: 4,
            VariantIndex: 0xb013,
            InitialSpritemapPointer: 0xb204,
            TileDataAddress: 0xaea600,
            ItemDropPointer: 0xf236,
            VulnerabilityPointer: 0xec1c,
            NamePointer: 0xdf81),
        new(
            Name: "Mella",
            DefinitionPointer: RoomEnemySystem.MellaDefinition,
            RoomPointer: 0xa815,
            StatePointer: 0xa822,
            ExpectedEnemyCount: 12,
            ExpectedFlyCount: 6,
            PalettePointer: 0xb20c,
            Health: 30,
            Damage: 16,
            XRadius: 8,
            YRadius: 4,
            VariantIndex: 0xb22c,
            InitialSpritemapPointer: 0xb25c,
            TileDataAddress: 0xaec920,
            ItemDropPointer: 0xf23c,
            VulnerabilityPointer: 0xee00,
            NamePointer: 0xdf65),
        new(
            Name: "Memu",
            DefinitionPointer: RoomEnemySystem.MemuDefinition,
            RoomPointer: 0xd16d,
            StatePointer: 0xd17a,
            ExpectedEnemyCount: 12,
            ExpectedFlyCount: 5,
            PalettePointer: 0xb264,
            Health: 100,
            Damage: 60,
            XRadius: 8,
            YRadius: 8,
            VariantIndex: 0xb284,
            InitialSpritemapPointer: 0xb2b4,
            TileDataAddress: 0xaecd20,
            ItemDropPointer: 0xf242,
            VulnerabilityPointer: 0xec1c,
            NamePointer: 0xdf73),
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (FlyProfile profile in Profiles)
        {
            VerifyHeader(bus, profile);
            VerifyNaturalAnimationAndMotion(bus, profile);
            VerifyCommonCombat(bus, profile);
            VerifyGrappleKill(bus, profile);
        }

        Console.WriteLine(
            "Fly-family audit passed: retail Flyway/Maridia/pre-Spring-Ball populations " +
            "loaded 12 Mellows, 6 Mellas, and 5 Memus; exact headers, four-map ROM " +
            "animation, both circular directions, aimed attack/retreat motion, OBJ, " +
            "definition-specific contact/beam damage, and Grapple kills matched cartridge data.");
        return 0;
    }

    private static void VerifyHeader(ISnesAddressSpace bus, FlyProfile profile)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(
            bus,
            profile.DefinitionPointer);

        // The three headers deliberately share code pointers but not all data fields.
        // Checking both groups prevents a convenient "generic fly" record from erasing
        // Mella's doubled/quadrupled vulnerabilities or Memu's larger hitbox and damage.
        if (definition.TileDataSize != 0x0400 ||
            definition.PalettePointer != profile.PalettePointer ||
            definition.Health != profile.Health ||
            definition.Damage != profile.Damage ||
            definition.XRadius != profile.XRadius ||
            definition.YRadius != profile.YRadius ||
            definition.Bank != 0xa2 ||
            definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0020 ||
            definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xb06b ||
            definition.PartCount != 1 ||
            definition.MainAiPointer != 0xb11f ||
            definition.GrappleAiPointer != 0x800a ||
            definition.HurtAiPointer != 0x804c ||
            definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 ||
            definition.DeathAnimation != 0 ||
            definition.PowerBombReactionPointer != 0 ||
            definition.VariantIndex != profile.VariantIndex ||
            definition.TouchAiPointer != 0x8023 ||
            definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != profile.InitialSpritemapPointer ||
            definition.TileDataAddress != profile.TileDataAddress ||
            definition.Layer != 5 ||
            definition.ItemDropChancesPointer != profile.ItemDropPointer ||
            definition.VulnerabilityPointer != profile.VulnerabilityPointer ||
            definition.NamePointer != profile.NamePointer)
        {
            throw new InvalidDataException(
                $"{profile.Name} header $A0:{profile.DefinitionPointer:X4} diverged: " +
                $"init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, health/damage={definition.Health}/" +
                $"{definition.Damage}, radii={definition.XRadius}x{definition.YRadius}, " +
                $"palette/vulnerability=${definition.PalettePointer:X4}/" +
                $"${definition.VulnerabilityPointer:X4}.");
        }
    }

    private static void VerifyNaturalAnimationAndMotion(
        SuperMetroidAddressSpace bus,
        FlyProfile profile)
    {
        LoadedFlyRoom loaded = Load(bus, profile);
        RoomEnemySlot fly = loaded.Fly;
        FlyEnemyState state = loaded.State;
        (ushort cameraX, ushort cameraY) = CenterCamera(loaded.Room, fly);

        if (loaded.Room.State.Pointer != profile.StatePointer ||
            loaded.Enemies.EnemyCount != profile.ExpectedEnemyCount ||
            loaded.Enemies.Slots.Take(profile.ExpectedEnemyCount).Count(slot =>
                slot.EnemyDefinitionPointer == profile.DefinitionPointer) !=
                profile.ExpectedFlyCount ||
            loaded.Enemies.Slots.Take(profile.ExpectedEnemyCount).Where(slot =>
                slot.EnemyDefinitionPointer == profile.DefinitionPointer).Any(slot =>
                    loaded.Enemies.FlyStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"{profile.Name} retail population mismatch: state=" +
                $"$8F:{loaded.Room.State.Pointer:X4}, count={loaded.Enemies.EnemyCount}, " +
                $"expected={profile.ExpectedEnemyCount} total with " +
                $"{profile.ExpectedFlyCount} $A0:{profile.DefinitionPointer:X4} records.");
        }

        if (fly.CurrentInstruction != FlyInstructionList ||
            fly.SpritemapPointer != 0x804d ||
            fly.InstructionTimer != 1 ||
            fly.XSubposition != 0 || fly.YSubposition != 0 ||
            state.RetreatTimer != 0 || state.XVelocity != 0 ||
            state.YVelocity != 0 || state.TargetYPosition != 0 ||
            state.Angle != 0 || state.Function != FlyEnemyFunction.ClockwiseCircle)
        {
            throw new InvalidDataException(
                $"{profile.Name} initializer $A2:B06B diverged: list/map/timer=" +
                $"${fly.CurrentInstruction:X4}/${fly.SpritemapPointer:X4}/" +
                $"{fly.InstructionTimer}, angle=${state.Angle:X4}, function=" +
                $"$A2:{(ushort)state.Function:X4}.");
        }

        // Keep Samus outside the strict `< $70` activation range. Native still advances
        // RNG and circular motion, so the following thirty-two calls traverse a complete
        // clockwise cycle and a complete anti-clockwise cycle without host intervention.
        loaded.Samus.XPosition = unchecked((ushort)(fly.XPosition + AttackHorizontalRange));
        loaded.Samus.YPosition = fly.YPosition;

        ushort expectedX = fly.XPosition;
        ushort expectedXSubposition = fly.XSubposition;
        ushort expectedY = fly.YPosition;
        ushort expectedYSubposition = fly.YSubposition;
        ushort expectedAngle = 0;
        var observedMaps = new HashSet<ushort>();
        bool observedAntiClockwise = false;
        bool returnedClockwise = false;

        for (int frame = 0; frame < 32; frame++)
        {
            // Follow the moving actor at the exact excluded boundary. Leaving Samus at a
            // fixed world coordinate would eventually shrink the distance below $70 and
            // correctly interrupt the circle with an attack, invalidating this path proof.
            loaded.Samus.XPosition = unchecked((ushort)(fly.XPosition + AttackHorizontalRange));
            loaded.Samus.YPosition = fly.YPosition;

            // `$A2:B090` uses the angle as an even BYTE offset. Reading the signed word at
            // `$A0:B443 + angle` supplies horizontal sine; `$A0:B3C3 + angle` supplies
            // vertical negative cosine. This independent address calculation catches a
            // table-base or sample-vs-byte-offset error in the C# movement routine.
            ushort horizontalVelocity = ReadWord(
                bus,
                SignedSineTable + 0x80 + expectedAngle);
            ushort verticalVelocity = ReadWord(bus, SignedSineTable + expectedAngle);
            (expectedX, expectedXSubposition) = AddEightBitVelocityReference(
                expectedX,
                expectedXSubposition,
                horizontalVelocity);
            (expectedY, expectedYSubposition) = AddEightBitVelocityReference(
                expectedY,
                expectedYSubposition,
                verticalVelocity);

            bool clockwise = frame < 16;
            expectedAngle = unchecked((ushort)(
                (expectedAngle + (clockwise ? 0x20 : -0x20)) & 0x01ff));
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: loaded.Assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));
            observedMaps.Add(fly.SpritemapPointer);
            observedAntiClockwise |= state.Function == FlyEnemyFunction.AntiClockwiseCircle;
            returnedClockwise |= observedAntiClockwise &&
                state.Function == FlyEnemyFunction.ClockwiseCircle;

            if (fly.XPosition != expectedX ||
                fly.XSubposition != expectedXSubposition ||
                fly.YPosition != expectedY ||
                fly.YSubposition != expectedYSubposition ||
                state.Angle != expectedAngle)
            {
                throw new InvalidDataException(
                    $"{profile.Name} circular frame {frame} diverged: position=" +
                    $"({fly.XPosition:X4}.{fly.XSubposition:X4}," +
                    $"{fly.YPosition:X4}.{fly.YSubposition:X4})/" +
                    $"({expectedX:X4}.{expectedXSubposition:X4}," +
                    $"{expectedY:X4}.{expectedYSubposition:X4}), angle=" +
                    $"${state.Angle:X4}/${expectedAngle:X4}.");
            }
        }

        ushort[] expectedMaps =
        [
            ReadWord(bus, 0xa2b015),
            ReadWord(bus, 0xa2b019),
            ReadWord(bus, 0xa2b01d),
            ReadWord(bus, 0xa2b021),
        ];
        if (!observedAntiClockwise || !returnedClockwise ||
            !expectedMaps.All(observedMaps.Contains) || observedMaps.Count != 4)
        {
            throw new InvalidDataException(
                $"{profile.Name} circle/list coverage diverged: anti=" +
                $"{observedAntiClockwise}, returned={returnedClockwise}, maps=" +
                $"{observedMaps.Count}/4.");
        }

        VerifyAimedAttackAndRetreat(bus, loaded, profile, cameraX, cameraY);

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException($"{profile.Name} emitted no OBJ from its live ROM map.");
    }

    private static void VerifyAimedAttackAndRetreat(
        ISnesAddressSpace bus,
        LoadedFlyRoom loaded,
        FlyProfile profile,
        ushort cameraX,
        ushort cameraY)
    {
        RoomEnemySlot fly = loaded.Fly;
        FlyEnemyState state = loaded.State;
        ushort sourceX = fly.XPosition;
        ushort sourceY = fly.YPosition;

        // Equality at 112 pixels is excluded; verify that boundary before moving one pixel
        // inward. The attack setup call writes velocity/target/function but does not move.
        loaded.Samus.XPosition = unchecked((ushort)(sourceX + AttackHorizontalRange));
        loaded.Samus.YPosition = unchecked((ushort)(sourceY + 64));
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: loaded.Assets.LevelData);
        if (state.Function != FlyEnemyFunction.ClockwiseCircle)
            throw new InvalidDataException($"{profile.Name} attacked at excluded X distance $70.");

        loaded.Samus.XPosition = unchecked((ushort)(sourceX + AttackHorizontalRange - 1));
        short deltaX = unchecked((short)(loaded.Samus.XPosition - fly.XPosition));
        short deltaY = unchecked((short)(loaded.Samus.YPosition - fly.YPosition));
        byte angle = CalculateCartridgeAngleReference(deltaX, deltaY);
        ushort expectedXVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(bus, SignedSineTable + (angle + 64) * 2)) * 2));
        ushort expectedYVelocity = unchecked((ushort)(
            unchecked((short)ReadWord(bus, SignedSineTable + angle * 2)) * 4));
        ushort attackStartX = fly.XPosition;
        ushort attackStartY = fly.YPosition;
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: loaded.Assets.LevelData);
        if (state.Function != FlyEnemyFunction.AttackSamus ||
            state.XVelocity != expectedXVelocity ||
            state.YVelocity != expectedYVelocity ||
            state.TargetYPosition != loaded.Samus.YPosition ||
            fly.XPosition != attackStartX || fly.YPosition != attackStartY)
        {
            throw new InvalidDataException(
                $"{profile.Name} attack setup diverged: function=" +
                $"$A2:{(ushort)state.Function:X4}, velocity=" +
                $"({state.XVelocity:X4},{state.YVelocity:X4})/" +
                $"({expectedXVelocity:X4},{expectedYVelocity:X4}), target=" +
                $"${state.TargetYPosition:X4}/${loaded.Samus.YPosition:X4}.");
        }

        bool observedRetreat = false;
        ushort retreatVelocity = 0;
        int attackFrames = 0;
        for (; attackFrames < 256 && !observedRetreat; attackFrames++)
        {
            // The aimed vector can cross a screen boundary before it crosses target Y.
            // Follow it with a legal room camera so the ordinary activity scan continues
            // to schedule the actor; this is observation, not forced process-all-enemies.
            (cameraX, cameraY) = CenterCamera(loaded.Room, fly);
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                false,
                loaded.Samus,
                level: loaded.Assets.LevelData);
            if (state.Function == FlyEnemyFunction.Retreat)
            {
                observedRetreat = true;
                retreatVelocity = state.YVelocity;
            }
        }
        if (!observedRetreat || retreatVelocity != unchecked((ushort)-expectedYVelocity) ||
            state.RetreatTimer != attackFrames)
        {
            throw new InvalidDataException(
                $"{profile.Name} did not cross its target exactly: retreat=" +
                $"{observedRetreat}, Y velocity=${retreatVelocity:X4}/" +
                $"${unchecked((ushort)-expectedYVelocity):X4}, timer=" +
                $"{state.RetreatTimer}/{attackFrames}.");
        }

        bool returnedToCircle = false;
        for (int frame = 0; frame < 1024 && !returnedToCircle; frame++)
        {
            (cameraX, cameraY) = CenterCamera(loaded.Room, fly);
            loaded.Enemies.StepFrame(
                cameraX,
                cameraY,
                false,
                loaded.Samus,
                level: loaded.Assets.LevelData);
            returnedToCircle = state.Function == FlyEnemyFunction.ClockwiseCircle;
        }
        if (!returnedToCircle || state.RetreatTimer != 24)
        {
            throw new InvalidDataException(
                $"{profile.Name} retreat did not restore clockwise cooldown: function=" +
                $"$A2:{(ushort)state.Function:X4}, timer={state.RetreatTimer}/24.");
        }
    }

    private static void VerifyCommonCombat(
        SuperMetroidAddressSpace bus,
        FlyProfile profile)
    {
        LoadedFlyRoom loaded = Load(bus, profile);
        Isolate(loaded.Enemies, loaded.Fly);
        (ushort cameraX, ushort cameraY) = CenterCamera(loaded.Room, loaded.Fly);
        loaded.Samus.XPosition = unchecked((ushort)(loaded.Fly.XPosition + 0x0100));
        loaded.Samus.YPosition = loaded.Fly.YPosition;
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: loaded.Assets.LevelData);

        loaded.Samus.XPosition = loaded.Fly.XPosition;
        loaded.Samus.YPosition = loaded.Fly.YPosition;
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackTimer = 0;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != 999 - profile.Damage ||
            !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"{profile.Name} contact diverged: health={loaded.Samus.Health}/" +
                $"{999 - profile.Damage}, knockback={loaded.Samus.KnockbackActive}.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        SamusProjectileSlot shot = shots.Slots[0];
        ArmShot(shot, loaded.Fly, damage: 20);
        byte powerBeamVulnerability = bus.ReadByte(
            0xb40000 | profile.VulnerabilityPointer);
        int expectedDamage = (shot.Damage >> 1) * (powerBeamVulnerability & 0x7f);
        ushort expectedHealth = expectedDamage >= profile.Health
            ? (ushort)0
            : unchecked((ushort)(profile.Health - expectedDamage));
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        bool expectedDeleted = expectedHealth == 0;
        if (hits != 1 || loaded.Fly.Health != expectedHealth ||
            loaded.Fly.Properties.HasAny(EnemyProperties.Deleted) != expectedDeleted)
        {
            throw new InvalidDataException(
                $"{profile.Name} Power Beam reaction diverged: hits={hits}, health=" +
                $"{loaded.Fly.Health}/{expectedHealth}, deleted=" +
                $"{loaded.Fly.Properties.HasAny(EnemyProperties.Deleted)}/" +
                $"{expectedDeleted}, vulnerability=${powerBeamVulnerability:X2}.");
        }
    }

    private static void VerifyGrappleKill(
        SuperMetroidAddressSpace bus,
        FlyProfile profile)
    {
        LoadedFlyRoom loaded = Load(bus, profile);
        Isolate(loaded.Enemies, loaded.Fly);
        (ushort cameraX, ushort cameraY) = CenterCamera(loaded.Room, loaded.Fly);
        loaded.Samus.XPosition = unchecked((ushort)(loaded.Fly.XPosition + 0x0100));
        loaded.Samus.YPosition = loaded.Fly.YPosition;
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: loaded.Assets.LevelData);

        GrappleEnemyCollision collision = loaded.Enemies.ResolveGrappleEndpoint(
            loaded.Fly.XPosition,
            loaded.Fly.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Kill ||
            collision.EnemyNativeIndex != loaded.Fly.NativeIndex ||
            loaded.Fly.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"{profile.Name} Grapple selection diverged: collided=" +
                $"{collision.Collided}, reaction={collision.Reaction}, index=" +
                $"${collision.EnemyNativeIndex:X4}/${loaded.Fly.NativeIndex:X4}, handler=" +
                $"${loaded.Fly.AiHandlerBits:X4}.");
        }

        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            false,
            loaded.Samus,
            level: loaded.Assets.LevelData);
        if (loaded.Fly.Health != 0 ||
            !loaded.Fly.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"{profile.Name} Grapple kill diverged: health={loaded.Fly.Health}, " +
                $"deleted={loaded.Fly.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}/1.");
        }
    }

    private static LoadedFlyRoom Load(
        SuperMetroidAddressSpace bus,
        FlyProfile profile)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, profile.RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
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
        RoomEnemySlot fly = enemies.Slots.Take(enemies.EnemyCount).FirstOrDefault(slot =>
            slot.EnemyDefinitionPointer == profile.DefinitionPointer) ??
            throw new InvalidDataException(
                $"Room $8F:{profile.RoomPointer:X4} loaded no {profile.Name}.");
        FlyEnemyState state = enemies.FlyStates[fly.SlotIndex] ??
            throw new InvalidDataException(
                $"{profile.Name} slot {fly.SlotIndex} has no typed fly state.");
        return new LoadedFlyRoom(room, assets, enemies, samus, fly, state);
    }

    private static void Isolate(RoomEnemySystem enemies, RoomEnemySlot retained)
    {
        foreach (RoomEnemySlot slot in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(slot, retained))
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
        }
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 256);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 128, 0, maximumY)));
    }

    private static void ArmShot(
        SamusProjectileSlot shot,
        RoomEnemySlot target,
        ushort damage)
    {
        shot.ClearFields();
        shot.Type = 0;
        shot.Damage = damage;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = target.XPosition;
        shot.YPosition = target.YPosition;
        shot.XRadius = 4;
        shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
    }

    private static (ushort Position, ushort Subposition) AddEightBitVelocityReference(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    /// <summary>
    /// Independent integer port of `$A0:C0AE`. This deliberately does not call the private
    /// runtime helper: the audit must be able to detect a broken quadrant or division path
    /// in the implementation it is checking.
    /// </summary>
    private static byte CalculateCartridgeAngleReference(short x, short y)
    {
        int quadrant = 0;
        ushort absoluteX = unchecked((ushort)x);
        ushort absoluteY = unchecked((ushort)y);
        if (x < 0)
        {
            quadrant += 2;
            absoluteX = unchecked((ushort)-absoluteX);
        }
        if (y < 0)
        {
            quadrant++;
            absoluteY = unchecked((ushort)-absoluteY);
        }

        if (absoluteY < absoluteX)
        {
            int divided = absoluteX == 0 ? 0 : (absoluteY << 8) / absoluteX;
            return quadrant switch
            {
                0 => unchecked((byte)((divided >> 3) + 64)),
                1 => unchecked((byte)(64 - (divided >> 3))),
                2 => unchecked((byte)(-64 - (divided >> 3))),
                _ => unchecked((byte)((divided >> 3) - 64)),
            };
        }

        int inverseDivided = absoluteY == 0 ? 0 : (absoluteX << 8) / absoluteY;
        return quadrant switch
        {
            0 => unchecked((byte)(128 - (inverseDivided >> 3))),
            1 => unchecked((byte)(inverseDivided >> 3)),
            2 => unchecked((byte)((inverseDivided >> 3) + 128)),
            _ => unchecked((byte)(-(inverseDivided >> 3))),
        };
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) |
        (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));

    private readonly record struct FlyProfile(
        string Name,
        ushort DefinitionPointer,
        ushort RoomPointer,
        ushort StatePointer,
        int ExpectedEnemyCount,
        int ExpectedFlyCount,
        ushort PalettePointer,
        ushort Health,
        ushort Damage,
        ushort XRadius,
        ushort YRadius,
        ushort VariantIndex,
        ushort InitialSpritemapPointer,
        int TileDataAddress,
        ushort ItemDropPointer,
        ushort VulnerabilityPointer,
        ushort NamePointer);

    private sealed record LoadedFlyRoom(
        CartridgeRoomHeader Room,
        CartridgeRoomAssets Assets,
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Fly,
        FlyEnemyState State);
}
