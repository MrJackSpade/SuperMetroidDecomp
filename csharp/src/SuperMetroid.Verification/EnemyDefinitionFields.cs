using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

/// <summary>The enemy definition header fields a synthetic enemy fixture writes (#633).</summary>
internal readonly record struct EnemyDefinitionFields
{
    /// <summary>Tile data size.</summary>
    public required ushort TileDataSize { get; init; }

    /// <summary>Palette pointer.</summary>
    public required ushort PalettePointer { get; init; }

    /// <summary>Bank.</summary>
    public required byte Bank { get; init; }

    /// <summary>Tile data address.</summary>
    public required int TileDataAddress { get; init; }

    /// <summary>Boss id.</summary>
    public required ushort BossId { get; init; }

    /// <summary>Name pointer.</summary>
    public required ushort NamePointer { get; init; }

    /// <summary>Field seed.</summary>
    public required ushort FieldSeed { get; init; }
}
