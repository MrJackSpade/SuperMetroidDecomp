using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does to Samus while she is inside it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.InsideOf"/>): each reaction states what it does (#2571), the plant it wakes, the sand
/// reaction it places, the quicksand surface's hold or the sand's carry, so no switch over reaction identities runs it.
/// </summary>
public sealed partial class SpecialAirInsideReaction
{
    /// <summary>The reaction's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>A reaction named <paramref name="name"/> that does what the other arguments state.</summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    /// <param name="plant">The plant that bites Samus, or null.</param>
    /// <param name="sand">What the block's sand does to Samus inside it.</param>
    /// <param name="sinking">How far the sand pulls Samus down each update, for sand that does.</param>
    private SpecialAirInsideReaction(string name, SamusEaterObjectDefinition? plant = null,
        SpecialAirInsideSand sand = SpecialAirInsideSand.None, SignedSixteenSixteen? sinking = null)
    {
        _name = name;
        Plant = plant;
        _sand = sand;
        Sinking = sinking;
    }

    /// <summary>What the block's sand does to Samus inside it.</summary>
    private readonly SpecialAirInsideSand _sand;

    /// <summary>Nothing happens.</summary>
    public static SpecialAirInsideReaction Nothing { get; } = new(nameof(Nothing));

    /// <summary>The Brinstar plant that bites Samus from the floor.</summary>
    public static SpecialAirInsideReaction BrinstarFloorPlant { get; } = new(nameof(BrinstarFloorPlant), plant: SamusEaterObjectDefinitions.Floor);

    /// <summary>The Brinstar plant that bites Samus from the ceiling.</summary>
    public static SpecialAirInsideReaction BrinstarCeilingPlant { get; } = new(nameof(BrinstarCeilingPlant), plant: SamusEaterObjectDefinitions.Ceiling);

    /// <summary>Maridia's quicksand surface: it cancels running momentum and sinks or holds Samus by her vertical direction.</summary>
    public static SpecialAirInsideReaction QuicksandSurface { get; } = new(nameof(QuicksandSurface), sand: SpecialAirInsideSand.Surface);

    /// <summary>Maridia's submerging quicksand, which pulls Samus down.</summary>
    public static SpecialAirInsideReaction SubmergingQuicksand { get; } = new(nameof(SubmergingQuicksand), sand: SpecialAirInsideSand.Sinks, sinking: QuicksandDefinitions.SubmergingSinkDisplacement);

    /// <summary>Maridia's slow sandfall, which carries Samus down.</summary>
    public static SpecialAirInsideReaction SlowSandfall { get; } = new(nameof(SlowSandfall), sand: SpecialAirInsideSand.Sinks, sinking: QuicksandDefinitions.SlowSandfallDisplacement);

    /// <summary>Maridia's fast sandfall, which carries Samus down faster.</summary>
    public static SpecialAirInsideReaction FastSandfall { get; } = new(nameof(FastSandfall), sand: SpecialAirInsideSand.Sinks, sinking: QuicksandDefinitions.FastSandfallDisplacement);

    /// <summary>The Brinstar plant the block wakes to bite Samus, or null when it is no plant.</summary>
    internal SamusEaterObjectDefinition? Plant { get; }

    /// <summary>
    /// Whether the reaction is one of the four inside sand reactions, each of which places a sand reaction object at its
    /// block before it acts (<see cref="QuicksandObjects"/>); every one leaves the room on its first update.
    /// </summary>
    internal bool PlacesSandReaction => _sand != SpecialAirInsideSand.None;

    /// <summary>Whether the block is Maridia's quicksand surface, which cancels running momentum and holds or sinks Samus.</summary>
    internal bool IsQuicksandSurface => _sand == SpecialAirInsideSand.Surface;

    /// <summary>How far the submerging quicksand or a sandfall pulls Samus down each update, or null.</summary>
    internal SignedSixteenSixteen? Sinking { get; }

    /// <summary>Names the reaction.</summary>
    public override string ToString() => _name;
}
