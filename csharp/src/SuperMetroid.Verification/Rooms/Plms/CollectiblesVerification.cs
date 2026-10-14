using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Samus.Grapple.GrappleDoorsVerification;
using static SuperMetroid.Verification.Hud.PermanentMessageFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Samus.Projectiles.SuitPickupBeamCurveDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Drawing;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Constructed cartridge checks for permanent item PLMs, messages, saves, and suit acquisition.</summary>
internal static class CollectiblesVerification
{
    /// <summary>
    /// Exercises the common bank-$84 item implementation across all twenty-one identities
    /// and all three retail presentations. The fixture deliberately supplies ROM-shaped
    /// headers, instruction pointers, draw lists, graphics, and room-population records;
    /// no test-only spawn route bypasses cartridge decoding.
    /// </summary>
    internal static void VerifyPermanentCollectibles()
    {
        Suite(nameof(VerifyMorphBallPickupCollision), () => VerifyMorphBallPickupCollision());
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        // Full-table audit: unused native entries still allocate a deleting PLM,
        // not a breakable-block range failure or a fabricated terrain mutation.
        for (int bts = 0x11; bts <= 0x4e; bts++)
        {
            if (bts is > 0x3f and not 0x4e) continue;
            var emptyLevel = CreateRoom(new(16, 16), new ushort[256], new byte[256], blockDefinitions: new byte[0x400 * 8]);
            var unusedPlms = new RoomPlmSystem();
            AssertTrue(unusedPlms.TrySpawnProjectileShotBlock(emptyLevel, 0, (byte)bts, 0, true),
                "unused shootable entry allocates native nothing PLM");
            AssertEqual(1, unusedPlms.SlotTable.ActiveCount, "nothing PLM retains its provisional slot");
            AssertEqual((ushort)0, emptyLevel.ForegroundEntries.Span[0], "nothing PLM leaves terrain unchanged");
        }

        for (int kindIndex = 0; kindIndex < 21; kindIndex++)
        {
            InWorldCollectibleKind kind = (InWorldCollectibleKind)kindIndex;
            CollectibleFixture fixture = LoadCollectible(bus, header: unchecked((ushort)(0xeed7 + kindIndex * 4)), roomArgument: unchecked((ushort)kindIndex), precollected: false);

            AssertEqual(1, fixture.Plms.CollectiblePlms.Collectibles.Count,
                $"{kind} exposed header loads through room population");
            CollectiblePlmSnapshot loaded = fixture.Plms.CollectiblePlms.Collectibles[0];
            AssertEqual(kind, loaded.Kind, $"{kind} header identity");
            AssertEqual(CollectiblePresentation.Exposed, fixture.Plms.CollectiblePresentations[0],
                $"{kind} exposed presentation");
            if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                fixture.Samus.ProjectileFlareCounter = 60;

            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
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
            ushort visibleItemWord = fixture.Level.ForegroundEntries.Span[fixture.BlockIndex];
            AssertTrue(fixture.Plms.CollectiblePlms.TryNotifyCollectibleTouch(fixture.BlockIndex),
                $"{kind} visible block accepts Samus contact");
            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });

            AssertTrue(fixture.System.HasCollectedItemBit(kindIndex),
                $"{kind} marks its physical item bit");
            AssertEqual(1, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count,
                $"{kind} publishes exactly one acquisition");
            AssertEqual(kind, fixture.Plms.CollectiblePlms.CollectiblePickupEvents[0].Kind,
                $"{kind} acquisition identity");
            AssertTrue(
                fixture.Plms.CollectiblePlms.ConsumeCollectibleFanfareRequest(),
                $"{kind} publishes one shared permanent-item fanfare edge");
            AssertTrue(
                !fixture.Plms.CollectiblePlms.ConsumeCollectibleFanfareRequest(),
                $"{kind} fanfare edge cannot repeat during its synchronous message");
            AssertPermanentCollectibleEffect(kind, fixture.Samus);

            AssertEqual(visibleItemWord, fixture.Level.ForegroundEntries.Span[fixture.BlockIndex],
                $"{kind} retains pickup artwork during its message");
            fixture.Plms.CollectiblePlms.CompleteCollectibleMessage(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });

            // Returning from the message frees the native physical ID. Reuse that same highest slot for
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
            AssertEqual(0, fixture.Plms.CollectiblePlms.Collectibles.Count,
                $"{kind} semantic owner does not survive physical slot reuse");
            fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count,
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
        CollectibleFixture collected = LoadCollectible(bus, header: 0xeed7, roomArgument: 37, precollected: true);
        collected.Plms.Step(bus, collected.Level, collected.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, collected.Plms.SlotTable.ActiveCount,
            "previously collected exposed item removes itself");
        AssertEqual(99, collected.Samus.MaxHealth,
            "previously collected exposed item does not grant twice");

        // A Chozo orb accepts projectile contact, performs its authored burst, then becomes
        // the same visible/touchable item. Acquisition must retain the triggering physical
        // location rather than confusing the projectile type with an item bit.
        CollectibleFixture orb = LoadCollectible(bus, header: unchecked((ushort)(0xef2b + (int)InWorldCollectibleKind.MorphBall * 4)), roomArgument: 91, precollected: false);
        orb.Plms.Step(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        var grappleSamus = new SamusState();
        grappleSamus.Position = grappleSamus.Position.WithWhole(new((ushort)((orb.BlockIndex % orb.Level.Size.X) * 16 + 8), (ushort)((orb.BlockIndex / orb.Level.Size.X) * 16 + 8)));
        SeedStationaryGrappleCollisionProbe(bus, grappleSamus);
        var orbHit = SamusGrappleMovement.StepFiring(bus, orb.Level, grappleSamus,
            (ushort)SnesButton.X, orb.Plms);
        AssertEqual(GrapplePhase.CancelPending, grappleSamus.Grapple.Phase,
            "grapple collides with the solid Chozo orb");
        orb.Plms.Step(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(!orb.System.HasRoomChozoBit(91),
            "breaking a Chozo orb persists nothing; only unused PLMs $D700/$D708 use $84:8865");
        AssertTrue(!orb.System.HasCollectedItemBit(91),
            "breaking a Chozo orb does not prematurely collect its item");
        StepFrames(9, _ => orb.Plms.Step(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }));
        AssertEqual(CollectiblePhase.Visible, orb.Plms.CollectiblePhases[0],
            "Chozo burst exposes the item");
        ushort orbItemWord = orb.Level.ForegroundEntries.Span[orb.BlockIndex];
        AssertTrue(orb.Plms.CollectiblePlms.TryNotifyCollectibleTouch(orb.BlockIndex),
            "exposed Chozo item accepts touch");
        orb.Plms.Step(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertTrue(orb.System.HasCollectedItemBit(91),
            "Chozo item persists its room argument bit");
        AssertTrue((orb.Samus.CollectedItems & (ushort)SamusEquipmentFlags.MorphBall) != 0,
            "Chozo Morph Ball grants equipment");

        AssertEqual(orbItemWord, orb.Level.ForegroundEntries.Span[orb.BlockIndex],
            "Chozo pickup remains visible throughout message");
        orb.Plms.CollectiblePlms.CompleteCollectibleMessage(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        orb.Plms.Step(bus, orb.Level, orb.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(0, orb.Plms.CollectiblePlms.Collectibles.Count, "Chozo pickup deletes after message");

        CollectibleFixture reopenedOrb = LoadCollectible(bus, header: unchecked((ushort)(0xef2b + (int)InWorldCollectibleKind.MorphBall * 4)), roomArgument: 91, precollected: false, preopenedChozo: true);
        AssertEqual(CollectiblePhase.ChozoOrb, reopenedOrb.Plms.CollectiblePhases[0],
            "an uncollected Chozo item rebuilds its orb on reload, whatever the chozo bit holds");
        AssertTrue(!reopenedOrb.System.HasCollectedItemBit(91),
            "reloaded broken orb remains independently uncollected");

        // Concealed shot items reveal through the same generic projectile callback, remain
        // visible for their native timed window, and after pickup restore as an empty
        // shootable block rather than deleting the location owner.
        CollectibleFixture shot = LoadCollectible(bus, header: unchecked((ushort)(0xef7f + (int)InWorldCollectibleKind.MissileTank * 4)), roomArgument: 123, precollected: false);
        AssertEqual(0xc123, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).LevelWord,
            "shot-item setup installs type-C concealed block");
        AssertTrue(shot.Plms.CollectiblePlms.TryNotifyCollectibleProjectileHit(shot.BlockIndex, 0x0100),
            "concealed shot item accepts projectile reaction");
        StepUntil(
            () => shot.Plms.CollectiblePhases[0] == CollectiblePhase.ShotBlockVisible,
            _ => shot.Plms.Step(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }),
            maximumFrames: 20,
            context: "shot item reveal");
        ushort shotItemWord = shot.Level.ForegroundEntries.Span[shot.BlockIndex];
        AssertTrue(shot.Plms.CollectiblePlms.TryNotifyCollectibleTouch(shot.BlockIndex),
            "revealed shot item accepts Samus contact");
        shot.Plms.Step(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(5, shot.Samus.MaxMissiles, "shot item grants missile capacity");
        StepFrames(180, _ => shot.Plms.Step(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }));
        AssertEqual(shotItemWord, shot.Level.ForegroundEntries.Span[shot.BlockIndex],
            "shot pickup cannot clear or begin respawn countdown during message");
        shot.Plms.CollectiblePlms.CompleteCollectibleMessage(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        shot.Plms.Step(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual(CollectiblePhase.CollectedShotBlockEmpty,
            shot.Plms.CollectiblePhases[0],
            "collected shot item retains empty respawn owner");
        StepUntil(
            () => shot.Plms.CollectiblePhases[0] == CollectiblePhase.CollectedShotBlock,
            _ => shot.Plms.Step(bus, shot.Level, shot.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }),
            maximumFrames: 220,
            context: "collected shot block restoration");
        AssertEqual(0xc123, shot.Level.GetCollisionBlockByIndex(shot.BlockIndex).LevelWord,
            "collected shot block restores concealed collision word");

        Suite(nameof(VerifyCollectedItemSaveRoundTrip), () => VerifyCollectedItemSaveRoundTrip());
        Suite(nameof(VerifyPermanentItemMessageBox), () => VerifyPermanentItemMessageBox());
        Suite(nameof(VerifySuitPickupTransformation), () => VerifySuitPickupTransformation());
        Console.WriteLine(
            "  Permanent collectibles: 63 ROM headers, all 21 effects, three presentations, SRAM bits, bank-$85 messages, and suit transformations agree.");
    }

    /// <summary>Checks Morph Ball contact, synchronous message return, the empty draw, and passable terrain after deletion.</summary>
    internal static void VerifyMorphBallPickupCollision()
    {
        var bus = new TestAddressSpace();
        SeedCollectibleRom(bus);
        // Retail room $9E9F/$9EB1 places Morph Ball at ($45,$29): block 5317.
        var fixture = LoadCollectible(bus, (ushort)(PlmHeaderId.ExposedEnergyTank + (int)InWorldCollectibleKind.MorphBall * 4), 26, precollected: false, layout: new CollectibleRoomLayout { RoomSize = new(128, 48), Block = new(69, 41) });
        fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(1024, 512), Bg1XOffset = 0 });
        var movement = new SamusKinematicsState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(1124, 651)), Radius = new(5, 14),
        };
        SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = fixture.Level, Plms = fixture.Plms }, movement, new HorizontalBlockMove { Displacement = -1 << 16, AlignToSlopeAfterMovement = false });
        fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(1024, 512), Bg1XOffset = 0 });
        AssertTrue(fixture.System.HasCollectedItemBit(26), "Morph Ball pickup persists");
        AssertPermanentCollectibleEffect(InWorldCollectibleKind.MorphBall, fixture.Samus);
        AssertEqual(CollectiblePhase.AwaitingMessage, fixture.Plms.CollectiblePhases[0],
            "Morph Ball retains its owner during the synchronous message");
        AssertEqual(1, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count, "Morph Ball publishes one pickup");
        // The message returns into the item's list, which draws empty for one frame
        // within that same PLM_Handler call and deletes the PLM on the next pass.
        fixture.Plms.CollectiblePlms.CompleteCollectibleMessage(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(1024, 512), Bg1XOffset = 0 });
        AssertEqual((ushort)0x00ff, fixture.Level.GetCollisionBlockByIndex(fixture.BlockIndex).LevelWord,
            "Morph Ball pickup draws native empty level word on message return");
        var next = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = fixture.Level, Plms = fixture.Plms }, movement, new HorizontalBlockMove { Displacement = -1 << 16, AlignToSlopeAfterMovement = false });
        AssertEqual((ushort)1122, movement.Position.X.Whole, "message-return horizontal movement advances");
        AssertTrue(!next.Collided, "message-return Morph Ball block remains passable");
        AssertEqual(CollectiblePhase.EmptyAwaitingDelete, fixture.Plms.CollectiblePhases[0],
            "repeat contact preserves the pending delete");
        fixture.Plms.Step(bus, fixture.Level, fixture.Streamer, new PlmView { Layer1Position = new(1024, 512), Bg1XOffset = 0 });
        AssertEqual(0, fixture.Plms.SlotTable.ActiveCount, "Morph Ball owner deletes on the next PLM pass"); 
        AssertEqual(0, fixture.Plms.CollectiblePlms.CollectiblePickupEvents.Count, "Morph Ball cannot be awarded twice");
        var cleared = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = fixture.Level, Plms = fixture.Plms }, movement, new HorizontalBlockMove { Displacement = -1 << 16, AlignToSlopeAfterMovement = false });
        AssertTrue(!cleared.Collided, "deleted Morph Ball leaves passable air");
        AssertEqual((ushort)1121, movement.Position.X.Whole, "movement also advances after owner deletion");
        Console.WriteLine("  Morph Ball message return and subsequent horizontal collision agree.");
    }

    /// <summary>Loads one authored-shape item PLM into a synthetic room with selectable prior collection and Chozo state.</summary>
    internal static CollectibleFixture LoadCollectible(TestAddressSpace bus, ushort header, ushort roomArgument, bool precollected, bool preopenedChozo = false, RoomLayer3FxState? roomFx = null, CollectibleRoomLayout layout = default)
    {
        const ushort population = 0x9000;
        Extent16<RoomBlocks> size = layout.RoomSize ?? new(8, 8);
        Position16<RoomBlocks> itemBlock = layout.Block ?? new(3, 3);
        int blockIndex = size.RowMajorIndex(itemBlock);
        SeedRoomPlmPopulation(bus, population, [
            unchecked((byte)header), SnesLittleEndian.HighByte(header),
            unchecked((byte)itemBlock.X), unchecked((byte)itemBlock.Y),
            unchecked((byte)roomArgument), SnesLittleEndian.HighByte(roomArgument),
            0x00, 0x00,
        ]);

        var foreground = new ushort[size.Area];
        foreground[blockIndex] = 0x0123;
        var definitions = new byte[0x400 * 8];
        RoomLevelData level = CreateRoom(size,
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
        int loaded = plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, streamer, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => samus), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false }, RoomFx = roomFx });
        AssertEqual(1, loaded, "one-item room population load count");
        return new CollectibleFixture(plms, level, streamer, system, samus, blockIndex);
    }

    /// <summary>Seeds the three item-header families and their dynamic graphics operands in the fixture cartridge.</summary>
    internal static void SeedCollectibleRom(TestAddressSpace bus)
    {
        const ushort sharedDynamicList = 0xf100;
        // Bomb-special BTS 8 reuses the freed item ID with the retail one-word delete
        // list. Seed that generic instruction because this synthetic item ROM otherwise
        // contains only collectible draw lists.
        SeedPlmDeleteList(bus);
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
                SeedPlmProgramBytes(bus, unchecked((ushort)(header + 2)), [
                    unchecked((byte)instruction), SnesLittleEndian.HighByte(instruction),
                ]);
            }
        }

        // Item-GFX instruction, bank-$89 source pointer, then four palette words (eight
        // bytes). Zero palette words are intentional fixture data, not omitted operands.
        SeedPlmProgramBytes(bus, sharedDynamicList, [
            0x64, 0x87, 0x00, 0x80,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        ]);

        // Permanent-item draw words and dynamic-slot selections are now compiled.
        // Do not seed their old ROM ranges: the ordinary pickup suite should fail if
        // production ever accidentally returns to those presentation reads.
        for (int frame = 0; frame < 3; frame++)
        {
            WriteOneBlockDraw(bus, 0xa345 + frame * 6, unchecked((ushort)(0xc0b0 + frame)));
        }
    }

    /// <summary>Writes a one-block PLM draw instruction with a chosen level word.</summary>
    internal static void WriteOneBlockDraw(TestAddressSpace bus, int pointer, ushort levelWord) =>
        SeedPlmProgramBytes(bus, unchecked((ushort)pointer), [
            0x01, 0x00,
            unchecked((byte)levelWord), SnesLittleEndian.HighByte(levelWord),
            0x00, 0x00,
        ]);

    /// <summary>Checks the resource increase or equipment bit specific to a picked-up item identity.</summary>
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

    /// <summary>Checks SRAM round-trip of edge item bits, Chozo and door bits, events, bosses, and independent beam words.</summary>
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
        var saveRam = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        saveRam.SaveSlot(
            0,
            SuperMetroidSaveSnapshot.Capture(sourceSamus, sourceSystem, new SaveSnapshotContext { Area = 2, SaveStation = 7 }));
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

    /// <summary>Checks item-message tilemap, fanfare wait, input dismissal, and PPU restoration timing.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void VerifyPermanentItemMessageBox()
    {
        var bus = new TestAddressSpace();
        var message = CreatePermanentMessageFixture();

        message.Begin(bus, GameplayMessageIds.EnergyTank, 0);
        // $85:808D-$8096 spend eight lag frames before Open_MessageBox; the first ends the
        // dispatch that opened the box.
        AssertEqual(GameplayMessageBoxPhase.Preparing, message.Phase,
            "item message prepares the PPU, tilemap and HDMA before opening");
        for (int preparingFrame = 0; preparingFrame < 7; preparingFrame++)
        {
            message.Step(0);
            AssertEqual(preparingFrame == 0 ? default : MessageBoxFrameAudio.MusicAndSounds, message.LastFrameAudio,
                $"item message preparing audio frame {preparingFrame}");
        }
        AssertEqual(GameplayMessageBoxPhase.Opening, message.Phase,
            "item message enters shared opening coroutine");
        AssertEqual(3, message.TilemapRowCount, "small item message has border/content/border");
        AssertEqual((ushort)0x3801, message.Tilemap[0], "small item message uses custom installed border");
        AssertEqual(GameplayMessageTitleDefinitions.TransparentWord, message.Tilemap[32],
            "small item message preserves transparent outer columns");
        ushort[] expectedTitle = [0x38f3, 0x38e4, 0x38f2, 0x38f3];
        AssertTrue(message.Tilemap.Slice(45, 4).SequenceEqual(expectedTitle),
            "small item message centers the installed TEST title with palette six");

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
        AssertEqual(24, message.RadiusPixels, "the accepting frame also draws the first close step");

        for (int closingFrame = 1; closingFrame < 13; closingFrame++)
        {
            message.Step(0);
            AssertEqual(24 - closingFrame * 2, message.RadiusPixels,
                $"item message closing radius frame {closingFrame}");
        }
        // $85:80AA-$80B4: tilemap clear, Restore_PPU's two waits, two music/sound frames.
        AssertEqual(GameplayMessageBoxPhase.Restoring, message.Phase, "item message restores the PPU");
        MessageBoxFrameAudio[] restoringAudio =
        [
            MessageBoxFrameAudio.MusicAndSounds,
            MessageBoxFrameAudio.MusicAndSounds,
            default,
            MessageBoxFrameAudio.MusicAndSounds with { RunsHdmaObjects = true },
            MessageBoxFrameAudio.MusicAndSounds,
        ];
        foreach (MessageBoxFrameAudio expected in restoringAudio)
        {
            message.Step(0);
            AssertTrue(message.IsActive, "item message remains active while restoring");
            AssertEqual(expected, message.LastFrameAudio, "item message restoring audio");
        }
        message.Step(0);
        AssertTrue(!message.IsActive, "item message returns after its restore frames");
        AssertEqual(MessageBoxFrameAudio.MusicAndSounds with { SoundHandlerCalls = 2 }, message.LastFrameAudio,
            "the return frame adds the dispatch's own HandleSounds");

        // Message two is the first large box and patches its shoot-button placeholder.
        // Supplying B proves the glyph is selected from the binding word, not hard-coded X.
        message.Begin(bus, GameplayMessageIds.MissileTank, 0, shootBinding: (ushort)SnesButton.B);
        for (int preparingFrame = 0; preparingFrame < 7; preparingFrame++)
            message.Step(0);
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
        cgram.SetColor(25, Bgr555.FromWord(0x7fff));
        var frame = new SuperMetroid.Core.Assets.Rgba32[256 * 224];
        message.Step(0); // radius 0
        message.Step(0); // radius 2: visible scanlines 122-125
        GameplayMessageBoxRenderer.Composite(frame, message, vram, cgram);
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), frame[121 * 256],
            "message HDMA clip leaves scanline above radius untouched");
        AssertEqual(
            Bgr555.FromWord(0x0bb1).ToRgba32(),
            frame[122 * 256],
            "message renderer applies temporary CGRAM color 25 inside clip");

        // Retail message $14 is the map-station message. Its definition chooses the
        // small-border drawing routine but delimits three complete content rows. This
        // verifies the native variable-length copy instead of inferring height from the
        // border routine's name, and exercises that shape through the compositor too.
        message = CreatePermanentMessageFixture();
        message.Begin(bus, GameplayMessageIds.MapDataAccessCompleted, 0);
        for (int preparingFrame = 0; preparingFrame < 7; preparingFrame++)
            message.Step(0);
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
        // Twelve more close steps, five restore frames, then the return.
        for (int returnFrame = 0; returnFrame < 12 + 5; returnFrame++)
            message.Step(0);
        AssertTrue(message.IsActive, "map-station message restores before returning");
        message.Step(0);
        AssertTrue(!message.IsActive,
            "map-station message returns to its suspended station PLM");

        // Parse every message accepted by the translated bank-$85 owner from the retail
        // cartridge. This catches table-boundary and uncommon-layout regressions without
        // needing controller playback through each collectible or station room.
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (File.Exists(romPath))
        {
            var retailBus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(File.ReadAllBytes(romPath));
            for (byte messageId = 1; messageId <= 26; messageId++)
            {
                var retailMessage = CreateGameplayMessageFixture();
                retailMessage.Begin(
                    retailBus,
                    GameplayMessageIds.FromCartridge(messageId, "retail definition-table audit"),
                    controllerRead: 0);
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

    /// <summary>Checks suit-pickup window staging, beam curve, palette reveal, input release, and Gravity priority.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifySuitPickupTransformation()
    {
        TestAddressSpace bus = CreateSuitPickupCartridge();
        SamusSuitColorCatalog suitColors = SamusSuitColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.Samus.SamusSuitColorExtractor.Extract(bus)));
        Suite(nameof(VerifySuitPickupHistory), () => VerifySuitPickupHistory(bus, suitColors));
        var cgram = new SnesCgram();
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(500, 600)),
            ProjectileFlareCounter = 60,
            EquippedItems = (ushort)SamusEquipmentFlags.VariaSuit,
            CollectedItems = (ushort)SamusEquipmentFlags.VariaSuit,
        };
        samus.Presentation.SuitColors = suitColors;
        SamusPresentation.LoadPowerSuitPalette(bus, cgram, suitColors);

        var pickup = new SamusSuitPickupState();
        var curveGuard = NewSuitPickupBeamCurveReadGuard(bus);
        pickup.Begin(bus, samus, layer1: new(0x0100, 0x0200),
            SamusSuitPickupKind.Varia);
        AssertTrue(pickup.IsActive, "Varia transformation starts after message return");
        AssertTrue(samus.InputLocked, "suit transformation locks Samus input");
        AssertEqual(SamusPoseId.ForwardFacingPowerSuitPose, samus.Pose,
            "first suit begins in front-facing power-suit pose");
        AssertEqual((ushort)(0x0100 + 120), samus.Position.X.Whole,
            "suit transformation centers Samus horizontally");
        AssertEqual((ushort)(0x0200 + 136), samus.Position.Y.Whole,
            "suit transformation centers Samus vertically");
        AssertEqual((ushort)0x001f, cgram.Colors[192],
            "suit palette is not revealed at transformation setup");

        // Begin publishes an empty beam. The first stage call lights exactly eight
        // scanlines at each edge with the one-pixel `$7878` beam.
        MotherBrainBeamWindowVerification.AssertOwnsNoScanlineTable(typeof(SamusSuitPickupState));
        AssertEqual(SamusSuitPickupBeam.None, pickup.Beam, "suit setup publishes an empty beam");
        var pixels = new SuperMetroid.Core.Assets.Rgba32[256 * 224];
        SamusSuitPickupRenderer.Composite(pixels, pickup.Flash);
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), pixels[0],
            "initial inverted suit window applies no fixed color");
        // The HDMA instruction list spends two handler calls before installing the
        // stage pre-instruction ($91:D5A2): neither changes the window table.
        for (int installCall = 0; installCall < 2; installCall++)
        {
            pickup.Step(curveGuard, samus, cgram);
            AssertEqual(SamusSuitPickupBeam.None, pickup.Beam,
                "instruction-list setup calls leave the beam empty");
            AssertEqual((byte)0, pickup.Substate, "instruction-list setup calls stay in stage zero");
        }
        pickup.Step(curveGuard, samus, cgram);
        AssertEqual(new SamusSuitPickupBeam(SamusSuitPickupBeamShape.GrowingFromEdges, 120, 120, 8), pickup.Beam,
            "stage zero publishes eight grown scanlines of the X=120 beam");
        ushort[] stageZero = SuitPickupBeamWindowBuilder.Build(pickup.Beam);
        AssertEqual((ushort)0x7878, stageZero[0],
            "stage zero narrows first top scanline");
        AssertEqual((ushort)0x7878, stageZero[255],
            "stage zero narrows first bottom scanline");
        AssertEqual((ushort)0x00ff, stageZero[8],
            "stage zero changes exactly eight top scanlines per frame");
        Array.Clear(pixels);
        SamusSuitPickupRenderer.Composite(pixels, pickup.Flash);
        AssertTrue(pixels[120].R != 0 && pixels[120].G != 0,
            "stage-zero one-pixel beam receives fixed color");
        AssertEqual(default(SuperMetroid.Core.Assets.Rgba32), pixels[119],
            "pixel beside stage-zero beam remains outside color window");

        int transformationFrames = 1;
        bool observedReveal = false;
        var beams = new List<SamusSuitPickupBeam> { pickup.Beam };
        var flashes = new List<SamusSuitPickupFlash> { (pickup.Flash ?? throw new InvalidOperationException("Verification requires pickup.Flash.")) };
        while (pickup.IsActive && transformationFrames < 2000)
        {
            pickup.Step(curveGuard, samus, cgram);
            transformationFrames++;
            if (pickup.Flash is { } flash)
            {
                AssertEqual(new SamusSuitPickupFlash(pickup.Beam, pickup.FixedColorRed, pickup.FixedColorGreen,
                    pickup.FixedColorBlue), flash, "active suit publishes its beam and fixed color by value");
                beams.Add(pickup.Beam);
                flashes.Add(flash);
            }
            if (!observedReveal && pickup.Substate == 4)
            {
                observedReveal = true;
                AssertEqual(SamusPoseId.ForwardFacingSuitedPose, samus.Pose,
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
        AssertEqual(0, curveGuard.ForbiddenReadAttempts,
            "production suit transformation performs no beam-curve ROM reads");
        AssertTrue(pickup.Flash is null, "finished suit transformation publishes no flash");
        VerifySuitPickupBeamSequence(beams, flashes);

        // Gravity has priority when both suit bits are equipped, matching `$91:DEBA`.
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.GravitySuit;
        samus.Presentation.LoadSuitPalette(bus, cgram);
        AssertEqual((ushort)0x7c00, cgram.Colors[192],
            "normal suit palette loader gives Gravity priority over Varia");
    }

    /// <summary>Checks the Varia light beam's published shape, progress and color across the whole transformation.</summary>
    private static void VerifySuitPickupBeamSequence(List<SamusSuitPickupBeam> beams, List<SamusSuitPickupFlash> flashes)
    {
        var shapes = new List<SamusSuitPickupBeamShape>();
        foreach (SamusSuitPickupBeam beam in beams)
            if (shapes.Count == 0 || shapes[^1] != beam.Shape) shapes.Add(beam.Shape);
        AssertTrue(shapes.SequenceEqual(new[]
        {
            SamusSuitPickupBeamShape.GrowingFromEdges, SamusSuitPickupBeamShape.Column,
            SamusSuitPickupBeamShape.CurvedColumn, SamusSuitPickupBeamShape.ClosingCurvedColumn,
            SamusSuitPickupBeamShape.CenterLine,
        }), $"suit beam stages publish shapes in order: {string.Join(", ", shapes)}");

        SamusSuitPickupBeam[] growing = [.. beams.Where(beam => beam.Shape == SamusSuitPickupBeamShape.GrowingFromEdges)];
        for (int i = 0; i < growing.Length; i++)
            AssertEqual(8 * (i + 1), growing[i].Progress, "stage zero grows eight scanlines per update");
        AssertEqual(128, growing[^1].Progress, "stage zero ends when the beam meets at the center");

        SamusSuitPickupBeam[] column = [.. beams.Where(beam => beam.Shape == SamusSuitPickupBeamShape.Column)];
        AssertEqual(new SamusSuitPickupBeam(SamusSuitPickupBeamShape.Column, 119, 121, 0), column[0],
            "stage one widens the narrow beam by one pixel per side");
        for (int i = 1; i < column.Length; i++)
            AssertTrue(column[i].Left < column[i - 1].Left && column[i].Right > column[i - 1].Right,
                "stage one widens every update");
        AssertTrue(column[^1].Left < 97 && column[^2].Left >= 97, "stage one ends once the left edge passes X=97");

        SamusSuitPickupBeam[] curved = [.. beams.Where(beam => beam.Shape == SamusSuitPickupBeamShape.CurvedColumn)];
        AssertEqual(new SamusSuitPickupBeam(SamusSuitPickupBeamShape.CurvedColumn, 0, 255, 0), curved[^1],
            "stage two ends at the full-screen column");
        ushort[] full = SuitPickupBeamWindowBuilder.Build(curved[^1]);
        AssertTrue(full.All(word => word == SamusSpecialSequenceRomData.SuitPickup.FullScreenEndpoints),
            "the final curved column covers every scanline");
        ushort[] widening = SuitPickupBeamWindowBuilder.Build(curved[0]);
        AssertTrue(SnesLittleEndian.LowByte(widening[127]) < SnesLittleEndian.LowByte(widening[0])
            && SnesLittleEndian.HighByte(widening[128]) > SnesLittleEndian.HighByte(widening[255]),
            "the curved column is widest at the vertical center");

        SamusSuitPickupBeam[] closing = [.. beams.Where(beam => beam.Shape == SamusSuitPickupBeamShape.ClosingCurvedColumn)];
        for (int i = 0; i < closing.Length; i++)
        {
            AssertTrue(closing[i].Left == 0 && closing[i].Right == 255, "stage four closes the final curved column");
            if (i > 0) AssertTrue(closing[i].Progress > closing[i - 1].Progress, "stage four wipes further every update");
        }
        AssertTrue(closing[^1].Progress >= 128, "stage four ends once the wipe reaches the center");
        ushort[] wiped = SuitPickupBeamWindowBuilder.Build(closing[0]);
        int top = closing[0].Progress;
        AssertEqual(SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints, wiped[top - 1], "closing wipes the top progress scanlines");
        AssertEqual(SamusSpecialSequenceRomData.SuitPickup.FullScreenEndpoints, wiped[top], "closing keeps the scanline below the top wipe");
        AssertEqual(SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints, wiped[256 - (top - 1)], "closing wipes one fewer bottom scanline");
        AssertEqual(SamusSpecialSequenceRomData.SuitPickup.FullScreenEndpoints, wiped[255 - (top - 1)], "closing keeps the scanline above the bottom wipe");

        SamusSuitPickupBeam[] line = [.. beams.Where(beam => beam.Shape == SamusSuitPickupBeamShape.CenterLine)];
        AssertEqual(new SamusSuitPickupBeam(SamusSuitPickupBeamShape.CenterLine, 7, 240, 0), line[0],
            "stage five starts from the native dissipation endpoints");
        for (int i = 1; i < line.Length; i++)
            AssertTrue(line[i].Left == line[i - 1].Left + 8 && line[i].Right == line[i - 1].Right - 8,
                "stage five narrows eight pixels per side every update");
        ushort[] center = SuitPickupBeamWindowBuilder.Build(line[0]);
        AssertTrue(center.Select((word, y) => (word, y)).All(row => row.y == 128
            ? row.word == SnesLittleEndian.Word(7, 240)
            : row.word == SamusSpecialSequenceRomData.SuitPickup.EmptyWindowEndpoints),
            "stage five lights only the center scanline");

        // Color: unchanged until the curved stage, white by the reveal, then toward Varia orange.
        int firstCurved = beams.FindIndex(beam => beam.Shape == SamusSuitPickupBeamShape.CurvedColumn);
        int firstClosing = beams.FindIndex(beam => beam.Shape == SamusSuitPickupBeamShape.ClosingCurvedColumn);
        AssertTrue(flashes.Take(firstCurved).All(flash => flash with { Beam = default } == flashes[0] with { Beam = default }),
            "fixed color holds until the curved stage");
        SamusSuitPickupFlash white = flashes[firstClosing - 1];
        AssertEqual((SamusPaletteRomData.SuitPickup.WhiteRed, SamusPaletteRomData.SuitPickup.WhiteGreen, SamusPaletteRomData.SuitPickup.WhiteBlue),
            (white.Red, white.Green, white.Blue), "the curved stage brightens the fixed color to white");
        SamusSuitPickupFlash last = flashes[^1];
        AssertEqual((SamusPaletteRomData.SuitPickup.WhiteRed, SamusPaletteRomData.SuitPickup.VariaOrangeGreen, SamusPaletteRomData.SuitPickup.VariaOrangeBlue),
            (last.Red, last.Green, last.Blue), "the shrinking Varia beam settles on orange");
        for (int i = firstClosing; i < flashes.Count; i++)
            AssertTrue(flashes[i].Green <= flashes[i - 1].Green && flashes[i].Blue <= flashes[i - 1].Blue,
                "the shrinking beam's color only steps down");
    }

    /// <summary>Checks pose-history shifts at transformation entry and reveal for both suits and preowned-suit states.</summary>
    private static void VerifySuitPickupHistory(TestAddressSpace bus, SamusSuitColorCatalog suitColors)
    {
        foreach (SamusSuitPickupKind kind in new[] { SamusSuitPickupKind.Varia, SamusSuitPickupKind.Gravity })
        foreach (bool otherSuit in new[] { false, true })
        {
            var samus = new SamusState { EquippedItems = otherSuit
                ? (ushort)(kind == SamusSuitPickupKind.Varia ? SamusEquipmentFlags.GravitySuit : SamusEquipmentFlags.VariaSuit)
                : (ushort)0 };
            samus.Presentation.SuitColors = suitColors;
            var history = samus.PoseHistory;
            history.PreviousPose = SamusPoseId.SpinJumpRightPose;
            history.PreviousDirectionAndMovement = 0x0308;
            history.LastDifferentPose = SamusPoseId.WallJumpLeftPose;
            history.LastDifferentDirectionAndMovement = 0x1404;
            var pickup = new SamusSuitPickupState();
            pickup.Begin(bus, samus, new(0, 0), kind);
            SamusPoseId initialPose = samus.Pose;
            ushort initialMetadata = (ushort)(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(bus), (byte)samus.PoseMetadata.ReadMovementType(bus)));
            AssertEqual(SamusPoseId.SpinJumpRightPose, history.LastDifferentPose, "suit entry shifts prior pose");
            AssertEqual(0x0308, history.LastDifferentDirectionAndMovement, "suit entry shifts prior metadata");
            AssertEqual(initialPose, history.PreviousPose, "suit entry commits front pose");
            AssertEqual(initialMetadata, history.PreviousDirectionAndMovement, "suit entry commits front metadata");
            int frames = 0;
            var cgram = new SnesCgram();
            while (pickup.Substate < 4 && frames++ < 2000) pickup.Step(bus, samus, cgram);
            AssertEqual(4, pickup.Substate, "suit history fixture reaches reveal");
            AssertEqual(initialPose, history.LastDifferentPose, "suit reveal shifts initial pose even if already suited");
            AssertEqual(initialMetadata, history.LastDifferentDirectionAndMovement, "suit reveal shifts initial metadata");
            AssertEqual(SamusPoseId.ForwardFacingSuitedPose, history.PreviousPose, "suit reveal commits suited pose");
            AssertEqual(SnesLittleEndian.Word(samus.PoseMetadata.ReadPoseXDirection(bus), (byte)samus.PoseMetadata.ReadMovementType(bus)),
                history.PreviousDirectionAndMovement, "suit reveal commits suited metadata");
            AssertTrue(!history.AllowsWallJumpProbe, "suit reveal clears pre-acquisition spin eligibility");
        }
    }

    /// <summary>Room, PLM, system, and Samus owners returned by a constructed item population load.</summary>
    internal readonly record struct CollectibleFixture(
        RoomPlmSystem Plms,
        RoomLevelData Level,
        BackgroundTilemapStreamer Streamer,
        Bank80SystemState System,
        SamusState Samus,
        int BlockIndex);

    /// <summary>Synthetic cartridge with the suit-pickup fade curve and distinct suit palette first words.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for suit pickup fades and suit palettes")]
    private static TestAddressSpace CreateSuitPickupCartridge()
    {
        var bus = new TestAddressSpace();
        for (int i = 0; i < 128; i++)
            bus.WriteBytes(0x88e3c9 + i, [unchecked((byte)Math.Min(i / 8 + 1, 15))]);

        // Distinct first words prove that the generic suit loader selects from equipment,
        // and that stage three—not Begin—performs the newly acquired palette reveal.
        WriteTestWord(bus, 0x9b9400, 0x001f);
        WriteTestWord(bus, 0x9b9520, 0x03e0);
        WriteTestWord(bus, 0x9b9800, 0x7c00);
        return bus;
    }

    /// <summary>Runs the focused Morph Ball message-return collision fixture.</summary>
    [VerificationCommand("--morph-ball-pickup-collision")]
    internal static int MorphBallPickupCollisionCommand()
    {
        Suite(nameof(VerifyMorphBallPickupCollision), () => VerifyMorphBallPickupCollision());
        return 0;
    }

    /// <summary>Runs the full authored-shape permanent collectible fixture.</summary>
    [VerificationCommand("--permanent-collectibles-fixture")]
    internal static int PermanentCollectiblesFixtureCommand()
    {
     VerifyPermanentCollectibles(); return 0; 
    }
}
