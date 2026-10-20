using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Landmark identities, native slot order and eligibility remain application-owned.</summary>
public static class MapLandmarkDefinitions
{
    /// <summary>$82:C853 first save coordinate is also the unconditional Crateria gunship icon.</summary>
    public const string Gunship = "Crateria.Gunship";

    /// <summary>
    /// $82:C7CB..C7D7 selects native boss slot lists ($82:C83B/C89D/C90B/C981/C9DB/CA9B for
    /// Crateria/Kraid/Ridley/Phantoon/Draygon/Ceres); Crateria's three FFFE slots still consume bits.
    /// </summary>
    /// <remarks>Each slot is a boss landmark identity, or null for a reserved native slot.</remarks>
    public static ImmutableArray<string?> Bosses(AreaId area) => area switch
    {
        AreaId.Crateria => [null, null, null],
        AreaId.Brinstar => ["Boss.Kraid"],
        AreaId.Norfair => ["Boss.Ridley"],
        AreaId.WreckedShip => ["Boss.Phantoon"],
        AreaId.Maridia => ["Boss.Draygon"],
        AreaId.Ceres => ["Boss.CeresRidley"],
        AreaId.Tourian => [],
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>$82:C74D..C757 selects the six Zebes elevator destination lists; Ceres has no list.</summary>
    public static ElevatorSequence Elevators(AreaId area) => new(area, area switch
    {
        AreaId.Crateria or AreaId.Brinstar => 5,
        AreaId.Norfair or AreaId.Tourian => 1,
        AreaId.WreckedShip => 2,
        AreaId.Maridia => 3,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels."),
    });

    /// <summary>
    /// $82:C759/C779/C799/C7A1/C7AF/C7C3 are area-ordered native destination
    /// label records. The ordinal names identify independently editable anchors;
    /// the cases below preserve the physical area connection of each label.
    /// </summary>
    public readonly struct ElevatorSequence : IReadOnlyList<MapElevatorLabel>
    {
        /// <summary>Source area whose fixed map labels and physical destinations are projected.</summary>
        private readonly AreaId area;
        /// <summary>Number of destination labels in the area's native record list.</summary>
        private readonly int count;
        /// <summary>Binds a source area to its native elevator label count.</summary>
        internal ElevatorSequence(AreaId area, int count) { this.area = area; this.count = count; }
        /// <summary>Gets the number of elevator label records for the area.</summary>
        public int Count => count;
        /// <summary>Gets the sequence length for array-style consumers.</summary>
        public int Length => Count;
        /// <summary>Gets the stable label identity and physical destination for one native record.</summary>
        /// <param name="index">Zero-based record index within the source area.</param>
        public MapElevatorLabel this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                AreaId destination = (area, index) switch
                {
                    (AreaId.Crateria, <= 2) or (AreaId.Norfair, 0) or (AreaId.Maridia, 1 or 2) => AreaId.Brinstar,
                    (AreaId.Crateria, 3) => AreaId.WreckedShip,
                    (AreaId.Crateria, 4) or (AreaId.Brinstar, 3) => AreaId.Maridia,
                    (AreaId.Brinstar, 4) => AreaId.Norfair,
                    (AreaId.Brinstar, <= 2) or (AreaId.WreckedShip, 0 or 1) or (AreaId.Maridia, 0) or (AreaId.Tourian, 0) => AreaId.Crateria,
                    _ => throw new ArgumentOutOfRangeException(nameof(area)),
                };
                return new($"{area}.Elevator.{index}", destination);
            }
        }
        /// <summary>Enumerates the area's elevator labels in native record order.</summary>
        /// <returns>An enumerator over the fixed label sequence.</returns>
        public IEnumerator<MapElevatorLabel> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Exposes native elevator label order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$82:C759..C7CA use menu spritemaps $59..$5D for Crateria through Maridia labels.</summary>
    public static MapSpriteId ElevatorSpritemap(AreaId destination) => destination switch
    {
        AreaId.Crateria => MapSpriteId.ElevatorCrateria, AreaId.Brinstar => MapSpriteId.ElevatorBrinstar,
        AreaId.Norfair => MapSpriteId.ElevatorNorfair, AreaId.WreckedShip => MapSpriteId.ElevatorWreckedShip,
        AreaId.Maridia => MapSpriteId.ElevatorMaridia,
        AreaId.Tourian or
            AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(destination)),
        _ => throw new ArgumentOutOfRangeException(nameof(destination))
    };

    /// <summary>Enumerates every published boss, elevator, and gunship landmark identity.</summary>
    /// <returns>All non-reserved landmark IDs in area and native-slot order.</returns>
    public static IEnumerable<string> AllIds()
    {
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            foreach (string? id in Bosses(area)) if (id is not null) yield return id;
            if (area != AreaId.Ceres)
                foreach (var label in Elevators(area)) yield return label.Id;
        }
        yield return Gunship;
    }
}
