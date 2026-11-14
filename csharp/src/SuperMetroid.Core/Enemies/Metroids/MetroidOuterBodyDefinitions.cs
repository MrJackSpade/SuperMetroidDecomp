using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A frozen Metroid's two outer-body sprite objects: their frozen animations and colours.</summary>
internal static class MetroidOuterBodyDefinitions
{
    /// <summary>The frozen animation of the Metroid's electricity.</summary>
    internal const RoomSpriteObjectSegment ElectricityFrozenAnimation = RoomSpriteObjectSegment.MetroidElectricity;

    /// <summary>The frozen animation of the Metroid's shell.</summary>
    internal const RoomSpriteObjectSegment ShellFrozenAnimation = RoomSpriteObjectSegment.MetroidShell;

    /// <summary>The frozen outer bodies' graphics: the shared sheet in the frozen colours.</summary>
    internal static SpriteGraphics FrozenGraphics { get; } = SpriteGraphics.SharedIn(EnemyShades.FrozenLook);
}
