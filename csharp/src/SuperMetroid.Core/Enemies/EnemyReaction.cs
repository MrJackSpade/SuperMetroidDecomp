namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A reaction an enemy has to Samus's touch, to her shots and bombs, or to her Power Bombs. The engine shares four: the
/// ordinary touch and shot reactions, the dud shot and the reaction that does nothing. A definition names one of them for
/// each kind of contact, or names its species' own (<see cref="OfSpecies"/>), which its species' entity answers in its
/// own way (#1759, #2558); for a Power Bomb it names the ignoring reaction or the common damage
/// (<see cref="CommonPowerBomb"/>). A species whose frames' hitboxes tell several of its own reactions apart declares them
/// beside its hitboxes; only that species' code reads them. Each reaction is its own identity.
/// </summary>
public sealed class EnemyReaction
{
    /// <summary>Creates the reaction named <paramref name="name"/>.</summary>
    private EnemyReaction(string name) => Name = name;

    /// <summary>The ordinary contact reaction: Samus takes the definition's damage, or her contact attack damages the enemy.</summary>
    public static EnemyReaction OrdinaryTouch { get; } = new(nameof(OrdinaryTouch));

    /// <summary>The ordinary shot reaction: the enemy takes the weapon's damage by its vulnerabilities.</summary>
    public static EnemyReaction OrdinaryShot { get; } = new(nameof(OrdinaryShot));

    /// <summary>The shot glances off as a dud.</summary>
    public static EnemyReaction DudShot { get; } = new(nameof(DudShot));

    /// <summary>
    /// The reaction that does nothing. Every reaction that returns at once is this one reaction, whichever enemy names
    /// it; whether the engine still tests an enemy's extended hitboxes is the definition's separate
    /// <see cref="EnemyHitboxScan"/>, not a property of this reaction.
    /// </summary>
    public static EnemyReaction Ignore { get; } = new(nameof(Ignore));

    /// <summary>
    /// The common Power Bomb reaction (#2558): the definition names no shared one, so the blast's common damage applies,
    /// which the enemy's species may replace or add stages to.
    /// </summary>
    public static EnemyReaction CommonPowerBomb { get; } = new(nameof(CommonPowerBomb));

    /// <summary>The reaction's name, for diagnostics.</summary>
    public string Name { get; }

    /// <summary>
    /// A reaction of one species' own that its definition or its frames' hitboxes name, which only that species' handlers
    /// tell apart from its other reactions. Each reaction made is a new identity; a species declares each of its reactions
    /// once.
    /// </summary>
    /// <param name="name">The reaction's name, for diagnostics.</param>
    internal static EnemyReaction OfSpecies(string name) => new(Ensure.NotNull(name));

    /// <inheritdoc/>
    public override string ToString() => Name;
}
