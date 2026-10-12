using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using static EnemyAnimationIsolationFixtureVerification;
using static VerificationHelpersVerification;

internal static class EnemyEffectsFixtureVerification
{
    /// <summary>Known melt phases, pure RAM and authored PNG/JSON; no cartridge capability.</summary>
    internal sealed class CrocomireEffectFixture
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        internal SnesVram Vram { get; } = new();
        internal RoomEnemySystem Enemies { get; } = new();
        internal CrocomireEnemyState Actor { get; }
        internal CrocomireDeathState Effect { get; } = new();

        internal CrocomireEffectFixture(CrocomireMeltingArtwork? art = null)
        {
            if (art is not null)
                Enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), CrocomireMelting = art });
            Actor = new CrocomireEnemyState(Enemies.SlotTable.Slots[0]) { Tongue = Enemies.SlotTable.Slots[1] };
            Actor.Body.Position = Actor.Body.Position.WithWhole(new(400, 144));
            Actor.Tongue.Position = Actor.Tongue.Position.WithWholeY(160);
            Effect.AcidSmokeTimer = 1;
            Effect.MutableMeltingGraphics.Fill(0x39);
            Effect.MutableBg2WorkingTilemap.Fill(0x137);
            Effect.MutableBg2ScrollByScanline.Fill(19);
            Enemies.BindFixtureBus(Memory);
            Enemies.Presentation._vram = Vram;
            Enemies.CrocomireBehavior.Crocomire = Actor;
            Enemies.CrocomireBehavior.CrocomireDeath = Effect;
            Enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 0x1234);
        }

        private void Bind(string field, object value) => typeof(RoomEnemySystem)
            .GetField(field, PrivateInstance)!.SetValue(Enemies, value);

        internal void InitializeMap(int source) => typeof(RoomEnemySystem)
            .GetMethod("InitializeCrocomireMeltingTilemap", PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState, int, ushort>>(Enemies)(
                Actor, source, source == (int)CrocomireMeltingTilemapAddress.SecondTilemap
                    ? CrocomireInstructionProgramDefinitions.MeltingTwoTopRow
                    : CrocomireInstructionProgramDefinitions.MeltingOneTopRow);

        /// <summary>Runs one Crocomire melting step, selected from the fixture's Crocomire behavior, on its actor.</summary>
        internal void Call(Func<CrocomireBehavior, Action<CrocomireEnemyState>> select) => select(Enemies.CrocomireBehavior)(Actor);

        internal void Dissolve() => new Action<CrocomireEnemyState, SamusState?>(Enemies.CrocomireBehavior.RunCrocomireMelting)(Actor, null);
    }

    internal static void AssertCrocomireEffectMechanics(CrocomireEffectFixture expected,
        CrocomireEffectFixture actual, string context)
    {
        AssertAnimationValues(expected.Actor, actual.Actor, context);
        AssertAnimationValues(expected.Effect, actual.Effect, context);
        AssertAnimationValues(expected.Enemies, actual.Enemies, context);
        for (int slot = 0; slot < expected.Enemies.SlotTable.Slots.Count; slot++)
            AssertAnimationValues(expected.Enemies.SlotTable.Slots[slot], actual.Enemies.SlotTable.Slots[slot], context);
        for (int slot = 0; slot < expected.Enemies.RoomSpriteObjects.Count; slot++)
            AssertAnimationValues(expected.Enemies.RoomSpriteObjects[slot], actual.Enemies.RoomSpriteObjects[slot], context);
        AssertTrue(expected.Effect.MeltingColumnHeights.SequenceEqual(actual.Effect.MeltingColumnHeights), context + " erase schedule");
        AssertTrue(expected.Effect.Bg2ScrollByScanline.SequenceEqual(actual.Effect.Bg2ScrollByScanline), context + " HDMA geometry");
        AssertSameBytes(expected.Memory.WorkRam, actual.Memory.WorkRam, context + " WRAM");
        AssertSameBytes(expected.Memory.SaveRam, actual.Memory.SaveRam, context + " SRAM");
        AssertTrue(expected.Enemies.Audio.SoundRequests.SequenceEqual(actual.Enemies.Audio.SoundRequests), context + " audio");
        AssertTrue(expected.Enemies.Audio.MusicRequests.SequenceEqual(actual.Enemies.Audio.MusicRequests), context + " music");
        AssertTrue(expected.Enemies.CrocomireBehavior.CrocomirePlmRequests.SequenceEqual(actual.Enemies.CrocomireBehavior.CrocomirePlmRequests), context + " room changes");
    }

    // Decode authored bitplanes and write a genuine indexed PNG so fixtures exercise the
    // production asset compiler, rather than injecting unvalidated private buffers.
    internal static MemoryStream EffectPng(int byteCount, int usedBytes, byte pattern)
    {
        var bytes = new byte[byteCount];
        bytes.AsSpan(0, usedBytes).Fill(pattern);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(bytes, 4, RoomCharacterAtlasFormat.TileColumns,
            out Extent16<ImagePixels> size);
        var output = new MemoryStream();
        IndexedPng.Write(output, size, pixels,
            Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 16), 0, 0)).ToArray());
        output.Position = 0;
        return output;
    }

    internal static RoomCharacterAtlas CorpseEffectArtwork(byte pattern) => RoomCharacterAtlas.Load(
        EffectPng(MotherBrainCorpseArtworkDefinitions.ByteCount, MotherBrainCorpseArtworkDefinitions.ByteCount, pattern),
        MotherBrainCorpseArtworkDefinitions.ByteCount);

    internal static string MeltEffectJson(int seed = 0) => JsonSerializer.Serialize(new CrocomireMeltingTilemapDocument
    {
        Version = CrocomireMeltingArtworkFormat.TilemapVersion,
        Width = CrocomireMeltingArtworkFormat.TilemapWidth,
        Height = CrocomireMeltingArtworkFormat.TilemapHeight,
        Cells = Enumerable.Range(0, CrocomireMeltingArtworkFormat.TilemapCellCount).Select(index =>
            new CrocomireMeltingTilemapCell
            {
                TileIndex = (index + seed) % 512, Palette = seed % 8,
                Priority = seed != 0, FlipX = seed != 0, FlipY = seed != 0,
            }).ToArray(),
    }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    internal static CrocomireMeltingArtwork MeltEffectArtwork(byte pattern = 0x55,
        int seed = 0, string? firstJson = null)
    {
        using var first = EffectPng(CrocomireMeltingArtworkFormat.FirstByteCount,
            CrocomireMeltingArtwork.UsedByteCount(CrocomireMeltingTransferDefinitions.Passes[0]), pattern);
        using var second = EffectPng(CrocomireMeltingArtworkFormat.SecondByteCount,
            CrocomireMeltingArtwork.UsedByteCount(CrocomireMeltingTransferDefinitions.Passes[1]), pattern);
        using var firstMap = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(firstJson ?? MeltEffectJson(seed)));
        using var secondMap = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MeltEffectJson(seed)));
        return CrocomireMeltingArtwork.Load(first, second, firstMap, secondMap);
    }
}
