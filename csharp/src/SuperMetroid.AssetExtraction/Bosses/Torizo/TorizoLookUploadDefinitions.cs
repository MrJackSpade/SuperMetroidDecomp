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
    /// <summary>The dark eyes' copy of the statue-crumble characters over the eyes, the first eye-glow stage.</summary>
    private static TorizoLookUpload EyesDarkUpload { get; } = new(TorizoTilePage.StatueCrumble, 0, 64, 472);

    /// <summary>The faintly glowing eyes' copy, the second eye-glow stage.</summary>
    private static TorizoLookUpload EyesGlowingFaintlyUpload { get; } = new(TorizoTilePage.StatueCrumble, 64, 64, 472);

    /// <summary>The brightly glowing eyes' copy, the third eye-glow stage.</summary>
    private static TorizoLookUpload EyesGlowingBrightlyUpload { get; } = new(TorizoTilePage.StatueCrumble, 128, 64, 472);

    /// <summary>The fully glowing eyes' copy, the last eye-glow stage.</summary>
    private static TorizoLookUpload EyesGlowingFullyUpload { get; } = new(TorizoTilePage.StatueCrumble, 192, 64, 472);

    /// <summary>The chest's two halves parting, or nearly closed again, from the attack pages.</summary>
    private static TorizoLookUpload[] ChestPartingUploads { get; } = [new(TorizoTilePage.LeftAttack, 128, 64, 304), new(TorizoTilePage.RightAttack, 128, 64, 320)];

    /// <summary>The chest's two halves half open, opening or closing.</summary>
    private static TorizoLookUpload[] ChestHalfOpenUploads { get; } = [new(TorizoTilePage.LeftAttack, 192, 64, 304), new(TorizoTilePage.RightAttack, 192, 64, 320)];

    /// <summary>The chest's two halves open, and held open.</summary>
    private static TorizoLookUpload[] ChestOpenUploads { get; } = [new(TorizoTilePage.LeftAttack, 256, 64, 304), new(TorizoTilePage.RightAttack, 256, 64, 320)];

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
        [L.EyesDark] = [EyesDarkUpload],
        [L.EyesGlowingFaintly] = [EyesGlowingFaintlyUpload],
        [L.EyesGlowingBrightly] = [EyesGlowingBrightlyUpload],
        [L.EyesGlowingFully] = [EyesGlowingFullyUpload],
        [L.EyesDarkAgain] = [EyesDarkUpload],
        [L.EyesGlowingFaintlyAgain] = [EyesGlowingFaintlyUpload],
        [L.EyesGlowingBrightlyAgain] = [EyesGlowingBrightlyUpload],
        [L.EyesGlowingFullyAgain] = [EyesGlowingFullyUpload],
        [L.Awakening] = [new(TorizoTilePage.GoldenAwakening, 0, 1536, 208)],
        [L.ChestParting] = ChestPartingUploads,
        [L.ChestHalfOpen] = ChestHalfOpenUploads,
        [L.ChestOpen] = ChestOpenUploads,
        [L.ChestHeldOpen] = ChestOpenUploads,
        [L.ChestHalfClosed] = ChestHalfOpenUploads,
        [L.ChestNearlyClosed] = ChestPartingUploads,
        [L.ChestClosed] = [new(TorizoTilePage.GoldenLeftAttack, 0, 64, 304), new(TorizoTilePage.GoldenRightAttack, 0, 64, 320)],
        [L.EyeBeamEyesDark] = [EyesDarkUpload],
        [L.EyeBeamEyesGlowingFaintly] = [EyesGlowingFaintlyUpload],
        [L.EyeBeamEyesGlowingBrightly] = [EyesGlowingBrightlyUpload],
        [L.EyeBeamEyesGlowingFully] = [EyesGlowingFullyUpload],
        [L.StunnedEyesDark] = [EyesDarkUpload],
        [L.StunnedEyesGlowingFaintly] = [EyesGlowingFaintlyUpload],
        [L.StunnedEyesGlowingBrightly] = [EyesGlowingBrightlyUpload],
        [L.StunnedEyesGlowingFully] = [EyesGlowingFullyUpload],
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
