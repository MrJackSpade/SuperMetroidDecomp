using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed file-select map arrows against native timing and OAM.</summary>
internal static class MapArrowsVerification
{
    /// <summary>Checks installed arrow timing and OAM against native output.</summary>
    internal static void VerifyMapArrows(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewMapArrowGuard(bus);
        var native = new FileSelectMapAnimations(original.Arrows);
        var installed = new FileSelectMapAnimations(original.Arrows);
        for (int tick = 0; tick < 600; tick++)
        {
            bool Available(MapScrollDirection direction) => (tick / 17 + (int)direction) % 5 != 0;
            native.StepArrows(Available); installed.StepArrows(Available);
            AssertTrue(Draw(native).SequenceEqual(Draw(installed)), "all four extracted arrow sprites match native OAM");
            AssertTrue(Counters(native).SequenceEqual(Counters(installed)), "arrow phase/timer and visibility match native including hidden pauses");
        }
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original));
        Console.WriteLine("Map arrows: 600 native phase/OAM ticks and guarded whole-menu transitions pass.");

        byte[] Draw(FileSelectMapAnimations animation)
        {
            var oam = ScreenSpritesFixtures.Blank(); oam.BeginFrame();
            animation.DrawArrows(oam, animation.HighlightShading, MenuSheetFixtures.MapSprites(InstalledAreaMapPresentation.SpritesOf(original)));
            oam.FinalizeFrame();
            return oam.LowTable.ConcatToArray(oam.HighTable);
        }
        static int[] Counters(FileSelectMapAnimations animation)
        {
            return [.. animation.arrows.SelectMany(arrow => new[] { arrow.Timer, arrow.Frame, arrow.Visible ? 1 : 0 })];
        }
    }

    /// <summary>Forbids the native arrow records, animation tables, and sprite programs during installed arrow playback.</summary>
    private static MutableCartridgeReadGuard NewMapArrowGuard(ISnesAddressSpace source)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(source);
        var forbidden = new HashSet<int>();
        for (int index = 0; index < MapArrowDefinitions.Count; index++)
        {
            int record = MapSourceDefinitions.FileSelectMap.ScrollArrows + index * 10;
            Add(record, 10);
            int animation = RomDataReader.ReadWordFixedBank(cartridge, record + 4);
            int pointer = MapSourceDefinitions.Animation.SpritePrograms + (animation - 1) * 2;
            int bases = MapSourceDefinitions.Animation.SpriteBases + (animation - 1) * 2;
            Add(pointer, 2); Add(bases, 2);
            Add(MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(cartridge, bases), 2);
            int program = MapSourceDefinitions.Menu.ObjectBank | RomDataReader.ReadWordFixedBank(cartridge, pointer);
            while (CpuBusRegionRead.Read(source, program) != byte.MaxValue) { Add(program, 3); program += 3; }
            Add(program, 1);
        }
        void Add(int start, int length) { for (int offset = 0; offset < length; offset++) forbidden.Add(start + offset); }
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(forbidden.Contains, address => $"Installed arrows read visual metadata from ROM at {address:X6}.");
        return guard;
    }
}
