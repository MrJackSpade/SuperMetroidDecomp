namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>A frozen Metroid's two outer-body sprite objects: their native frozen lists and palette.</summary>
internal static class MetroidOuterBodyDefinitions
{
    /// <summary>The first outer body's frozen instruction list, <c>InstList_SpriteObject_32_MetroidElectricity</c> at $B4:C3BA.</summary>
    internal const ushort OuterBodyAFrozenInstructionList = 0xc3ba;

    /// <summary>The second outer body's frozen instruction list, <c>InstList_SpriteObject_34_MetroidShell</c> at $B4:C4B6.</summary>
    internal const ushort OuterBodyBFrozenInstructionList = 0xc4b6;

    /// <summary>The frozen outer bodies' sprite palette bits.</summary>
    internal const ushort FrozenSpritePalette = 0x0c00;
}
