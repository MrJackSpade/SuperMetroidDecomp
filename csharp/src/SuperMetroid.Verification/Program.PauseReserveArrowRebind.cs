using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Rebinding the same presentation on a settled reserve page must keep the arrow palette
    /// that page setup latched; a rebind is not a tank dispatch. Previously the rebind recomputed
    /// the arrow from the selection and changed ten arrow tilemap words (palette 6 to 7).
    /// </summary>
    private static void VerifyPauseReserveArrowRebindKeepsLatch()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var catalog = RetailPresentationFixture();
        var samus = new SamusState { MaxReserveEnergy = 100, ReserveEnergy = 100, ReserveTankMode = 2, Health = 50, MaxHealth = 99 };
        var menu = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0, mapPresentation: catalog);
        EnterPauseEquipment(menu);
        AssertEqual(PauseEquipmentCategory.Reserves, menu.SelectedCategory, "fixture settles on the reserve page selection");
        var pixels = menu.Render();
        byte[] vram = menu.CaptureRenderSnapshot().Memory.Vram.ToArray();

        menu.BindMapPresentation(catalog);
        AssertTrue(vram.AsSpan().SequenceEqual(menu.CaptureRenderSnapshot().Memory.Vram), "same-content rebind keeps the latched reserve arrow tilemap");
        AssertTrue(pixels.AsSpan().SequenceEqual(menu.Render()), "same-content rebind keeps exact reserve page pixels");

        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, menu);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(state);
        restored.BindMapPresentation(catalog);
        AssertTrue(pixels.AsSpan().SequenceEqual(restored.Render()), "restore plus rebind keeps exact reserve page pixels");
        Console.WriteLine("Reserve arrow rebind: same-content rebind and restore keep the latched arrow palette and exact pixels.");
    }
}
