using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>A cosmetic elevator label attached to a fixed map destination identity.</summary>
/// <param name="Id">Stable presentation key for the source area's elevator label.</param>
/// <param name="Destination">Physical area reached by the elevator represented by the label.</param>
public readonly record struct MapElevatorLabel(string Id, AreaId Destination);

/// <summary>Landmark identities, native slot order and eligibility remain application-owned.</summary>
public static class MapLandmarkDefinitions
{
    /// <summary>$82:C853 first save coordinate is also the unconditional Crateria gunship icon.</summary>
    public const string Gunship = "Crateria.Gunship";

    /// <summary>$82:C7CB..C7D7 selects native boss slot lists; Crateria's three FFFE slots still consume bits.</summary>
    public static BossSequence Bosses(AreaId area) => new(area, area switch
    {
        AreaId.Crateria => 3,
        AreaId.Brinstar or AreaId.Norfair or AreaId.WreckedShip or AreaId.Maridia or AreaId.Ceres => 1,
        AreaId.Tourian => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    });

    /// <summary>$82:C74D..C757 selects the six Zebes elevator destination lists; Ceres has no list.</summary>
    public static ElevatorSequence Elevators(AreaId area) => new(area, area switch
    {
        AreaId.Crateria or AreaId.Brinstar => 5,
        AreaId.Norfair or AreaId.Tourian => 1,
        AreaId.WreckedShip => 2,
        AreaId.Maridia => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels."),
    });

    /// <summary>$82:C83B/C89D/C90B/C981/C9DB/CA9B are Crateria/Kraid/Ridley/Phantoon/Draygon/Ceres boss slots.</summary>
    public readonly struct BossSequence : IReadOnlyList<string?>
    {
        /// <summary>Area whose native boss-slot identities are exposed by this sequence.</summary>
        private readonly AreaId area;

        /// <summary>Number of native boss slots consumed, including reserved slots without a landmark.</summary>
        private readonly int count;

        /// <summary>Creates a view of the area's fixed boss-slot list without allocating an array.</summary>
        /// <param name="area">Area selecting the native slot identities.</param>
        /// <param name="count">Number of slots to expose, including reserved entries.</param>
        internal BossSequence(AreaId area, int count) { this.area = area; this.count = count; }
        /// <summary>Gets the number of native boss slots consumed by the area.</summary>
        public int Count => count;
        /// <summary>Gets the sequence length for array-style consumers.</summary>
        public int Length => Count;
        /// <summary>Gets whether the area consumes no boss-map slots.</summary>
        public bool IsEmpty => Count == 0;
        /// <summary>Gets a boss landmark identity, or <see langword="null"/> for a reserved native slot.</summary>
        /// <param name="index">Zero-based native slot index.</param>
        public string? this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                return area switch
                {
                    AreaId.Crateria => null,
                    AreaId.Brinstar => "Boss.Kraid",
                    AreaId.Norfair => "Boss.Ridley",
                    AreaId.WreckedShip => "Boss.Phantoon",
                    AreaId.Maridia => "Boss.Draygon",
                    AreaId.Ceres => "Boss.CeresRidley",
                    _ => throw new ArgumentOutOfRangeException(nameof(area)),
                };
            }
        }
        /// <summary>Enumerates boss identities and reserved slots in native order.</summary>
        /// <returns>An enumerator over the area's fixed slot sequence.</returns>
        public IEnumerator<string?> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// $82:C759/C779/C799/C7A1/C7AF/C7C3 are area-ordered native destination
    /// label records. The ordinal names identify independently editable anchors;
    /// the cases below preserve the physical area connection of each label.
    /// </summary>
    public readonly struct ElevatorSequence : IReadOnlyList<MapElevatorLabel>
    {
        /// <summary>Source area whose native elevator records determine the sequence.</summary>
        private readonly AreaId area;

        /// <summary>Number of elevator records present for the source area.</summary>
        private readonly int count;

        /// <summary>Creates a view of the source area's fixed elevator-label records without allocating an array.</summary>
        /// <param name="area">Area whose native records select the ordered destination labels.</param>
        /// <param name="count">Number of destination records to expose.</param>
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
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
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
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$82:C759..C7CA use menu spritemaps $59..$5D for Crateria through Maridia labels.</summary>
    public static ushort ElevatorSpritemap(AreaId destination) => destination switch
    {
        AreaId.Crateria => 0x59, AreaId.Brinstar => 0x5a, AreaId.Norfair => 0x5b,
        AreaId.WreckedShip => 0x5c, AreaId.Maridia => 0x5d,
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
