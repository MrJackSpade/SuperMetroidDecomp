using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static PaletteRgb5[] PaletteContractColors(int count, int seed = 0) => Enumerable.Range(0, count)
        .Select(index => new PaletteRgb5 { Red = (index + seed) % 32, Green = (index * 3 + seed) % 32,
            Blue = (index * 7 + seed) % 32 }).ToArray();

    private static PaletteRgb5[][] PaletteContractRows(int rows, int colors) => Enumerable.Range(0, rows)
        .Select(row => PaletteContractColors(colors, row)).ToArray();

    private static IEnumerable<PaletteJsonContract> PaletteJsonContracts()
    {
        yield return new("room static", PaletteContractDocument(new RoomStaticPaletteDocument
        { Version = RoomStaticPaletteFormat.Version, Colors = PaletteContractColors(RoomStaticPaletteFormat.ColorCount) }),
            json => _ = RoomStaticPalette.Load(json), CaseInsensitive: true);
        yield return new("gameplay base", PaletteContractDocument(new GameplayBasePaletteDocument(
            GameplayBasePaletteFormat.Version, PaletteContractColors(SnesCgram.ColorCount),
            PaletteContractColors(GameplayBasePaletteFormat.SpriteColorCount))),
            json => _ = GameplayBasePaletteCatalog.Load(json), CaseInsensitive: true);
        yield return new("map static", PaletteContractDocument(new MapStaticPalettesDocument
        {
            Version = MapStaticPalettesFormat.Version, Pause = PaletteContractColors(SnesCgram.ColorCount),
            FileSelect = PaletteContractColors(SnesCgram.ColorCount), World = Enum.GetValues<AreaId>()
                .Where(area => area != AreaId.Ceres).ToDictionary(area => area.ToString(), _ => PaletteContractColors(SnesCgram.ColorCount)),
        }), json => _ = MapStaticPalettes.Load(json));
        yield return new("map cycle", PaletteContractDocument(new MapPaletteCycleDocument
        {
            Version = MapPaletteCycleFormat.Version, Frames = Enumerable.Range(0, 3).Select(frame => new MapPaletteCycleFrame
            { DurationTicks = frame + 2, Colors = PaletteContractColors(MapPaletteCycleFormat.ColorCount, frame) }).ToArray(),
        }), json => _ = MapPaletteCycle.Load(json));
        yield return new("Ceres flight", PaletteContractDocument(new CeresFlightPaletteDocument
        { Version = CeresFlightPaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = CeresFlightPalette.Load(json));
        yield return new("intro", PaletteContractDocument(new IntroCinematicPaletteDocument
        { Version = IntroCinematicPaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = IntroCinematicPalette.Load(json));
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
            yield return new($"ending {id}", PaletteContractDocument(new EndingPaletteDocument
            { Version = EndingPaletteDefinitions.Version, Colors = PaletteContractColors(EndingPaletteDefinitions.ColorCount(id)) }),
                json => _ = EndingPalette.Load(json, id));
        var tube = TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
        var displays = TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays);
        yield return new("title", PaletteContractDocument(new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount),
            BabyMetroidTubeLight = PaletteContractRows(tube.FrameCount, tube.ColorsPerFrame),
            FlickeringDisplays = PaletteContractRows(displays.FrameCount, displays.ColorsPerFrame),
            SkipCopyrightWhite = PaletteContractColors(1)[0], SkipCopyrightRed = PaletteContractColors(1, 1)[0],
        }), json => _ = TitlePalettePresentation.Load(json));
        yield return new("Mother Brain health", PaletteContractDocument(new MotherBrainHealthPaletteDocument
        { Version = MotherBrainHealthPaletteFormat.Version,
            Body = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteRomData.ColorCount),
            BackLegs = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteRomData.ColorCount) }),
            json => _ = MotherBrainHealthPalettePresentation.Load(json));
        yield return new("Mother Brain room", PaletteContractDocument(new MotherBrainRoomColorDocument
        {
            Version = MotherBrainRoomColorFormat.Version,
            Flash = PaletteContractRows(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount, MotherBrainRoomColorRomData.SliceColors * 2),
            FinalRoom = PaletteContractColors(MotherBrainRoomColorRomData.SliceColors * 2),
            PhaseTwoAttack = PaletteContractColors(MotherBrainRoomColorRomData.PhaseTwoColors),
            PhaseTwoRearLeg = PaletteContractColors(MotherBrainRoomColorRomData.PhaseTwoColors),
            InitialGlassShard = PaletteContractColors(MotherBrainRoomColorRomData.InitialColors),
            InitialTubeProjectile = PaletteContractColors(MotherBrainRoomColorRomData.InitialColors),
            RecoveryLights = PaletteContractRows(MotherBrainRoomColorRomData.RecoveryLightsFrames,
                MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * 2),
        }), json => _ = MotherBrainRoomColorPresentation.Load(json));
        MotherBrainRainbowPaletteFrameDocument[] Frames(int count, int body, int legs, bool trailing) => Enumerable.Range(0, count)
            .Select(frame => new MotherBrainRainbowPaletteFrameDocument { Body = PaletteContractColors(body, frame),
                BackLegs = PaletteContractColors(legs, frame), TrailingColor = trailing ? PaletteContractColors(1, frame)[0] : null }).ToArray();
        yield return new("Mother Brain rainbow", PaletteContractDocument(new MotherBrainRainbowPaletteDocument
        {
            Version = (int)MotherBrainRainbowPaletteVersion.Current,
            Rainbow = Frames(MotherBrainRainbowPaletteFormat.RainbowFrameCount, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false),
            ToGrey = Frames(MotherBrainRainbowPaletteFormat.GreyFrameCount, MotherBrainDrainedPaletteRomData.DrainedColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true),
            FromGrey = Frames(MotherBrainRainbowPaletteFormat.GreyFrameCount, MotherBrainDrainedPaletteRomData.RevivalColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true),
            Normal = Frames(1, MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.ColorCount, false)[0],
            FakeDeathToGrey = PaletteContractRows(MotherBrainFakeDeathPaletteRomData.FrameCount, MotherBrainFakeDeathPaletteRomData.ColorCount),
            BeamInitial = PaletteContractColors(1)[0], BeamCycle = PaletteContractColors(MotherBrainRainbowPaletteFormat.BeamCycleColorCount),
        }), json => _ = MotherBrainRainbowPalettePresentation.Load(json));

        // Only the extractor sees this constructed image. No retail ROM/file is needed
        // and the imported bytes encode colors, not runtime-readable cartridge data.
        var synthetic = new CartridgeImportAddressSpace(Enumerable.Range(0, CartridgeImportAddressSpace.RetailRomByteCount)
            .Select(index => (byte)(index * 13 + 7)).ToArray());
        yield return new("room effects", (JsonObject)JsonNode.Parse(RoomPaletteFxPresentationExtractor.Extract(synthetic))!,
            json => _ = RoomPaletteFxPresentation.Load(json));
    }
}
