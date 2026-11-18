using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyRoomEffects"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyRoomEffectsDependencies
{
    /// <summary>The escape countdown, which Mother Brain's defeat starts.</summary>
    public required EscapeTimer EscapeTimer { get; init; }

    /// <summary>The session's shared game state, which records the bosses the enemies' requests mark defeated and the Zebes timebomb.</summary>
    public required SharedGameState System { get; init; }
}
