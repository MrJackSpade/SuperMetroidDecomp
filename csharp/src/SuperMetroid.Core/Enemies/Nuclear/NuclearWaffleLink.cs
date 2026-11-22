namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// One persistent link of the Nuclear Waffle's articulated body (#2162): the actor that shows it, a damaging enemy
/// projectile or a cosmetic effect actor, and the turn threshold it last crossed, whose change spawns its joint-turn
/// effect.
/// </summary>
/// <typeparam name="TActor">The link's actor: an enemy projectile or an effect actor.</typeparam>
public sealed partial class NuclearWaffleLink<TActor>
    where TActor : class
{
    /// <summary>Links the waffle's body to <paramref name="actor"/>, before either turn.</summary>
    /// <param name="actor">The actor the waffle spawned for the link at its room load.</param>
    internal NuclearWaffleLink(TActor actor) => Actor = Ensure.NotNull(actor);

    /// <summary>The actor showing the link, which the waffle spawned at its room load and moves every sweep update.</summary>
    public TActor Actor { get; }

    /// <summary>The turn threshold the link last crossed; each sweep starts it before the turns.</summary>
    public NuclearWaffleTurnState TurnState { get; internal set; }
}
