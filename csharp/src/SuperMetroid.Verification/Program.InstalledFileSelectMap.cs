using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyInstalledFileSelectMenu(ISnesAddressSpace bus, ISnesAddressSpace guard,
        AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited)
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
            AssertTrue(control.Render().AsSpan().SequenceEqual(installed.Render()), "installed map navigation renders exact stock frame");
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
        Console.WriteLine("Installed file-select: stock frame parity through entry, scroll, debugger restore, return and reentry with map ROM reads forbidden.");
    }
}
