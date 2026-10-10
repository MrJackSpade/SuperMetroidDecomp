using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static T ReadPaletteContract<T>(JsonObject document, Func<Stream, T> load)
    {
        using var json = new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()), writable: false);
        return load(json);
    }

    /// <summary>Strict field validation must not delete the supported stock-assisted migrations.</summary>
    private static void VerifyPaletteContractLegacyOverrides()
    {
        Dictionary<string, JsonObject> documents = PaletteJsonContracts().ToDictionary(item => item.Name, item => item.Document);
        JsonObject effects = documents["room effects"];
        var effectsStock = ReadPaletteContract(effects, json => RoomPaletteFxPresentation.Load(json));
        var oldEffects = (JsonObject)effects.DeepClone();
        oldEffects["version"] = RoomPaletteFxPresentationFormat.PreviousVersion;
        foreach (string field in new[] { "samusHeatPowerSuit", "samusHeatVariaSuit", "samusHeatGravitySuit" }) oldEffects.Remove(field);
        var editedColor = (JsonObject)oldEffects["norfairForegroundPalette4"]![0]![0]!;
        editedColor["red"] = (editedColor["red"]!.GetValue<int>() + 1) % 32;
        AssertThrows<InvalidDataException>(() => ReadPaletteContract(oldEffects, json => RoomPaletteFxPresentation.Load(json)),
            "v17 room effects require explicitly installed current stock");
        var migratedEffects = ReadPaletteContract(oldEffects, json => RoomPaletteFxPresentation.Load(json, effectsStock));
        ushort editedPointer = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(
            item => item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundPalette4).ColorPointer(0, 0);
        AssertTrue(migratedEffects.TryReadColor(editedPointer, out Bgr555 edited), "legacy Norfair colors remain installed");
        AssertEqual(PackPaletteContractColor(editedColor), edited, "v17 keeps existing environmental override");
        foreach (var heat in PaletteFxHeatProgramMechanicsDefinitions.All)
        foreach (var frame in heat.Frames)
        for (int color = 0; color < PaletteFxHeatProgramDefinition.ColorsPerFrame; color++)
        {
            ushort pointer = unchecked((ushort)(frame.FirstColorPointer + color * sizeof(ushort)));
            AssertTrue(effectsStock.TryReadColor(pointer, out Bgr555 expected), "current heat color exists");
            AssertTrue(migratedEffects.TryReadColor(pointer, out Bgr555 actual), "legacy inherits only new heat color");
            AssertEqual(expected, actual, "exact inherited heat colors");
        }

        JsonObject room = documents["Mother Brain room"];
        var roomStock = ReadPaletteContract(room, json => MotherBrainRoomColorPresentation.Load(json));
        foreach (int version in new[] { MotherBrainRoomColorFormat.PreRoomEntryVersion, MotherBrainRoomColorFormat.PreRecoveryLightsVersion })
        {
            var oldRoom = (JsonObject)room.DeepClone();
            oldRoom["version"] = version;
            oldRoom.Remove("recoveryLights");
            if (version == MotherBrainRoomColorFormat.PreRoomEntryVersion)
            { oldRoom.Remove("initialGlassShard"); oldRoom.Remove("initialTubeProjectile"); }
            ((JsonObject)oldRoom["finalRoom"]![0]!)["red"] = 31;
            AssertThrows<InvalidDataException>(() => ReadPaletteContract(oldRoom, json => MotherBrainRoomColorPresentation.Load(json)),
                "old Mother Brain room colors require current stock");
            var migrated = ReadPaletteContract(oldRoom, json => MotherBrainRoomColorPresentation.Load(json, roomStock));
            var actual = new SnesCgram();
            var expected = new SnesCgram();
            migrated.ApplyRoomEntry(actual); roomStock.ApplyRoomEntry(expected);
            AssertTrue(actual.Colors.SequenceEqual(expected.Colors), "legacy inherits exact room-entry colors");
            for (int frame = 0; frame < MotherBrainRoomColorRomData.RecoveryLightsFrames; frame++)
            {
                migrated.ApplyRecoveryLights(actual, frame); roomStock.ApplyRecoveryLights(expected, frame);
                AssertTrue(actual.Colors.SequenceEqual(expected.Colors), "legacy inherits exact room recovery colors");
            }
            migrated.ApplyFinal(actual);
            AssertEqual(PackPaletteContractColor((JsonObject)oldRoom["finalRoom"]![0]!),
                actual.Colors[MotherBrainRoomColorRomData.FirstColor], "legacy retains edited final-room colors");
        }

        JsonObject rainbow = documents["Mother Brain rainbow"];
        var rainbowStock = ReadPaletteContract(rainbow, json => MotherBrainRainbowPalettePresentation.Load(json));
        var oldRainbow = (JsonObject)rainbow.DeepClone();
        oldRainbow["version"] = MotherBrainRainbowPaletteFormat.PreFakeDeathVersion;
        oldRainbow.Remove("fakeDeathToGrey");
        ((JsonObject)oldRainbow["normal"]!["body"]![0]!)["red"] = 31;
        AssertThrows<InvalidDataException>(() => ReadPaletteContract(oldRainbow, json => MotherBrainRainbowPalettePresentation.Load(json)),
            "old Mother Brain rainbow colors require current stock");
        var migratedRainbow = ReadPaletteContract(oldRainbow, json => MotherBrainRainbowPalettePresentation.Load(json, rainbowStock));
        var legacyCgram = new SnesCgram();
        var stockCgram = new SnesCgram();
        for (int frame = 0; frame < MotherBrainFakeDeathPaletteRomData.FrameCount; frame++)
        {
            migratedRainbow.ApplyFakeDeathToGrey(legacyCgram, frame); rainbowStock.ApplyFakeDeathToGrey(stockCgram, frame);
            AssertTrue(legacyCgram.Colors.SequenceEqual(stockCgram.Colors), "legacy inherits exact fake-death colors");
        }
        migratedRainbow.ApplyNormal(legacyCgram);
        AssertEqual(PackPaletteContractColor((JsonObject)oldRainbow["normal"]!["body"]![0]!),
            legacyCgram.Colors[MotherBrainRainbowPaletteRomData.BodyColor], "legacy retains edited normal colors");
    }

    private static ushort PackPaletteContractColor(JsonObject color) => (ushort)(color["red"]!.GetValue<int>() |
        color["green"]!.GetValue<int>() << 5 | color["blue"]!.GetValue<int>() << 10);
}
