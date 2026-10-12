using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;
using static EnemyAnimationIsolationFixtureVerification;
using static EnemyEffectsFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Represents enemy effect resources verification.</summary>
internal static class EnemyEffectResourcesVerification
{
    /// <summary>Verifies enemy effect resources.</summary>
    internal static void VerifyEnemyEffectResources()
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

        CheckCrocomireResourceFailure(null, fixture => fixture.InitializeMap((int)CrocomireMeltingTilemapAddress.FirstTilemap),
            "missing melt tilemap");
        CheckCrocomireResourceFailure(null, fixture => fixture.Call(c => c.InitializeCrocomireMeltingGraphics),
            "missing melt graphics");
        CrocomireMeltingArtwork art = MeltEffectArtwork();
        CheckCrocomireResourceFailure(art, fixture => fixture.InitializeMap(0), "unknown melt tilemap");
        CheckCrocomireResourceFailure(art, fixture => fixture.Call(c => c.InitializeCrocomireMeltingGraphics),
            "unknown melt header", invalidHeader: true);
        Suite(nameof(VerifyDeadTourianMissingArtwork), () => VerifyDeadTourianMissingArtwork());
        Console.WriteLine("  Enemy effect resources: missing/unknown art fails before changing WRAM, phases, actors, graphics or VRAM.");
    }

    /// <summary>Verifies dead tourian missing artwork.</summary>
    private static void VerifyDeadTourianMissingArtwork()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach ((EnemyDefinitionId definition, int variants) in new[] { (EnemyDefinitionId.CorpseZoomer, 3),
            (EnemyDefinitionId.CorpseRipper, 2), (EnemyDefinitionId.CorpseSkree, 3) })
        for (int variant = 0; variant < variants; variant++)
        foreach (bool emptyCatalog in new[] { false, true })
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var expected = new RoomEnemySystem(); var actual = new RoomEnemySystem();
            expected.SlotTable.Slots[0].EnemyDefinitionPointer = actual.SlotTable.Slots[0].EnemyDefinitionPointer = definition;
            expected.SlotTable.Slots[0].Parameter1 = actual.SlotTable.Slots[0].Parameter1 = (ushort)(variant * 2);
            if (emptyCatalog) actual.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
            actual.BindFixtureBus(memory);
            Action<RoomEnemySlot> initialize = new Action<RoomEnemySlot>(actual.DeadTourianBehavior.InitializeDeadTourianCorpse);
            byte[] before = memory.WorkRam.ToArray();
            AssertThrows<InvalidDataException>(() => initialize(actual.SlotTable.Slots[0]), "missing Tourian corpse art fails loudly");
            AssertTrue(before.AsSpan().SequenceEqual(memory.WorkRam), "missing Tourian corpse art leaves its rot table/graphics unchanged");
            AssertTrue(actual.DeadTourianCorpses.All(state => state is null), "failed corpse setup must not publish an owner");
            AssertAnimationValues(expected.SlotTable.Slots[0], actual.SlotTable.Slots[0], "failed Tourian corpse setup must not mutate the slot");
        }
    }

    /// <summary>Checks crocomire resource failure.</summary>
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
        AssertTrue(expected.Vram.ImageEquals(actual.Vram), context + " VRAM unchanged");
    }

    /// <summary>Verifies crocomire melt json.</summary>
    internal static void VerifyCrocomireMeltJson()
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

    /// <summary>Enemy effect resources command.</summary>
    [VerificationCommand("--enemy-effect-resources")]
    internal static int EnemyEffectResourcesCommand()
    {
        Suite(nameof(VerifyEnemyEffectResources), () => VerifyEnemyEffectResources());
        return 0;
    }

    /// <summary>Crocomire melt json command.</summary>
    [VerificationCommand("--crocomire-melt-json")]
    internal static int CrocomireMeltJsonCommand()
    {
        Suite(nameof(VerifyCrocomireMeltJson), () => VerifyCrocomireMeltJson());
        return 0;
    }
}
