using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Ridley's death breakup fragments, instruction programs, installed OAM, and expiry.</summary>
internal static class RidleyBreakupProgramsVerification
{

    /// <summary>Drives the grab-release death branch and compares all 29 fragment programs with native data.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyRidleyBreakupPrograms()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
        Action<RoomEnemySlot, RidleyEnemyState, SamusState?> death = enemies.RidleyBehavior.TickNorfairRidleyDeathExplosions;
        var process = new Action<RoomEnemySlot, SamusState?, RoomLevelData?, Position16<RoomPixels>, ushort>(enemies.Instructions.ProcessInstructions);
        var body = enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Ridley;
        var state = new RidleyEnemyState
        {
            GrabState = 1, FunctionTimer = NativeCountdown.Start(0),
            TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                { Angle = index == 6 ? (ushort)0x10 : (ushort)0, Position = new Position16<RoomPixels>(128, 128) }).ToArray(),
        };
        enemies.RidleyEncounter.Ridley = state;
        death(body, state, new SamusState());
        AssertEqual((ushort)0, state.GrabState, "Ridley death releases Samus before breakup");
        var fragments = enemies.SlotTable.Slots.Where(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.RidleyExplosion).ToArray();
        AssertEqual(12, fragments.Length, "Ridley death spawns all twelve body/tail fragments");
        AssertEqual(RidleyBreakupFrame.TailTip1, state.BreakupPrograms[fragments[0].SlotIndex].Program,
            "reported tail orientation selects the second tip pose ($A6:CA9B)");
        foreach (var fragment in fragments)
        {
            AssertEqual(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap, fragment.SpritemapPointer,
                "native breakup spawn installs empty frame before instruction processing");
            process(fragment, null, null, Position16<RoomPixels>.Zero, 0);
            AssertTrue(fragment.SpritemapPointer is not CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap and
                not 0, "first breakup instruction replaces empty frame");
        }
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapCatalog.Load(new MemoryStream(stockBytes));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<EnemySpritemapDocument>(stockBytes, options);
        var previous = EnemySpritemapDefinitions.Frames.Take(EnemySpritemapDefinitions.PreRidleyBreakupFrameCount).ToArray();
        var legacy = document with
        {
            Version = (int)EnemySpritemapSchema.PreRidleyBreakup,
            Frames = previous.ToDictionary(frame => frame.Name, frame => document.Frames[frame.Name]),
            DisplayFrames = previous.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
        string edited = previous[0].Name;
        legacy.Frames[edited] = [legacy.Frames[edited][0] with { OffsetX = -11 }];
        byte[] legacyBytes = JsonSerializer.SerializeToUtf8Bytes(legacy, options);
        var merged = EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes), stock);
        AssertTrue(merged.TryGetDisplay(previous[0].Bank, previous[0].Pointer, out var parts), "schema-68 override retains existing frame");
        AssertEqual(-11, parts[0].X.SignedOffset, "schema-68 edit survives stock refresh");
        AssertThrows<InvalidDataException>(() => EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes)), "schema-68 incomplete stock requires refresh");
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = merged });
        // VerifyRidleyAnimations matches each typed breakup program to its native list; this draws every pose.
        foreach (RidleyBreakupFrame frame in Enum.GetValues<RidleyBreakupFrame>())
        {
            ushort operand = (ushort)(RidleyAnimationsVerification.BreakupList(frame) + 2);
            ushort expectedPointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(RidleyNativeListAddresses.BodyBankNumber << 16 | operand);
            var fragment = fragments[0];
            state.BreakupPrograms[fragment.SlotIndex] = AnimationProgramCursor<RidleyBreakupFrame>.Start(frame);
            fragment.InstructionTimer = NativeCountdown.Start(1);
            process(fragment, null, null, Position16<RoomPixels>.Zero, 0);
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup selects native sprite");
            process(fragment, null, null, Position16<RoomPixels>.Zero, 0);
            AssertEqual(1, state.BreakupPrograms[fragment.SlotIndex].Position, "breakup sleeps after its one frame");
            var actual = new OamBuffer();
            enemies.Presentation.DrawEnemySpritemap(actual, new EnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new Position16<OamPixels>((ushort)128, (ushort)112), PaletteBits = 0x0e00, BaseTileIndex = 0, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, new EnemySpritemapDraw { Bank = 0xa6, Pointer = expectedPointer, Origin = new(128, 112), PaletteBits = 0x0e00, BaseTileIndex = 0 });
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "breakup OAM count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), "breakup exact OAM bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), "breakup exact OAM size/X bits");
        }
        var expiring = fragments[0];
        ushort deathX = expiring.Position.X.Whole, deathY = expiring.Position.Y.Whole;
        expiring.VariableF = 0; expiring.VariableB = 0; expiring.VariableC = 0;
        expiring.Position = expiring.Position.WithFractionX(0).WithFractionY(0);
        enemies.RidleyBehavior.RunNorfairRidleyExplosionMain(expiring);
        AssertEqual(EnemyDefinitionId.None, expiring.EnemyDefinitionPointer, "native breakup expiry clears the actor immediately");
        var explosion = enemies.Projectiles.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        AssertEqual(deathX, explosion.Position.X.Whole, "fragment death effect retains final X");
        AssertEqual(deathY, explosion.Position.Y.Whole, "fragment death effect retains final Y");
        AssertEqual(EnemyDefinitionId.RidleyExplosion, explosion.EnemyHeaderPointer, "fragment drop retains native header");
        AssertEqual(EnemyDeathExplosionDefinitions.InstructionPointer((ushort)EnemyDeathAnimation.SmallExplosion),
            explosion.InstructionPointer, "fragment expiry selects native small death animation");
        AssertEqual((ushort)1, enemies.RoomStatus.EnemiesKilled, "fragment death updates shared kill count");
        Console.WriteLine("Ridley breakup: death while grabbing releases Samus and runs all twelve fragment programs without cartridge access.");
        Console.WriteLine("All 29 breakup programs match native mechanics, frame selection, sleep and installed OAM.");
    }

    /// <summary>Runs the Ridley breakup program and artwork check.</summary>
    [VerificationCommand("--ridley-breakup-programs")]
    internal static int RidleyBreakupProgramsCommand()
    {
        Suite(nameof(VerifyRidleyBreakupPrograms), () => VerifyRidleyBreakupPrograms());
        return 0;
    }
}
