using SuperMetroid.Core.Assets;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsFixturesVerification;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsLegacyVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Title;

namespace SuperMetroid.Verification.Assets;

/// <summary>Checks palette JSON edits change visible color while preserving FX clocks and live state.</summary>
internal static class PaletteJsonContractsVisibleVerification
{
    /// <summary>Real compiled FX/animation and drawn pixels use only installed color data and live RAM.</summary>
    internal static void VerifyPaletteContractVisibleIsolation()
    {
        var documents = PaletteJsonContracts().ToDictionary(item => item.Name, item => item.Document);
        JsonObject title = documents["title"];
        var editedTitle = (JsonObject)title.DeepClone();
        InvertPaletteContractColors(editedTitle);
        var original = ReadPaletteContract(title, json => TitlePaletteFile.Read(json));
        var replacement = ReadPaletteContract(editedTitle, json => TitlePaletteFile.Read(json));
        var stockBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var editedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var stockFxSession = new PaletteFxSessionState();
        var stockFx = new RoomPaletteFxSystem(stockFxSession);
        var editedFxSession = new PaletteFxSessionState();
        var editedFx = new RoomPaletteFxSystem(editedFxSession);
        foreach (var definition in TitleScreenAmbientPaletteOwners.All)
        {
            stockFx.SpawnDefinition(definition.DefinitionPointer, 0);
            editedFx.SpawnDefinition(definition.DefinitionPointer, 0);
        }
        // Two complete tube-light cycles, with the independent one-frame displays
        // ticking concurrently. Compare all native instruction timers, not endpoints.
        for (int tick = 0; tick < 160; tick++)
        {
            // A frontend screen's step shows records without writing colours (#1512); the import pre-colours each record's
            // shade from the installed colours, so the inverted colours reach the shade the screen names.
            var stockShown = new List<(PaletteFxColours Colours, PaletteFxFrame Record)>();
            var editedShown = new List<(PaletteFxColours Colours, PaletteFxFrame Record)>();
            stockFx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false },
                (colours, record) => stockShown.Add((colours, record)));
            editedFx.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false },
                (colours, record) => editedShown.Add((colours, record)));
            AssertPaletteContractFxState(stockFx, editedFx);
            AssertSameBytes(stockBus.WorkRam, editedBus.WorkRam, "color replacement preserves all live WRAM");
            AssertTrue(stockShown.SequenceEqual(editedShown), "color replacement shows the same records on the same frames");
            foreach ((PaletteFxColours colours, PaletteFxFrame record) in stockShown)
            {
                SheetShade stockShade = PaletteFxScreenShades.Shade(colours, record, 0, int.MaxValue, original);
                SheetShade editedShade = PaletteFxScreenShades.Shade(colours, record, 0, int.MaxValue, replacement);
                AssertTrue(stockShade.Inks.SequenceEqual(editedShade.Inks), "an inverted record recolours the same inks");
                for (int colour = 0; colour < stockShade.Colours.Length; colour++)
                    AssertEqual((ushort)(stockShade.Colours[colour].ToWord() ^ 0x7fff), editedShade.Colours[colour].ToWord(),
                        "exact authored color inversion reaches the record's installed shade");
            }
        }

        JsonObject cycle = documents["map cycle"];
        var editedCycle = (JsonObject)cycle.DeepClone();
        InvertPaletteContractColors(editedCycle);
        var stockMap = new MapPaletteAnimation(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteScreen.Pause);
        var editedMap = new MapPaletteAnimation(SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteScreen.Pause);
        var stockHighlight = ReadPaletteContract(cycle, MapPaletteCycleFile.Read);
        var editedHighlight = ReadPaletteContract(editedCycle, MapPaletteCycleFile.Read);
        stockMap.Bind(stockHighlight.Timing);
        editedMap.Bind(editedHighlight.Timing);
        for (int tick = 0; tick < 90; tick++)
        {
            AssertEqual(stockMap.Step(), editedMap.Step(), "map cycle sound/wrap timing unchanged");
            AssertEqual(stockMap.Timer, editedMap.Timer, "map cycle frame/timer unchanged");
            AssertEqual(stockMap.Frame, editedMap.Frame, "map cycle frame/timer unchanged");
            // The highlighted sprites name the same frame's shade; the edit reaches the colours that shade is
            // pre-coloured from (#1512).
            AssertEqual(stockMap.Shading, editedMap.Shading, "map cycle names the same highlight shade");
            RgbColor stockColour = stockHighlight.Colours(stockMap.Frame)[1];
            AssertEqual(new RgbColor(31 - stockColour.Red, 31 - stockColour.Green, 31 - stockColour.Blue),
                editedHighlight.Colours(editedMap.Frame)[1], "map replacement changes exactly the selected colour");
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
}
