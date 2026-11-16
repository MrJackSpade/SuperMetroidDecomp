using SuperMetroid.Core.Bosses.Torizo;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// The import's knowledge of the Torizos' artwork looks (#1406, #1463, #1538, #1761): what each look copies over the sprite
/// characters, decoded from the Bomb and Golden Torizo instruction lists' character copies, and the order each Torizo's
/// body animations show the looks in, the order in which each look's copies add to the Torizo's artwork: the import composes
/// every look's finished pictures over the looks before it. A look the player sees may be several copies the cartridge made
/// in one update (#1942): the gut's four, the face's first four and its last five, and each chest stage's two halves.
/// </summary>
internal static class TorizoLookUploadDefinitions
{
    /// <summary>
    /// What each look copies, in the order its copies land: each copy's page of installed characters, the page's byte it
    /// starts at, its bytes and where it lands.
    /// </summary>
    internal static readonly IReadOnlyDictionary<TorizoArtworkLook, TorizoLookUpload[]> Uploads = new Dictionary<TorizoArtworkLook, TorizoLookUpload[]>
    {
        [L.GutBlownUp] =
        [
            new(TorizoTilePage.LeftAttack, 0, 64, 304),
            new(TorizoTilePage.RightAttack, 0, 64, 320),
            new(TorizoTilePage.LeftAttack, 64, 32, 487),
            new(TorizoTilePage.RightAttack, 64, 32, 503),
        ],
        [L.FaceHalfBlownAway] =
        [
            new(TorizoTilePage.LeftAttack, 96, 32, 485),
            new(TorizoTilePage.RightAttack, 96, 32, 501),
            new(TorizoTilePage.SharedDeath, 0, 64, 456),
            new(TorizoTilePage.SharedDeath, 0, 64, 458),
        ],
        [L.FaceBlownUp] =
        [
            new(TorizoTilePage.SharedDeath, 0, 64, 472),
            new(TorizoTilePage.SharedDeath, 0, 64, 474),
            new(TorizoTilePage.SharedDeath, 0, 64, 488),
            new(TorizoTilePage.SharedDeath, 0, 64, 504),
            new(TorizoTilePage.SharedDeath, 0, 32, 415),
        ],
        [L.EyesDark] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyesGlowingFaintly] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyesGlowingBrightly] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyesGlowingFully] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.EyesDarkAgain] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyesGlowingFaintlyAgain] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyesGlowingBrightlyAgain] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyesGlowingFullyAgain] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.Awakening] = [new(TorizoTilePage.GoldenAwakening, 0, 1536, 208)],
        [L.ChestParting] = [new(TorizoTilePage.LeftAttack, 128, 64, 304), new(TorizoTilePage.RightAttack, 128, 64, 320)],
        [L.ChestHalfOpen] = [new(TorizoTilePage.LeftAttack, 192, 64, 304), new(TorizoTilePage.RightAttack, 192, 64, 320)],
        [L.ChestOpen] = [new(TorizoTilePage.LeftAttack, 256, 64, 304), new(TorizoTilePage.RightAttack, 256, 64, 320)],
        [L.ChestHeldOpen] = [new(TorizoTilePage.LeftAttack, 256, 64, 304), new(TorizoTilePage.RightAttack, 256, 64, 320)],
        [L.ChestHalfClosed] = [new(TorizoTilePage.LeftAttack, 192, 64, 304), new(TorizoTilePage.RightAttack, 192, 64, 320)],
        [L.ChestNearlyClosed] = [new(TorizoTilePage.LeftAttack, 128, 64, 304), new(TorizoTilePage.RightAttack, 128, 64, 320)],
        [L.ChestClosed] = [new(TorizoTilePage.GoldenLeftAttack, 0, 64, 304), new(TorizoTilePage.GoldenRightAttack, 0, 64, 320)],
        [L.EyeBeamEyesDark] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyeBeamEyesGlowingFaintly] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyeBeamEyesGlowingBrightly] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyeBeamEyesGlowingFully] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.StunnedEyesDark] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.StunnedEyesGlowingFaintly] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.StunnedEyesGlowingBrightly] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.StunnedEyesGlowingFully] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
    };

    /// <summary>
    /// Bomb Torizo's looks, in the order its animations show them: its eyes lighting up as it wakes and flickering back on,
    /// then its gut blowing up, which its health reaches before its face blowing up.
    /// </summary>
    internal static readonly TorizoArtworkLook[] BombSequence =
    [
        L.EyesDark, L.EyesGlowingFaintly, L.EyesGlowingBrightly, L.EyesGlowingFully,
        L.EyesDarkAgain, L.EyesGlowingFaintlyAgain, L.EyesGlowingBrightlyAgain, L.EyesGlowingFullyAgain,
        L.GutBlownUp, L.FaceHalfBlownAway, L.FaceBlownUp,
    ];

    /// <summary>
    /// Golden Torizo's looks, in the order its animations show them: its awakening, its eyes lighting up and flickering back
    /// on, its chest opening and closing for its egg release, its eyes' glow as its eye beam fires, and their lighting up as
    /// it recovers from a stun.
    /// </summary>
    internal static readonly TorizoArtworkLook[] GoldenSequence =
    [
        L.Awakening,
        L.EyesDark, L.EyesGlowingFaintly, L.EyesGlowingBrightly, L.EyesGlowingFully,
        L.EyesDarkAgain, L.EyesGlowingFaintlyAgain, L.EyesGlowingBrightlyAgain, L.EyesGlowingFullyAgain,
        L.ChestParting, L.ChestHalfOpen, L.ChestOpen, L.ChestHeldOpen, L.ChestHalfClosed, L.ChestNearlyClosed, L.ChestClosed,
        L.EyeBeamEyesDark, L.EyeBeamEyesGlowingFaintly, L.EyeBeamEyesGlowingBrightly, L.EyeBeamEyesGlowingFully,
        L.StunnedEyesDark, L.StunnedEyesGlowingFaintly, L.StunnedEyesGlowingBrightly, L.StunnedEyesGlowingFully,
    ];
}
