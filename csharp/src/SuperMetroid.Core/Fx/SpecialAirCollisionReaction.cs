namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a special-air block does when Samus's movement reaches it, as her room's area names the block's reaction
/// (<see cref="SpecialAirReactions.CollisionOf"/>): each reaction states what it does (#2571), so no switch over reaction
/// identities runs it.
/// </summary>
public sealed partial class SpecialAirCollisionReaction
{
    /// <summary>The reaction's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>A reaction named <paramref name="name"/> that does what the other arguments state.</summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    /// <param name="effect">What the block does when Samus's movement reaches it.</param>
    private SpecialAirCollisionReaction(string name, SpecialAirCollisionEffect effect = SpecialAirCollisionEffect.None)
    {
        _name = name;
        _effect = effect;
    }

    /// <summary>What the block does when Samus's movement reaches it.</summary>
    private readonly SpecialAirCollisionEffect _effect;

    /// <summary>Nothing happens: Samus passes on.</summary>
    public static SpecialAirCollisionReaction Nothing { get; } = new(nameof(Nothing));

    /// <summary>Samus passes through the block as through air.</summary>
    public static SpecialAirCollisionReaction PassesThrough { get; } = new(nameof(PassesThrough));

    /// <summary>Brinstar's slow respawning Speed Booster block.</summary>
    public static SpecialAirCollisionReaction BrinstarSlowRespawningSpeedBlock { get; } = new(nameof(BrinstarSlowRespawningSpeedBlock), SpecialAirCollisionEffect.SpeedBlock);

    /// <summary>Brinstar's slow permanent Speed Booster block.</summary>
    public static SpecialAirCollisionReaction BrinstarSlowPermanentSpeedBlock { get; } = new(nameof(BrinstarSlowPermanentSpeedBlock), SpecialAirCollisionEffect.SpeedBlock);

    /// <summary>The Dachora room's respawning Speed Booster block.</summary>
    public static SpecialAirCollisionReaction DachoraRespawningSpeedBlock { get; } = new(nameof(DachoraRespawningSpeedBlock), SpecialAirCollisionEffect.SpeedBlock);

    /// <summary>Brinstar's permanent Speed Booster block.</summary>
    public static SpecialAirCollisionReaction PermanentSpeedBlock { get; } = new(nameof(PermanentSpeedBlock), SpecialAirCollisionEffect.SpeedBlock);

    /// <summary>The Lower Norfair Chozo hand's trigger.</summary>
    public static SpecialAirCollisionReaction LowerNorfairChozoHand { get; } = new(nameof(LowerNorfairChozoHand));

    /// <summary>The Wrecked Ship Chozo hand's trigger.</summary>
    public static SpecialAirCollisionReaction WreckedShipChozoHand { get; } = new(nameof(WreckedShipChozoHand));

    /// <summary>Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    public static SpecialAirCollisionReaction QuicksandSurface { get; } = new(nameof(QuicksandSurface), SpecialAirCollisionEffect.QuicksandSurface);

    /// <summary>Maridia's submerging quicksand, which stops Samus's vertical speed and gravity.</summary>
    public static SpecialAirCollisionReaction SubmergingQuicksand { get; } = new(nameof(SubmergingQuicksand), SpecialAirCollisionEffect.SubmergingQuicksand);

    /// <summary>Maridia's slow sandfall, through which Samus passes.</summary>
    public static SpecialAirCollisionReaction SlowSandfall { get; } = new(nameof(SlowSandfall), SpecialAirCollisionEffect.Sandfall);

    /// <summary>Maridia's fast sandfall, through which Samus passes.</summary>
    public static SpecialAirCollisionReaction FastSandfall { get; } = new(nameof(FastSandfall), SpecialAirCollisionEffect.Sandfall);

    /// <summary>Whether the block is one of the Speed Booster blocks Samus's boosted movement breaks.</summary>
    internal bool IsSpeedBlock => _effect == SpecialAirCollisionEffect.SpeedBlock;

    /// <summary>
    /// Whether the reaction is one of the four collision sand reactions, each of which places a sand reaction object at its
    /// block before it acts (<see cref="QuicksandObjects"/>); every one leaves the room on its first update.
    /// </summary>
    internal bool PlacesSandReaction => _effect is SpecialAirCollisionEffect.QuicksandSurface or SpecialAirCollisionEffect.SubmergingQuicksand
        or SpecialAirCollisionEffect.Sandfall;

    /// <summary>Whether the block is Maridia's quicksand surface, on which a falling or standing Samus lands.</summary>
    internal bool IsQuicksandSurface => _effect == SpecialAirCollisionEffect.QuicksandSurface;

    /// <summary>Whether the block is Maridia's submerging quicksand, which stops Samus's vertical speed and gravity.</summary>
    internal bool StopsFall => _effect == SpecialAirCollisionEffect.SubmergingQuicksand;

    /// <summary>Names the reaction.</summary>
    public override string ToString() => _name;
}
