namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A reaction an enemy has to Samus's touch, to her shots and bombs, or to her Power Bombs. The engine shares four: the
/// ordinary touch and shot reactions, the dud shot and the reaction that does nothing. A definition names one of them for
/// each kind of contact, or names none and leaves the contact to its species' entity, which reacts in its own way or not
/// at all (#1759). A species whose frames' hitboxes tell several of its own reactions apart declares them beside its
/// hitboxes (<see cref="OfSpecies"/>); only that species' code reads them. Each reaction is its own identity.
/// </summary>
public sealed class EnemyReaction
{
    /// <summary>Creates the reaction named <paramref name="name"/>.</summary>
    private EnemyReaction(string name) => Name = name;

    /// <summary>The ordinary contact reaction: Samus takes the definition's damage, or her contact attack damages the enemy.</summary>
    public static EnemyReaction NormalEnemyTouch { get; } = new(nameof(NormalEnemyTouch));

    /// <summary>The ordinary shot reaction: the enemy takes the weapon's damage by its vulnerabilities.</summary>
    public static EnemyReaction NormalEnemyShot { get; } = new(nameof(NormalEnemyShot));

    /// <summary>The shot glances off as a dud.</summary>
    public static EnemyReaction DudShot { get; } = new(nameof(DudShot));

    /// <summary>
    /// The reaction that does nothing. Every reaction that returns at once is this one reaction, whichever enemy names
    /// it; whether the engine still tests an enemy's extended hitboxes is the definition's separate
    /// <see cref="EnemyHitboxScan"/>, not a property of this reaction.
    /// </summary>
    public static EnemyReaction Ignore { get; } = new(nameof(Ignore));

    /// <summary>The engine's shared reactions, which a definition may name.</summary>
    public static IReadOnlyList<EnemyReaction> Shared { get; } = [NormalEnemyTouch, NormalEnemyShot, DudShot, Ignore];

    /// <summary>The reaction's name, for diagnostics.</summary>
    public string Name { get; }

    /// <summary>
    /// A reaction of one species' own that its frames' hitboxes name, which only that species' handlers tell apart from
    /// its other reactions. Each call is a new identity; a species declares each of its reactions once.
    /// </summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    internal static EnemyReaction OfSpecies(string name) => new(Ensure.NotNull(name));

    /// <inheritdoc/>
    public override string ToString() => Name;
}
