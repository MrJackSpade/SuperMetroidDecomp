using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Reading a shade back from the key the sheet manifest stores it under (#1512).</summary>
internal static class PictureShadeKeys
{
    /// <summary>The shade an installed key names.</summary>
    extension(PictureShade)
    {
        /// <summary>The shade an installed key names.</summary>
        /// <exception cref="InvalidDataException">The key is not an effect name, a slash and a step.</exception>
        internal static PictureShade FromKey(string key)
        {
            ArgumentException.ThrowIfNullOrEmpty(key);
            int slash = key.LastIndexOf('/');
            return slash > 0 && int.TryParse(key.AsSpan(slash + 1), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out int step)
                ? new PictureShade(key[..slash], step)
                : throw new InvalidDataException($"Shade key {key} is not an effect name, a slash and a step.");
        }
    }
}
