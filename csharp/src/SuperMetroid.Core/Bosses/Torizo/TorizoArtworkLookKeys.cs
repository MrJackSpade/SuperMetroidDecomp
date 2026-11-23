namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The installed key of each <see cref="TorizoArtworkLook"/> (#2409, #2548): the name the look's encounter look is
/// installed under, written out so renaming a look's identity never changes a name on disk.
/// </summary>
internal static partial class TorizoArtworkLookKeys
{
    /// <summary>Each TorizoArtworkLook member's installed key.</summary>
    internal static InstalledKeys<TorizoArtworkLook> Installed { get; } = new(new Dictionary<TorizoArtworkLook, string>
    {
        [TorizoArtworkLook.ChozoCrumbled] = "ChozoCrumbled",
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
    });
}
