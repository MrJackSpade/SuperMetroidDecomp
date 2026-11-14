using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Assets.PaletteJsonContractsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
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
            FileSelect = PaletteContractColors(SnesCgram.ColorCount), World = AreaIds.All
                .Where(area => area != AreaId.Ceres).ToDictionary(area => area.ToString(), _ => PaletteContractColors(SnesCgram.ColorCount)),
        }), json => _ = MapStaticPalettesFile.Read(json));
        yield return new("map cycle", PaletteContractDocument(new MapPaletteCycleDocument
        {
            Version = MapPaletteCycleFile.Version, Frames = Enumerable.Range(0, 3).Select(frame => new MapPaletteCycleFrame
            { DurationTicks = frame + 2, Highlight = PaletteContractColors(1, frame)[0] }).ToArray(),
        }), json => _ = MapPaletteCycleFile.Read(json));
        yield return new("Ceres flight", PaletteContractDocument(new CeresFlightPaletteDocument
        { Version = CeresFlightPaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = CeresFlightPaletteFile.Read(json));
        yield return new("intro", PaletteContractDocument(new IntroCinematicPaletteDocument
        { Version = IntroCinematicPaletteFile.Version, Colors = PaletteContractColors(SnesCgram.ColorCount) }),
            json => _ = IntroCinematicPaletteFile.Read(json));
        var tube = TitleScreenAmbientPaletteOwners.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
        var displays = TitleScreenAmbientPaletteOwners.All.Single(
            item => item.Owner == TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays);
        yield return new("title", PaletteContractDocument(new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version, Colors = PaletteContractColors(SnesCgram.ColorCount),
            BabyMetroidTubeLight = PaletteContractRows(tube.FrameCount, tube.ColorsPerFrame),
            FlickeringDisplays = PaletteContractRows(displays.FrameCount, displays.ColorsPerFrame),
            SkipCopyrightWhite = PaletteContractColors(1)[0], SkipCopyrightRed = PaletteContractColors(1, 1)[0],
        }), json => _ = TitlePaletteFile.Read(json));
        yield return new("Mother Brain health", PaletteContractDocument(new MotherBrainHealthPaletteDocument
        { Version = MotherBrainHealthPaletteFile.Version,
            Body = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteGeometry.ColorCount),
            BackLegs = PaletteContractRows(MotherBrainHealthPaletteFormat.StateCount, MotherBrainRainbowPaletteGeometry.ColorCount) }),
            json => _ = MotherBrainHealthPaletteFile.Read(json));
        yield return new("Mother Brain room", PaletteContractDocument(new MotherBrainRoomColorDocument
        {
            Version = MotherBrainRoomColorFormat.Version,
            Flash = PaletteContractRows(MotherBrainRoomFlashDefinitions.FlashFrameCount, MotherBrainRoomColorGeometry.SliceColors * 2),
            FinalRoom = PaletteContractColors(MotherBrainRoomColorGeometry.SliceColors * 2),
            PhaseTwoAttack = PaletteContractColors(MotherBrainRoomColorGeometry.PhaseTwoColors),
            PhaseTwoRearLeg = PaletteContractColors(MotherBrainRoomColorGeometry.PhaseTwoColors),
            InitialGlassShard = PaletteContractColors(MotherBrainRoomColorGeometry.InitialColors),
            InitialTubeProjectile = PaletteContractColors(MotherBrainRoomColorGeometry.InitialColors),
            RecoveryLights = PaletteContractRows(MotherBrainRoomColorDefinitions.RecoveryLightsFrames,
                MotherBrainRoomColorGeometry.RecoveryLightsSliceColors * 2),
        }), json => _ = MotherBrainRoomColorFile.Read(json));
        MotherBrainRainbowPaletteFrameDocument[] Frames(int count, int body, int legs, bool trailing) => Enumerable.Range(0, count)
            .Select(frame => new MotherBrainRainbowPaletteFrameDocument { Body = PaletteContractColors(body, frame),
                BackLegs = PaletteContractColors(legs, frame), TrailingColor = trailing ? PaletteContractColors(1, frame)[0] : null }).ToArray();
        yield return new("Mother Brain rainbow", PaletteContractDocument(new MotherBrainRainbowPaletteDocument
        {
            Version = MotherBrainRainbowPaletteFile.Version,
            Rainbow = Frames(MotherBrainRainbowColourFormat.RainbowFrameCount, MotherBrainRainbowPaletteGeometry.ColorCount,
                MotherBrainRainbowPaletteGeometry.ColorCount, false),
            ToGrey = Frames(MotherBrainRainbowPaletteGeometry.GreyFrameCount, MotherBrainRainbowPaletteGeometry.DrainedColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, true),
            FromGrey = Frames(MotherBrainRainbowPaletteGeometry.GreyFrameCount, MotherBrainRainbowPaletteGeometry.RevivalColors,
                MotherBrainRainbowPaletteGeometry.BackLegCount, true),
            Normal = Frames(1, MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainRainbowPaletteGeometry.ColorCount, false)[0],
            FakeDeathToGrey = PaletteContractRows(MotherBrainFakeDeathFadeDefinitions.FrameCount, MotherBrainRainbowPaletteGeometry.FakeDeathColors),
        }), json => _ = MotherBrainRainbowPaletteFile.Read(json));
        yield return new("Mother Brain beam", PaletteContractDocument(new MotherBrainBeamColourDocument
        {
            Version = MotherBrainBeamColourFile.Version,
            First = PaletteContractColors(1)[0],
            Cycle = PaletteContractColors(MotherBrainRainbowPaletteGeometry.BeamCycleColorCount),
        }), json => _ = MotherBrainBeamColourFile.Read(json));

        // Only the extractor sees this constructed image. No retail ROM/file is needed
        // and the imported bytes encode colors, not runtime-readable cartridge data.
        var synthetic = new CartridgeImportAddressSpace(Enumerable.Range(0, CartridgeImportAddressSpace.RetailRomByteCount)
            .Select(index => (byte)(index * 13 + 7)).ToArray());
        yield return new("room effects", (JsonObject)RequiredJson.Parse(RoomPaletteFxPresentationExtractor.Extract(synthetic)),
            json => _ = RoomPaletteFxPresentationFile.Read(json));
    }
}
