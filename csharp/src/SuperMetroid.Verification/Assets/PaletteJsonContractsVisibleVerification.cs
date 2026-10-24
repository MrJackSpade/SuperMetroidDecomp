using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsFixturesVerification;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsLegacyVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Assets;

/// <summary>Checks palette JSON edits change visible color while preserving FX clocks and live state.</summary>
internal static class PaletteJsonContractsVisibleVerification
{
    /// <summary>Real compiled FX/animation and drawn pixels use only installed color data and live RAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyPaletteContractVisibleIsolation()
    {
        var documents = PaletteJsonContracts().ToDictionary(item => item.Name, item => item.Document);
        JsonObject title = documents["title"];
        var editedTitle = (JsonObject)title.DeepClone();
        InvertPaletteContractColors(editedTitle);
        var original = ReadPaletteContract(title, json => TitlePalettePresentation.Load(json));
        var replacement = ReadPaletteContract(editedTitle, json => TitlePalettePresentation.Load(json));
        var stockBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var editedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var stockFxSession = new PaletteFxSessionState();
        var stockFx = new RoomPaletteFxSystem(stockFxSession);
        var editedFxSession = new PaletteFxSessionState();
        var editedFx = new RoomPaletteFxSystem(editedFxSession);
        foreach (var definition in TitleScreenAmbientPaletteFxColorDefinitions.All)
        {
            stockFx.SpawnDefinition(definition.DefinitionPointer, 0);
            editedFx.SpawnDefinition(definition.DefinitionPointer, 0);
        }
        var stockCgram = new ScreenPalette();
        var editedCgram = new ScreenPalette();
        original.Apply(stockCgram); replacement.Apply(editedCgram);
        // Two complete tube-light cycles, with the independent one-frame displays
        // ticking concurrently. Compare all native instruction timers, not endpoints.
        for (int tick = 0; tick < 160; tick++)
        {
            stockFx.Step(new ScreenPaletteFxColours(stockCgram), original, new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            editedFx.Step(new ScreenPaletteFxColours(editedCgram), replacement, new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            AssertPaletteContractFxState(stockFx, editedFx);
            AssertSameBytes(stockBus.WorkRam, editedBus.WorkRam, "color replacement preserves all live WRAM");
            foreach (var definition in TitleScreenAmbientPaletteFxColorDefinitions.All)
            {
                int destination = definition.ColorByteIndex / sizeof(ushort) + 1;
                AssertEqual((ushort)(stockCgram.Colors[destination].ToWord() ^ 0x7fff), editedCgram.Colors[destination],
                    "exact authored color inversion reaches the compiled native destination");
                AssertPaletteContractPixels(stockCgram, editedCgram, destination);
            }
        }

        JsonObject cycle = documents["map cycle"];
        var editedCycle = (JsonObject)cycle.DeepClone();
        InvertPaletteContractColors(editedCycle);
        var stockMap = new MapPaletteAnimation();
        var editedMap = new MapPaletteAnimation();
        stockMap.Bind(ReadPaletteContract(cycle, MapPaletteCycle.Load));
        editedMap.Bind(ReadPaletteContract(editedCycle, MapPaletteCycle.Load));
        for (int tick = 0; tick < 90; tick++)
        {
            AssertEqual(stockMap.Step(stockCgram), editedMap.Step(editedCgram), "map cycle sound/wrap timing unchanged");
            AssertEqual(stockMap.Timer, editedMap.Timer, "map cycle frame/timer unchanged");
            AssertEqual(stockMap.Frame, editedMap.Frame, "map cycle frame/timer unchanged");
            int destination = ScreenColourPositions.Sprite(MapScreenDefinitions.HighlightSpriteRow) + 1;
            AssertEqual((ushort)(stockCgram.Colors[destination].ToWord() ^ 0x7fff), editedCgram.Colors[destination],
                "map replacement changes exactly the selected color");
            AssertPaletteContractPixels(stockCgram, editedCgram, destination);
        }
    }

    /// <summary>Inverts authored RGB5 channels recursively while leaving non-color JSON fields intact.</summary>
    private static void InvertPaletteContractColors(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("red"))
                foreach (string channel in new[] { "red", "green", "blue" }) obj[channel] = 31 - obj.Required(channel).GetValue<int>();
            else foreach (var pair in obj) InvertPaletteContractColors(pair.Value);
        }
        else if (node is JsonArray array) foreach (JsonNode? child in array) InvertPaletteContractColors(child);
    }

    /// <summary>Compares palette FX slots, animation fields, requests, and heat phase after color edits.</summary>
    private static void AssertPaletteContractFxState(RoomPaletteFxSystem stock, RoomPaletteFxSystem edited)
    {
        AssertAnimationValues(stock, edited, "FX state and heat phase unchanged");
        AssertTrue(stock.SoundRequests.SequenceEqual(edited.SoundRequests), "FX sound requests unchanged");
        AssertTrue(stock.MusicRequests.SequenceEqual(edited.MusicRequests), "FX music requests unchanged");
        var stockSlots = (Array)stock.slots;
        var editedSlots = (Array)edited.slots;
        for (int slot = 0; slot < stockSlots.Length; slot++)
        {
            object stockSlot = (stockSlots.GetValue(slot) ?? throw new InvalidOperationException("Verification requires stockSlots.GetValue(slot)."));
            object editedSlot = (editedSlots.GetValue(slot) ?? throw new InvalidOperationException("Verification requires editedSlots.GetValue(slot)."));
            foreach (PropertyInfo property in stockSlot.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                AssertEqual(property.GetValue(stockSlot), property.GetValue(editedSlot),
                    "every native palette slot field unchanged: " + property.Name);
        }
    }

    /// <summary>
    /// Loads both palettes and observes the exact RGB5 replacement: a background colour as the loaded colour itself, a
    /// sprite colour as the pixels the display renderer draws of a sprite whose characters select it.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void AssertPaletteContractPixels(ScreenPalette stockColours, ScreenPalette editedColours, int color)
    {
        var stock = new SnesCgram();
        new CgramPalettes(stock).LoadImage(stockColours.Colors);
        var edited = new SnesCgram();
        new CgramPalettes(edited).LoadImage(editedColours.Colors);
        int index = color % 16;
        AssertTrue(index != 0, "visible fixture does not confuse transparent color zero with authored color");
        if (color < SnesCgram.ColorCount / 2)
        {
            AssertTrue(stock.Colors[color] == stockColours.Colors[color], "the original background colour is loaded exactly");
            AssertTrue(edited.Colors[color] == editedColours.Colors[color], "the edited background colour is loaded exactly");
            AssertTrue(stock.Colors[color] != edited.Colors[color], "the loaded palette carries the installed visual edit");
            return;
        }
        var vram = new SnesVram();
        byte[] tile = new byte[RoomCharacterAtlasFormat.BytesPerTile];
        for (int row = 0; row < 8; row++)
        for (int plane = 0; plane < 4; plane++)
            tile[(plane / 2) * 16 + row * 2 + plane % 2] = (index & 1 << plane) != 0 ? byte.MaxValue : (byte)0;
        vram.LoadBytes(0, tile);
        var oam = new OamBuffer();
        oam.BeginFrame();
        oam.AddOnScreenSpritePart(new SnesSpritemapXWord(0), new SnesSpritemapYByte(0),
            SnesObjAttributeWord.Create(0, (color - SnesCgram.ColorCount / 2) / 16, 0), new Position16<OamPixels>(0, 0));
        oam.FinalizeFrame();
        Rgba32[] stockPixels = SpritePixels(FramePaintConstants.CaptureFrontend(vram, stock, oam, objectSelection: 0));
        Rgba32[] editedPixels = SpritePixels(FramePaintConstants.CaptureFrontend(vram, edited, oam, objectSelection: 0));
        AssertTrue(stockPixels.All(pixel => pixel == stock.GetRgba(color)), "all original raster pixels have the exact selected RGB5 color");
        AssertTrue(editedPixels.All(pixel => pixel == edited.GetRgba(color)), "all edited raster pixels have the exact replacement RGB5 color");
        AssertTrue(!stockPixels.SequenceEqual(editedPixels), "the real renderer observes the installed visual edit");

        // The 8x8 sprite's pixels at the screen's top-left corner, as the display renderer draws them.
        static Rgba32[] SpritePixels(FramePaint paint)
        {
            Rgba32[] frame = DisplayRendering.Render(new LayeredScene(paint, new RenderLayer[] { new ObjRenderLayer() }, FadeLevel.Full));
            return [.. Enumerable.Range(0, 64).Select(pixel => frame[pixel / 8 * SnesPpuLayout.ScreenSize.X + pixel % 8])];
        }
    }
}
