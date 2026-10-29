using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveFramesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks reserve tank visuals across supply, flicker, edits, restore, and health transfer.</summary>
internal static class PauseReserveTankAssetsVerification
{
    /// <summary>Compares native menu OAM and pixels with installed reserve art, then checks edits and state isolation.</summary>
    internal static void VerifyPauseReserveTankAssets(ISnesAddressSpace bus, string stock, string overrides, AreaMapPresentation catalog)
    {
        Directory.CreateDirectory(overrides);
        string stockPath = Path.Combine(stock, PauseReserveTankDefinitions.FileName), path = Path.Combine(overrides, PauseReserveTankDefinitions.FileName);
        byte[] bytes = File.ReadAllBytes(stockPath);
        var document = RequiredJson.Deserialize<PauseReserveTankDocument>(bytes, ArtworkJson.Options);
        var guard = NewReserveTankAssetReadGuard(bus);
        Suite(nameof(VerifyPauseReserveAnchors), () => VerifyPauseReserveAnchors(bus));
        int comparisons = 0;
        Suite(nameof(VerifyPauseReserveNativeFrames), () => VerifyPauseReserveNativeFrames(bus, catalog.PauseReserveTanks));
        foreach (ushort capacity in new ushort[] { 0, 100, 200, 300, 400 })
        {
            var nativeSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = ReserveTankMode.Manual };
            var installedSamus = new SamusState { MaxReserveEnergy = capacity, ReserveTankMode = ReserveTankMode.Manual };
            var native = Create(bus, nativeSamus, catalog);
            var installed = Create(guard, installedSamus, catalog);
            foreach (ushort supply in ReserveSupplySamples(capacity))
            foreach (byte phase in new byte[] { 0, 4 })
            {
                nativeSamus.ReserveEnergy = installedSamus.ReserveEnergy = supply;
                native.Step(0, 0, frameCounterByte: phase); installed.Step(0, 0, frameCounterByte: phase);
                var expected = native.CaptureRenderSnapshot(); var actual = installed.CaptureRenderSnapshot();
                AssertTrue(expected.Paint.SpriteParts().SequenceEqual(actual.Paint.SpriteParts()), "every legal reserve supply/flicker state matches native ordered OAM with reads forbidden");
                AssertTrue(native.Render().SequenceEqual(installed.Render()), "every legal reserve supply/flicker state matches native pixels");
                if (capacity == 100 && supply == 1 && phase == 4)
                {
                    using var state = new MemoryStream(); DebuggerObjectGraphSerializer.Serialize(state, installed); state.Position = 0;
                    var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state); restored.BindFixtureArtwork(catalog);
                    AssertTrue(actual.Paint.SpriteParts().SequenceEqual(restored.CaptureRenderSnapshot().Paint.SpriteParts()), "restored nonzero fill flicker phase retains exact OAM");
                    AssertTrue(restored.Render().SequenceEqual(restored.Render()), "repeated draw does not advance fill flicker");
                }
                comparisons++;
            }
        }
        var anchors = document.Anchors.ToArray(); anchors[0] = new(120, 140);
        var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames)
        {
            ["Full"] = [new() { OffsetX = 12, OffsetY = -4, TileColumn = 2, TileRow = 3, Size = 16,
                Palette = null, Priority = 3, FlipX = true, FlipY = true }], ["EndCap"] = [],
        };
        var custom = document with { Anchors = anchors, Palette = 5, Frames = frames };
        using (var output = File.Create(path)) PauseReserveTankSourcePresentation.Write(output, custom);
        var edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(catalog.ContentIdentity != edited.ContentIdentity, "reserve-only edit changes content identity");
        var samus = new SamusState { MaxReserveEnergy = 100, ReserveEnergy = 100, ReserveTankMode = ReserveTankMode.Manual, Health = 50, MaxHealth = 99 };
        var menu = Create(guard, samus, edited);
        var snapshot = menu.CaptureRenderSnapshot();
        // The authored 16-pixel part at the edited anchor: X 120+12, Y 140-4, tile 50, palette 5, priority 3, both flips.
        var tankParts = new OamBuffer();
        tankParts.BeginFrame();
        tankParts.AddOnScreenSpritePart(SnesSpritemapXWord.Create(0, true), new SnesSpritemapYByte(0),
            SnesObjAttributeWord.Create(50, 5, 3, TileFlip.Horizontal | TileFlip.Vertical), new Position16<SpritePixels>(132, 136));
        tankParts.FinalizeFrame();
        PlacedSpritePart[] tank = PlacedSpritePart.OfOamObjects(tankParts, SuperMetroid.AssetExtraction.Frontend.PauseSheetComposer.PauseObjectSelection, MapSpriteArtwork.Of(edited.Sprites).Page.Id);
        AssertTrue(snapshot.Paint.SpriteParts()[^tank.Length..].SequenceEqual(tank),
            "actual menu draws the custom tank's position, size, tile, palette, priority and flips");
        var editedPixels = menu.Render();
        AssertTrue(editedPixels.SequenceEqual(RenderForComparison(snapshot)), "edited reserve reaches captured renderer");
        using (var state = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(state, new object[] { menu, samus }); state.Position = 0;
            var restored = DebuggerObjectGraphSerializer.Deserialize<object[]>(state); menu = (PauseMenuState)restored[0]; samus = (SamusState)restored[1];
        }
        menu.BindFixtureArtwork(catalog);
        AssertTrue(!editedPixels.SequenceEqual(menu.Render()), "restore binds current stock artwork instead of frozen edited content");
        menu.BindFixtureArtwork(edited);
        AssertTrue(editedPixels.SequenceEqual(menu.Render()), "restoring current edited content preserves exact pixels and phase");
        menu.Step(0, ControllerButton.Down); menu.Step(0, ControllerButton.A);
        AssertEqual(51, samus.Health, "custom reserve art cannot change transfer amount");
        AssertEqual(99, samus.ReserveEnergy, "custom reserve art cannot change reserve consumption");
        void Reject(PauseReserveTankDocument invalid) => AssertThrows<InvalidDataException>(() => PauseReserveTankSourcePresentation.Write(new MemoryStream(), invalid), "invalid reserve presentation rejected");
        Reject(document with { Version = 2 }); Reject(document with { Palette = 8 }); Reject(document with { Anchors = [] }); Reject(document with { Frames = [] });
        var badAnchors = anchors.ToArray(); badAnchors[0] = new(256, 0); Reject(document with { Anchors = badAnchors });
        Reject(custom with { Frames = new(frames) { ["Full"] = [frames["Full"][0] with { TileColumn = 15 }] } });
        File.WriteAllText(path, "bad JSON"); AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "bad reserve override never falls back");
        File.WriteAllBytes(path, bytes);
        try
        {
            File.Delete(stockPath); AssertThrows<IOException>(() => MapPresentationFiles.Load(stock, overrides), "override cannot hide missing reserve stock");
            File.WriteAllText(stockPath, "corrupt stock"); AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock, overrides), "override cannot hide corrupt reserve provenance");
        }
        finally { File.WriteAllBytes(stockPath, bytes); }
        Console.WriteLine($"Reserve presentation: 180 native composition/capacity cases and {comparisons} fill-step boundary supply/flicker menu frames; edits, restore, transfer and strict failures pass.");
        // $82:B2AA draws full tanks from supply / 100, then one partial tank from the remainder's
        // 14-unit fill step, flickering when the remainder is between steps. Each tank's empty,
        // step-start, step-start+1 and step-end supplies cover every distinct drawing case.
        static IEnumerable<ushort> ReserveSupplySamples(ushort capacity)
        {
            var samples = new SortedSet<ushort>();
            for (int tank = 0; tank * PauseReserveEnergyDefinitions.EnergyPerTank <= capacity; tank++)
            {
                int baseSupply = tank * PauseReserveEnergyDefinitions.EnergyPerTank;
                samples.Add((ushort)baseSupply);
                for (int step = 0; step * PauseReserveEnergyDefinitions.EnergyPerFillStep < PauseReserveEnergyDefinitions.EnergyPerTank; step++)
                {
                    int start = step * PauseReserveEnergyDefinitions.EnergyPerFillStep;
                    int end = Math.Min(start + PauseReserveEnergyDefinitions.EnergyPerFillStep, PauseReserveEnergyDefinitions.EnergyPerTank) - 1;
                    foreach (int remainder in new[] { start, start + 1, end })
                        if (remainder is > 0 and < PauseReserveEnergyDefinitions.EnergyPerTank && baseSupply + remainder <= capacity)
                            samples.Add((ushort)(baseSupply + remainder));
                }
            }
            return samples;
        }
        static PauseMenuState Create(ISnesAddressSpace source, SamusState state, AreaMapPresentation content)
        {
            var pause = new PauseMenuState(new PauseMenuEntry { Samus = state, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), MapPresentation = content, SpriteSheets = MenuSheetFixtures.For(content) });
            EnterPauseEquipment(pause);
            return pause;
        }
    }
    /// <summary>Blocks native reserve anchor and spritemap reads after installed artwork is bound.</summary>
    private static CartridgeReadGuard NewReserveTankAssetReadGuard(ISnesAddressSpace source)
    {
        var blocked = new HashSet<int>();
        void Add(int address, int count) { for (int i = 0; i < count; i++) blocked.Add(address + i); }
        Add(0x82c1d6, 14); Add(0x82b3d9, 32);
        foreach (ushort id in new ushort[] { 0x1b, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27 })
        {
            Add(0x82c569 + id * 2, 2);
            int pointer = 0x820000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), 0x82c569 + id * 2);
            Add(pointer, 2 + RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), pointer) * 5);
        }
        return new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Installed pause read reserve visual ROM at {address:X6}.");
    }
}
