using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The encounter look a <see cref="TorizoArtworkLook"/> gives the Torizo that shows it (#1761, #2409). Each look's name is
/// its installed key, written out in <see cref="TorizoArtworkLookKeys"/>, so renaming a look's identity never changes a name on disk.
/// </summary>
internal static partial class TorizoArtworkLooks
{
    /// <summary>The look of a Torizo's artwork.</summary>
    extension(TorizoArtworkLook look)
    {
        /// <summary>The look <paramref name="torizo"/> shows when its body animation names this look, by the look's installed name.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The look is no TorizoArtworkLook member.</exception>
        internal EncounterLook Of(EncounterArtwork torizo) => new(torizo, TorizoArtworkLookKeys.Installed.Of(look));
    }
}
