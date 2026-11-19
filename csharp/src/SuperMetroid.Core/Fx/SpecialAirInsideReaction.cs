using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does to Samus while she is inside it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.InsideOf"/>): each reaction states what it does (#2571), the plant it wakes, the sand
/// reaction it places, the quicksand surface's hold or the sand's carry, so no switch over reaction identities runs it.
/// </summary>
public sealed class SpecialAirInsideReaction
{
    /// <summary>The reaction's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>A reaction named <paramref name="name"/> that does what the other arguments state.</summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    /// <param name="plant">The plant that bites Samus, or null.</param>
    /// <param name="placesSandReaction">Whether the reaction places a sand reaction object before it acts.</param>
    /// <param name="quicksandSurface">Whether the block is the quicksand surface, which holds or sinks Samus by her vertical direction.</param>
    /// <param name="carry">How far the sand carries Samus down each update, or null.</param>
    private SpecialAirInsideReaction(string name, SamusEaterObjectDefinition? plant = null, bool placesSandReaction = false,
        bool quicksandSurface = false, SignedSixteenSixteen? carry = null)
    {
        _name = name;
        Plant = plant;
        PlacesSandReaction = placesSandReaction;
        IsQuicksandSurface = quicksandSurface;
        Carry = carry;
    }

    /// <summary>Nothing happens.</summary>
    public static SpecialAirInsideReaction Nothing { get; } = new(nameof(Nothing));

    /// <summary>The Brinstar plant that bites Samus from the floor.</summary>
    public static SpecialAirInsideReaction BrinstarFloorPlant { get; } = new(nameof(BrinstarFloorPlant), plant: SamusEaterObjectDefinitions.Floor);

    /// <summary>The Brinstar plant that bites Samus from the ceiling.</summary>
    public static SpecialAirInsideReaction BrinstarCeilingPlant { get; } = new(nameof(BrinstarCeilingPlant), plant: SamusEaterObjectDefinitions.Ceiling);

    /// <summary>Maridia's quicksand surface: it cancels running momentum and sinks or holds Samus by her vertical direction.</summary>
    public static SpecialAirInsideReaction QuicksandSurface { get; } = new(nameof(QuicksandSurface), placesSandReaction: true, quicksandSurface: true);

    /// <summary>Maridia's submerging quicksand, which pulls Samus down.</summary>
    public static SpecialAirInsideReaction SubmergingQuicksand { get; } = new(nameof(SubmergingQuicksand), placesSandReaction: true, carry: QuicksandDefinitions.SubmergingSinkDisplacement);

    /// <summary>Maridia's slow sandfall, which carries Samus down.</summary>
    public static SpecialAirInsideReaction SlowSandfall { get; } = new(nameof(SlowSandfall), placesSandReaction: true, carry: QuicksandDefinitions.SlowSandfallDisplacement);

    /// <summary>Maridia's fast sandfall, which carries Samus down faster.</summary>
    public static SpecialAirInsideReaction FastSandfall { get; } = new(nameof(FastSandfall), placesSandReaction: true, carry: QuicksandDefinitions.FastSandfallDisplacement);

    /// <summary>The Brinstar plant the block wakes to bite Samus, or null when it is no plant.</summary>
    internal SamusEaterObjectDefinition? Plant { get; }

    /// <summary>
    /// Whether the reaction is one of the four inside sand reactions, each of which places a sand reaction object at its
    /// block before it acts (<see cref="QuicksandObjects"/>); every one leaves the room on its first update.
    /// </summary>
    internal bool PlacesSandReaction { get; }

    /// <summary>Whether the block is Maridia's quicksand surface, which cancels running momentum and holds or sinks Samus.</summary>
    internal bool IsQuicksandSurface { get; }

    /// <summary>How far the submerging quicksand or a sandfall carries Samus down each update, or null.</summary>
    internal SignedSixteenSixteen? Carry { get; }

    /// <summary>Names the reaction.</summary>
    public override string ToString() => _name;
}
