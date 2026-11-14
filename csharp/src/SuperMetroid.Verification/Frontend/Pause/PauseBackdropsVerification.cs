using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Buffers.Binary;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks installed pause backdrops, labels, and button tiles through live page transitions.</summary>
internal static class PauseBackdropsVerification
{
    /// <summary>Checks native VRAM and pixels across fades and debugger restores, and the Boots-to-Plasma toggle.</summary>
    internal static void VerifyPauseBackdrops(ISnesAddressSpace bus, AreaMapPresentation original)
    {
        AssertTrue(InstalledFixtures.ImportOf(original).Pause.Backdrops.CreateButtonTilemap().SequenceEqual(BackgroundTileWordBytes.Tiles(
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e400, 0x400))), "all native mutable button tiles survive extraction");
        // A debugger save state binds the installed content rather than writing it, as the debugger's store does, so
        // the round trip below carries only the pause session's own state.
        var bound = new BoundObjectGraph([original, MenuSheetFixtures.For(original), .. InstalledFixtures.BoundContent]);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            byte[] expected = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0xb6e000, 0x800);
            ushort label = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0x82965f + (int)area * 2);
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), 0x820000 | label, 24).CopyTo(expected, 170 * 2);
            var tiles = new BackgroundTile[PauseBackdropDefinitions.Cells]; InstalledFixtures.ImportOf(original).Pause.Backdrops.LoadTo(tiles, area);
            AssertTrue(BackgroundTileWordBytes.Tiles(expected).SequenceEqual(tiles), "all 1024 backdrop tiles match native frame plus unmasked area lettering");
            var pause = Create(area, original);
            // Compare at every page/fade position, including the instant R/L updates buttons before ScreenMode
            // changes. Each page button lands on the first tick after the previous change completes.
            int toMap = 2 + PausePageTransitionNativeLength + 1, unpause = toMap + PausePageTransitionNativeLength + 1;
            for (int tick = 0; tick < unpause + 2; tick++)
            {
                ControllerButton input = tick == 2 ? ControllerButton.R : tick == toMap ? ControllerButton.L :
                    tick == unpause ? ControllerButton.Start : 0;
                pause.Step(input, input);
                // Map page, mid page change, and the settled equipment page survive a debugger round trip.
                if (tick == 0 || tick == 2 + PausePageTransitionNativeLength / 2 || tick == toMap)
                {
                    var pixels = pause.Render();
                    using var capture = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(capture, pause, bound); capture.Position = 0;
                    pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(capture, bound);
                    pause.BindFixtureArtwork(original);
                    AssertTrue(pixels.SequenceEqual(pause.Render()), "a restored pause keeps its complete pixels and animation phase");
                }
            }
        }
        // The native invalid-beam selection toggles Plasma past the beam handler's Spazer exclusion. The cartridge also
        // writes Plasma's label nine cells long, into the cells after it; that overrun is a presentation quirk the port
        // does not keep (#1515), so the page shows each label as its item's state describes it and the template after it.
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), EquippedItems = (SamusEquipmentFlags.HiJumpBoots | SamusEquipmentFlags.SpaceJump | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.SpeedBooster), CollectedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma, EquippedBeams = SamusBeamFlags.Spazer };
        var glitch = new PauseMenuState(new PauseMenuEntry { Samus = samus, System = new SharedGameState(new SharedRandom()), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        EnterPauseEquipment(glitch);
        SelectPauseBoots(glitch);
        AssertEqual<PauseEquipmentControl>(PauseEquipmentControls.HiJump, glitch.SelectedControl, "fixture selects Hi-Jump Boots");
        glitch.Step(0, (ControllerButton.Left | ControllerButton.A));
        AssertEqual((ushort)12, samus.EquippedBeams.ToEquipmentWord(), "fixture actually performs native Boots-to-Plasma simultaneous-input glitch");
        byte[] glitchPage = glitch.ShownVram(original).AsSpan(0x6000, 0x800).ToArray();
        BackgroundTile[] template = InstalledFixtures.ImportOf(original).Pause.EquipmentBase.CreateTilemap();
        AssertEqual(BackgroundTileWords.ToWord(template[0x514 / sizeof(ushort)]), BinaryPrimitives.ReadUInt16LittleEndian(glitchPage.AsSpan(0x514)),
            "the cell after Plasma's label shows the template, not the cartridge's overrun");
        Console.WriteLine("Pause backdrops: seven native frames/labels, button artwork, debugger restores and the Boots-to-Plasma toggle pass.");

        static PauseMenuState Create(AreaId area, AreaMapPresentation catalog)
        {
            // Ceres has no map station, so its map byte is never set natively; since #1252 the
            // pause map draws elevator destinations for acquired maps, which Ceres does not have.
            var system = new SharedGameState(new SharedRandom());
            if (area != AreaId.Ceres) system.SetAreaMapAcquired(area);
            return new(new PauseMenuEntry { Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { CollectedBeams = SamusBeamFlags.Charge, EquippedBeams = SamusBeamFlags.Charge }, System = system, AreaIndex = area, RoomMapPosition = new(10, 10), MapPresentation = catalog, SpriteSheets = MenuSheetFixtures.For(catalog) });
        }
    }
}
