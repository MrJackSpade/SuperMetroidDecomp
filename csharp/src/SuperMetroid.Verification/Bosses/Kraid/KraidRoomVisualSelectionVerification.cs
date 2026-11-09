using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks ten named Kraid room PLM visual roles and their independently editable block words.</summary>
internal static class KraidRoomVisualSelectionVerification
{
    /// <summary>Checks each role's published name and the cells its installed picture covers, and that no other draw is pictured (#1557).</summary>
    internal static void VerifyKraidRoomVisualSelection()
    {
        // Independent published pointer/ID/shape contract, not production enumeration.
        KraidRoomVisualSelectionDefinition[] definitions =
        [
            new KraidRoomVisualSelectionDefinition(0x9367, "crumble-first", 1), new KraidRoomVisualSelectionDefinition(0x936d, "crumble-second", 1),
            new KraidRoomVisualSelectionDefinition(0x9373, "crumble-third", 1), new KraidRoomVisualSelectionDefinition(0x9379, "ceiling-background-one", 1),
            new KraidRoomVisualSelectionDefinition(0x937f, "ceiling-background-two", 1), new KraidRoomVisualSelectionDefinition(0x9385, "ceiling-background-three", 1),
            new KraidRoomVisualSelectionDefinition(0x9391, "spike-first", 1), new KraidRoomVisualSelectionDefinition(0x9397, "spike-second", 1),
            new KraidRoomVisualSelectionDefinition(0x939d, "clear-ceiling", 15), new KraidRoomVisualSelectionDefinition(0x93bf, "clear-spikes", 22),
        ];
        InstalledRoomObjectKraidVisuals stock = InstalledRoomObjectKraidVisuals.Stock();
        foreach (var definition in definitions)
        {
            RoomObjectDraw draw = NativeObjectDraws.At<KraidRoomDraw>(definition.Pointer);
            AssertEqual(definition.Id, RoomObjectKraidVisualFiles.Family.Ids[draw], "Kraid visual role's published name");
            RoomObjectFrameImage image = stock.Image(draw);
            AssertEqual(definition.Count, image.Cells.Count, "Kraid visual role covers its blocks");
            for (int block = 0; block < definition.Count; block++)
                AssertTrue(image.Cells.ContainsKey(new Offset16<RoomBlocks>(block, 0)), "Kraid visual role covers each block of its row");
        }
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (definitions.All(definition => definition.Pointer != pointer))
                AssertThrows<InvalidDataException>(() => stock.Image(NativeObjectDraws.At<KraidRoomDraw>((ushort)pointer)),
                    "Kraid visual selector rejects holes and neighboring draws");
        foreach (ushort pointer in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.Image(NativeObjectDraws.At<KraidRoomDraw>(pointer)),
                "Kraid visual selector rejects distant pointers");
        Console.WriteLine("Kraid room visual selection: ten roles, their cells and the pointer bounds pass.");
    }

    /// <summary>Runs the Kraid room visual selection checks.</summary>
    [VerificationCommand("--lookup-kraid-room-visual-selection")]
    internal static int LookupKraidRoomVisualSelectionCommand()
    {
        Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
        return 0;
    }
}
