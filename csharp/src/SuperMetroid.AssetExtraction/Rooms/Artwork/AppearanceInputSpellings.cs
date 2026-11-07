using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms.Artwork;

/// <summary>
/// Reads back the appearance inputs the installed records spell (#1466, #1502), so the records of which inputs each
/// picture depends on load as the inputs the room reads.
/// </summary>
internal static class AppearanceInputSpellings
{
    /// <summary>The one Kraid backdrop input.</summary>
    internal static KraidBackdropAppearance KraidBackdrop { get; } = new();

    /// <summary>The input <paramref name="token"/> spells.</summary>
    /// <exception cref="InvalidDataException">The token spells no appearance input.</exception>
    internal static AppearanceInput Parse(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        if (token == AppearanceInputTokens.KraidBackdrop)
            return KraidBackdrop;
        if (token.StartsWith(AppearanceInputTokens.AnimationPrefix, StringComparison.Ordinal) && token.Length > AppearanceInputTokens.AnimationPrefix.Length)
            return new AnimationAppearance(new ArtworkAnimation(token[AppearanceInputTokens.AnimationPrefix.Length..]));
        if (token.StartsWith(AppearanceInputTokens.EncounterLookPrefix, StringComparison.Ordinal)
            && token[AppearanceInputTokens.EncounterLookPrefix.Length..].Split(' ') is [string encounterName, string lookName]
            && Enum.TryParse(encounterName, out EncounterArtwork encounter) && Enum.IsDefined(encounter) && lookName.Length > 0)
            return new EncounterLookAppearance(new EncounterLook(encounter, lookName));
        throw new InvalidDataException($"No appearance input is spelled \"{token}\".");
    }
}
