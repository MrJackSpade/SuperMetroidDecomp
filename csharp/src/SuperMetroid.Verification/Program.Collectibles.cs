using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Exercises the common bank-$84 item implementation across all twenty-one identities
    /// and all three retail presentations. The fixture deliberately supplies ROM-shaped
    /// headers, instruction pointers, draw lists, graphics, and room-population records;
    /// no test-only spawn route bypasses cartridge decoding.
    /// </summary>
    static void VerifyPermanentCollectibles()
    {
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        // Full-table audit: unused native entries still allocate a deleting PLM,
        // not a breakable-block range failure or a fabricated terrain mutation.
        for (int bts = 0x11; bts <= 0x4e; bts++)
        {
            if (bts > 0x3f && bts != 0x4e) continue;
            var emptyLevel = CreateRoom(16, 16, new ushort[256], new byte[256], blockDefinitions: new byte[0x400 * 8]);
            var unusedPlms = new RoomPlmSystem();
            AssertTrue(unusedPlms.TrySpawnProjectileShotBlock(emptyLevel, 0, (byte)bts, 0, true),
                "unused shootable entry allocates native nothing PLM");
            AssertEqual(1, unusedPlms.ActiveCount, "nothing PLM retains its provisional slot");
            AssertEqual((ushort)0, emptyLevel.ForegroundEntries.Span[0], "nothing PLM leaves terrain unchanged");
        }

        for (int kindIndex = 0; kindIndex < 21; kindIndex++)
        {
            InWorldCollectibleKind kind = (InWorldCollectibleKind)kindIndex;
            CollectibleFixture fixture = LoadCollectible(
                bus,
                header: unchecked((ushort)(0xeed7 + kindIndex * 4)),
                roomArgument: unchecked((ushort)kindIndex),
                precollected: false);

            AssertEqual(1, fixture.Plms.Collectibles.Count,
                $"{kind} exposed header loads through room population");
            CollectiblePlmSnapshot loaded = fixture.Plms.Collectibles[0];
            AssertEqual(kind, loaded.Kind, $"{kind} header identity");
            AssertEqual(CollectiblePresentation.Exposed, loaded.Presentation,
                $"{kind} exposed presentation");
            if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                fixture.Samus.ProjectileFlareCounter = 60;

            fixture.Plms.Step(
                bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            if (kind >= InWorldCollectibleKind.Bombs)
            {
                // LoadItemPLMGfx mutates the live block-definition table before the first
                // item draw. Verify the camera/DrawPLM staging source—not only LevelData's
                // parallel debugger copy—expands the assigned dynamic character names.
                PlmTilemapUpdate visibleUpdate = fixture.Streamer.BuildPlmLevelBlockUpdate(
                    fixture.BlockIndex,
                    bg1XOffset: 0);
                int expectedFirstCharacter = 0x03e0 + loaded.GraphicsSlot * 8;
                AssertEqual(
                    expectedFirstCharacter,
                    visibleUpdate.TopRow[0] & 0x03ff,
                    $"{kind} live streamer uses its dynamic PLM character definition");
            }
            AssertTrue(fixture.Plms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                $"{kind} visible block accepts Samus contact");
            fixture.Plms.Step(
                bus, fixture.Level, fixture.Streamer, 0, 0, 0);

            AssertTrue(fixture.System.HasCollectedItemBit(kindIndex),
                $"{kind} marks its physical item bit");
            AssertEqual(1, fixture.Plms.CollectiblePickupEvents.Count,
                $"{kind} publishes exactly one acquisition");
            AssertEqual(kind, fixture.Plms.CollectiblePickupEvents[0].Kind,
                $"{kind} acquisition identity");
            AssertTrue(
                fixture.Plms.ConsumeCollectibleFanfareRequest(),
                $"{kind} publishes one shared permanent-item fanfare edge");
            AssertTrue(
                !fixture.Plms.ConsumeCollectibleFanfareRequest(),
                $"{kind} fanfare edge cannot repeat during its synchronous message");
            AssertPermanentCollectibleEffect(kind, fixture.Samus);

            // Acquisition frees the native physical ID. Reuse that same highest slot for
            // a destructible special block, exactly as the block beside Morph Ball does.
            // The new actor must not inherit Item/Triggered and publish the old message.
            AssertTrue(
                fixture.Plms.TrySpawnBombedSpecialBlock(
                    fixture.Level,
                    fixture.BlockIndex,
                    behavior: 8,
                    areaIndex: AreaId.Crateria,
                    projectileType: 0x0500),
                $"{kind} freed PLM slot can be reused by a bomb-special actor");
            AssertEqual(0, fixture.Plms.Collectibles.Count,
                $"{kind} semantic owner does not survive physical slot reuse");
            fixture.Plms.Step(
                bus, fixture.Level, fixture.Streamer, 0, 0, 0);
            AssertEqual(0, fixture.Plms.CollectiblePickupEvents.Count,
                $"{kind} reused slot cannot republish its pickup message");
            if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
            {
                AssertEqual((ushort)0, fixture.Samus.ProjectileFlareCounter,
                    $"{kind} PLM clears charge counter before its message");
            }
        }

        // A collected exposed item must draw blank and delete without applying its effect
        // again. This is the critical distinction between physical-location bits and the
        // inventory words that multiple tanks share.
        CollectibleFixture collected = LoadCollectible(
            bus,
            header: 0xeed7,
            roomArgument: 37,
            precollected: true);
        collected.Plms.Step(bus, collected.Level, collected.Streamer, 0, 0, 0);
        AssertEqual(0, collected.Plms.ActiveCount,
            "previously collected exposed item removes itself");
        AssertEqual(99, collected.Samus.MaxHealth,
            "previously collected exposed item does not grant twice");

        // A Chozo orb accepts projectile contact, performs its authored burst, then becomes
        // the same visible/touchable item. Acquisition must retain the triggering physical
        // location rather than confusing the projectile type with an item bit.
        CollectibleFixture orb = LoadCollectible(
            bus,
            header: unchecked((ushort)(0xef2b + (int)InWorldCollectibleKind.MorphBall * 4)),
            roomArgument: 91,
            precollected: false);
        orb.Plms.Step(bus, orb.Level, orb.Streamer, 0, 0, 0);
        var grappleSamus = new SamusState();
        grappleSamus.XPosition = (ushort)((orb.BlockIndex % orb.Level.WidthInBlocks) * 16 + 8);
        grappleSamus.YPosition = (ushort)((orb.BlockIndex / orb.Level.WidthInBlocks) * 16 + 8);
        grappleSamus.Grapple.Phase = GrapplePhase.Firing;
        var orbHit = SamusGrappleMovement.StepFiring(bus, orb.Level, grappleSamus,
            (ushort)SnesButton.X, orb.Plms);
        AssertTrue(orbHit.CancelQueued, "grapple collides with the solid Chozo orb");
        orb.Plms.Step(bus, orb.Level, orb.Streamer, 0, 0, 0);
        AssertTrue(orb.System.HasRoomChozoBit(91),
            "breaking a Chozo orb persists before item acquisition");
        AssertTrue(!orb.System.HasCollectedItemBit(91),
            "breaking a Chozo orb does not prematurely collect its item");
        StepFrames(9, _ => orb.Plms.Step(bus, orb.Level, orb.Streamer, 0, 0, 0));
        AssertEqual(CollectiblePhase.Visible, orb.Plms.Collectibles[0].Phase,
            "Chozo burst exposes the item");
        AssertTrue(orb.Plms.TryNotifyCollectibleTouch(orb.BlockIndex),
            "exposed Chozo item accepts touch");
        orb.Plms.Step(bus, orb.Level, orb.Streamer, 0, 0, 0);
        AssertTrue(orb.System.HasCollectedItemBit(91),
            "Chozo item persists its room argument bit");
        AssertTrue((orb.Samus.CollectedItems & (ushort)SamusEquipmentFlags.MorphBall) != 0,
            "Chozo Morph Ball grants equipment");

        CollectibleFixture reopenedOrb = LoadCollectible(
            bus,
            header: unchecked((ushort)(0xef2b + (int)InWorldCollectibleKind.MorphBall * 4)),
            roomArgument: 91,
            precollected: false,
            preopenedChozo: true);
        AssertEqual(CollectiblePhase.Visible, reopenedOrb.Plms.Collectibles[0].Phase,
            "previously broken Chozo orb reloads as exposed item");
        AssertTrue(!reopenedOrb.System.HasCollectedItemBit(91),
            "reloaded broken orb remains independently uncollected");

        // Concealed shot items reveal through the same generic projectile callback, remain
        // visible for their native timed window, and after pickup restore as an empty
        // shootable block rather than deleting the location owner.
        CollectibleFixture shot = LoadCollectible(
            bus,
            header: unchecked((ushort)(0xef7f + (int)InWorldCollectibleKind.MissileTank * 4)),
            roomArgument: 123,
            precollected: false);
        AssertEqual(0xc123, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).LevelWord,
            "shot-item setup installs type-C concealed block");
        AssertTrue(shot.Plms.TryNotifyCollectibleProjectileHit(shot.BlockIndex, 0x0100),
            "concealed shot item accepts projectile reaction");
        StepUntil(
            () => shot.Plms.Collectibles[0].Phase == CollectiblePhase.ShotBlockVisible,
            _ => shot.Plms.Step(bus, shot.Level, shot.Streamer, 0, 0, 0),
            maximumFrames: 20,
            context: "shot item reveal");
        AssertTrue(shot.Plms.TryNotifyCollectibleTouch(shot.BlockIndex),
            "revealed shot item accepts Samus contact");
        shot.Plms.Step(bus, shot.Level, shot.Streamer, 0, 0, 0);
        AssertEqual(5, shot.Samus.MaxMissiles, "shot item grants missile capacity");
        AssertEqual(CollectiblePhase.CollectedShotBlockEmpty,
            shot.Plms.Collectibles[0].Phase,
            "collected shot item retains empty respawn owner");
        StepUntil(
            () => shot.Plms.Collectibles[0].Phase == CollectiblePhase.CollectedShotBlock,
            _ => shot.Plms.Step(bus, shot.Level, shot.Streamer, 0, 0, 0),
            maximumFrames: 220,
            context: "collected shot block restoration");
        AssertEqual(0xc123, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).LevelWord,
            "collected shot block restores concealed collision word");

        VerifyCollectedItemSaveRoundTrip();
        VerifyPermanentItemMessageBox();
        VerifySuitPickupTransformation();
        Console.WriteLine(
            "  Permanent collectibles: 63 ROM headers, all 21 effects, three presentations, SRAM bits, bank-$85 messages, and suit transformations agree.");
    }

    private static CollectibleFixture LoadCollectible(
        TestAddressSpace bus,
        ushort header,
        ushort roomArgument,
        bool precollected,
        bool preopenedChozo = false)
    {
        const ushort population = 0x9000;
        const int width = 8;
        const int blockX = 3;
        const int blockY = 3;
        int blockIndex = blockY * width + blockX;
        bus.WriteBytes(0x8f0000 | population, [
            unchecked((byte)header), unchecked((byte)(header >> 8)),
            blockX, blockY,
            unchecked((byte)roomArgument), unchecked((byte)(roomArgument >> 8)),
            0x00, 0x00,
        ]);

        var foreground = new ushort[width * width];
        foreground[blockIndex] = 0x0123;
        var definitions = new byte[0x400 * 8];
        RoomLevelData level = CreateRoom(
            width,
            width,
            foreground,
            new byte[foreground.Length],
            blockDefinitions: definitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        if (precollected)
            system.SetCollectedItemBit(roomArgument);
        if (preopenedChozo)
            system.SetRoomChozoBit(roomArgument);
        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
        };
        var plms = new RoomPlmSystem();
        int loaded = plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            population,
            system,
            areaIndex: AreaId.Crateria,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false);
        AssertEqual(1, loaded, "one-item room population load count");
        return new CollectibleFixture(plms, level, streamer, system, samus, blockIndex);
    }

    private static void SeedCollectibleRom(TestAddressSpace bus)
    {
        const ushort sharedDynamicList = 0xf100;
        // Bomb-special BTS 8 reuses the freed item ID with the retail one-word delete
        // list. Seed that generic instruction because this synthetic item ROM otherwise
        // contains only collectible draw lists.
        bus.WriteBytes(0x84aae3, [0xbc, 0x86]);
        for (int presentation = 0; presentation < 3; presentation++)
        {
            ushort firstHeader = presentation switch
            {
                0 => (ushort)0xeed7,
                1 => (ushort)0xef2b,
                _ => (ushort)0xef7f,
            };
            for (int kind = 0; kind < 21; kind++)
            {
                ushort header = unchecked((ushort)(firstHeader + kind * 4));
                ushort instruction = kind >= (int)InWorldCollectibleKind.Bombs
                    ? sharedDynamicList
                    : (ushort)0xf000;
                bus.WriteBytes(0x840000 | unchecked((ushort)(header + 2)), [
                    unchecked((byte)instruction), unchecked((byte)(instruction >> 8)),
                ]);
            }
        }

        // Item-GFX instruction, bank-$89 source pointer, then four palette words (eight
        // bytes). Zero palette words are intentional fixture data, not omitted operands.
        bus.WriteBytes(0x840000 | sharedDynamicList, [
            0x64, 0x87, 0x00, 0x80,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ]);

        // Both dynamic animation tables select one-block draw records for slot zero.
        bus.WriteBytes(0x84e05f, [0x00, 0xe5]);
        bus.WriteBytes(0x84e077, [0x06, 0xe5]);
        WriteOneBlockDraw(bus, 0xe500, 0xb08e);
        WriteOneBlockDraw(bus, 0xe506, 0xb08f);

        WriteOneBlockDraw(bus, 0xa2b5, 0x00ff);
        WriteOneBlockDraw(bus, 0xa2c7, 0xc090);
        WriteOneBlockDraw(bus, 0xa2cd, 0xc091);
        WriteOneBlockDraw(bus, 0xa2d3, 0xc092);
        WriteOneBlockDraw(bus, 0xa2d9, 0x0093);
        for (int kind = 0; kind < 4; kind++)
        {
            WriteOneBlockDraw(bus, 0xa2df + kind * 12, unchecked((ushort)(0xb080 + kind * 2)));
            WriteOneBlockDraw(bus, 0xa2e5 + kind * 12, unchecked((ushort)(0xb081 + kind * 2)));
        }
        for (int frame = 0; frame < 3; frame++)
        {
            WriteOneBlockDraw(bus, 0xa3dd + frame * 6, unchecked((ushort)(0xc0a0 + frame)));
            WriteOneBlockDraw(bus, 0xa345 + frame * 6, unchecked((ushort)(0xc0b0 + frame)));
        }
    }

    private static void WriteOneBlockDraw(TestAddressSpace bus, int pointer, ushort levelWord) =>
        bus.WriteBytes(0x840000 | pointer, [
            0x01, 0x00,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x00,
        ]);

    private static void AssertPermanentCollectibleEffect(
        InWorldCollectibleKind kind,
        SamusState samus)
    {
        switch (kind)
        {
            case InWorldCollectibleKind.EnergyTank:
                AssertEqual(199, samus.MaxHealth, "energy tank capacity");
                AssertEqual(199, samus.Health, "energy tank refill");
                return;
            case InWorldCollectibleKind.MissileTank:
                AssertEqual(5, samus.MaxMissiles, "missile tank capacity");
                AssertEqual(5, samus.Missiles, "missile tank refill");
                return;
            case InWorldCollectibleKind.SuperMissileTank:
                AssertEqual(5, samus.MaxSuperMissiles, "super tank capacity");
                AssertEqual(5, samus.SuperMissiles, "super tank refill");
                return;
            case InWorldCollectibleKind.PowerBombTank:
                AssertEqual(5, samus.MaxPowerBombs, "power-bomb tank capacity");
                AssertEqual(5, samus.PowerBombs, "power-bomb tank refill");
                return;
            case InWorldCollectibleKind.ReserveTank:
                AssertEqual(100, samus.MaxReserveEnergy, "reserve tank capacity");
                AssertEqual(1, samus.ReserveTankMode, "first reserve tank enables auto mode");
                return;
        }

        ushort equipment = kind switch
        {
            InWorldCollectibleKind.Bombs => (ushort)SamusEquipmentFlags.Bombs,
            InWorldCollectibleKind.HiJumpBoots => (ushort)SamusEquipmentFlags.HiJumpBoots,
            InWorldCollectibleKind.SpeedBooster => (ushort)SamusEquipmentFlags.SpeedBooster,
            InWorldCollectibleKind.SpringBall => (ushort)SamusEquipmentFlags.SpringBall,
            InWorldCollectibleKind.VariaSuit => (ushort)SamusEquipmentFlags.VariaSuit,
            InWorldCollectibleKind.GravitySuit => (ushort)SamusEquipmentFlags.GravitySuit,
            InWorldCollectibleKind.XrayScope => (ushort)SamusEquipmentFlags.XrayScope,
            InWorldCollectibleKind.GrappleBeam => (ushort)SamusEquipmentFlags.GrappleBeam,
            InWorldCollectibleKind.SpaceJump => (ushort)SamusEquipmentFlags.SpaceJump,
            InWorldCollectibleKind.ScrewAttack => (ushort)SamusEquipmentFlags.ScrewAttack,
            InWorldCollectibleKind.MorphBall => (ushort)SamusEquipmentFlags.MorphBall,
            _ => 0,
        };
        if (equipment != 0)
        {
            AssertTrue((samus.CollectedItems & equipment) != 0,
                $"{kind} enters collected equipment word");
            AssertTrue((samus.EquippedItems & equipment) != 0,
                $"{kind} enters equipped equipment word");
            return;
        }

        ushort beam = kind switch
        {
            InWorldCollectibleKind.ChargeBeam => (ushort)SamusBeamFlags.Charge,
            InWorldCollectibleKind.IceBeam => (ushort)SamusBeamFlags.Ice,
            InWorldCollectibleKind.WaveBeam => (ushort)SamusBeamFlags.Wave,
            InWorldCollectibleKind.SpazerBeam => (ushort)SamusBeamFlags.Spazer,
            InWorldCollectibleKind.PlasmaBeam => (ushort)SamusBeamFlags.Plasma,
            _ => throw new InvalidOperationException($"Missing effect assertion for {kind}."),
        };
        AssertTrue((samus.CollectedBeams & beam) != 0,
            $"{kind} enters collected beam word");
        AssertTrue((samus.EquippedBeams & beam) != 0,
            $"{kind} enters equipped beam word");
    }

    private static void VerifyCollectedItemSaveRoundTrip()
    {
        var bus = new TestAddressSpace();
        var sourceSystem = new Bank80SystemState();
        sourceSystem.SetCollectedItemBit(0);
        sourceSystem.SetCollectedItemBit(91);
        sourceSystem.SetCollectedItemBit(511);
        sourceSystem.SetRoomChozoBit(0);
        sourceSystem.SetRoomChozoBit(91);
        sourceSystem.SetRoomChozoBit(511);
        sourceSystem.SetOpenedDoorBit(0);
        sourceSystem.SetOpenedDoorBit(91);
        sourceSystem.SetOpenedDoorBit(511);
        sourceSystem.SetEvent(EventNumber.TourianUnlocked);
        sourceSystem.SetBossBits(2, BossBits.AreaBoss | BossBits.AreaTorizo);
        var sourceSamus = new SamusState
        {
            CollectedBeams = (ushort)(SamusBeamFlags.Charge | SamusBeamFlags.Ice),
            EquippedBeams = (ushort)SamusBeamFlags.Ice,
        };
        var saveRam = new SuperMetroidSaveRam(bus);
        saveRam.SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(sourceSamus, sourceSystem, 2, 7));
        SuperMetroidSaveSlot slot = saveRam.ReadSlot(0)
            ?? throw new InvalidOperationException("collectible save slot checksum failed");

        var restoredSystem = new Bank80SystemState();
        var restoredSamus = new SamusState();
        slot.ApplyTo(restoredSamus, restoredSystem);
        AssertTrue(restoredSystem.HasCollectedItemBit(0), "first physical item bit round-trips");
        AssertTrue(restoredSystem.HasCollectedItemBit(91), "middle physical item bit round-trips");
        AssertTrue(restoredSystem.HasCollectedItemBit(511), "last physical item bit round-trips");
        AssertTrue(restoredSystem.HasRoomChozoBit(0), "first room Chozo bit round-trips");
        AssertTrue(restoredSystem.HasRoomChozoBit(91), "middle room Chozo bit round-trips");
        AssertTrue(restoredSystem.HasRoomChozoBit(511), "last room Chozo bit round-trips");
        AssertTrue(restoredSystem.HasOpenedDoorBit(0), "first opened-door bit round-trips");
        AssertTrue(restoredSystem.HasOpenedDoorBit(91), "middle opened-door bit round-trips");
        AssertTrue(restoredSystem.HasOpenedDoorBit(511), "last opened-door bit round-trips");
        AssertTrue(restoredSystem.HasEvent(EventNumber.TourianUnlocked),
            "event bytes round-trip through SRAM");
        AssertTrue(restoredSystem.HasAnyBossBits(2, BossBits.AreaBoss),
            "area boss byte round-trips through SRAM");
        AssertTrue(restoredSystem.HasAnyBossBits(2, BossBits.AreaTorizo),
            "area Torizo bit round-trips through SRAM");
        AssertEqual(sourceSamus.CollectedBeams, restoredSamus.CollectedBeams,
            "collected beam word round-trips independently from equipped beams");
        AssertEqual(sourceSamus.EquippedBeams, restoredSamus.EquippedBeams,
            "equipped beam word round-trips independently from collected beams");
    }

    private static void VerifyPermanentItemMessageBox()
    {
        var bus = new TestAddressSpace();
        SeedPermanentItemMessageBoxRom(bus);
        var message = new GameplayMessageBoxState();

        message.Begin(bus, GameplayMessageIds.EnergyTank);
        AssertEqual(GameplayMessageBoxPhase.Opening, message.Phase,
            "item message enters shared opening coroutine");
        AssertEqual(3, message.TilemapRowCount, "small item message has border/content/border");
        AssertEqual((ushort)0x3801, message.Tilemap[0], "small item message reads ROM border");
        AssertEqual((ushort)0x3801, message.Tilemap[32], "small item message reads ROM content");

        for (int openingFrame = 0; openingFrame < 13; openingFrame++)
        {
            message.Step(0);
            AssertEqual(openingFrame * 2, message.RadiusPixels,
                $"item message opening radius frame {openingFrame}");
        }
        AssertEqual(GameplayMessageBoxPhase.MinimumDisplay, message.Phase,
            "item message reaches mandatory display interval");
        AssertEqual(360, message.MinimumDisplayFramesRemaining,
            "ordinary item message uses native 360-frame wait");

        // Held input cannot shorten the fanfare. The frame that decrements 1 -> 0 only
        // enters the input-wait coroutine; its next accepted NMI performs the read.
        for (int waitFrame = 0; waitFrame < 359; waitFrame++)
            message.Step((ushort)SnesButton.X);
        AssertEqual(1, message.MinimumDisplayFramesRemaining,
            "item message retains final mandatory wait frame");
        message.Step((ushort)SnesButton.X);
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "item message waits one NMI boundary before reading held input");
        message.Step((ushort)SnesButton.X);
        AssertEqual(GameplayMessageBoxPhase.Closing, message.Phase,
            "retail held-input fix dismisses item message");

        for (int closingFrame = 0; closingFrame < 13; closingFrame++)
        {
            message.Step(0);
            if (closingFrame < 12)
            {
                AssertEqual(24 - closingFrame * 2, message.RadiusPixels,
                    $"item message closing radius frame {closingFrame}");
                AssertTrue(message.IsActive,
                    $"item message remains active through visible close frame {closingFrame}");
            }
        }
        AssertTrue(!message.IsActive, "item message exits after thirteen close NMIs");

        // Message two is the first large box and patches its shoot-button placeholder.
        // Supplying B proves the glyph is selected from the binding word, not hard-coded X.
        message.Begin(bus, GameplayMessageIds.MissileTank, shootBinding: (ushort)SnesButton.B);
        AssertEqual(6, message.TilemapRowCount, "large item message has four content rows");
        AssertEqual((ushort)0x3ce1, message.Tilemap[0x12a / 2],
            "large item message patches configured shoot-button glyph");

        // Exercise the resulting centered HDMA clip and temporary message palette. A
        // palette-six/color-one character must use $0BB1 rather than gameplay CGRAM 25.
        var vram = new SnesVram();
        var character = new byte[16];
        for (int row = 0; row < 8; row++)
            character[row * 2] = 0xff;
        vram.LoadBytes((0x4000 + 8) * 2, character);
        var cgram = new SnesCgram();
        cgram.SetColor(25, 0x7fff);
        var frame = new SuperMetroid.Core.Assets.Rgba32[256 * 224];
        message.Step(0); // radius 0
        message.Step(0); // radius 2: visible scanlines 122-125
        GameplayMessageBoxRenderer.Composite(frame, message, vram, cgram);
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), frame[121 * 256],
            "message HDMA clip leaves scanline above radius untouched");
        AssertEqual(
            SuperMetroid.Core.Assets.SnesGraphics.DecodeBgr555Color(0x0bb1),
            frame[122 * 256],
            "message renderer applies temporary CGRAM color 25 inside clip");

        // Retail message $14 is the map-station message. Its definition chooses the
        // small-border drawing routine but delimits three complete content rows. This
        // verifies the native variable-length copy instead of inferring height from the
        // border routine's name, and exercises that shape through the compositor too.
        message = new GameplayMessageBoxState();
        message.Begin(bus, GameplayMessageIds.MapDataAccessCompleted);
        AssertEqual(5, message.TilemapRowCount,
            "map-station message accepts three rows inside the small border");
        AssertEqual((ushort)0x3820, message.Tilemap[32],
            "map-station message copies the first variable-height content row");
        AssertEqual((ushort)0x3822, message.Tilemap[96],
            "map-station message copies the third variable-height content row");
        message.Step(0);
        message.Step(0);
        Array.Clear(frame);
        GameplayMessageBoxRenderer.Composite(frame, message, vram, cgram);

        // Completion notices $14/$15/$16/$18 use the short `$000A` wait at
        // `$85:847A`; item descriptions use `$0168`. Prove the map-station path reaches
        // held-input dismissal and completely restores the suspended gameplay owner in
        // tens of frames rather than appearing softlocked for six seconds.
        for (int openingFrame = 2; openingFrame < 13; openingFrame++)
            message.Step(0);
        AssertEqual(GameplayMessageBoxPhase.MinimumDisplay, message.Phase,
            "map-station message reaches its mandatory display interval");
        AssertEqual(10, message.MinimumDisplayFramesRemaining,
            "map-station message uses native ten-frame completion wait");
        for (int waitFrame = 0; waitFrame < 10; waitFrame++)
            message.Step((ushort)SnesButton.X);
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "map-station message waits one NMI boundary before held input");
        message.Step((ushort)SnesButton.X);
        AssertEqual(GameplayMessageBoxPhase.Closing, message.Phase,
            "map-station message accepts held input after ten frames");
        for (int closingFrame = 0; closingFrame < 13; closingFrame++)
            message.Step(0);
        AssertTrue(!message.IsActive,
            "map-station message returns to its suspended station PLM");

        // Parse every message accepted by the translated bank-$85 owner from the retail
        // cartridge. This catches table-boundary and uncommon-layout regressions without
        // needing controller playback through each collectible or station room.
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (File.Exists(romPath))
        {
            var retailBus = new SuperMetroidAddressSpace(File.ReadAllBytes(romPath));
            for (byte messageId = 1; messageId <= 26; messageId++)
            {
                var retailMessage = new GameplayMessageBoxState();
                retailMessage.Begin(
                    retailBus,
                    GameplayMessageIds.FromCartridge(messageId, "retail definition-table audit"));
                AssertTrue(
                    retailMessage.TilemapRowCount is >=
                        GameplayMessageRomData.Layout.MinimumRows and <=
                        GameplayMessageRomData.Layout.MaximumRows,
                    $"retail gameplay message {messageId} has a supported layout");
            }
        }

        AssertEqual((byte)0x1a, (byte)GameplayMessageIds.GravitySuit,
            "Gravity Suit uses retail message $1A rather than map-station message $14");
        NotSupportedException unsupported = AssertThrows<NotSupportedException>(
            () => GameplayMessageIds.FromCartridge(0x1b, "constructed PLM"),
            "message terminator is not exposed as a gameplay message");
        AssertTrue(unsupported.Message.Contains("$1B", StringComparison.Ordinal) &&
            unsupported.Message.Contains("constructed PLM", StringComparison.Ordinal),
            "unsupported message identifies numeric ID and source context");
    }

    private static void VerifySuitPickupTransformation()
    {
        var bus = new TestAddressSpace();
        for (int i = 0; i < 128; i++)
            bus.WriteBytes(0x88e3c9 + i, [unchecked((byte)Math.Min(i / 8 + 1, 15))]);

        // Distinct first words prove that the generic suit loader selects from equipment,
        // and that stage three—not Begin—performs the newly acquired palette reveal.
        WriteWord(bus, 0x9b9400, 0x001f);
        WriteWord(bus, 0x9b9520, 0x03e0);
        WriteWord(bus, 0x9b9800, 0x7c00);
        var cgram = new SnesCgram();
        var samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 500,
            YPosition = 600,
            ProjectileFlareCounter = 60,
            EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit,
            CollectedItems = (ushort)SamusEquipmentFlags.VariaSuit,
        };
        SamusState.LoadPowerSuitPalette(bus, cgram);

        var pickup = new SamusSuitPickupState();
        pickup.Begin(bus, samus, layer1X: 0x0100, layer1Y: 0x0200,
            SamusSuitPickupKind.Varia);
        AssertTrue(pickup.IsActive, "Varia transformation starts after message return");
        AssertTrue(samus.InputLocked, "suit transformation locks Samus input");
        AssertEqual(SamusPoseIds.ForwardFacingPowerSuitPose, samus.Pose,
            "first suit begins in front-facing power-suit pose");
        AssertEqual((ushort)(0x0100 + 120), samus.XPosition,
            "suit transformation centers Samus horizontally");
        AssertEqual((ushort)(0x0200 + 136), samus.YPosition,
            "suit transformation centers Samus vertically");
        AssertEqual((ushort)0x001f, cgram.Colors[192],
            "suit palette is not revealed at transformation setup");

        // Begin's inverted `$00FF` table is empty. The first stage call replaces exactly
        // eight scanlines at each edge with the one-pixel `$7878` beam.
        var pixels = new SuperMetroid.Core.Assets.Rgba32[256 * 224];
        SamusSuitPickupRenderer.Composite(pixels, pickup);
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), pixels[0],
            "initial inverted suit window applies no fixed color");
        pickup.Step(bus, samus, cgram);
        AssertEqual((ushort)0x7878, pickup.WindowTable[0],
            "stage zero narrows first top scanline");
        AssertEqual((ushort)0x7878, pickup.WindowTable[255],
            "stage zero narrows first bottom scanline");
        AssertEqual((ushort)0x00ff, pickup.WindowTable[8],
            "stage zero changes exactly eight top scanlines per frame");
        Array.Clear(pixels);
        SamusSuitPickupRenderer.Composite(pixels, pickup);
        AssertTrue(pixels[120].R != 0 && pixels[120].G != 0,
            "stage-zero one-pixel beam receives fixed color");
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), pixels[119],
            "pixel beside stage-zero beam remains outside color window");

        int transformationFrames = 1;
        bool observedReveal = false;
        while (pickup.IsActive && transformationFrames < 2000)
        {
            pickup.Step(bus, samus, cgram);
            transformationFrames++;
            if (!observedReveal && pickup.Substate == 4)
            {
                observedReveal = true;
                AssertEqual(SamusPoseIds.ForwardFacingSuitedPose, samus.Pose,
                    "stage three installs suited front-facing pose");
                AssertEqual((ushort)0x03e0, cgram.Colors[192],
                    "stage three loads Varia palette from ROM");
            }
        }
        AssertTrue(observedReveal, "Varia transformation reaches stage-three reveal");
        AssertTrue(!pickup.IsActive, "Varia transformation reaches native cleanup");
        AssertTrue(!samus.InputLocked, "suit transformation cleanup unlocks input");
        AssertTrue(transformationFrames < 2000,
            "suit transformation terminates without a host timeout");

        // Gravity has priority when both suit bits are equipped, matching `$91:DEBA`.
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.GravitySuit;
        samus.LoadSuitPalette(bus, cgram);
        AssertEqual((ushort)0x7c00, cgram.Colors[192],
            "normal suit palette loader gives Gravity priority over Varia");
    }

    private static void SeedPermanentItemMessageBoxRom(TestAddressSpace bus)
    {
        const int definitions = 0x85869b;
        // ID 1: small Energy Tank message, one 64-byte content row.
        WriteWord(bus, definitions + 0, 0x8436);
        WriteWord(bus, definitions + 2, 0x8289);
        WriteWord(bus, definitions + 4, 0x9000);
        // ID 2: large Missile message, four 64-byte content rows.
        WriteWord(bus, definitions + 6, 0x83c5);
        WriteWord(bus, definitions + 8, 0x825a);
        WriteWord(bus, definitions + 10, 0x9040);
        // Only ID 3's content pointer is needed to delimit ID 2.
        WriteWord(bus, definitions + 16, 0x9140);

        // ID $14: retail map-station layout -- small border, three content rows.
        int message20 = definitions + 19 * 6;
        WriteWord(bus, message20 + 0, 0x8436);
        WriteWord(bus, message20 + 2, 0x8289);
        WriteWord(bus, message20 + 4, 0x9200);
        // ID $15's content pointer delimits message $14 at three rows ($C0 bytes).
        WriteWord(bus, message20 + 10, 0x92c0);

        for (int word = 0; word < 32; word++)
        {
            WriteWord(bus, 0x858000 + word * 2, 0x3801);
            WriteWord(bus, 0x858040 + word * 2, 0x3801);
            WriteWord(bus, 0x859000 + word * 2, 0x3801);
        }
        for (int word = 0; word < 128; word++)
            WriteWord(bus, 0x859040 + word * 2, 0x3801);
        for (int row = 0; row < 3; row++)
        {
            for (int word = 0; word < 32; word++)
                WriteWord(bus, 0x859200 + (row * 32 + word) * 2, (ushort)(0x3820 + row));
        }
    }

    private readonly record struct CollectibleFixture(
        RoomPlmSystem Plms,
        RoomLevelData Level,
        BackgroundTilemapStreamer Streamer,
        Bank80SystemState System,
        SamusState Samus,
        int BlockIndex);
}
