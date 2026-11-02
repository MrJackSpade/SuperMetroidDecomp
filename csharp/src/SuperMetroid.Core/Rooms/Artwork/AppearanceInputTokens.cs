namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// How a picture's name and the installed artwork spell each appearance input (#1466); the import reads the spellings
/// back.
/// </summary>
public static class AppearanceInputTokens
{
    /// <summary>What a collectible slot's spelling begins with, before the slot.</summary>
    internal const string CollectibleSlotPrefix = "collectible-slot-";

    /// <summary>Kraid's backdrop's spelling.</summary>
    internal const string KraidBackdrop = "kraid-backdrop";

    /// <summary>What an animation's spelling begins with, before its name.</summary>
    internal const string AnimationPrefix = "animation ";

    /// <summary>What an encounter look's spelling begins with, before its name.</summary>
    internal const string EncounterLookPrefix = "look ";
}
