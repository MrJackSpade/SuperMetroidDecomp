namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Where a population entry places its enemy as the room spawns it (#2558): at a room position, or by its species'
/// setup, as a Zebetite's setup places it where its generation stands. No position stands for an entry its setup places.
/// </summary>
public abstract partial record EnemyPlacing
{
    /// <summary>Only the placings below exist.</summary>
    private protected EnemyPlacing()
    {
    }

    /// <summary>An entry that places its enemy at <paramref name="position"/>, in whole pixels, each coordinate wrapping around the room.</summary>
    /// <param name="position">The enemy's initial room position.</param>
    public static EnemyPlacing At(Position16<RoomPixels> position) => new EnemyPlacedAt(position);

    /// <summary>An entry whose enemy its species' setup places.</summary>
    public static EnemyPlacing BySetup { get; } = new EnemyPlacedBySetup();

    /// <summary>The position a spawn gives an enemy whose place stands at <paramref name="current"/>.</summary>
    /// <param name="current">Where the place stands before the spawn, fraction of a pixel included.</param>
    internal abstract FixedPosition<RoomPixels> Place(FixedPosition<RoomPixels> current);
}
