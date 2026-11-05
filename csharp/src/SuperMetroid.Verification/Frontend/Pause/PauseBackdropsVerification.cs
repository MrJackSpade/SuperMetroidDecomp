using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using System.Buffers.Binary;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed pause backdrops, labels, and button tiles through live page transitions.</summary>
internal static class PauseBackdropsVerification
{
    /// <summary>Checks native VRAM and pixels across fades and debugger restores, and the native equipment overrun.</summary>
    internal static void VerifyPauseBackdrops(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        var guard = NewPauseBackdropReadGuard(bus);
        AssertTrue(InstalledAreaMapPresentation.PauseOf(original).Backdrops.CreateButtonTilemap().SequenceEqual(BackgroundTileWordBytes.Tiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e400, 0x400))), "all native mutable button tiles survive extraction");
        // A debugger save state binds the installed content rather than writing it, as the debugger's store does, so
        // the round trip below carries only the pause session's own state.
        var bound = new BoundObjectGraph([original, MenuSheetFixtures.For(original), .. InstalledFixtures.BoundContent]);
        AreaId everyTickArea = Enum.GetValues<AreaId>()[0];
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] expected = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e000, 0x800);
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82965f + (int)area * 2);
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | label, 24).CopyTo(expected, 170 * 2);
            var tiles = new BackgroundTile[PauseBackdropDefinitions.Cells]; InstalledAreaMapPresentation.PauseOf(original).Backdrops.LoadTo(tiles, area);
            AssertTrue(BackgroundTileWordBytes.Tiles(expected).SequenceEqual(tiles), "all 1024 backdrop tiles match native frame plus unmasked area lettering");
            var native = Create(bus, area, original);
            var pause = Create(guard, area, original);
            // Compare at every page/fade position, including the instant R/L updates buttons before ScreenMode
            // changes. Each page button lands on the first tick after the previous change completes.
            int toMap = 2 + PausePageTransitionNativeLength + 1, unpause = toMap + PausePageTransitionNativeLength + 1;
            for (int tick = 0; tick < unpause + 2; tick++)
            {
                ControllerButton input = tick == 2 ? ControllerButton.R : tick == toMap ? ControllerButton.L :
                    tick == unpause ? ControllerButton.Start : 0;
                AssertEqual(native.Step(input, input), pause.Step(input, input), "backdrop migration preserves Start acceptance and page controls");
                // Each area's complete backdrop is compared word-for-word above, and the fade and
                // page logic is area-independent: one area compares every tick, the others the
                // page-change, mid-fade, settled and unpause ticks.
                bool checkpoint = tick is 0 or 2 or 3 || tick == 2 + PausePageTransitionNativeLength / 2 ||
                    tick == toMap || tick == toMap + 1 || tick == unpause || tick == unpause + 1;
                if (area != everyTickArea && !checkpoint) continue;
                byte[] snapshotVram = pause.ShownVram();
                var shownNative = native.ShownVram();
                AssertTrue(shownNative.SequenceEqual(snapshotVram), "every stock transition frame retains exact native VRAM");
                var pixels = pause.Render();
                AssertTrue(native.Render().SequenceEqual(pixels), "seven-area stock pixels match native path with frame, buttons and label ROM reads blocked");
                // Map page, mid page change, and the settled equipment page survive a debugger round trip.
                if (tick == 0 || tick == 2 + PausePageTransitionNativeLength / 2 || tick == toMap)
                {
                    using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, pause, bound); capture.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(capture, bound);
                    pause.BindFixtureArtwork(original);
                    AssertTrue(pixels.SequenceEqual(pause.Render()), "a restored pause keeps its complete pixels and animation phase");
                }
            }
        }
        // The native invalid-beam selection deliberately leaves an overlong label in equipment VRAM.
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { CollectedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), EquippedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), CollectedBeams = (SamusBeamFlags)0x100f, EquippedBeams = (SamusBeamFlags)4 };
        var glitch = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EnterPauseEquipment(glitch);
        SelectPauseBoots(glitch);
        AssertEqual(0, glitch.SelectedItem, "fixture selects Hi-Jump Boots");
        glitch.Step(0, (ControllerButton.Left | ControllerButton.A));
        AssertEqual((ushort)12, samus.EquippedBeams.ToEquipmentWord(), "fixture actually performs native Boots-to-Plasma simultaneous-input glitch");
        byte[] glitchPage = glitch.ShownVram().AsSpan(0x6000, 0x800).ToArray();
        AssertEqual((ushort)0x0900, BinaryPrimitives.ReadUInt16LittleEndian(glitchPage.AsSpan(0x514)), "fixture retains native VAR overrun beyond ordinary Plasma label");
        Console.WriteLine("Pause backdrops: seven native frames/labels, button artwork, 532 guarded transition frames and the native equipment overrun pass.");

        static PauseMenuState Create(ISnesAddressSpace addressSpace, AreaId area, AreaMapPresentation catalog)
        {
            // Ceres has no map station, so its map byte is never set natively; since #1252 the
            // pause map draws elevator destinations for acquired maps, which Ceres does not have.
            var system = new SharedGameState();
            if (area != AreaId.Ceres) system.SetAreaMapAcquired(area);
            return new(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge }, System = system, AreaIndex = area, RoomMapPosition = new(10, 10), MapPresentation = catalog, SpriteSheets = MenuSheetFixtures.For(catalog) });
        }
    }

    /// <summary>Forbids native backdrop, button, and area-label reads after installed art is bound.</summary>
    private static CartridgeReadGuard NewPauseBackdropReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int size) { for (int i = 0; i < size; i++) blocked.Add(address + i); }
        Add(0xb6e000, 0x800); Add(0x82965f, 14);
        for (int area = 0; area < 7; area++) Add(0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82965f + area * 2), 24);
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read backdrop/button/label at {address:X6}.");
    }
}
