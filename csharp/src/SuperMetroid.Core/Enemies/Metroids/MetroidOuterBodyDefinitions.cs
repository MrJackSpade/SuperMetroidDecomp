using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A frozen Metroid's two outer-body sprite objects: their frozen programs and palette.</summary>
internal static class MetroidOuterBodyDefinitions
{
    /// <summary>The first outer body's frozen program, <c>InstList_SpriteObject_32_MetroidElectricity</c>.</summary>
    internal const RoomSpriteObjectProgram OuterBodyAFrozenProgram = RoomSpriteObjectProgram.MetroidElectricity;

    /// <summary>The second outer body's frozen program, <c>InstList_SpriteObject_34_MetroidShell</c>.</summary>
    internal const RoomSpriteObjectProgram OuterBodyBFrozenProgram = RoomSpriteObjectProgram.MetroidShell;

    /// <summary>The frozen outer bodies' graphics: the shared sheet in the frozen colours.</summary>
    internal static SpriteGraphics FrozenGraphics { get; } = SpriteGraphics.SharedIn(EnemyShades.FrozenLook);
}
