using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The encounter look a <see cref="TorizoArtworkLook"/> gives the Torizo that shows it (#1761, #2409). Each look's name is
/// its installed key, written out, so renaming a look's identity never changes a name on disk.
/// </summary>
internal static class TorizoArtworkLooks
{
    /// <summary>Each look's installed name.</summary>
    private static readonly FrozenDictionary<TorizoArtworkLook, string> Names =
        new Dictionary<TorizoArtworkLook, string>
        {
            [TorizoArtworkLook.EyesDark] = "EyesDark",
            [TorizoArtworkLook.EyesGlowingFaintly] = "EyesGlowingFaintly",
            [TorizoArtworkLook.EyesGlowingBrightly] = "EyesGlowingBrightly",
            [TorizoArtworkLook.EyesGlowingFully] = "EyesGlowingFully",
            [TorizoArtworkLook.EyesDarkAgain] = "EyesDarkAgain",
            [TorizoArtworkLook.EyesGlowingFaintlyAgain] = "EyesGlowingFaintlyAgain",
            [TorizoArtworkLook.EyesGlowingBrightlyAgain] = "EyesGlowingBrightlyAgain",
            [TorizoArtworkLook.EyesGlowingFullyAgain] = "EyesGlowingFullyAgain",
            [TorizoArtworkLook.GutBlownUp] = "GutBlownUp",
            [TorizoArtworkLook.FaceHalfBlownAway] = "FaceHalfBlownAway",
            [TorizoArtworkLook.FaceBlownUp] = "FaceBlownUp",
            [TorizoArtworkLook.Awakening] = "Awakening",
            [TorizoArtworkLook.ChestParting] = "ChestParting",
            [TorizoArtworkLook.ChestHalfOpen] = "ChestHalfOpen",
            [TorizoArtworkLook.ChestOpen] = "ChestOpen",
            [TorizoArtworkLook.ChestHeldOpen] = "ChestHeldOpen",
            [TorizoArtworkLook.ChestHalfClosed] = "ChestHalfClosed",
            [TorizoArtworkLook.ChestNearlyClosed] = "ChestNearlyClosed",
            [TorizoArtworkLook.ChestClosed] = "ChestClosed",
            [TorizoArtworkLook.EyeBeamEyesDark] = "EyeBeamEyesDark",
            [TorizoArtworkLook.EyeBeamEyesGlowingFaintly] = "EyeBeamEyesGlowingFaintly",
            [TorizoArtworkLook.EyeBeamEyesGlowingBrightly] = "EyeBeamEyesGlowingBrightly",
            [TorizoArtworkLook.EyeBeamEyesGlowingFully] = "EyeBeamEyesGlowingFully",
            [TorizoArtworkLook.StunnedEyesDark] = "StunnedEyesDark",
            [TorizoArtworkLook.StunnedEyesGlowingFaintly] = "StunnedEyesGlowingFaintly",
            [TorizoArtworkLook.StunnedEyesGlowingBrightly] = "StunnedEyesGlowingBrightly",
            [TorizoArtworkLook.StunnedEyesGlowingFully] = "StunnedEyesGlowingFully",
        }.ToFrozenDictionary();

    /// <summary>The look of a Torizo's artwork.</summary>
    extension(TorizoArtworkLook look)
    {
        /// <summary>The look <paramref name="torizo"/> shows when its body animation names this look, by the look's installed name.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The look has no installed name.</exception>
        internal EncounterLook Of(EncounterArtwork torizo) => new(torizo, Ensure.Listed(Names, look));
    }
}
