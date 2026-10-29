using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One of the eight shared sprite colour stores a load places colourings in (#1464): the store a room's graphics set
/// loads a member's colours into, the one a shared role names, the one an encounter fills or the one a screen colour group
/// is. The stores are shared, so a colouring shows whatever its store last held, whoever wrote it, as the escape Climb's
/// silver Ninja Space Pirate shows a stance store its room never loads.
/// </summary>
/// <remarks>
/// Writers name a store by what it holds in gameplay. Only this type, the import pipeline and the native boundary know
/// which number the sprite colour sets give each store.
/// </remarks>
public readonly record struct SpriteColourStore
{
    /// <summary>The common sprite colours a room load fills, which an enemy's hit flash shows.</summary>
    public static readonly SpriteColourStore Common = new(0);

    /// <summary>
    /// The first store a room's graphics set loads its members' colours into, which bosses, the Chozo statues and the
    /// glass tube also fill for themselves.
    /// </summary>
    public static readonly SpriteColourStore EnemiesFirst = new(1);

    /// <summary>The second store a room's graphics set loads its members' colours into, which encounters also fill.</summary>
    public static readonly SpriteColourStore EnemiesSecond = new(2);

    /// <summary>The third store a room's graphics set loads its members' colours into, which encounters also fill.</summary>
    public static readonly SpriteColourStore EnemiesThird = new(3);

    /// <summary>The store whose colours Samus's suit shows over its own.</summary>
    public static readonly SpriteColourStore SamusSuit = new(4);

    /// <summary>The enemy projectiles' colours a room load fills, whose last colour is Samus's grapple flare.</summary>
    public static readonly SpriteColourStore EnemyProjectiles = new(5);

    /// <summary>The store whose colours Samus's beams show over its own, which a frozen enemy shows.</summary>
    public static readonly SpriteColourStore SamusBeams = new(6);

    /// <summary>
    /// The fourth store a room's graphics set loads its members' colours into, which Samus without her suit shows and
    /// bosses fill for their own sprites.
    /// </summary>
    public static readonly SpriteColourStore EnemiesFourth = new(7);

    /// <summary>Every store, in the order the sprite colour sets hold them.</summary>
    public static ImmutableArray<SpriteColourStore> Every { get; } =
        [Common, EnemiesFirst, EnemiesSecond, EnemiesThird, SamusSuit, EnemyProjectiles, SamusBeams, EnemiesFourth];

    /// <summary>Names the store the sprite colour sets number <paramref name="set"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is not one of the sprite colour sets'.</exception>
    internal SpriteColourStore(int set) => Set = Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, set);

    /// <summary>The number the sprite colour sets give this store, which their colours are read and written by.</summary>
    internal int Set { get; }

    /// <summary>This store's colours among <paramref name="sets"/>, read and written in place.</summary>
    internal ColourRun In(IColourSets sets) => Ensure.NotNull(sets).Set(Set);

    /// <summary>The colours of <paramref name="stores"/> among <paramref name="sets"/>, one transfer written store after store.</summary>
    internal static ColourRunSequence InOrder(ImmutableArray<SpriteColourStore> stores, IColourSets sets)
    {
        Ensure.NotNull(sets);
        return new ColourRunSequence([.. stores.Select(store => store.In(sets))]);
    }
}
