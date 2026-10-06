using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyEnemyEffectResources()
    {
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var corpse = new MotherBrainCorpseRottingState();
        byte[] before = memory.WorkRam.ToArray();
        AssertThrows<ArgumentNullException>(() => corpse.Initialize(memory, null!), "missing corpse art fails loudly");
        AssertTrue(before.AsSpan().SequenceEqual(memory.WorkRam), "missing corpse art must not initialize the rot table before throwing");
        AssertEqual(false, corpse.IsInitialized, "failed corpse initialization remains uninitialized");
        AssertEqual(0u, corpse.ProcessCallCount, "failed corpse initialization preserves its clock");
        using var shortPng = EffectPng(RoomCharacterAtlasFormat.BytesPerTile, RoomCharacterAtlasFormat.BytesPerTile, 0x55);
        RoomCharacterAtlas shortArt = RoomCharacterAtlas.Load(shortPng, RoomCharacterAtlasFormat.BytesPerTile);
        AssertThrows<InvalidDataException>(() => corpse.Initialize(memory, shortArt), "wrong-size corpse art fails loudly");
        AssertTrue(before.AsSpan().SequenceEqual(memory.WorkRam), "wrong-size corpse art leaves WRAM unchanged");
        AssertThrows<InvalidDataException>(() => new MotherBrainRainbowBeamAttackSequence().InitializeCorpseRotting(memory),
            "sequence setup requires corpse art");
        AssertTrue(before.AsSpan().SequenceEqual(memory.WorkRam), "missing sequence corpse art leaves WRAM unchanged");

        CheckCrocomireResourceFailure(null, fixture => fixture.InitializeMap(CrocomireMeltingArtworkAddresses.FirstTilemap),
            "missing melt tilemap");
        CheckCrocomireResourceFailure(null, fixture => fixture.Call("InitializeCrocomireMeltingGraphics"),
            "missing melt graphics");
        CrocomireMeltingArtwork art = MeltEffectArtwork();
        CheckCrocomireResourceFailure(art, fixture => fixture.InitializeMap(0), "unknown melt tilemap");
        CheckCrocomireResourceFailure(art, fixture => fixture.Call("InitializeCrocomireMeltingGraphics"),
            "unknown melt header", invalidHeader: true);
        Suite(nameof(VerifyDeadTourianMissingArtwork), () => VerifyDeadTourianMissingArtwork());
        Console.WriteLine("  Enemy effect resources: missing/unknown art fails before changing WRAM, phases, actors, graphics or VRAM.");
    }

    private static void VerifyDeadTourianMissingArtwork()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach ((ushort definition, int variants) in new[] { (RoomEnemySystem.DeadZoomerDefinition, 3),
            (RoomEnemySystem.DeadRipperDefinition, 2), (RoomEnemySystem.DeadSkreeDefinition, 3) })
        for (int variant = 0; variant < variants; variant++)
        foreach (bool emptyCatalog in new[] { false, true })
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var expected = new RoomEnemySystem(); var actual = new RoomEnemySystem();
            expected.Slots[0].EnemyDefinitionPointer = actual.Slots[0].EnemyDefinitionPointer = definition;
            expected.Slots[0].Parameter1 = actual.Slots[0].Parameter1 = (ushort)(variant * 2);
            if (emptyCatalog) actual.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>());
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(actual, memory);
            Action<RoomEnemySlot> initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadTourianCorpse", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(actual);
            byte[] before = memory.WorkRam.ToArray();
            AssertThrows<InvalidDataException>(() => initialize(actual.Slots[0]), "missing Tourian corpse art fails loudly");
            AssertTrue(before.AsSpan().SequenceEqual(memory.WorkRam), "missing Tourian corpse art leaves its rot table/graphics unchanged");
            AssertTrue(actual.DeadTourianCorpses.All(state => state is null), "failed corpse setup must not publish an owner");
            AssertAnimationValues(expected.Slots[0], actual.Slots[0], "failed Tourian corpse setup must not mutate the slot");
        }
    }

    private static void CheckCrocomireResourceFailure(CrocomireMeltingArtwork? art,
        Action<CrocomireEffectFixture> operation, string context, bool invalidHeader = false)
    {
        var expected = new CrocomireEffectFixture(art);
        var actual = new CrocomireEffectFixture(art);
        if (invalidHeader) expected.Effect.MeltingTableOffset = actual.Effect.MeltingTableOffset = 1;
        AssertThrows<InvalidDataException>(() => operation(actual), context + " fails loudly");
        AssertCrocomireEffectMechanics(expected, actual, context);
        AssertTrue(expected.Effect.MeltingGraphics.SequenceEqual(actual.Effect.MeltingGraphics), context + " graphics unchanged");
        AssertTrue(expected.Effect.Bg2WorkingTilemap.SequenceEqual(actual.Effect.Bg2WorkingTilemap), context + " tilemap unchanged");
        AssertTrue(expected.Vram.Bytes.SequenceEqual(actual.Vram.Bytes), context + " VRAM unchanged");
    }

    private static void VerifyCrocomireMeltJson()
    {
        string valid = MeltEffectJson();
        _ = MeltEffectArtwork(firstJson: valid);
        _ = MeltEffectArtwork(firstJson: valid.Replace("\"version\"", "\"Version\"", StringComparison.Ordinal));
        foreach ((string name, string malformed) in new[]
        {
            ("unknown root field", valid.Replace("\"version\":1", "\"version\":1,\"unknown\":0", StringComparison.Ordinal)),
            ("duplicate root field", valid.Replace("\"version\":1", "\"version\":1,\"version\":1", StringComparison.Ordinal)),
            ("case-aliased root field", valid.Replace("\"version\":1", "\"version\":1,\"VERSION\":1", StringComparison.Ordinal)),
            ("unknown cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"unknown\":0", StringComparison.Ordinal)),
            ("duplicate cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"tileIndex\":0", StringComparison.Ordinal)),
            ("case-aliased cell field", valid.Replace("\"tileIndex\":0", "\"tileIndex\":0,\"TILEINDEX\":0", StringComparison.Ordinal)),
        })
            AssertThrows<InvalidDataException>(() => MeltEffectArtwork(firstJson: malformed), name + " cannot be silently discarded");
        Console.WriteLine("  Crocomire melt JSON: valid schema/casing preserved; unknown, duplicate and case-aliased root/cell fields rejected.");
    }
}
