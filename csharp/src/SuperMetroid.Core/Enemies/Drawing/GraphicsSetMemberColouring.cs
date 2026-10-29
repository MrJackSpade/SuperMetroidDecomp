using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The colours the room's graphics set loads for one of its members (#1464): what an enemy draws in by default, and
/// what an actor that shows another member's colours, such as a projectile drawn in its spawner's, names.
/// </summary>
/// <param name="Enemy">The graphics-set member whose loaded colours are shown.</param>
internal sealed record GraphicsSetMemberColouring(EnemyDefinitionId Enemy) : SpriteColouring;
