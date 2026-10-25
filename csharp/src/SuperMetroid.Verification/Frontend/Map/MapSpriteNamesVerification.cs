using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend.Map;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the installed map-sprite role catalog against independent native IDs.</summary>
internal static class MapSpriteNamesVerification
{
    /// <summary>Lists original arrows, stations, markers, elevators, and world labels in document order.</summary>
    internal static MapSpriteRoleOracleResult[] MapSpriteRoleOracle() =>
        [
            new MapSpriteRoleOracleResult(0x4, "Arrow.Right"),
            new MapSpriteRoleOracleResult(0x5, "Arrow.Left"),
            new MapSpriteRoleOracleResult(0x6, "Arrow.Up"),
            new MapSpriteRoleOracleResult(0x7, "Arrow.Down"),
            new MapSpriteRoleOracleResult(0x9, "Marker.Boss"),
            new MapSpriteRoleOracleResult(0xa, "Station.Energy"),
            new MapSpriteRoleOracleResult(0xb, "Station.Missile"),
            new MapSpriteRoleOracleResult(0x4e, "Station.Map"),
            new MapSpriteRoleOracleResult(0x62, "Marker.DefeatedBoss"),
            new MapSpriteRoleOracleResult(0x63, "Marker.Gunship"),
            new MapSpriteRoleOracleResult(0x12, "Indicator.Backing"),
            new MapSpriteRoleOracleResult(0x5f, "Indicator.Frame0"),
            new MapSpriteRoleOracleResult(0x60, "Indicator.Frame1"),
            new MapSpriteRoleOracleResult(0x61, "Indicator.Frame2"),
            new MapSpriteRoleOracleResult(0x59, "Elevator.Crateria"),
            new MapSpriteRoleOracleResult(0x5a, "Elevator.Brinstar"),
            new MapSpriteRoleOracleResult(0x5b, "Elevator.Norfair"),
            new MapSpriteRoleOracleResult(0x5c, "Elevator.WreckedShip"),
            new MapSpriteRoleOracleResult(0x5d, "Elevator.Maridia"),
            new MapSpriteRoleOracleResult(0x38, "World.Title"),
            new MapSpriteRoleOracleResult(0x39, "World.Crateria"),
            new MapSpriteRoleOracleResult(0x3a, "World.Brinstar"),
            new MapSpriteRoleOracleResult(0x3b, "World.Norfair"),
            new MapSpriteRoleOracleResult(0x3c, "World.WreckedShip"),
            new MapSpriteRoleOracleResult(0x3d, "World.Maridia"),
            new MapSpriteRoleOracleResult(0x3e, "World.Tourian")
        ];

    /// <summary>Checks document order, the complete ushort membership domain, and invalid names.</summary>
    internal static void VerifyMapSpriteNameCases()
    {
        // Independent original table from9b54e512, retained only for verification.
        var original = MapSpriteRoleOracle();
        AssertEqual(original.Length, MapSpriteDefinitions.Count, "map sprite complete role count");
        AssertTrue(original.SequenceEqual(MapSpriteDefinitions.Frames.Select(frame => new MapSpriteRoleOracleResult(MapSpriteOrdinals.Spritemaps.Word(frame.Id), frame.Name))), "original map sprite document order");
        foreach (var entry in original)
            AssertEqual(entry.Name, MapSpriteDefinitions.Name(MapSpriteOrdinals.Spritemaps.Decode(entry.NativeId, "map spritemap ordinal")), "original native identity name");
        var supported = original.Select(entry => entry.NativeId).ToHashSet();
        for (int id = 0; id <= ushort.MaxValue; id++)
            AssertEqual(supported.Contains((ushort)id), MapSpriteOrdinals.Spritemaps.TryDecode((ushort)id, out _), "complete map sprite ordinal domain: the import's table holds the native ordinals; the identities' values are arbitrary");
        AssertEqual(MapSpriteDefinitions.Count, Enum.GetValues<MapSpriteId>().Length, "one map sprite identity per role");
        foreach (ushort invalid in new ushort[] { 0, MapSpriteDefinitions.Count + 1, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapSpriteDefinitions.Name((MapSpriteId)invalid), "unsupported map sprite name");
    }

    /// <summary>Runs the native map-sprite role audit.</summary>
    [VerificationCommand("--lookup-map-sprite-names")]
    internal static int LookupMapSpriteNamesCommand()
    {
        Suite(nameof(VerifyMapSpriteNameCases), () => VerifyMapSpriteNameCases());
        Console.WriteLine("Map sprite names: all original identities and order, complete ushort membership and invalid names pass.");
        return 0;
    }
}
