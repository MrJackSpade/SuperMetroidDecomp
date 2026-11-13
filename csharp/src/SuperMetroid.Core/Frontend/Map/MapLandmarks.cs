using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Lists each area's map landmarks (<see cref="MapLandmarkDefinitions"/>): its boss markers in slot order, its elevator
/// labels with their destinations, and the sprite each destination's elevator shows.
/// </summary>
public static class MapLandmarks
{
    /// <summary>
    /// Each area selects its boss slot list (Crateria, Kraid, Ridley, Phantoon, Draygon and
    /// Ceres); Crateria's three unused slots still consume bits.
    /// </summary>
    /// <remarks>Each slot is a boss landmark identity, or null for a reserved slot.</remarks>
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

    /// <summary>Selects the six Zebes elevator destination lists; Ceres has no list.</summary>
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
    /// The elevator destination labels, in area order: these are the
    /// label records. The ordinal names identify independently editable anchors;
    /// the cases below preserve the physical area connection of each label.
    /// </summary>
    public readonly struct ElevatorSequence : IReadOnlyList<MapElevatorLabel>
    {
        /// <summary>Source area whose fixed map labels and physical destinations are projected.</summary>
        private readonly AreaId area;
        /// <summary>Number of destination labels in the area's record list.</summary>
        private readonly int count;
        /// <summary>Binds a source area to its elevator label count.</summary>
        internal ElevatorSequence(AreaId area, int count) { this.area = area; this.count = count; }
        /// <summary>Gets the number of elevator label records for the area.</summary>
        public int Count => count;
        /// <summary>Gets the stable label identity and physical destination for one record.</summary>
        /// <param name="index">Zero-based record index within the source area.</param>
        public MapElevatorLabel this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                AreaId destination = new AreaTableEntry(area, index) switch
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
        /// <summary>Enumerates the area's elevator labels in record order.</summary>
        /// <returns>An enumerator over the fixed label sequence.</returns>
        public IEnumerator<MapElevatorLabel> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }
        /// <summary>Exposes the elevator label order through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The map picture of the elevator label leading to <paramref name="destination"/>, one of the five for Crateria through Maridia.</summary>
    public static MapSpriteId ElevatorPicture(AreaId destination) => destination switch
    {
        AreaId.Crateria => MapSpriteId.ElevatorCrateria, AreaId.Brinstar => MapSpriteId.ElevatorBrinstar,
        AreaId.Norfair => MapSpriteId.ElevatorNorfair, AreaId.WreckedShip => MapSpriteId.ElevatorWreckedShip,
        AreaId.Maridia => MapSpriteId.ElevatorMaridia,
        AreaId.Tourian or
            AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(destination)),
        _ => throw new ArgumentOutOfRangeException(nameof(destination))
    };

}
