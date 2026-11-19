using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy's placement started a glow for the room's colour effects to run (#1733): the gunship's top hull starts its
/// lights' glow as it is placed, and the glow keeps the gunship it shows on (#1783).
/// </summary>
/// <param name="Glow">The glow and the enemy it shows on.</param>
public sealed record EnemyGlowStarted(EnemyGlowStart Glow) : EnemyPassEvent;
