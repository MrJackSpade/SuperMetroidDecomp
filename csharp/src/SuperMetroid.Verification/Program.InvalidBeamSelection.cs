using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyInvalidBeamSelection()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Begin on Boots using the production initial-selection rule. Publish the rest
        // of the fixture inventory afterward, before any tested input. This avoids a
        // private cursor setter and isolates the Boots-handler Left+A ordering.
        var samus = new SamusState { CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots };
        var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), AreaId.Crateria, 0, 0);
        AssertEqual(3, pause.SelectedCategory, "fixture starts on Boots");
        samus.CollectedBeams = 0x100f;
        samus.EquippedBeams = 4;
        samus.CollectedItems = samus.EquippedItems = 0x3300;
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int frame = 0; frame < 32; frame++) pause.Step(0, 0);
        pause.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual(1, pause.SelectedCategory, "same-frame Left+A moves Boots to Beams");
        AssertEqual(4, pause.SelectedItem, "same-frame Left+A selects Plasma");
        AssertEqual(0x000c, samus.EquippedBeams, "native Boots handler retains Spazer while enabling Plasma");
    }
}
