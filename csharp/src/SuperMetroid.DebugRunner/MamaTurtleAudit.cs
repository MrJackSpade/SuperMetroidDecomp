using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// End-to-end ROM audit for the untouched Mama Turtle room at $8F:D055. Population records,
/// headers, graphics, instruction lists, spritemaps, shell contour, collision data, and pose
/// metadata all remain cartridge reads; the harness supplies only player position/input and
/// ordinary beam actors, exactly the stimuli the retail room receives during play.
/// </summary>
internal static class MamaTurtleAudit
{
    private const ushort RoomPointer = 0xd055;
    private const ushort MamaDefinition = 0xcf3f;
    private const ushort BabyDefinition = 0xcf7f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeaders(bus);
        VerifyPopulationLinkageCrawlingAndDrawing(bus);
        VerifySleepingShellContourAndCorrection(bus);
        VerifyBabyTouchWake(bus);
        VerifyBabyShotWake(bus);
        VerifyBabyHideSpinAndReturn(bus);
        VerifyBabyContinuousRiderCarry(bus);
        VerifyMamaWakeHoverRiderDamageAndLanding(bus);

        Console.WriteLine(
            "Mama Turtle audit passed: the untouched $D055 room loaded one parent and four " +
            "linked children; asymmetric shell contour, both crawl directions, rider " +
            "displacement, hide/spin/return animation, touch/shot wake callbacks, Mama's " +
            "opening/hover/rise/peak/fall cycle, expanded 200-damage box, wall quake, " +
            "landing, Grapple cancel, and ROM OBJ maps were verified.");
        return 0;
    }

    private static void VerifySleepingShellContourAndCorrection(
        SuperMetroidAddressSpace bus)
    {
        LoadedTurtles leftLoad = Load(bus);
        Step(leftLoad, 0);
        RoomEnemySlot leftMama = leftLoad.Mama;
        leftLoad.Samus.XPosition = unchecked((ushort)(leftMama.XPosition - 8));
        leftLoad.Samus.YPosition = 0;
        Step(leftLoad, 1);
        if (leftMama.YRadius != 15)
        {
            throw new InvalidDataException(
                $"Sleeping shell left contour at distance eight was {leftMama.YRadius}, expected 15.");
        }

        LoadedTurtles rightLoad = Load(bus);
        Step(rightLoad, 0);
        RoomEnemySlot rightMama = rightLoad.Mama;
        rightLoad.Samus.XPosition = unchecked((ushort)(rightMama.XPosition + 8));
        rightLoad.Samus.YPosition = 0;
        Step(rightLoad, 1);
        if (rightMama.YRadius != 12)
        {
            throw new InvalidDataException(
                $"Sleeping shell right contour at distance eight was {rightMama.YRadius}, expected 12.");
        }

        LoadedTurtles correctionLoad = Load(bus);
        Step(correctionLoad, 0);
        RoomEnemySlot correctionMama = correctionLoad.Mama;
        correctionLoad.Samus.XPosition = correctionMama.XPosition;
        // Center contour radius is 16. Put Samus's feet two pixels inside its top surface;
        // the native overlap correction must publish -2 in extra Y rather than teleporting.
        correctionLoad.Samus.YPosition = unchecked((ushort)(
            correctionMama.YPosition - 16 - correctionLoad.Samus.Kinematics.YRadius + 2));
        correctionLoad.Samus.Kinematics.ExtraYDisplacement = 0;
        Step(correctionLoad, 1);
        if (correctionMama.YRadius != 16 ||
            correctionLoad.Samus.Kinematics.ExtraYDisplacement != 0xfffe)
        {
            throw new InvalidDataException(
                $"Sleeping shell rider correction failed: radius={correctionMama.YRadius}, " +
                $"extraY=${correctionLoad.Samus.Kinematics.ExtraYDisplacement:X4}.");
        }
    }

    private static void VerifyHeaders(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition mama = RoomEnemySystem.ReadDefinition(bus, MamaDefinition);
        RoomEnemyDefinition baby = RoomEnemySystem.ReadDefinition(bus, BabyDefinition);
        if (mama.TileDataSize != 0x0c00 || mama.PalettePointer != 0x8b60 ||
            mama.Health != 20000 || mama.Damage != 200 || mama.XRadius != 20 ||
            mama.YRadius != 16 || mama.Bank != 0xa2 || mama.InitializationAiPointer != 0x8d6c ||
            mama.PartCount != 5 || mama.MainAiPointer != 0x8dd2 ||
            mama.GrappleAiPointer != 0x800f || mama.HurtAiPointer != 0x804c ||
            mama.FrozenAiPointer != 0x8041 || mama.DeathAnimation != 4 ||
            mama.TouchAiPointer != 0x9281 || mama.ShotAiPointer != 0x802d ||
            mama.TileDataAddress != 0xacd400 || mama.Layer != 5)
        {
            throw new InvalidDataException("Mama Turtle header does not match $A0:CF3F.");
        }

        if (baby.TileDataSize != 0x0c00 || baby.PalettePointer != 0x8b60 ||
            baby.Health != 20000 || baby.Damage != 0 || baby.XRadius != 8 ||
            baby.YRadius != 5 || baby.Bank != 0xa2 || baby.InitializationAiPointer != 0x8d9d ||
            baby.MainAiPointer != 0x912e || baby.GrappleAiPointer != 0x800f ||
            baby.HurtAiPointer != 0x804c || baby.FrozenAiPointer != 0x8041 ||
            baby.DeathAnimation != 0 || baby.TouchAiPointer != 0x929f ||
            baby.ShotAiPointer != 0x930f || baby.TileDataAddress != 0xacd400 ||
            baby.Layer != 5)
        {
            throw new InvalidDataException(
                $"Baby Turtle header does not match $A0:CF7F: tile=${baby.TileDataSize:X4}, " +
                $"palette=${baby.PalettePointer:X4}, health/damage={baby.Health}/{baby.Damage}, " +
                $"radii={baby.XRadius}/{baby.YRadius}, bank=${baby.Bank:X2}, init/main=" +
                $"${baby.InitializationAiPointer:X4}/${baby.MainAiPointer:X4}, grapple/hurt/frozen=" +
                $"${baby.GrappleAiPointer:X4}/${baby.HurtAiPointer:X4}/${baby.FrozenAiPointer:X4}, " +
                $"death={baby.DeathAnimation}, touch/shot=${baby.TouchAiPointer:X4}/" +
                $"${baby.ShotAiPointer:X4}, tiles=${baby.TileDataAddress:X6}, layer={baby.Layer}.");
        }
    }

    private static void VerifyPopulationLinkageCrawlingAndDrawing(
        SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        RoomEnemySlot mama = loaded.Mama;
        MamaTurtleEnemyState mamaState = MamaState(loaded);
        ushort[] expectedX = [0x01d8, 0x01b0, 0x01d8, 0x01e8, 0x0218];
        ushort[] expectedVelocity = [0, 0xffff, 0xffff, 1, 1];

        if (loaded.Room.State.Pointer != 0xd062 || loaded.Enemies.EnemyCount != 5 ||
            loaded.Enemies.DeathQuota != 1 || loaded.Enemies.GraphicsSet.Count != 1 ||
            loaded.Enemies.GraphicsSet[0].DefinitionPointer != MamaDefinition ||
            mama.EnemyDefinitionPointer != MamaDefinition || mama.XPosition != expectedX[0] ||
            mama.YPosition != 0x03cd || mama.YRadius != 0 || mama.Properties != 0xa800 ||
            mama.CurrentInstruction != 0x8c44 || mama.SpritemapPointer != 0x804d ||
            mamaState.Function != MamaTurtleAiFunction.Initial || mamaState.AsleepFlag != 1)
        {
            throw new InvalidDataException(
                $"Mama Turtle retail population/header initialization failed: state=" +
                $"${loaded.Room.State.Pointer:X4}, population=" +
                $"${loaded.Room.State.EnemyPopulationPointer:X4}, set=" +
                $"${loaded.Room.State.EnemyTilesetPointer:X4}, count={loaded.Enemies.EnemyCount}, " +
                $"Mama=({mama.XPosition:X4},{mama.YPosition:X4})/" +
                $"{mamaState.Function}/${mamaState.AsleepFlag}.");
        }

        for (int index = 1; index < 5; index++)
        {
            RoomEnemySlot baby = loaded.Enemies.Slots[index];
            BabyTurtleEnemyState state = BabyState(loaded, index);
            if (baby.EnemyDefinitionPointer != BabyDefinition || baby.XPosition != expectedX[index] ||
                baby.YPosition != 0x03cd || baby.YRadius != 5 || baby.Properties != 0xa800 ||
                state.SpawnXPosition != expectedX[index] || state.SpawnTopBoundary != 0x03c8 ||
                state.XVelocity != expectedVelocity[index] ||
                state.Function != BabyTurtleAiFunction.CrawlingNotCarryingSamus ||
                baby.CurrentInstruction != (index <= 2 ? 0x8b80 : 0x8c72))
            {
                throw new InvalidDataException(
                    $"Baby Turtle slot {index} did not retain its exact retail record: " +
                    $"position=({baby.XPosition:X4},{baby.YPosition:X4}), velocity=" +
                    $"${state.XVelocity:X4}, list=$A2:{baby.CurrentInstruction:X4}.");
            }
        }

        Step(loaded, 0);
        if (mamaState.Function != MamaTurtleAiFunction.Asleep || mama.SpritemapPointer != 0x9535)
            throw new InvalidDataException("Mama Turtle did not execute its first-frame family link.");
        for (int index = 1; index < 5; index++)
        {
            RoomEnemySlot baby = loaded.Enemies.Slots[index];
            BabyTurtleEnemyState state = BabyState(loaded, index);
            if (state.ParentNativeIndex != mama.NativeIndex ||
                baby.PaletteIndex != mama.PaletteIndex || baby.VramTilesIndex != mama.VramTilesIndex)
            {
                throw new InvalidDataException(
                    $"Baby Turtle slot {index} was not linked to parent ${mama.NativeIndex:X4} " +
                    "with the parent's palette/VRAM indexes.");
            }
        }

        var leftMaps = new HashSet<ushort>();
        var rightMaps = new HashSet<ushort>();
        ushort leftStart = loaded.Enemies.Slots[1].XPosition;
        ushort rightStart = loaded.Enemies.Slots[4].XPosition;
        for (int frame = 1; frame <= 200; frame++)
        {
            Step(loaded, unchecked((byte)frame));
            leftMaps.Add(loaded.Enemies.Slots[1].SpritemapPointer);
            rightMaps.Add(loaded.Enemies.Slots[4].SpritemapPointer);
        }
        ushort[] expectedLeftMaps = [0x94d9, 0x94e0, 0x94e7, 0x94ee, 0x94f5, 0x9501, 0x950d];
        ushort[] expectedRightMaps = [0x9733, 0x973a, 0x9741, 0x9748, 0x974f, 0x975b, 0x9767];
        if (expectedLeftMaps.Any(map => !leftMaps.Contains(map)) ||
            expectedRightMaps.Any(map => !rightMaps.Contains(map)) ||
            loaded.Enemies.Slots[1].XPosition >= leftStart ||
            loaded.Enemies.Slots[4].XPosition <= rightStart)
        {
            throw new InvalidDataException(
                $"Baby crawl bytecode failed: left X=${leftStart:X4}->" +
                $"${loaded.Enemies.Slots[1].XPosition:X4}, maps={string.Join(',', leftMaps)}, " +
                $"right X=${rightStart:X4}->${loaded.Enemies.Slots[4].XPosition:X4}, " +
                $"maps={string.Join(',', rightMaps)}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(loaded);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 10)
        {
            throw new InvalidDataException(
                $"Five live Turtle ROM maps emitted only {oam.LastFinalizedSpriteCount} OBJ pieces.");
        }

        GrappleEnemyCollision grapple = loaded.Enemies.ResolveGrappleEndpoint(
            loaded.Enemies.Slots[1].XPosition,
            loaded.Enemies.Slots[1].YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Baby Turtle did not execute common Grapple-cancel AI.");
    }

    private static void VerifyBabyTouchWake(SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        Step(loaded, 0);
        RoomEnemySlot baby = loaded.Enemies.Slots[1];
        BabyTurtleEnemyState babyState = BabyState(loaded, 1);
        loaded.Samus.XPosition = baby.XPosition;
        loaded.Samus.YPosition = baby.YPosition;
        ushort oldVelocity = babyState.XVelocity;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(
                loaded.Samus, 0, loaded.Assets.LevelData) ||
            MamaState(loaded).AsleepFlag != 0 || oldVelocity != 0xffff ||
            babyState.XVelocity != 1 ||
            babyState.Function != BabyTurtleAiFunction.CrawlingNotCarryingSamus)
        {
            throw new InvalidDataException(
                $"Baby touch wake failed: asleep={MamaState(loaded).AsleepFlag}, " +
                $"velocity=${oldVelocity:X4}->${babyState.XVelocity:X4}, " +
                $"function={babyState.Function}.");
        }
    }

    private static void VerifyBabyShotWake(SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        Step(loaded, 0);
        RoomEnemySlot baby = loaded.Enemies.Slots[2];
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], baby, damage: 100);
        ushort health = baby.Health;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus, projectiles, shared, loaded.Samus);
        if (hits != 1 || MamaState(loaded).AsleepFlag != 0 || baby.Health != health ||
            baby.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Baby shot wake failed: hits={hits}, asleep={MamaState(loaded).AsleepFlag}, " +
                $"health={health}->{baby.Health}, properties=${baby.Properties:X4}.");
        }
    }

    private static void VerifyBabyHideSpinAndReturn(SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        Step(loaded, 0);
        RoomEnemySlot baby = loaded.Enemies.Slots[1];
        BabyTurtleEnemyState state = BabyState(loaded, 1);

        PutSamusOnTop(loaded.Samus, baby);
        loaded.Samus.Kinematics.ExtraXDisplacement = 0;
        loaded.Samus.Kinematics.ExtraYDisplacement = 0;
        Step(loaded, 1);
        if (state.Function != BabyTurtleAiFunction.HidingCarryingSamus ||
            state.NotCarryingSamusReactionTimer != 4 || baby.SpritemapPointer != 0x94f5)
        {
            throw new InvalidDataException(
                $"Baby did not hide while ridden: function={state.Function}, timer=" +
                $"{state.NotCarryingSamusReactionTimer}, map=$A2:{baby.SpritemapPointer:X4}.");
        }

        PutSamusFarAway(loaded);
        for (byte frame = 2; frame <= 5; frame++)
            Step(loaded, frame);
        if (state.Function != BabyTurtleAiFunction.HidingNotCarryingSamus ||
            state.FunctionTimer != 60)
        {
            throw new InvalidDataException(
                $"Baby release debounce failed: function={state.Function}, " +
                $"timer={state.FunctionTimer}.");
        }

        PutSamusOnTop(loaded.Samus, baby);
        Step(loaded, 6);
        if (state.Function != BabyTurtleAiFunction.SpinningUnstoppable ||
            state.XVelocity != 3 || state.YVelocity != 1 || baby.SpritemapPointer != 0x9519)
        {
            throw new InvalidDataException(
                $"Baby launch failed: function={state.Function}, velocity=" +
                $"${state.XVelocity:X4}/${state.YVelocity:X4}, map=$A2:{baby.SpritemapPointer:X4}.");
        }

        PutSamusFarAway(loaded);
        Step(loaded, 7);
        if (loaded.Enemies.LastMamaTurtleSoundEffect != 0x003a)
            throw new InvalidDataException("Baby spin list did not queue cartridge sound $3A.");

        ushort launchedX = baby.XPosition;
        ushort launchedY = baby.YPosition;
        for (int frame = 8; frame < 40 &&
            state.Function != BabyTurtleAiFunction.SpinningStoppable; frame++)
        {
            Step(loaded, unchecked((byte)frame));
        }
        if (state.Function != BabyTurtleAiFunction.SpinningStoppable ||
            baby.XPosition <= launchedX || baby.YPosition <= launchedY)
        {
            throw new InvalidDataException(
                $"Baby spinning motion/list handoff failed: function={state.Function}, " +
                $"position=({launchedX:X4},{launchedY:X4})->" +
                $"({baby.XPosition:X4},{baby.YPosition:X4}).");
        }

        PutSamusOnTop(loaded.Samus, baby);
        Step(loaded, 40);
        if (state.Function != BabyTurtleAiFunction.CrawlingNotCarryingSamus)
            throw new InvalidDataException("Ridden stoppable Baby did not return to crawling.");
    }

    private static void VerifyBabyContinuousRiderCarry(SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        Step(loaded, 0);

        // Use the final physical child. Baby main AI resets the parent's extended word six
        // before dispatch; choosing the last child proves the carrying state republishes its
        // radius after every earlier sibling has run, exactly like native slot ordering.
        RoomEnemySlot baby = loaded.Enemies.Slots[4];
        BabyTurtleEnemyState state = BabyState(loaded, 4);
        MamaTurtleEnemyState parent = MamaState(loaded);
        bool sawCarryingFunction = false;
        bool sawCrawlCarryX = false;
        bool sawParentHeight = false;
        for (int frame = 1; frame < 180; frame++)
        {
            PutSamusOnTop(loaded.Samus, baby);
            loaded.Samus.Kinematics.ExtraXDisplacement = 0;
            loaded.Samus.Kinematics.ExtraYDisplacement = 0;
            Step(loaded, unchecked((byte)frame));
            sawCarryingFunction |= state.Function ==
                BabyTurtleAiFunction.CrawlingCarryingSamus;
            sawCrawlCarryX |= state.Function == BabyTurtleAiFunction.CrawlingCarryingSamus &&
                loaded.Samus.Kinematics.ExtraXDisplacement == 1;
            sawParentHeight |= parent.CarryingChildHeight == baby.YRadius;
        }

        if (!sawCarryingFunction || !sawCrawlCarryX || !sawParentHeight)
        {
            throw new InvalidDataException(
                $"Continuous Baby rider path failed: carrying={sawCarryingFunction}, " +
                $"crawl extra-X={sawCrawlCarryX}, parent height={sawParentHeight}, " +
                $"final function={state.Function}.");
        }
    }

    private static void VerifyMamaWakeHoverRiderDamageAndLanding(
        SuperMetroidAddressSpace bus)
    {
        LoadedTurtles loaded = Load(bus);
        Step(loaded, 0);
        WakeMamaWithBeam(loaded, loaded.Enemies.Slots[2]);
        PutSamusFarAway(loaded);

        MamaTurtleEnemyState state = MamaState(loaded);
        RoomEnemySlot mama = loaded.Mama;
        var functions = new HashSet<MamaTurtleAiFunction>();
        var maps = new HashSet<ushort>();
        int frame = 1;
        for (; frame < 512 && state.Function != MamaTurtleAiFunction.Hovering; frame++)
        {
            Step(loaded, unchecked((byte)frame));
            functions.Add(state.Function);
            maps.Add(mama.SpritemapPointer);
        }
        MamaTurtleAiFunction[] expectedOpeningFunctions =
        [
            MamaTurtleAiFunction.LeavingShell,
            MamaTurtleAiFunction.Idle,
            MamaTurtleAiFunction.EnteringShell,
            MamaTurtleAiFunction.RisingToHover,
            MamaTurtleAiFunction.Hovering,
        ];
        ushort[] expectedOpeningMaps = [0x9535, 0x9555, 0x959d, 0x95e5, 0x96c9, 0x96e9];
        if (state.Function != MamaTurtleAiFunction.Hovering ||
            expectedOpeningFunctions.Any(function => !functions.Contains(function)) ||
            expectedOpeningMaps.Any(map => !maps.Contains(map)) || mama.YRadius != 16)
        {
            throw new InvalidDataException(
                $"Mama opening/hover handoff failed after {frame} frames: function=" +
                $"{state.Function}, maps={string.Join(',', maps)}, functions=" +
                $"{string.Join(',', functions)}, radius={mama.YRadius}.");
        }

        // $9315 expands Mama's X box by eight pixels. Center distance 29 is outside the
        // ordinary 20+5 radius sum yet overlaps that private box, proving main AI owns it.
        loaded.Samus.Health = 999;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.XPosition = unchecked((ushort)(mama.XPosition + 29));
        loaded.Samus.YPosition = mama.YPosition;
        Step(loaded, unchecked((byte)frame++));
        if (loaded.Samus.Health != 799 || !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Mama expanded touch box failed: health={loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}.");
        }

        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackTimer = 0;
        PutSamusOnTop(loaded.Samus, mama);
        loaded.Samus.Kinematics.ExtraXDisplacement = 0;
        loaded.Samus.Kinematics.ExtraXSubdisplacement = 0;
        Step(loaded, unchecked((byte)frame++));
        if (state.Function != MamaTurtleAiFunction.RisingToPeak)
            throw new InvalidDataException("Landing on hovering Mama did not start peak ascent.");

        bool sawSevenPixelCarry = false;
        for (; frame < 768 && state.Function != MamaTurtleAiFunction.HoveringAtPeak; frame++)
        {
            PutSamusOnTop(loaded.Samus, mama);
            loaded.Samus.Kinematics.ExtraYDisplacement = 0;
            Step(loaded, unchecked((byte)frame));
            sawSevenPixelCarry |= loaded.Samus.Kinematics.ExtraYDisplacement == 0xfff9;
        }
        if (state.Function != MamaTurtleAiFunction.HoveringAtPeak ||
            !sawSevenPixelCarry || mama.YPosition >= 0x01e8 || state.FunctionTimer != 30)
        {
            throw new InvalidDataException(
                $"Mama rider ascent failed: function={state.Function}, Y=${mama.YPosition:X4}, " +
                $"timer={state.FunctionTimer}, carried={sawSevenPixelCarry}.");
        }

        PutSamusFarAway(loaded);
        for (int pause = 0; pause < 30; pause++, frame++)
            Step(loaded, unchecked((byte)frame));
        if (state.Function != MamaTurtleAiFunction.Falling)
            throw new InvalidDataException("Mama's exact 30-frame peak pause did not end in falling.");

        ushort fallStart = mama.YPosition;
        bool landed = false;
        for (; frame < 1200; frame++)
        {
            Step(loaded, unchecked((byte)frame));
            if (state.Function == MamaTurtleAiFunction.Idle && mama.YPosition > fallStart)
            {
                landed = true;
                break;
            }
        }
        // EnemyMain installs the list, then the same frame's instruction phase consumes its
        // first timed map and leaves the cursor at the following entry.
        if (!landed || unchecked((short)state.YVelocity) > 4 ||
            mama.CurrentInstruction is not (0x8c4e or 0x8d2c))
        {
            throw new InvalidDataException(
                $"Mama fall/landing failed: landed={landed}, function={state.Function}, " +
                $"Y=${fallStart:X4}->${mama.YPosition:X4}, speed=${state.YVelocity:X4}, " +
                $"list=$A2:{mama.CurrentInstruction:X4}.");
        }

        // A separate untouched load keeps Mama hovering without a rider until the retail
        // room wall produces the quirky fixed-point reversal and global quake.
        LoadedTurtles wallLoad = Load(bus);
        Step(wallLoad, 0);
        WakeMamaWithBeam(wallLoad, wallLoad.Enemies.Slots[2]);
        PutSamusFarAway(wallLoad);
        MamaTurtleEnemyState wallState = MamaState(wallLoad);
        int wallFrame = 1;
        while (wallFrame < 512 && wallState.Function != MamaTurtleAiFunction.Hovering)
            Step(wallLoad, unchecked((byte)wallFrame++));
        ushort velocityBeforeWall = wallState.XVelocity;
        while (wallFrame < 1600 && wallLoad.Enemies.EarthquakeTimer != 16)
        {
            velocityBeforeWall = wallState.XVelocity;
            Step(wallLoad, unchecked((byte)wallFrame++));
        }
        if (wallLoad.Enemies.EarthquakeTimer != 16 || wallLoad.Enemies.EarthquakeType != 0 ||
            unchecked((short)velocityBeforeWall) == 0 ||
            Math.Sign(unchecked((short)velocityBeforeWall)) ==
                Math.Sign(unchecked((short)wallState.XVelocity)))
        {
            throw new InvalidDataException(
                $"Mama wall reversal failed: quake={wallLoad.Enemies.EarthquakeType}/" +
                $"{wallLoad.Enemies.EarthquakeTimer}, velocity=" +
                $"${velocityBeforeWall:X4}->${wallState.XVelocity:X4}.");
        }
    }

    private static LoadedTurtles Load(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        SamusState samus = CreateSamus(bus);
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
        return new LoadedTurtles(bus, room, assets, enemies, samus);
    }

    private static void Step(LoadedTurtles loaded, byte nmiFrameCounter8)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(loaded);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: loaded.Assets.LevelData,
            nmiFrameCounter8: nmiFrameCounter8);
    }

    private static (ushort X, ushort Y) CenterCamera(LoadedTurtles loaded)
    {
        int maximumX = Math.Max(0, loaded.Room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, loaded.Room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(loaded.Mama.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(loaded.Mama.YPosition - 112, 0, maximumY)));
    }

    private static void WakeMamaWithBeam(LoadedTurtles loaded, RoomEnemySlot baby)
    {
        var projectiles = new SamusProjectileSystem();
        ArmProjectile(projectiles.Slots[0], baby, damage: 5);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            loaded.Bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (hits != 1 || MamaState(loaded).AsleepFlag != 0)
            throw new InvalidDataException("Controlled Baby beam did not wake Mama Turtle.");
    }

    private static void PutSamusOnTop(SamusState samus, RoomEnemySlot actor)
    {
        samus.XPosition = actor.XPosition;
        samus.YPosition = unchecked((ushort)(
            actor.YPosition - actor.YRadius - samus.Kinematics.YRadius));
    }

    private static void PutSamusFarAway(LoadedTurtles loaded)
    {
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackTimer = 0;
    }

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static MamaTurtleEnemyState MamaState(LoadedTurtles loaded) =>
        loaded.Enemies.MamaTurtleStates[loaded.Mama.SlotIndex] ??
        throw new InvalidDataException("Mama Turtle typed state is absent.");

    private static BabyTurtleEnemyState BabyState(LoadedTurtles loaded, int slotIndex) =>
        loaded.Enemies.BabyTurtleStates[slotIndex] ??
        throw new InvalidDataException($"Baby Turtle slot {slotIndex} typed state is absent.");

    private readonly record struct LoadedTurtles(
        SuperMetroidAddressSpace Bus,
        CartridgeRoomHeader Room,
        CartridgeRoomAssets Assets,
        RoomEnemySystem Enemies,
        SamusState Samus)
    {
        public RoomEnemySlot Mama => Enemies.Slots[0];
    }
}
