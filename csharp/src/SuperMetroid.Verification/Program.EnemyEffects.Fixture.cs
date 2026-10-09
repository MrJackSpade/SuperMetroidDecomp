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
        /// <summary>Reflection flags used to bind private runtime dependencies into an isolated fixture.</summary>
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        /// <summary>Cartridge-free address space supplied to the enemy system.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>VRAM state used by effect upload and draw operations.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Enemy system whose private runtime dependencies are bound to this fixture.</summary>
        internal RoomEnemySystem Enemies { get; } = new();
        /// <summary>Crocomire actor occupying the body and tongue fixture slots.</summary>
        internal CrocomireEnemyState Actor { get; }
        /// <summary>Death-effect state initialized for deterministic melting operations.</summary>
        internal CrocomireDeathState Effect { get; } = new();

        /// <summary>Creates isolated Crocomire effect state and binds the bus, VRAM, actor, and deterministic random source.</summary>
        /// <param name="art">Optional melting artwork to install in the enemy system before fixture setup.</param>
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

        /// <summary>Assigns a private enemy-system dependency so effect code uses fixture-owned state.</summary>
        /// <param name="field">Private instance field name on <see cref="RoomEnemySystem"/>.</param>
        /// <param name="value">Fixture-owned dependency assigned to that field.</param>
        private void Bind(string field, object value) => typeof(RoomEnemySystem)
            .GetField(field, PrivateInstance)!.SetValue(Enemies, value);

        /// <summary>Invokes Crocomire's melt-tilemap initializer using the list corresponding to the selected source map.</summary>
        /// <param name="source">Melting tilemap source address passed to the runtime initializer.</param>
        internal void InitializeMap(int source) => typeof(RoomEnemySystem)
            .GetMethod("InitializeCrocomireMeltingTilemap", PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState, int, ushort>>(Enemies)(
                Actor, source, source == CrocomireMeltingArtworkAddresses.SecondTilemap
                    ? CrocomireInstructionProgramDefinitions.MeltingTwoTopRow
                    : CrocomireInstructionProgramDefinitions.MeltingOneTopRow);

        /// <summary>Invokes a private single-actor enemy-system method against the fixture actor.</summary>
        /// <param name="method">Name of the private method to invoke.</param>
        internal void Call(string method) => typeof(RoomEnemySystem).GetMethod(method, PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState>>(Enemies)(Actor);

        /// <summary>Runs one Crocomire melting update without a Samus actor.</summary>
        internal void Dissolve() => typeof(RoomEnemySystem).GetMethod("RunCrocomireMelting", PrivateInstance)!
            .CreateDelegate<Action<CrocomireEnemyState, SamusState?>>(Enemies)(Actor, null);
    }

    /// <summary>Compares animation state, WRAM/SRAM, audio requests, and room-change requests between isolated Crocomire effect runs.</summary>
    /// <param name="expected">Reference fixture whose observed state is compared.</param>
    /// <param name="actual">Fixture state required to match the reference.</param>
    /// <param name="context">Label appended to assertion failures to identify the compared operation.</param>
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
    /// <summary>Builds an indexed PNG from synthetic SNES planar tile bytes.</summary>
    /// <param name="byteCount">Length of the planar input buffer.</param>
    /// <param name="usedBytes">Prefix length filled with the requested test pattern before decoding.</param>
    /// <param name="pattern">Byte value used to distinguish generated fixture artwork.</param>
    /// <returns>A rewound stream containing the decoded indexed PNG.</returns>
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

    /// <summary>Loads synthetic indexed art using the byte size and atlas validation for the corpse effect.</summary>
    /// <param name="pattern">Planar byte pattern used to produce distinguishable artwork.</param>
    /// <returns>The decoded room character atlas for the corpse effect.</returns>
    private static RoomCharacterAtlas CorpseEffectArtwork(byte pattern) => RoomCharacterAtlas.Load(
        EffectPng(MotherBrainCorpseArtworkDefinitions.ByteCount, MotherBrainCorpseArtworkDefinitions.ByteCount, pattern),
        MotherBrainCorpseArtworkDefinitions.ByteCount);

    /// <summary>Serializes a correctly sized melting tilemap document with deterministic seed-based cell attributes.</summary>
    /// <param name="seed">Value used to vary tile indices and palette/render flags in the fixture document.</param>
    /// <returns>JSON using the camel-case property names expected by the artwork loader.</returns>
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

    /// <summary>Loads synthetic paired melting graphics and tilemaps through the production artwork decoder.</summary>
    /// <param name="pattern">Byte pattern used for both graphics transfers.</param>
    /// <param name="seed">Seed for generated tilemap cell values when no custom first map is supplied.</param>
    /// <param name="firstJson">Optional JSON replacing the generated first tilemap, useful for loader edge cases.</param>
    /// <returns>Validated Crocomire melting artwork ready for installation in the fixture enemy system.</returns>
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
