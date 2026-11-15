using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="EnemyRoomEffects"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record EnemyRoomEffectsDependencies
{
    /// <summary>The escape countdown, which Mother Brain's defeat starts.</summary>
    public required EscapeTimer EscapeTimer { get; init; }

    /// <summary>Samus, whose equipment the enemies' requests read and whose drained get-up the escape ends.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The loaded room: its level, area, effect and scrolling, which the enemies' requests change.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The session's shared game state, which records the bosses the enemies' requests mark defeated and the Zebes timebomb.</summary>
    public required SharedGameState System { get; init; }
}
