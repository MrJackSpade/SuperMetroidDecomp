using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>The bosses defeated in one area of a saved game.</summary>
/// <param name="Area">The area.</param>
/// <param name="Bosses">Its defeated bosses.</param>
public sealed record SaveAreaBossesJsonDocument(AreaId Area, BossBits Bosses);
