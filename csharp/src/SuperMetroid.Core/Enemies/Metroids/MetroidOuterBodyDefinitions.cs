using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A frozen Metroid's two outer-body sprite objects: their frozen programs and palette.</summary>
internal static class MetroidOuterBodyDefinitions
{
    /// <summary>The first outer body's frozen program, <c>InstList_SpriteObject_32_MetroidElectricity</c>.</summary>
    internal const RoomSpriteObjectProgram OuterBodyAFrozenProgram = RoomSpriteObjectProgram.MetroidElectricity;

    /// <summary>The second outer body's frozen program, <c>InstList_SpriteObject_34_MetroidShell</c>.</summary>
    internal const RoomSpriteObjectProgram OuterBodyBFrozenProgram = RoomSpriteObjectProgram.MetroidShell;

    /// <summary>The frozen outer bodies' graphics: character zero in the frozen palette row six.</summary>
    internal static SpriteGraphics FrozenGraphics { get; } = SpriteGraphics.InPalette(6);
}
