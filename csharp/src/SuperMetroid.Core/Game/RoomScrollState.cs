namespace SuperMetroid.Core.Game;

/// <summary>Conventionally named values in the native room scroll-zone byte grid.</summary>
public enum RoomScrollState : byte
{
    /// <summary>An impassable camera boundary; camera motion may not enter this screen cell.</summary>
    RedBoundary = 0,

    /// <summary>A traversable screen cell using the cartridge's blue vertical-alignment behavior.</summary>
    Blue = 1,

    /// <summary>A traversable screen cell using the alternate green boundary behavior installed by room scripts.</summary>
    Green = 2,
}
