using SuperMetroid.Core.Samus.Abilities.Artwork;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.Verification.Runtime;

using SuperMetroid.Verification.Harness;

/// <summary>Checks canonical hashes for X-ray visuals; the standard OBJ characters are import data no identity covers (#1636).</summary>
internal static class GameplayContentIdentityVerification
{
    /// <summary>The fixture's first room overlay.</summary>
    private static readonly XrayRoomOverlayDefinition FirstFixtureOverlay = new("fixture overlay one");

    /// <summary>The fixture's second room overlay.</summary>
    private static readonly XrayRoomOverlayDefinition SecondFixtureOverlay = new("fixture overlay two");

    /// <summary>Known shared-presentation domains only; no ROM or gameplay path discovery.</summary>
    internal static void VerifyGameplayContentIdentity()
    {
        IReadOnlyDictionary<string, string> baseline = CreateGameplayIdentityFixture();
        IReadOnlyDictionary<string, string> reordered = CreateGameplayIdentityFixture(reverse: true);
        foreach ((string component, string digest) in baseline)
            AssertEqual(digest, reordered[component], component + " canonical record order");
        foreach ((string edit, string component) in new[]
        {
            new VerifyGameplayContentIdentityCase("xray-top-left", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("xray-top-right", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("xray-bottom-left", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("xray-bottom-right", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("xray-item", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("overlay-x", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("overlay-y", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("overlay-word", GameInstallationLayout.XrayRevealVisualDirectoryName),
            new VerifyGameplayContentIdentityCase("overlay-order", GameInstallationLayout.XrayRevealVisualDirectoryName),
        })
        {
            IReadOnlyDictionary<string, string> changed = CreateGameplayIdentityFixture(edit);
            AssertTrue(baseline[component] != changed[component], edit + " invalidates selected content");
            foreach (string other in baseline.Keys.Where(name => name != component))
                AssertEqual(baseline[other], changed[other], edit + " preserves unrelated " + other);
            var before = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, baseline);
            var after = GameContentIdentity.Create(new string('A', 64), new string('B', 64),
                new string('C', 64), Guid.Empty, changed);
            AssertTrue(before.CompositeSha256 != after.CompositeSha256, edit + " changes the host identity");
            AssertTrue(before.GetCompatibilityWarnings(after.ToSnapshot(), "test").Single()
                .Contains(component, StringComparison.Ordinal), edit + " gives a specific compatibility warning");
        }
        Console.WriteLine("  Shared gameplay content: all X-ray operands, items, " +
            "room overlay positions/order, canonical records and host warnings pass without a ROM.");
    }

    /// <summary>Builds decoded presentation identities with one optional edit or reversed record order.</summary>
    private static IReadOnlyDictionary<string, string> CreateGameplayIdentityFixture(
        string? edit = null, bool reverse = false)
    {
        // Every drawable rule shows the blocks its shape copies of four distinct ones; one square rule's block is edited.
        var entries = new List<XrayRevealVisualDefinition>();
        bool editedOperands = false;
        foreach (XrayRevealVisualDefinition entry in XrayRevealFixtures.EveryDrawable(shape => XrayRevealFixtures.Corners(shape, 10, 11, 12, 13)))
        {
            XrayRevealVisualBlocks visual = entry.Visual;
            if (!editedOperands && XrayRevealTable.Find(entry.Type, entry.Reaction) == XrayRevealShape.CopySquare &&
                edit?.StartsWith("xray-", StringComparison.Ordinal) == true)
            {
                visual = XrayRevealVisualBlocks.Square(
                    10 ^ (edit == "xray-top-left" ? 1 : 0),
                    11 ^ (edit == "xray-top-right" ? 1 : 0),
                    12 ^ (edit == "xray-bottom-left" ? 1 : 0),
                    13 ^ (edit == "xray-bottom-right" ? 1 : 0));
                editedOperands = true;
            }
            entries.Add(entry with { Visual = visual });
        }
        AssertEqual(42, entries.Count, "complete compiled drawable rule fixture");
        var items = new RoomBlockAppearance[InstalledXrayOverlays.TankCount];
        if (edit == "xray-item") items[^1] = RoomBlockAppearance.Of(1);
        XrayRoomOverlayVisual[] tiles =
        [
            new(new(1, 2), RoomBlockAppearance.Of(3)),
            new(new((byte)(edit == "overlay-x" ? 5 : 4), (byte)(edit == "overlay-y" ? 6 : 5)),
                RoomBlockAppearance.Of(edit == "overlay-word" ? 7 : 6)),
        ];
        if (edit == "overlay-order") Array.Reverse(tiles);
        XrayRoomOverlayBlocks[] rooms =
            [new(FirstFixtureOverlay, tiles), new(SecondFixtureOverlay, new XrayRoomOverlayVisual[] { new(new(8, 9), RoomBlockAppearance.Of(10)) })];
        var overlays = InstalledXrayOverlays.FromOverlaysForVerification(items, reverse ? rooms.Reverse() : rooms);
        var xray = new InstalledXrayReveals(reverse ? entries.AsEnumerable().Reverse() : entries, overlays);
        return GameplayPresentationIdentity.Create(xray);
    }

    /// <summary>Runs the shared gameplay content identity checks.</summary>
    [VerificationCommand("--gameplay-content-identity")]
    internal static int GameplayContentIdentityCommand()
    {
        Suite(nameof(VerifyGameplayContentIdentity), () => VerifyGameplayContentIdentity());
        return 0;
    }
}
