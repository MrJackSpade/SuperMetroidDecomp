using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Assets;

/// <summary>Constructs valid palette JSON baselines for the shared schema-mutation checks.</summary>
internal static class PaletteJsonContractsFixturesVerification
{
    /// <summary>Uses bounded five-bit channels with distinct patterns to expose misplaced color values.</summary>
    private static PaletteRgb5[] PaletteContractColors(int count, int seed = 0) => Enumerable.Range(0, count)
        .Select(index => new PaletteRgb5 { Red = (index + seed) % 32, Green = (index * 3 + seed) % 32,
            Blue = (index * 7 + seed) % 32 }).ToArray();

    /// <summary>Gives each animation row a different seed while retaining the requested native row width.</summary>
    private static PaletteRgb5[][] PaletteContractRows(int rows, int colors) => Enumerable.Range(0, rows)
        .Select(row => PaletteContractColors(colors, row)).ToArray();

    /// <summary>Supplies one valid document per supported palette family, including native frame and CGRAM dimensions.</summary>
    internal static IEnumerable<PaletteJsonContract> PaletteJsonContracts()
    {
        yield return new("room static", PaletteContractDocument(new RoomStaticPaletteDocument
        { Version = RoomStaticPaletteFormat.Version, Colors = PaletteContractColors(RoomStaticPaletteFormat.ColorCount) }),
            json => _ = RoomStaticPaletteFile.Read(json), CaseInsensitive: true);
        yield return new("gameplay base", PaletteContractDocument(new GameplayBasePaletteDocument(
            GameplayBasePaletteFormat.Version, PaletteContractColors(SnesCgram.ColorCount),
            PaletteContractColors(GameplayBasePaletteFormat.SpriteColorCount))),
            json => _ = GameplayBasePaletteFiles.Read(json), CaseInsensitive: true);
        yield return new("map static", PaletteContractDocument(new MapStaticPalettesDocument
        {
            Version = MapStaticPalettesFormat.Version, Pause = PaletteContractColors(SnesCgram.ColorCount),
            FileSelect = PaletteContractColors(SnesCgram.ColorCount), World = Enum.GetValues<AreaId>()
                .Where(area => area != AreaId.Ceres).ToDictionary(area => area.ToString(), _ => PaletteContractColors(SnesCgram.ColorCount)),
        }), json => _ = MapStaticPalettesFile.Read(json));
        yield return new("map cycle", PaletteContractDocument(new MapPaletteCycleDocument
        {
            Version = MapPaletteCycleFormat.Version, Frames = Enumerable.Range(0, 3).Select(frame => new MapPaletteCycleFrame
            { DurationTicks = frame + 2, Colors = PaletteContractColors(MapPaletteCycleFormat.ColorCount, frame) }).ToArray(),
        }), json => _ = MapPaletteCycleFile.Read(json));
        yield return new("Ceres flight", PaletteContractDocument(new CeresFlightPaletteDocument
        { Version = CeresFlightPaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = CeresFlightPaletteFile.Read(json));
        yield return new("intro", PaletteContractDocument(new IntroCinematicPaletteDocument
        { Version = IntroCinematicPaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = IntroCinematicPaletteFile.Read(json));
        foreach (EndingPaletteId id in Enum.GetValues<EndingPaletteId>())
            yield return new($"ending {id}", PaletteContractDocument(new EndingPaletteDocument
            { Version = EndingPaletteDefinitions.Version, Colors = PaletteContractColors(EndingPaletteFiles.ColorCount(id)) }),
                json => _ = EndingPaletteFile.Read(json, id));
        var tube = TitleScreenAmbientPaletteFxColorDefinitions.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
        var displays = TitleScreenAmbientPaletteFxColorDefinitions.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays);
        yield return new("title", PaletteContractDocument(new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount),
            BabyMetroidTubeLight = PaletteContractRows(tube.FrameCount, tube.ColorsPerFrame),
            FlickeringDisplays = PaletteContractRows(displays.FrameCount, displays.ColorsPerFrame),
            SkipCopyrightWhite = PaletteContractColors(1)[0], SkipCopyrightRed = PaletteContractColors(1, 1)[0],
        }), json => _ = TitlePaletteFile.Read(json));
        yield return new("Mother Brain health", PaletteContractDocument(new MotherBrainHealthPaletteDocument
        { Version = MotherBrainHealthPaletteFormat.Version,
            Body = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteDefinitions.ColorCount),
            BackLegs = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteDefinitions.ColorCount) }),
            json => _ = MotherBrainHealthPaletteFile.Read(json));
        yield return new("Mother Brain room", PaletteContractDocument(new MotherBrainRoomColorDocument
        {
            Version = MotherBrainRoomColorFormat.Version,
            Flash = PaletteContractRows(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount, MotherBrainRoomColorDefinitions.SliceColors * 2),
            FinalRoom = PaletteContractColors(MotherBrainRoomColorDefinitions.SliceColors * 2),
            PhaseTwoAttack = PaletteContractColors(MotherBrainRoomColorDefinitions.PhaseTwoColors),
            PhaseTwoRearLeg = PaletteContractColors(MotherBrainRoomColorDefinitions.PhaseTwoColors),
            InitialGlassShard = PaletteContractColors(MotherBrainRoomColorDefinitions.InitialColors),
            InitialTubeProjectile = PaletteContractColors(MotherBrainRoomColorDefinitions.InitialColors),
            RecoveryLights = PaletteContractRows(MotherBrainRoomColorDefinitions.RecoveryLightsFrames,
                MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors * 2),
        }), json => _ = MotherBrainRoomColorFile.Read(json));
        MotherBrainRainbowPaletteFrameDocument[] Frames(int count, int body, int legs, bool trailing) => Enumerable.Range(0, count)
            .Select(frame => new MotherBrainRainbowPaletteFrameDocument { Body = PaletteContractColors(body, frame),
                BackLegs = PaletteContractColors(legs, frame), TrailingColor = trailing ? PaletteContractColors(1, frame)[0] : null }).ToArray();
        yield return new("Mother Brain rainbow", PaletteContractDocument(new MotherBrainRainbowPaletteDocument
        {
            Version = (int)MotherBrainRainbowPaletteVersion.Current,
            Rainbow = Frames(MotherBrainRainbowPaletteFormat.RainbowFrameCount, MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, false),
            ToGrey = Frames(MotherBrainRainbowPaletteFormat.GreyFrameCount, MotherBrainDrainedPaletteDefinitions.DrainedColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, true),
            FromGrey = Frames(MotherBrainRainbowPaletteFormat.GreyFrameCount, MotherBrainDrainedPaletteDefinitions.RevivalColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, true),
            Normal = Frames(1, MotherBrainRainbowPaletteDefinitions.ColorCount, MotherBrainRainbowPaletteDefinitions.ColorCount, false)[0],
            FakeDeathToGrey = PaletteContractRows(MotherBrainFakeDeathPaletteDefinitions.FrameCount, MotherBrainFakeDeathPaletteDefinitions.ColorCount),
            BeamInitial = PaletteContractColors(1)[0], BeamCycle = PaletteContractColors(MotherBrainRainbowPaletteFormat.BeamCycleColorCount),
        }), json => _ = MotherBrainRainbowPaletteFile.Read(json));

        // Only the extractor sees this constructed image. No retail ROM/file is needed
        // and the imported bytes encode colors, not runtime-readable cartridge data.
        var synthetic = new CartridgeImportAddressSpace(Enumerable.Range(0, CartridgeImportAddressSpace.RetailRomByteCount)
            .Select(index => (byte)(index * 13 + 7)).ToArray());
        yield return new("room effects", (JsonObject)RequiredJson.Parse(RoomPaletteFxPresentationExtractor.Extract(synthetic)),
            json => _ = RoomPaletteFxPresentationFile.Read(json));
    }
}
