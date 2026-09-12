using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>Retail Butterfly-room regression for all three phases of Zoa behavior.</summary>
internal static class ZoaAudit
{
    private const ushort ButterflyRoomHeader = 0xd5ec;
    private const ushort ButterflyRoomState = 0xd5f9;
    private const ushort ZoaDefinition = 0xda7f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, ButterflyRoomHeader);
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

        if (room.State.Pointer != ButterflyRoomState || enemies.EnemyCount != 3 ||
            enemies.Slots.Take(3).Any(slot => slot.EnemyDefinitionPointer != ZoaDefinition) ||
            enemies.ZoaStates.Take(3).Any(state => state is null))
        {
            throw new InvalidDataException(
                $"Butterfly selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} non-uniform or uninitialized Zoa actors.");
        }

        RoomEnemySlot leftLaunchingSlot = enemies.Slots[0];
        RoomEnemySlot rightLaunchingSlot = enemies.Slots[1];
        ZoaEnemyState leftState = enemies.ZoaStates[0]!;
        ZoaEnemyState rightState = enemies.ZoaStates[1]!;
        if (!leftLaunchingSlot.Properties.HasAny(EnemyProperties.Invisible) ||
            leftState.Function != ZoaEnemyFunction.WaitForSamus ||
            leftState.SpawnXPosition != 0x0098 || leftState.SpawnYPosition != 0x00d8 ||
            leftLaunchingSlot.Health != 40 || leftLaunchingSlot.Definition.Damage != 15)
        {
            throw new InvalidDataException(
                "First Butterfly Zoa disagrees with retail init data: " +
                $"position=(${leftState.SpawnXPosition:X4},${leftState.SpawnYPosition:X4}), " +
                $"function=$A3:{(ushort)leftState.Function:X4}, " +
                $"health/damage={leftLaunchingSlot.Health}/{leftLaunchingSlot.Definition.Damage}.");
        }

        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        // Samus at X=$80 lies left of slot zero ($98) and right of slot one ($68), proving
        // both facing/launch branches against one unchanged retail population.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (leftState.Function != ZoaEnemyFunction.Rising ||
            rightState.Function != ZoaEnemyFunction.Rising ||
            leftState.InstructionListTableIndex != 1 ||
            rightState.InstructionListTableIndex != 3)
        {
            throw new InvalidDataException(
                $"Zoa wake/facing failed: left=$A3:{(ushort)leftState.Function:X4}/" +
                $"{leftState.InstructionListTableIndex}, right=$A3:{(ushort)rightState.Function:X4}/" +
                $"{rightState.InstructionListTableIndex}.");
        }

        ushort initialY = leftLaunchingSlot.YPosition;
        ushort minimumY = initialY;
        ushort minimumX = leftLaunchingSlot.XPosition;
        ushort maximumX = rightLaunchingSlot.XPosition;
        var maps = new HashSet<ushort>();
        var speedIndexes = new HashSet<ushort>();
        bool sawLeftShooting = false;
        bool sawRightShooting = false;
        bool leftReset = false;
        bool rightReset = false;
        for (int frame = 0; frame < 500 && !(leftReset && rightReset); frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            maps.Add(leftLaunchingSlot.SpritemapPointer);
            speedIndexes.Add(leftState.XSpeedTableIndex);
            minimumY = Math.Min(minimumY, leftLaunchingSlot.YPosition);
            minimumX = Math.Min(minimumX, leftLaunchingSlot.XPosition);
            maximumX = Math.Max(maximumX, rightLaunchingSlot.XPosition);
            sawLeftShooting |= leftState.Function == ZoaEnemyFunction.Shooting &&
                leftState.InstructionListTableIndex == 0;
            sawRightShooting |= rightState.Function == ZoaEnemyFunction.Shooting &&
                rightState.InstructionListTableIndex == 2;
            leftReset |= sawLeftShooting && leftState.Function == ZoaEnemyFunction.WaitForSamus;
            rightReset |= sawRightShooting && rightState.Function == ZoaEnemyFunction.WaitForSamus;
        }

        if (!sawLeftShooting || !sawRightShooting || !leftReset || !rightReset ||
            minimumY >= initialY || minimumX >= leftState.SpawnXPosition ||
            maximumX <= rightState.SpawnXPosition || maps.Count < 6 ||
            !speedIndexes.IsSupersetOf([0, 4, 8, 12]) ||
            leftLaunchingSlot.XPosition != leftState.SpawnXPosition ||
            leftLaunchingSlot.YPosition != leftState.SpawnYPosition ||
            !leftLaunchingSlot.Properties.HasAny(EnemyProperties.Invisible))
        {
            throw new InvalidDataException(
                $"Zoa rise/launch/reset failed: left/right shooting={sawLeftShooting}/{sawRightShooting}, " +
                $"reset={leftReset}/{rightReset}, minY={minimumY}, X range={minimumX}..{maximumX}, " +
                $"maps={maps.Count}, speeds={string.Join(',', speedIndexes)}, " +
                $"final=({leftLaunchingSlot.XPosition},{leftLaunchingSlot.YPosition}).");
        }

        // Wake the actors again before drawing and collision so their real rising map is
        // visible. Common contact must consume the header's literal fifteen damage.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Butterfly Zoa ROM spritemaps emitted no enemy OBJ.");

        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.XPosition = leftLaunchingSlot.XPosition;
        samus.YPosition = leftLaunchingSlot.YPosition;
        samus.Health = 99;
        samus.InvincibilityTimer = 0;
        var beforeContact = EnemyContactAuditAssertions.Capture(samus);
        if (!enemies.ResolveOrdinarySamusContact(samus, 0))
        {
            throw new InvalidDataException(
                $"Zoa common contact failed: health={samus.Health}, knockback={samus.KnockbackActive}.");
        }
        EnemyContactAuditAssertions.VerifyStandingAirHit(
            bus, samus, beforeContact, 15, 1, "Zoa body contact");

        Console.WriteLine(
            "Butterfly Zoa audit passed: three retail actors loaded, opposite-facing wake/" +
            $"rise/launch/reset paths ran through four speed stages and {maps.Count} maps, " +
            $"contact dealt 15 damage, and {oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }
}
