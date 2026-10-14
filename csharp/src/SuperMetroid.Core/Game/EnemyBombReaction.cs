namespace SuperMetroid.Core.Game;

/// <summary>
/// A species reaction to a normal bomb that enters common bomb damage. The bomb pass runs its stages around the
/// common damage at the cartridge's positions; a reaction sets only the stages it has.
/// </summary>
/// <param name="matches">Whether the overlap uses this reaction (definition and selected callback).</param>
internal sealed class EnemyBombReaction(Func<EnemyBombContext, bool> matches)
{
    /// <summary>Whether the overlap uses this reaction.</summary>
    internal Func<EnemyBombContext, bool> Matches { get; } = Ensure.NotNull(matches);

    /// <summary>Whether the private callback returns before common damage (the bomb stays marked).</summary>
    internal Func<EnemyBombContext, bool>? Rejects { get; init; }

    /// <summary>Whether common damage runs the generic death animation on a lethal hit.</summary>
    internal bool RunsGenericDeath { get; init; } = true;

    /// <summary>Runs immediately before common damage.</summary>
    internal Action<EnemyBombContext>? BeforeCommon { get; init; }

    /// <summary>The species tail after common damage.</summary>
    internal Action<EnemyBombContext>? AfterCommon { get; init; }
}
