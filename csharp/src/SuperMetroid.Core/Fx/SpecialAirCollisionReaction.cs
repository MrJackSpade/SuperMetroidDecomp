namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does when Samus's movement reaches it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.CollisionOf"/>): each reaction states what it does (#2571), so no switch over reaction
/// identities runs it.
/// </summary>
public sealed class SpecialAirCollisionReaction
{
    /// <summary>The reaction's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>A reaction named <paramref name="name"/> that does what the other arguments state.</summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    /// <param name="speedBlock">Whether the block is one of the Speed Booster blocks.</param>
    /// <param name="placesSandReaction">Whether the reaction places a sand reaction object before it acts.</param>
    /// <param name="quicksandSurface">Whether the block is the quicksand surface, on which a falling or standing Samus lands.</param>
    /// <param name="stopsFall">Whether the block stops Samus's vertical speed and gravity.</param>
    private SpecialAirCollisionReaction(string name, bool speedBlock = false, bool placesSandReaction = false,
        bool quicksandSurface = false, bool stopsFall = false)
    {
        _name = name;
        IsSpeedBlock = speedBlock;
        PlacesSandReaction = placesSandReaction;
        IsQuicksandSurface = quicksandSurface;
        StopsFall = stopsFall;
    }

    /// <summary>Nothing happens: Samus passes on.</summary>
    public static SpecialAirCollisionReaction Nothing { get; } = new(nameof(Nothing));

    /// <summary>Samus passes through the block as through air.</summary>
    public static SpecialAirCollisionReaction PassesThrough { get; } = new(nameof(PassesThrough));

    /// <summary>Brinstar's slow respawning Speed Booster block.</summary>
    public static SpecialAirCollisionReaction BrinstarSlowRespawningSpeedBlock { get; } = new(nameof(BrinstarSlowRespawningSpeedBlock), speedBlock: true);

    /// <summary>Brinstar's slow permanent Speed Booster block.</summary>
    public static SpecialAirCollisionReaction BrinstarSlowPermanentSpeedBlock { get; } = new(nameof(BrinstarSlowPermanentSpeedBlock), speedBlock: true);

    /// <summary>The Dachora room's respawning Speed Booster block.</summary>
    public static SpecialAirCollisionReaction DachoraRespawningSpeedBlock { get; } = new(nameof(DachoraRespawningSpeedBlock), speedBlock: true);

    /// <summary>Brinstar's permanent Speed Booster block.</summary>
    public static SpecialAirCollisionReaction PermanentSpeedBlock { get; } = new(nameof(PermanentSpeedBlock), speedBlock: true);

    /// <summary>The Lower Norfair Chozo hand's trigger.</summary>
    public static SpecialAirCollisionReaction LowerNorfairChozoHand { get; } = new(nameof(LowerNorfairChozoHand));

    /// <summary>The Wrecked Ship Chozo hand's trigger.</summary>
    public static SpecialAirCollisionReaction WreckedShipChozoHand { get; } = new(nameof(WreckedShipChozoHand));

    /// <summary>Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    public static SpecialAirCollisionReaction QuicksandSurface { get; } = new(nameof(QuicksandSurface), placesSandReaction: true, quicksandSurface: true);

    /// <summary>Maridia's submerging quicksand, which stops Samus's vertical speed and gravity.</summary>
    public static SpecialAirCollisionReaction SubmergingQuicksand { get; } = new(nameof(SubmergingQuicksand), placesSandReaction: true, stopsFall: true);

    /// <summary>Maridia's slow sandfall, through which Samus passes.</summary>
    public static SpecialAirCollisionReaction SlowSandfall { get; } = new(nameof(SlowSandfall), placesSandReaction: true);

    /// <summary>Maridia's fast sandfall, through which Samus passes.</summary>
    public static SpecialAirCollisionReaction FastSandfall { get; } = new(nameof(FastSandfall), placesSandReaction: true);

    /// <summary>Whether the block is one of the Speed Booster blocks Samus's boosted movement breaks.</summary>
    internal bool IsSpeedBlock { get; }

    /// <summary>
    /// Whether the reaction is one of the four collision sand reactions, each of which places a sand reaction object at its
    /// block before it acts (<see cref="QuicksandObjects"/>); every one leaves the room on its first update.
    /// </summary>
    internal bool PlacesSandReaction { get; }

    /// <summary>Whether the block is Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    internal bool IsQuicksandSurface { get; }

    /// <summary>Whether the block is Maridia's submerging quicksand, which stops Samus's vertical speed and gravity.</summary>
    internal bool StopsFall { get; }

    /// <summary>Names the reaction.</summary>
    public override string ToString() => _name;
}
