using SuperMetroid.Tooling;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause selector identities, native anchor geometry, and coordinate admission.</summary>
internal static class PauseSelectorAnchorsVerification
{
    /// <summary>Compares original selector names and installed coordinates with the cartridge and checks the controls' identities.</summary>
    internal static void VerifyPauseSelectorAnchors(ISnesAddressSpace rom)
    {
        // Original names from the preconversion definition table, kept only as the oracle.
        string[][] names = [ ["Reserve.Mode", "Reserve.Transfer"],
            ["Beam.Charge", "Beam.Ice", "Beam.Wave", "Beam.Spazer", "Beam.Plasma"],
            ["Equipment.Varia", "Equipment.Gravity", "Equipment.MorphBall", "Equipment.Bombs", "Equipment.SpringBall", "Equipment.ScrewAttack"],
            ["Boots.HiJump", "Boots.SpaceJump", "Boots.SpeedBooster"] ];
        var columns = PauseMenuSourceDefinitions.SelectorColumns;
        AssertEqual(names.Length, columns.Length, "one selector column per original category");
        for (int table = 0; table < names.Length; table++)
            AssertTrue(names[table].SequenceEqual(columns[table].Select(control => control.Anchor)), "original named identity order and case per column");
        AssertTrue(columns.SelectMany(column => column).SequenceEqual(PauseEquipmentControls.All), "every control, reserve column first, in page order");
        string[] upgradeNames = [.. names.Skip(1).SelectMany(column => column)];
        AssertTrue(upgradeNames.SequenceEqual(PauseEquipmentControls.Upgrades.Select(control => PauseEquipmentLabelKeys.Name(control.Label))),
            "label identity view contains exactly the fourteen ordinary controls in order");
        Suite(nameof(VerifyPauseLabelIdentityAdmission), () => VerifyPauseLabelIdentityAdmission(rom, [.. upgradeNames, "Beam.Hyper"]));
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseSelectorDocument>(bytes, ArtworkJson.Options);
        PauseSelectorPresentation stock = PauseSelectorSourcePresentation.Load(new MemoryStream(bytes)).Presentation;
        Suite(nameof(VerifyPauseSelectorAnchorField), () => VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: true));
        Suite(nameof(VerifyPauseSelectorAnchorField), () => VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: false));
    }

    /// <summary>Rejects omitted, substituted, case-changed, and extra pause label keys.</summary>
    private static void VerifyPauseLabelIdentityAdmission(ISnesAddressSpace rom, string[] names)
    {
        byte[] source = PauseEquipmentLabelExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(source, ArtworkJson.Options);
        AssertTrue(document.Labels.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(names), "native extractor supplies exact label identities");
        _ = PauseEquipmentLabelPresentation.Load(new MemoryStream(source));
        foreach (string name in names)
        {
            var labels = new Dictionary<string, PauseEquipmentLabel>(document.Labels);
            PauseEquipmentLabel original = labels[name];
            labels.Remove(name);
            Reject(labels);
            labels.Add("Absent.Key", original);
            Reject(labels);
            labels.Remove("Absent.Key");
            labels.Add(name.ToLowerInvariant(), original);
            Reject(labels);
        }
        var extra = new Dictionary<string, PauseEquipmentLabel>(document.Labels) { ["Absent.Key"] = document.Labels[names[0]] };
        Reject(extra);
        void Reject(Dictionary<string, PauseEquipmentLabel> labels) =>
            AssertThrows<InvalidDataException>(() =>
            {
                using var written = new MemoryStream();
                PauseEquipmentLabelPresentation.Write(written, document with { Labels = labels });
                _ = PauseEquipmentLabelPresentation.Load(new MemoryStream(written.ToArray(), writable: false));
            }, "label loader rejects missing, substituted, case-changed or extra identities on read");
    }

    /// <summary>Checks one coordinate axis against retail geometry and rejects out-of-range coordinates.</summary>
    private static void VerifyPauseSelectorAnchorField(ISnesAddressSpace rom, PauseSelectorDocument document,
        PauseSelectorPresentation stock, bool horizontal)
    {
        var columns = PauseMenuSourceDefinitions.SelectorColumns;
        for (int table = 0; table < columns.Length; table++)
        for (int item = 0; item < columns[table].Length; item++)
        {
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c18e + table * 2);
            int original = ReadVerificationWord(rom, pointer + item * 4 + (horizontal ? 0 : 2)) - 1;
            var point = stock.Anchor(columns[table][item]);
            AssertEqual(original, horizontal ? point.X : point.Y, "native installed selector coordinate field");
        }
        foreach (int invalid in new[] { -1, horizontal ? 256 : 224 })
        {
            var anchors = new Dictionary<string, Position16<SpritePixels>>(document.Anchors);
            var first = anchors["Reserve.Mode"];
            anchors["Reserve.Mode"] = horizontal ? new(unchecked((ushort)invalid), first.Y) : new(first.X, unchecked((ushort)invalid));
            AssertThrows<InvalidDataException>(() =>
            {
                using var written = new MemoryStream();
                PauseSelectorSourcePresentation.Write(written, document with { Anchors = anchors });
                _ = PauseSelectorSourcePresentation.Load(new MemoryStream(written.ToArray(), writable: false));
            }, "invalid authored coordinate on read");
        }
    }
}
