using System.Globalization;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// How a picture's name and the installed artwork spell each appearance input (#1466), and the input a spelling names,
/// so the installed records of which inputs each picture depends on read back as the inputs the room reads.
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

    /// <summary>The input <paramref name="token"/> spells, an encounter look's name resolved through <paramref name="lookNamed"/>.</summary>
    /// <exception cref="InvalidDataException">The token spells no appearance input.</exception>
    public static AppearanceInput Parse(string token, Func<string, SpriteArtworkSource> lookNamed)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        Ensure.NotNull(lookNamed);
        if (token == KraidBackdrop)
            return KraidBackdropAppearance.Instance;
        if (token.StartsWith(CollectibleSlotPrefix, StringComparison.Ordinal)
            && int.TryParse(token.AsSpan(CollectibleSlotPrefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int slot))
            return new CollectibleSlotAppearance(slot);
        if (token.StartsWith(AnimationPrefix, StringComparison.Ordinal) && token.Length > AnimationPrefix.Length)
            return new AnimationAppearance(new ArtworkAnimation(token[AnimationPrefix.Length..]));
        if (token.StartsWith(EncounterLookPrefix, StringComparison.Ordinal) && token.Length > EncounterLookPrefix.Length)
            return new EncounterLookAppearance(lookNamed(token[EncounterLookPrefix.Length..]));
        throw new InvalidDataException($"No appearance input is spelled \"{token}\".");
    }
}
