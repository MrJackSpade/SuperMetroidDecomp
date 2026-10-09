using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    /// <summary>Real compiled FX/animation and software pixels use only installed color data and live RAM.</summary>
    private static void VerifyPaletteContractVisibleIsolation()
    {
        var documents = PaletteJsonContracts().ToDictionary(item => item.Name, item => item.Document);
        JsonObject title = documents["title"];
        var editedTitle = (JsonObject)title.DeepClone();
        InvertPaletteContractColors(editedTitle);
        var original = ReadPaletteContract(title, json => TitlePalettePresentation.Load(json));
        var replacement = ReadPaletteContract(editedTitle, json => TitlePalettePresentation.Load(json));
        var stockBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var editedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var stockFx = new RoomPaletteFxSystem();
        var editedFx = new RoomPaletteFxSystem();
        foreach (var definition in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            stockFx.SpawnDefinition(stockBus, definition.DefinitionPointer, 0);
            editedFx.SpawnDefinition(editedBus, definition.DefinitionPointer, 0);
        }
        var stockCgram = new SnesCgram();
        var editedCgram = new SnesCgram();
        original.Apply(stockCgram); replacement.Apply(editedCgram);
        // Two complete tube-light cycles, with the independent one-frame displays
        // ticking concurrently. Compare all native instruction timers, not endpoints.
        for (int tick = 0; tick < 160; tick++)
        {
            stockFx.Step(stockBus, stockCgram, original, 0, 0, false, false);
            editedFx.Step(editedBus, editedCgram, replacement, 0, 0, false, false);
            AssertPaletteContractFxState(stockFx, editedFx);
            AssertSameBytes(stockBus.WorkRam, editedBus.WorkRam, "color replacement preserves all live WRAM");
            foreach (var definition in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
            {
                int destination = definition.ColorByteIndex / sizeof(ushort) + 1;
                AssertEqual((ushort)(stockCgram.Colors[destination] ^ 0x7fff), editedCgram.Colors[destination],
                    "exact authored color inversion reaches the compiled native destination");
                AssertPaletteContractPixels(stockCgram, editedCgram, destination);
            }
        }

        JsonObject cycle = documents["map cycle"];
        var editedCycle = (JsonObject)cycle.DeepClone();
        InvertPaletteContractColors(editedCycle);
        var stockMap = new MapPaletteAnimation(stockBus);
        var editedMap = new MapPaletteAnimation(editedBus);
        stockMap.Bind(ReadPaletteContract(cycle, MapPaletteCycle.Load));
        editedMap.Bind(ReadPaletteContract(editedCycle, MapPaletteCycle.Load));
        for (int tick = 0; tick < 90; tick++)
        {
            AssertEqual(stockMap.Step(stockCgram), editedMap.Step(editedCgram), "map cycle sound/wrap timing unchanged");
            foreach (string fieldName in new[] { "timer", "frame" })
            {
                FieldInfo field = typeof(MapPaletteAnimation).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
                AssertEqual((byte)field.GetValue(stockMap)!, (byte)field.GetValue(editedMap)!, "map cycle frame/timer unchanged");
            }
            int destination = MapAnimationRomData.PaletteDestination + 1;
            AssertEqual((ushort)(stockCgram.Colors[destination] ^ 0x7fff), editedCgram.Colors[destination],
                "map replacement changes exactly the selected color");
            AssertPaletteContractPixels(stockCgram, editedCgram, destination);
        }
    }

    /// <summary>Replaces every authored RGB5 component in a JSON palette node with its 31-minus-value inverse.</summary>
    /// <param name="node">Palette object or array to edit recursively; null nodes are ignored.</param>
    private static void InvertPaletteContractColors(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("red"))
                foreach (string channel in new[] { "red", "green", "blue" }) obj[channel] = 31 - obj[channel]!.GetValue<int>();
            else foreach (var pair in obj) InvertPaletteContractColors(pair.Value);
        }
        else if (node is JsonArray array) foreach (JsonNode? child in array) InvertPaletteContractColors(child);
    }

    /// <summary>Asserts that changing palette colors leaves FX animation, requests, and every live slot field unchanged.</summary>
    /// <param name="stock">FX system stepping with the original palette.</param>
    /// <param name="edited">FX system stepping with the edited palette.</param>
    private static void AssertPaletteContractFxState(RoomPaletteFxSystem stock, RoomPaletteFxSystem edited)
    {
        AssertAnimationValues(stock, edited, "FX state and heat phase unchanged");
        AssertTrue(stock.SoundRequests.SequenceEqual(edited.SoundRequests), "FX sound requests unchanged");
        AssertTrue(stock.MusicRequests.SequenceEqual(edited.MusicRequests), "FX music requests unchanged");
        FieldInfo field = typeof(RoomPaletteFxSystem).GetField("slots", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var stockSlots = (Array)field.GetValue(stock)!;
        var editedSlots = (Array)field.GetValue(edited)!;
        for (int slot = 0; slot < stockSlots.Length; slot++)
        {
            object stockSlot = stockSlots.GetValue(slot)!;
            object editedSlot = editedSlots.GetValue(slot)!;
            foreach (PropertyInfo property in stockSlot.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                AssertEqual(property.GetValue(stockSlot), property.GetValue(editedSlot),
                    "every native palette slot field unchanged: " + property.Name);
        }
    }

    /// <summary>Renders the selected BG or OBJ color through the real software renderer and checks both exact raster colors.</summary>
    /// <param name="stock">CGRAM containing the original palette used for the first render.</param>
    /// <param name="edited">CGRAM containing the replacement palette used for the second render.</param>
    /// <param name="color">CGRAM color index whose visible output is compared.</param>
    private static void AssertPaletteContractPixels(SnesCgram stock, SnesCgram edited, int color)
    {
        var vram = new SnesVram();
        int index = color % 16;
        AssertTrue(index != 0, "visible fixture does not confuse transparent color zero with authored color");
        byte[] tile = new byte[RoomCharacterAtlasFormat.BytesPerTile];
        for (int row = 0; row < 8; row++)
        for (int plane = 0; plane < 4; plane++)
            tile[(plane / 2) * 16 + row * 2 + plane % 2] = (index & 1 << plane) != 0 ? byte.MaxValue : (byte)0;
        vram.LoadBytes(0, tile);
        Rgba32[] stockPixels, editedPixels;
        if (color < SnesCgram.ColorCount / 2)
        {
            vram.ExecuteWordTransfer([SnesBgTilemapWord.Create(0, color / 16, priority: false)], 1024, 1);
            stockPixels = SnesBgTilemapRenderer.Render4BppViewport(vram, stock, 1024, 0, 0, 0, 8, 8, 32);
            editedPixels = SnesBgTilemapRenderer.Render4BppViewport(vram, edited, 1024, 0, 0, 0, 8, 8, 32);
        }
        else
        {
            var oam = new OamBuffer();
            oam.BeginFrame();
            oam.AddOnScreenSpritePart(new SnesSpritemapXWord(0), 0,
                SnesObjAttributeWord.Create(0, (color - SnesCgram.ColorCount / 2) / 16, 0), 0, 0);
            oam.FinalizeFrame();
            stockPixels = SnesObjRenderer.Render(oam, vram, stock, obsel: 0, width: 8, height: 8);
            editedPixels = SnesObjRenderer.Render(oam, vram, edited, obsel: 0, width: 8, height: 8);
        }
        AssertTrue(stockPixels.All(pixel => pixel == stock.GetRgba(color)), "all original raster pixels have the exact selected RGB5 color");
        AssertTrue(editedPixels.All(pixel => pixel == edited.GetRgba(color)), "all edited raster pixels have the exact replacement RGB5 color");
        AssertTrue(!stockPixels.SequenceEqual(editedPixels), "the real renderer observes the installed visual edit");
    }
}
