using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Full retail Green Brinstar Fireflea-room regression. This room is an unusually strong
/// family fixture: all five population entries are Firefleas, and their parameters exercise
/// clockwise/counter-clockwise circles plus two vertical radii/speeds without an overlay.
/// </summary>
internal static class FirefleaAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x9c5e);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        const ushort firefleaDefinition = 0xd6bf;
        RoomEnemySlot[] firefleas = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != 0x9c6b || enemies.EnemyCount != 5 ||
            firefleas.Any(slot => slot.EnemyDefinitionPointer != firefleaDefinition) ||
            firefleas.Any(slot => enemies.FirefleaStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Green Brinstar selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} enemies, of which " +
                $"{firefleas.Count(slot => slot.EnemyDefinitionPointer == firefleaDefinition)} " +
                "are initialized Firefleas.");
        }

        FirefleaEnemyState firstCircle = RequireState(enemies, firefleas[0]);
        FirefleaEnemyState secondCircle = RequireState(enemies, firefleas[1]);
        FirefleaEnemyState thirdCircle = RequireState(enemies, firefleas[2]);
        FirefleaEnemyState firstVertical = RequireState(enemies, firefleas[3]);
        FirefleaEnemyState secondVertical = RequireState(enemies, firefleas[4]);

        // The first actor selects circle mode, negative speed-table entry 16, and radius 24.
        // Cosine(0) uses table magnitude $FF, so the initializer places X at center + 23,
        // not the mathematically tempting center + 24.
        if (firefleas[0].Parameter1 != 0x0002 || firefleas[0].Parameter2 != 0x0210 ||
            firstCircle.SpeedTableIndex != 0x0084 || firstCircle.AngleDelta != -1 ||
            firstCircle.SubAngleDelta != 0 || firstCircle.Radius != 0x0018 ||
            firstCircle.Angle != 0 || firstCircle.XCenter != 0x0180 ||
            firstCircle.YCenter != 0x005f || firefleas[0].XPosition != 0x0197 ||
            firefleas[0].YPosition != 0x005f ||
            firefleas[0].CurrentInstruction != 0x8c2f)
        {
            throw new InvalidDataException(
                $"First Fireflea init failed: params=${firefleas[0].Parameter1:X4}/" +
                $"${firefleas[0].Parameter2:X4}, speed=${firstCircle.SpeedTableIndex:X4} " +
                $"({firstCircle.AngleDelta}:{firstCircle.SubAngleDelta:X4}), " +
                $"radius={firstCircle.Radius}, angle=${firstCircle.Angle:X4}, " +
                $"center=({firstCircle.XCenter:X4},{firstCircle.YCenter:X4}), " +
                $"position=({firefleas[0].XPosition:X4},{firefleas[0].YPosition:X4}), " +
                $"list=${firefleas[0].CurrentInstruction:X4}.");
        }

        if (!secondCircle.UsesCircularMovement || !thirdCircle.UsesCircularMovement ||
            firstVertical.UsesCircularMovement || secondVertical.UsesCircularMovement ||
            secondCircle.SpeedTableIndex != 0x0080 ||
            thirdCircle.SpeedTableIndex != 0x0084 ||
            firstVertical.SpeedTableIndex != 0x0044 ||
            firstVertical.Radius != 0x0020 ||
            firstVertical.MinimumYPosition != 0x00f8 ||
            firstVertical.MaximumYPosition != 0x0138 ||
            secondVertical.Radius != 0x0028 ||
            secondVertical.MinimumYPosition != 0x0110 ||
            secondVertical.MaximumYPosition != 0x0160)
        {
            throw new InvalidDataException(
                "Green Brinstar Fireflea parameter modes/radii did not match retail data.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x00c0,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        ushort secondMinimumX = firefleas[1].XPosition;
        ushort secondMaximumX = firefleas[1].XPosition;
        ushort secondMinimumY = firefleas[1].YPosition;
        ushort secondMaximumY = firefleas[1].YPosition;
        ushort thirdMinimumX = firefleas[2].XPosition;
        ushort thirdMaximumX = firefleas[2].XPosition;
        ushort thirdMinimumY = firefleas[2].YPosition;
        ushort thirdMaximumY = firefleas[2].YPosition;
        ushort verticalMinimum = firefleas[3].YPosition;
        ushort verticalMaximum = firefleas[3].YPosition;
        ushort secondVerticalMinimum = firefleas[4].YPosition;
        ushort secondVerticalMaximum = firefleas[4].YPosition;
        ushort firstVerticalInitialSpeedIndex = firstVertical.SpeedTableIndex;
        ushort secondVerticalInitialSpeedIndex = secondVertical.SpeedTableIndex;
        bool firstVerticalReversed = false;
        bool secondVerticalReversed = false;
        var animationMaps = new HashSet<ushort>();

        // Camera zero admits actors one through four and covers more than two complete ROM
        // animation cycles. It also lets each vertical actor cross both extrema naturally.
        for (int frame = 0; frame < 192; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            animationMaps.Add(firefleas[1].SpritemapPointer);
            secondMinimumX = Math.Min(secondMinimumX, firefleas[1].XPosition);
            secondMaximumX = Math.Max(secondMaximumX, firefleas[1].XPosition);
            secondMinimumY = Math.Min(secondMinimumY, firefleas[1].YPosition);
            secondMaximumY = Math.Max(secondMaximumY, firefleas[1].YPosition);
            thirdMinimumX = Math.Min(thirdMinimumX, firefleas[2].XPosition);
            thirdMaximumX = Math.Max(thirdMaximumX, firefleas[2].XPosition);
            thirdMinimumY = Math.Min(thirdMinimumY, firefleas[2].YPosition);
            thirdMaximumY = Math.Max(thirdMaximumY, firefleas[2].YPosition);
            verticalMinimum = Math.Min(verticalMinimum, firefleas[3].YPosition);
            verticalMaximum = Math.Max(verticalMaximum, firefleas[3].YPosition);
            secondVerticalMinimum = Math.Min(secondVerticalMinimum, firefleas[4].YPosition);
            secondVerticalMaximum = Math.Max(secondVerticalMaximum, firefleas[4].YPosition);
            firstVerticalReversed |=
                firstVertical.SpeedTableIndex != firstVerticalInitialSpeedIndex;
            secondVerticalReversed |=
                secondVertical.SpeedTableIndex != secondVerticalInitialSpeedIndex;
        }

        ushort secondAngleAfterCirclePhase = secondCircle.Angle;
        ushort thirdAngleAfterCirclePhase = thirdCircle.Angle;

        // The vertical pair lives at world Y $0118/$0138 and is deliberately outside the
        // upper viewport. Camera Y $00E0 admits their complete extrema while keeping every
        // upper circle off-screen, so scheduler behavior remains part of the regression.
        for (int frame = 0; frame < 320; frame++)
        {
            enemies.StepFrame(0, 0x00e0, false, samus, level: assets.LevelData);
            verticalMinimum = Math.Min(verticalMinimum, firefleas[3].YPosition);
            verticalMaximum = Math.Max(verticalMaximum, firefleas[3].YPosition);
            secondVerticalMinimum = Math.Min(secondVerticalMinimum, firefleas[4].YPosition);
            secondVerticalMaximum = Math.Max(secondVerticalMaximum, firefleas[4].YPosition);
            firstVerticalReversed |=
                firstVertical.SpeedTableIndex != firstVerticalInitialSpeedIndex;
            secondVerticalReversed |=
                secondVertical.SpeedTableIndex != secondVerticalInitialSpeedIndex;
        }

        if (animationMaps.Count != 21 ||
            secondMaximumX - secondMinimumX < 40 ||
            secondMaximumY - secondMinimumY < 40 ||
            thirdMaximumX - thirdMinimumX < 40 ||
            thirdMaximumY - thirdMinimumY < 40 ||
            !firstVerticalReversed || !secondVerticalReversed ||
            verticalMaximum - verticalMinimum < 60 ||
            secondVerticalMaximum - secondVerticalMinimum < 76 ||
            secondAngleAfterCirclePhase != 0xc000 ||
            thirdAngleAfterCirclePhase != 0x4000)
        {
            throw new InvalidDataException(
                $"Fireflea motion/animation failed: maps={animationMaps.Count}, " +
                $"circle1 X/Y={secondMinimumX:X4}-{secondMaximumX:X4}/" +
                $"{secondMinimumY:X4}-{secondMaximumY:X4}, circle2 X/Y=" +
                $"{thirdMinimumX:X4}-{thirdMaximumX:X4}/" +
                $"{thirdMinimumY:X4}-{thirdMaximumY:X4}, vertical=" +
                $"{verticalMinimum:X4}-{verticalMaximum:X4}/" +
                $"{secondVerticalMinimum:X4}-{secondVerticalMaximum:X4}, " +
                $"reversed={firstVerticalReversed}/{secondVerticalReversed}, angles=" +
                $"${secondAngleAfterCirclePhase:X4}/${thirdAngleAfterCirclePhase:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0x00e0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 6)
        {
            throw new InvalidDataException(
                $"Green Brinstar Firefleas emitted only {oam.LastFinalizedSpriteCount} OBJ pieces.");
        }

        // All four camera-zero actors are now in the live interaction list. Ordinary touch
        // deals the header's four damage and then Fireflea's buggy private handler kills the
        // otherwise healthy actor, advancing darkness and the kill counter once.
        RoomEnemySlot normalTouchTarget = firefleas[3];
        samus.XPosition = normalTouchTarget.XPosition;
        samus.YPosition = normalTouchTarget.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 995 ||
            normalTouchTarget.Health != 0 ||
            !normalTouchTarget.Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1 || enemies.FirefleaDarknessLevel != 2)
        {
            throw new InvalidDataException(
                $"Fireflea normal-touch bug failed: Samus={samus.Health}, " +
                $"enemy={normalTouchTarget.Health}/${normalTouchTarget.Properties:X4}, " +
                $"killed={enemies.EnemiesKilled}, darkness={enemies.FirefleaDarknessLevel}.");
        }

        // Default vulnerability multiplier two turns a 20-point power beam into exactly
        // 20 damage, killing a fresh Fireflea and running its private shot-darkness tail.
        RoomEnemySlot shotTarget = firefleas[4];
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], shotTarget, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || shotTarget.Health != 0 ||
            enemies.EnemiesKilled != 2 || enemies.FirefleaDarknessLevel != 4)
        {
            throw new InvalidDataException(
                $"Fireflea shot reaction failed: health={shotTarget.Health}, " +
                $"killed={enemies.EnemiesKilled}, darkness={enemies.FirefleaDarknessLevel}.");
        }

        // Radius 16 derives a 12-pixel vertical ellipse. At the third actor's live center it
        // admits exactly that actor, applies default 200-point power-bomb damage, and sets
        // process-off-screen just as the native descending-slot pass does.
        RoomEnemySlot powerBombTarget = firefleas[2];
        ushort untouchedHealth = firefleas[1].Health;
        int powerBombReactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombTarget.XPosition,
            powerBombTarget.YPosition,
            explosionRadius: 16);
        if (powerBombReactions != 1 || powerBombTarget.Health != 0 ||
            !powerBombTarget.Properties.HasAny(
                EnemyProperties.Deleted | EnemyProperties.ProcessOffScreen) ||
            firefleas[1].Health != untouchedHealth || enemies.EnemiesKilled != 3 ||
            enemies.FirefleaDarknessLevel != 6)
        {
            throw new InvalidDataException(
                $"Fireflea power-bomb reaction failed: reactions={powerBombReactions}, " +
                $"target={powerBombTarget.Health}/${powerBombTarget.Properties:X4}, " +
                $"neighbor={firefleas[1].Health}, killed={enemies.EnemiesKilled}, " +
                $"darkness={enemies.FirefleaDarknessLevel}.");
        }

        // Screw Attack first kills the fourth tested actor through common touch AI, then
        // Fireflea's unconditional EnemyDeath call increments the counter a second time.
        // Darkness advances only once because the private tail itself has one increment.
        RoomEnemySlot doubleDeathTarget = firefleas[1];
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.XPosition = doubleDeathTarget.XPosition;
        samus.YPosition = doubleDeathTarget.YPosition;
        samus.InvincibilityTimer = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 3;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            doubleDeathTarget.Health != 0 || enemies.EnemiesKilled != 5 ||
            enemies.FirefleaDarknessLevel != 8)
        {
            throw new InvalidDataException(
                $"Fireflea double-death bug failed: health={doubleDeathTarget.Health}, " +
                $"killed={enemies.EnemiesKilled}, darkness={enemies.FirefleaDarknessLevel}.");
        }

        // The first actor was outside camera zero and is still alive. Admit it with camera
        // $100, let its instruction list produce a map, and prove the fifth retail death caps
        // normal-room darkness progression at ten.
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        enemies.StepFrame(0x0100, 0, false, samus, level: assets.LevelData);
        RoomEnemySlot finalTarget = firefleas[0];
        var finalProjectile = new SamusProjectileSystem();
        ArmProjectile(finalProjectile.Slots[0], finalTarget, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                finalProjectile,
                sharedProjectiles,
                samus) != 1 || finalTarget.Health != 0 ||
            enemies.EnemiesKilled != 6 || enemies.FirefleaDarknessLevel != 10)
        {
            throw new InvalidDataException(
                $"Final Fireflea darkness progression failed: health={finalTarget.Health}, " +
                $"killed={enemies.EnemiesKilled}, darkness={enemies.FirefleaDarknessLevel}.");
        }

        Console.WriteLine(
            "Green Brinstar Fireflea audit passed: five retail actors loaded; opposite " +
            $"circles and two vertical oscillators animated all {animationMaps.Count} ROM " +
            $"maps, rendered {oam.LastFinalizedSpriteCount} OBJ pieces, touch/shot/power-" +
            "bomb damage advanced darkness to ten, and the native double-death counter " +
            "ended at six kills for five actors.");
        return 0;
    }

    private static FirefleaEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.FirefleaStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Fireflea slot {slot.SlotIndex} has no typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }
}
