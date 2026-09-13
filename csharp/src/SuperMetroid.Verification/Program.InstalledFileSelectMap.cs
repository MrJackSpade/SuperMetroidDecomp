using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyInstalledFileSelectMenu(ISnesAddressSpace bus, ISnesAddressSpace guard,
        AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited, bool verifyCapturedRendering = false)
    {
        var saves = new SuperMetroidSaveRam(bus);
        var snapshot = new SuperMetroidSaveSnapshot { Area = (ushort)AreaId.Maridia, SaveStation = 0, Health = 99, MaxHealth = 99 };
        snapshot.MapStationBytes[(int)AreaId.Maridia] = 1;
        snapshot.UsedSaveStationBytes[(int)AreaId.Maridia * 2] = 1;
        saves.SaveSlot(0, snapshot);
        var slot = saves.ReadSlot(0)!;
        var control = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0);
        var installed = new FileSelectMapMenuState(guard, new CartridgeAudioState(), slot, 0, original);

        void StepBoth(ushort input)
        {
            control.Step(input);
            installed.Step(input);
            AssertEqual(control.Phase, installed.Phase, "installed map navigation phase matches cartridge path");
            AssertEqual(control.LoadRequested, installed.LoadRequested, "installed map load handoff timing");
            AssertEqual(control.OptionsRequested, installed.OptionsRequested, "installed map options handoff timing");
            AssertTrue(control.Render().AsSpan().SequenceEqual(installed.Render()), "installed map navigation renders exact stock frame");
            if (verifyCapturedRendering)
            {
                var nativePixels = SoftwareLayeredSnapshotRenderer.Render(control.CaptureRenderSnapshot());
                var installedPixels = SoftwareLayeredSnapshotRenderer.Render(installed.CaptureRenderSnapshot());
                AssertTrue(nativePixels.AsSpan().SequenceEqual(installedPixels), "installed map render captures retain native pixels through every transition");
                AssertTrue(installed.Render().AsSpan().SequenceEqual(installedPixels), "captured and direct map rendering agree");
            }
        }
        for (int i = 0; i < 48; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "installed menu finishes entry");
        StepBoth((ushort)SnesButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "installed menu reaches room and recreates catalog scroll");
        ushort ScrollX(FileSelectMapMenuState menu) => ((FileSelectMapScroll)typeof(FileSelectMapMenuState)
            .GetField("scroll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(menu)!).Horizontal;
        ushort initialScrollX = ScrollX(installed);
        for (int i = 0; i < 16; i++) StepBoth((ushort)SnesButton.Left);
        AssertTrue(initialScrollX != ScrollX(installed), "installed menu test actually scrolls before saving");

        // Preserve a live, scrolled menu with its old delegate closure. The catalog
        // must be rebound, not captured into that closure or the debugger graph.
        using var state = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(state, installed);
        installed.BindMapPresentation(edited);
        using var reboundState = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(reboundState, installed);
        AssertTrue(state.ToArray().AsSpan().SequenceEqual(reboundState.ToArray()),
            "catalog-only identity change outside this area is not serialized into menu graph");
        state.Position = 0;
        installed = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<FileSelectMapMenuState>(state);
        installed.BindMapPresentation(original);
        AssertTrue(control.Render().AsSpan().SequenceEqual(installed.Render()), "restored room map retains scrolled frame");
        StepBoth((ushort)SnesButton.B);
        for (int i = 0; i < 60; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Area, installed.Phase, "restored menu returns to area");
        StepBoth((ushort)SnesButton.Start);
        for (int i = 0; i < 54; i++) StepBoth(0);
        AssertEqual(FileSelectMapNavigationPhase.Room, installed.Phase, "restored menu reenters room without map ROM access");
        if (verifyCapturedRendering)
        {
            StepBoth((ushort)SnesButton.Start);
            for (int tick = 0; tick < 128 && !installed.LoadRequested; tick++) StepBoth(0);
            AssertTrue(installed.LoadRequested, "ROM-free map reaches gameplay-load handoff after full fade");
            // Exercise the other exit from a fresh area view as well. The test
            // ends at the handoff; gameplay/options retain separate dependencies.
            control = new FileSelectMapMenuState(bus, new CartridgeAudioState(), slot, 0);
            installed = new FileSelectMapMenuState(guard, new CartridgeAudioState(), slot, 0, original);
            for (int tick = 0; tick < 48; tick++) StepBoth(0);
            StepBoth((ushort)SnesButton.B);
            for (int tick = 0; tick < 32 && !installed.OptionsRequested; tick++) StepBoth(0);
            AssertTrue(installed.OptionsRequested, "ROM-free map reaches options handoff after full fade");
        }
        Console.WriteLine("Installed file-select: stock frame parity through entry, scroll, debugger restore, return and reentry with map ROM reads forbidden.");
    }
}
