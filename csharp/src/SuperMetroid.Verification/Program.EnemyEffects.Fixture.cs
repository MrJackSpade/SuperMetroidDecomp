using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Known melt phases, pure RAM and authored PNG/JSON; no cartridge capability.</summary>
    private sealed class CrocomireEffectFixture
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
                Enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                    new Dictionary<ushort, RoomCharacterAtlas>(),
                    new Dictionary<ushort, EnemyPaletteSheet>(), crocomireMelting: art);
            Actor = new CrocomireEnemyState(Enemies.Slots[0]) { Tongue = Enemies.Slots[1] };
            Actor.Body.XPosition = 400;
            Actor.Body.YPosition = 144;
            Actor.Tongue.YPosition = 160;
            Effect.AcidSmokeTimer = 1;
            Effect.MutableMeltingGraphics.Fill(0x39);
            Effect.MutableBg2WorkingTilemap.Fill(0x137);
            Effect.MutableBg2ScrollByScanline.Fill(19);
            Bind("_bus", Memory);
            Bind("_vram", Vram);
            Bind("_crocomire", Actor);
            Bind("_crocomireDeath", Effect);
            Bind("_readRandomNumber", (Func<ushort>)(() => 0x1234));
        }

        private void Bind(string field, object value) => typeof(RoomEnemySystem)
            .GetField(field, PrivateInstance)!.SetValue(Enemies, value);

        internal void InitializeMap(int source) => typeof(RoomEnemySystem)
            .GetMethod("InitializeCrocomireMeltingTilemap", PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState, int, ushort>>(Enemies)(
                Actor, source, source == CrocomireMeltingArtworkAddresses.SecondTilemap
                    ? CrocomireInstructionProgramDefinitions.MeltingTwoTopRow
                    : CrocomireInstructionProgramDefinitions.MeltingOneTopRow);

        internal void Call(string method) => typeof(RoomEnemySystem).GetMethod(method, PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState>>(Enemies)(Actor);

        internal void Dissolve() => typeof(RoomEnemySystem).GetMethod("RunCrocomireMelting", PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState, SamusState?>>(Enemies)(Actor, null);
    }

    private static void AssertCrocomireEffectMechanics(CrocomireEffectFixture expected,
        CrocomireEffectFixture actual, string context)
    {
        AssertAnimationValues(expected.Actor, actual.Actor, context);
        AssertAnimationValues(expected.Effect, actual.Effect, context);
        AssertAnimationValues(expected.Enemies, actual.Enemies, context);
        for (int slot = 0; slot < expected.Enemies.Slots.Count; slot++)
            AssertAnimationValues(expected.Enemies.Slots[slot], actual.Enemies.Slots[slot], context);
        for (int slot = 0; slot < expected.Enemies.RoomSpriteObjects.Count; slot++)
            AssertAnimationValues(expected.Enemies.RoomSpriteObjects[slot], actual.Enemies.RoomSpriteObjects[slot], context);
        AssertTrue(expected.Effect.MeltingColumnHeights.SequenceEqual(actual.Effect.MeltingColumnHeights), context + " erase schedule");
        AssertTrue(expected.Effect.Bg2ScrollByScanline.SequenceEqual(actual.Effect.Bg2ScrollByScanline), context + " HDMA geometry");
        AssertSameBytes(expected.Memory.WorkRam, actual.Memory.WorkRam, context + " WRAM");
        AssertSameBytes(expected.Memory.SaveRam, actual.Memory.SaveRam, context + " SRAM");
        AssertTrue(expected.Enemies.SoundRequests.SequenceEqual(actual.Enemies.SoundRequests), context + " audio");
        AssertTrue(expected.Enemies.MusicRequests.SequenceEqual(actual.Enemies.MusicRequests), context + " music");
        AssertTrue(expected.Enemies.CrocomirePlmRequests.SequenceEqual(actual.Enemies.CrocomirePlmRequests), context + " room changes");
    }

    // Decode authored bitplanes and write a genuine indexed PNG so fixtures exercise the
    // production asset compiler, rather than injecting unvalidated private buffers.
    private static MemoryStream EffectPng(int byteCount, int usedBytes, byte pattern)
    {
        var bytes = new byte[byteCount];
        bytes.AsSpan(0, usedBytes).Fill(pattern);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(bytes, 4, RoomCharacterAtlasFormat.TileColumns,
            out int width, out int height);
        var output = new MemoryStream();
        IndexedPng.Write(output, width, height, pixels,
            Enumerable.Range(0, 16).Select(index => new Rgba32((byte)(index * 16), 0, 0)).ToArray());
        output.Position = 0;
        return output;
    }

    private static RoomCharacterAtlas CorpseEffectArtwork(byte pattern) => RoomCharacterAtlas.Load(
        EffectPng(MotherBrainCorpseArtworkDefinitions.ByteCount, MotherBrainCorpseArtworkDefinitions.ByteCount, pattern),
        MotherBrainCorpseArtworkDefinitions.ByteCount);

    private static string MeltEffectJson(int seed = 0) => JsonSerializer.Serialize(new CrocomireMeltingTilemapDocument
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

    private static CrocomireMeltingArtwork MeltEffectArtwork(byte pattern = 0x55,
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
