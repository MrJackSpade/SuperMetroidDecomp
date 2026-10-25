using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The shape of the Ceres door's installed visuals (#1415): how many characters, colours and animation rows the door
/// shows, and the two elevator-platform strips the rotating-elevator door draws onto the floor.
/// </summary>
public static class CeresDoorVisualDefinitions
{
    /// <summary>The door's characters, which its second variant shows over the sprite characters.</summary>
    public const int CharacterCount = 32;

    /// <summary>The bytes of the door's four-bit characters.</summary>
    public const int CharacterBytes = CharacterCount * RoomCharacterAtlasFormat.BytesPerTile;

    /// <summary>The visible colours of either setup appearance, normal or escape; colour zero stays transparent.</summary>
    public const int SetupColorCount = 15;

    /// <summary>The rows of the door's glow cycle, selected by its animation frame.</summary>
    public const int AnimationRowCount = 8;

    /// <summary>The colours each glow row replaces.</summary>
    public const int AnimationColorCount = 6;

    /// <summary>The elevator platform's two alternating strips: light, then dark.</summary>
    public const int PlatformFrameCount = 2;

    /// <summary>The floor cells each platform strip covers.</summary>
    public const int PlatformCellCount = 4;

    /// <summary>The floor cell both platform strips start at: row twelve, column fourteen.</summary>
    public const int PlatformFloorCell = 12 * FloorMap.Side + 14;
}
