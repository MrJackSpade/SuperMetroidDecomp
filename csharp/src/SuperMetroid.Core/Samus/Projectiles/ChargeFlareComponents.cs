namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The charge-flare components a frame shows, in the order the flare draw advances and draws them (#1859). Each set
/// names its parts and their order outright.
/// </summary>
public sealed class ChargeFlareComponents
{
    /// <summary>A set that shows <paramref name="drawOrder"/>, in that order.</summary>
    /// <param name="drawOrder">The shown components in drawing order.</param>
    private ChargeFlareComponents(params ChargeFlareComponent[] drawOrder) => DrawOrder = Array.AsReadOnly(drawOrder);

    /// <summary>No component: Samus is not charging, or her flare has not begun to show.</summary>
    public static ChargeFlareComponents None { get; } = new();

    /// <summary>The main flare alone, as an ordinary charge first shows it.</summary>
    public static ChargeFlareComponents MainFlareOnly { get; } = new(ChargeFlareComponent.MainFlare);

    /// <summary>The main flare, then the slow sparks, then the fast sparks, as a longer ordinary charge shows them.</summary>
    public static ChargeFlareComponents FlareAndSparks { get; } =
        new(ChargeFlareComponent.MainFlare, ChargeFlareComponent.SlowSparks, ChargeFlareComponent.FastSparks);

    /// <summary>
    /// Hyper Beam's descending flare: the fast sparks first, then the slow sparks, then the main flare.
    /// </summary>
    public static ChargeFlareComponents HyperBeamFlare { get; } =
        new(ChargeFlareComponent.FastSparks, ChargeFlareComponent.SlowSparks, ChargeFlareComponent.MainFlare);

    /// <summary>The shown components in drawing order.</summary>
    public IReadOnlyList<ChargeFlareComponent> DrawOrder { get; }

    /// <summary>Whether the frame shows any component.</summary>
    public bool ShowsAny => DrawOrder.Count != 0;
}
