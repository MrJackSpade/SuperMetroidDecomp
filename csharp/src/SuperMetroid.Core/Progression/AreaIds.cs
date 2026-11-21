namespace SuperMetroid.Core.Progression;

/// <summary>
/// The seven areas and checks on them. Imported data and save files number the areas; that numbering is the import's
/// and the save files' own, and the area identities' own values are never read.
/// </summary>
public static class AreaIds
{
    /// <summary>Every area: the six Zebes areas, then Ceres, the prologue station.</summary>
    public static IReadOnlyList<AreaId> All { get; } =
    [
        AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Tourian, AreaId.Ceres,
    ];

    /// <summary>The six Zebes areas; Ceres, the prologue station, is not one of them.</summary>
    public static IReadOnlyList<AreaId> Zebes { get; } = [.. All.Where(area => area != AreaId.Ceres)];

    /// <summary>Whether <paramref name="area"/> is one of the six Zebes areas.</summary>
    public static bool IsZebes(AreaId area) => area != AreaId.Ceres;

    /// <summary>Whether <paramref name="area"/> is one of the six Zebes areas, and which: <paramref name="zebes"/>.</summary>
    public static bool IsZebes(AreaId area, out ZebesArea zebes) => ZebesOf.TryGetValue(area, out zebes);

    /// <summary>The area <paramref name="zebes"/> is.</summary>
    public static AreaId Area(ZebesArea zebes) => ZebesAreas[zebes];

    /// <summary>The area each Zebes area is.</summary>
    private static IdentityTable<ZebesArea, AreaId> ZebesAreas => field ??= new(new Dictionary<ZebesArea, AreaId>
    {
        [ZebesArea.Crateria] = AreaId.Crateria,
        [ZebesArea.Brinstar] = AreaId.Brinstar,
        [ZebesArea.Norfair] = AreaId.Norfair,
        [ZebesArea.WreckedShip] = AreaId.WreckedShip,
        [ZebesArea.Maridia] = AreaId.Maridia,
        [ZebesArea.Tourian] = AreaId.Tourian,
    });

    /// <summary>The Zebes area each of the six Zebes areas is; Ceres is none.</summary>
    private static Dictionary<AreaId, ZebesArea> ZebesOf => field ??= ZebesAreas.ToDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The definition of <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is undefined.</exception>
    internal static AreaDefinition Of(AreaId area) => Ensure.Listed(AreaDefinitions.Areas, area);

    /// <summary>Whether <paramref name="area"/>'s machinery has power, given whether its boss is defeated.</summary>
    internal static bool HasPower(AreaId area, bool areaBossDefeated) => !Of(area).PoweredFromBossDefeat || areaBossDefeated;
}
