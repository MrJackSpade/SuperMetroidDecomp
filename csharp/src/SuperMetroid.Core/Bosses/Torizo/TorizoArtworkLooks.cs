using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The encounter look a <see cref="TorizoArtworkLook"/> gives the Torizo that shows it.</summary>
internal static class TorizoArtworkLooks
{
    /// <summary>The look of a Torizo's artwork.</summary>
    extension(TorizoArtworkLook look)
    {
        /// <summary>The look <paramref name="torizo"/> shows when its body animation names this look, by the look's name (#1761).</summary>
        internal EncounterLook Of(EncounterArtwork torizo) => new(torizo, look.ToString());
    }
}
