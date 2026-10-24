using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Rooms;

/// <summary>Constructs cartridge-free enemy-effect artwork and Crocomire state fixtures.</summary>
internal static class EnemyEffectsFixtureVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Known melt phases, pure RAM and authored PNG/JSON; no cartridge capability.</summary>
    internal sealed class CrocomireEffectFixture
    {
        /// <summary>Cartridge-free WRAM and SRAM used to detect unintended mutation.</summary>
        internal SuperMetroidAddressSpace Memory { get; } = SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Destination graphics memory checked after melt operations.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Enemy owner hosting the Crocomire body, tongue, and behavior.</summary>
        internal RoomEnemySystem Enemies { get; } = new(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new VerificationRoomEnemyServices(null));
        /// <summary>Crocomire body state bound to the first two enemy slots.</summary>
        internal CrocomireEnemyState Actor { get; }
        /// <summary>Seeded melt phase with recognizable graphics and BG2 state.</summary>
        internal CrocomireDeathState Effect { get; } = new();

        /// <summary>The frame band <paramref name="band"/>'s animation shows in the room, or null before any pass shows it.</summary>
        internal ArtworkFrame? MeltBand(int band) =>
            Enemies.Presentation.ArtworkFrames.TryShown(CrocomireMelting.MeltBandAnimation(band), out ArtworkFrame frame) ? frame : null;

        /// <summary>The frame band <paramref name="band"/> shows <paramref name="picture"/> with, or null for no picture.</summary>
        internal static ArtworkFrame? MeltBandFrame(CrocomireMeltBandPicture? picture, int band) =>
            picture is { } shown ? CrocomireMelting.MeltBandFrame(shown, band) : null;

        /// <summary>Seeds a reproducible Crocomire melt state with optional validated artwork.</summary>
        internal CrocomireEffectFixture(CrocomireMeltingArtwork? art = null)
        {
            if (art is not null)
                Enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() });
                Enemies.BossArtwork = new BossArtworkCatalog { CrocomireMelting = art };
            Actor = new CrocomireEnemyState(Enemies.SlotTable.Slots[0]) { Tongue = Enemies.SlotTable.Slots[1] };
            var tongueState = new CrocomireTongueState();
            Enemies.SlotTable.Slots[1].SetSpeciesState(tongueState, tongueState.ClearWithRecord);
            Actor.Body.Position = Actor.Body.Position.WithWhole(new(400, 144));
            Actor.Tongue.Position = Actor.Tongue.Position.WithWholeY(160);
            Effect.AcidSmokeTimer = WrappingCountdown.Start(1);
            Effect.MutableBg2WorkingTilemap.Fill(0x137);
            Effect.MutableBg2ScrollByScanline.Fill(19);
            Enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(Vram));
            // The fight's initialization attaches the cleared BG2 image the melting body draws into.
            Enemies.Presentation.AttachBg2Image(new EnemyBg2Image(CrocomireBg2.CrocomireBlankBg2Tile));
            Enemies.CrocomireBehavior.Access.Crocomire = Actor;
            Enemies.CrocomireBehavior.Access.CrocomireDeath = Effect;
            Enemies.VerificationServices().ReadRandom = () => 0x1234;
        }

        /// <summary>
        /// Loads one melting tilemap through the death-sequence phase that owns it. Any other source exercises the
        /// production tilemap resolution, which rejects it before a phase advances or an actor changes.
        /// </summary>
        internal void InitializeMap(int source)
        {
            var address = (CrocomireMeltingTilemapAddress)source;
            if (address == CrocomireMeltingTilemapAddress.FirstTilemap)
                Enemies.CrocomireBehavior.Melting.LoadFirstCrocomireMeltingTilemap(Actor);
            else if (address == CrocomireMeltingTilemapAddress.SecondTilemap)
                Enemies.CrocomireBehavior.Melting.LoadSecondCrocomireMeltingTilemap(Actor);
            else
                _ = Enemies.CrocomireBehavior.Melting.RequireCrocomireMeltingTilemap(address);
        }

        /// <summary>Runs one Crocomire melting step, selected from the fixture's Crocomire behavior, on its actor.</summary>
        internal void Call(Func<CrocomireBehavior, Action<CrocomireEnemyState>> select) => select(Enemies.CrocomireBehavior)(Actor);

        /// <summary>Advances the production melting behavior for the bound Crocomire actor.</summary>
        internal void Dissolve() => new Action<CrocomireEnemyState, SamusState?>(Enemies.CrocomireBehavior.Melting.RunCrocomireMelting)(Actor, null);
    }

    /// <summary>Compares actor, effect, slots, RAM, audio, and PLM requests after a melt operation.</summary>
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
        for (int band = 0; band < CrocomireMeltingDefinitions.SecondPassBandCount; band++)
            AssertEqual(expected.MeltBand(band), actual.MeltBand(band), context + " band " + band + " picture");
        AssertTrue(expected.Effect.Bg2ScrollByScanline.SequenceEqual(actual.Effect.Bg2ScrollByScanline), context + " HDMA geometry");
        AssertSameBytes(expected.Memory.WorkRam, actual.Memory.WorkRam, context + " WRAM");
        AssertTrue(expected.Enemies.Audio.SoundRequests.SequenceEqual(actual.Enemies.Audio.SoundRequests), context + " audio");
        AssertTrue(expected.Enemies.Audio.MusicRequests.SequenceEqual(actual.Enemies.Audio.MusicRequests), context + " music");
        AssertTrue(expected.Enemies.LastEnemyFrame.RoomEffects.CrocomirePlms.SequenceEqual(actual.Enemies.LastEnemyFrame.RoomEffects.CrocomirePlms), context + " room changes");
    }

    /// <summary>Encodes patterned planar tiles as a real indexed PNG for the production asset compiler.</summary>
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

    /// <summary>Compiles a patterned full-size Mother Brain corpse sheet through the PNG loader.</summary>
    internal static RoomCharacterAtlas CorpseEffectArtwork(byte pattern) => RoomCharacterAtlasPng.Read(
        EffectPng(MotherBrainCorpseArtworkDefinitions.ByteCount, MotherBrainCorpseArtworkDefinitions.ByteCount, pattern),
        MotherBrainCorpseArtworkDefinitions.ByteCount);

    /// <summary>Creates a valid Crocomire tilemap document with seed-dependent cell attributes.</summary>
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
    }, CamelCaseJson);

    /// <summary>Compiles both Crocomire PNG passes and tilemap documents through the production loader.</summary>
    internal static CrocomireMeltingArtwork MeltEffectArtwork(byte pattern = 0x55,
        int seed = 0, string? firstJson = null)
    {
        using var first = EffectPng(CrocomireMeltingArtworkFormat.FirstByteCount,
            CrocomireMeltingArtwork.UsedByteCount(CrocomireMeltingIllustration.First), pattern);
        using var second = EffectPng(CrocomireMeltingArtworkFormat.SecondByteCount,
            CrocomireMeltingArtwork.UsedByteCount(CrocomireMeltingIllustration.Second), pattern);
        using var firstMap = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(firstJson ?? MeltEffectJson(seed)));
        using var secondMap = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(MeltEffectJson(seed)));
        return CrocomireMeltingArtwork.Load(RoomCharacterAtlasPng.Read(first, CrocomireMeltingArtworkFormat.FirstByteCount),
            RoomCharacterAtlasPng.Read(second, CrocomireMeltingArtworkFormat.SecondByteCount), firstMap, secondMap);
    }
}
