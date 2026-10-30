using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The shape of the Ceres door's installed visuals (#1415): how many characters, colours and animation rows the door
/// shows. The elevator platform's strips are pictures of the room's floor (#1467).
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
}
