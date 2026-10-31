using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses;

/// <summary>The x, y, rectangles and reactions of one boss physical components result.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Boxes">Each hitbox's left, top, right and bottom words, in cartridge record order.</param>
/// <param name="Reactions">Each hitbox's touch reaction and shot reaction.</param>
internal readonly record struct BossPhysicalComponentsResult(short X, short Y, ushort[][] Boxes, EnemyReaction[][] Reactions);
