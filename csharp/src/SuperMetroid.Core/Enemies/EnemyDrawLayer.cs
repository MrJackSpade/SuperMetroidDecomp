namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The layer an enemy draws on, one of eight in draw order: enemies draw layer by layer, lower layers first, and the
/// room's draw passes place Samus, her projectiles and the room's foreground between ranges of layers.
/// </summary>
public readonly record struct EnemyDrawLayer
{
    /// <summary>The number of layers enemies draw on.</summary>
    public const int Count = 8;

    /// <summary>Keeps a layer already checked to be one of the eight.</summary>
    private EnemyDrawLayer(int order) => Order = order;

    /// <summary>The layer's place in the draw order, 0 through 7; lower layers draw first. The default is the first.</summary>
    public int Order { get; }

    /// <summary>The layer at place <paramref name="order"/> in the draw order.</summary>
    /// <param name="order">The layer's place in the draw order, 0 through 7.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is not one of the eight layers.</exception>
    public static EnemyDrawLayer Of(int order) => new(Ensure.BetweenInclusive(0, Count - 1, order));
}
