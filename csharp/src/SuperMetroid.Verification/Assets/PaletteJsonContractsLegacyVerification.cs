using SuperMetroid.Tooling;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Verification.Assets;

/// <summary>Checks stock-assisted migration of older room, Mother Brain, and rainbow palette JSON.</summary>
internal static class PaletteJsonContractsLegacyVerification
{
    /// <summary>Loads an in-memory palette document through its production stream parser.</summary>
    internal static T ReadPaletteContract<T>(JsonObject document, Func<Stream, T> load)
    {
        using var json = new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()), writable: false);
        return load(json);
    }

    /// <summary>Strict field validation must not delete the supported stock-assisted migrations.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyPaletteContractLegacyOverrides()
    {
        Dictionary<string, JsonObject> documents = PaletteJsonContracts().ToDictionary(item => item.Name, item => item.Document);
        JsonObject effects = documents["room effects"];
        var effectsStock = ReadPaletteContract(effects, json => RoomPaletteFxPresentation.Load(json));
        var oldEffects = (JsonObject)effects.DeepClone();
        oldEffects["version"] = RoomPaletteFxPresentationFormat.PreviousVersion;
        foreach (string field in new[] { "samusHeatPowerSuit", "samusHeatVariaSuit", "samusHeatGravitySuit" }) oldEffects.Remove(field);
        var editedColor = (JsonObject)oldEffects.Required("norfairForegroundPalette4").Required(0).Required(0);
        editedColor["red"] = (editedColor.Required("red").GetValue<int>() + 1) % 32;
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
            ((JsonObject)oldRoom.Required("finalRoom").Required(0))["red"] = 31;
            AssertThrows<InvalidDataException>(() => ReadPaletteContract(oldRoom, json => MotherBrainRoomColorPresentation.Load(json)),
                "old Mother Brain room colors require current stock");
            var migrated = ReadPaletteContract(oldRoom, json => MotherBrainRoomColorPresentation.Load(json, roomStock));
            var actual = new SnesCgram();
            var expected = new SnesCgram();
            migrated.ApplyRoomEntry(actual); roomStock.ApplyRoomEntry(expected);
            AssertTrue(new CgramPalettes(actual).ImageEquals(new CgramPalettes(expected)), "legacy inherits exact room-entry colors");
            for (int frame = 0; frame < MotherBrainRoomColorRomData.RecoveryLightsFrames; frame++)
            {
                migrated.ApplyRecoveryLights(actual, frame); roomStock.ApplyRecoveryLights(expected, frame);
                AssertTrue(new CgramPalettes(actual).ImageEquals(new CgramPalettes(expected)), "legacy inherits exact room recovery colors");
            }
            migrated.ApplyFinal(actual);
            AssertEqual(PackPaletteContractColor((JsonObject)oldRoom.Required("finalRoom").Required(0)),
                actual.Colors[MotherBrainRoomColorRomData.FirstColor], "legacy retains edited final-room colors");
        }

        JsonObject rainbow = documents["Mother Brain rainbow"];
        var rainbowStock = ReadPaletteContract(rainbow, json => MotherBrainRainbowPalettePresentation.Load(json));
        var oldRainbow = (JsonObject)rainbow.DeepClone();
        oldRainbow["version"] = (int)MotherBrainRainbowPaletteVersion.PreFakeDeath;
        oldRainbow.Remove("fakeDeathToGrey");
        ((JsonObject)oldRainbow.Required("normal").Required("body").Required(0))["red"] = 31;
        AssertThrows<InvalidDataException>(() => ReadPaletteContract(oldRainbow, json => MotherBrainRainbowPalettePresentation.Load(json)),
            "old Mother Brain rainbow colors require current stock");
        var migratedRainbow = ReadPaletteContract(oldRainbow, json => MotherBrainRainbowPalettePresentation.Load(json, rainbowStock));
        var legacyCgram = new SnesCgram();
        var stockCgram = new SnesCgram();
        for (int frame = 0; frame < MotherBrainFakeDeathPaletteRomData.FrameCount; frame++)
        {
            migratedRainbow.ApplyFakeDeathToGrey(legacyCgram, frame); rainbowStock.ApplyFakeDeathToGrey(stockCgram, frame);
            AssertTrue(new CgramPalettes(legacyCgram).ImageEquals(new CgramPalettes(stockCgram)), "legacy inherits exact fake-death colors");
        }
        migratedRainbow.ApplyNormal(legacyCgram);
        AssertEqual(PackPaletteContractColor((JsonObject)oldRainbow.Required("normal").Required("body").Required(0)),
            legacyCgram.Colors[MotherBrainRainbowPaletteRomData.BodyColor], "legacy retains edited normal colors");
    }

    /// <summary>Packs JSON red, green, and blue channels into the cartridge's 15-bit color word.</summary>
    private static ushort PackPaletteContractColor(JsonObject color) => (ushort)(color.Required("red").GetValue<int>() |
        color.Required("green").GetValue<int>() << 5 | color.Required("blue").GetValue<int>() << 10);
}
