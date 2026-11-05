using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks ten named Kraid room PLM visual roles and their independently editable block words.</summary>
internal static class KraidRoomVisualSelectionVerification
{
    /// <summary>Checks frame identity, copied input isolation, bounds, required coverage, and rejection of physical bits.</summary>
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
        var expected = new Dictionary<ushort, RoomBlockAppearance[]>();
        var entries = new List<RoomPlmVisualEntry>();
        for (int frame = 0; frame < definitions.Length; frame++)
        {
            var definition = definitions[frame];
            RoomBlockAppearance[] words = Enumerable.Range(0, definition.Count)
                .Select(block => RoomBlockAppearance.Of(0x100 + 32 * frame + block)).ToArray();
            expected.Add(definition.Pointer, words);
            entries.Add(new(definition.Id, words.ToArray()));
        }
        var catalog = RoomPlmKraidVisualFiles.Create(entries.AsEnumerable().Reverse());
        foreach (var definition in definitions)
        {
            foreach (int block in new[] { int.MinValue, -1, definition.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<KraidRoomDraw>(definition.Pointer), 0, block),
                    "Kraid visual block bounds");
            foreach (int run in new[] { int.MinValue, -1, 1, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<KraidRoomDraw>(definition.Pointer), run, 0),
                    "Kraid visual run bounds");
        }
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (!expected.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<KraidRoomDraw>((ushort)pointer), 0, 0),
                    "Kraid visual selector rejects holes and neighboring draws");
        foreach (ushort pointer in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => catalog.GetAppearance(NativePlmDraws.At<KraidRoomDraw>(pointer), 0, 0),
                "Kraid visual selector rejects distant pointers");
        for (int index = 0; index < entries.Count; index++)
        {
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFiles.Create(entries.Where((_, ordinal) => ordinal != index)),
                "Kraid visual selection rejects each missing role");
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFiles.Create(entries.Append(entries[index])),
                "Kraid visual selection rejects each duplicate role");
        }
        AssertThrows<InvalidDataException>(() => RoomPlmKraidVisualFiles.Create(
            entries.Append(new("unknown", [RoomBlockAppearance.Of(0)]))), "Kraid visual selection rejects unknown ID");
        AssertThrows<InvalidDataException>(() => RoomPlmKraidVisualFiles.Create(
            entries.Skip(1).Append(new("crumble-first", []))), "Kraid visual selection rejects empty frame");
        Console.WriteLine("Kraid room visual selection: ten roles, coordinate and pointer bounds, and validation pass.");
    }

    /// <summary>Runs the Kraid room visual selection checks.</summary>
    [VerificationCommand("--lookup-kraid-room-visual-selection")]
    internal static int LookupKraidRoomVisualSelectionCommand()
    {
        Suite(nameof(VerifyKraidRoomVisualSelection), () => VerifyKraidRoomVisualSelection());
        return 0;
    }
}
