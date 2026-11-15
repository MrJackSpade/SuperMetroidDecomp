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
        [L.EyesLightingUp1] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyesLightingUp2] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyesLightingUp3] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyesLightingUp4] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.EyesLightingAgain1] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyesLightingAgain2] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyesLightingAgain3] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyesLightingAgain4] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.Awakening] = [new(TorizoTilePage.GoldenAwakening, 0, 1536, 208)],
        [L.ChestOpening1] = [new(TorizoTilePage.LeftAttack, 128, 64, 304), new(TorizoTilePage.RightAttack, 128, 64, 320)],
        [L.ChestOpening2] = [new(TorizoTilePage.LeftAttack, 192, 64, 304), new(TorizoTilePage.RightAttack, 192, 64, 320)],
        [L.ChestOpen] = [new(TorizoTilePage.LeftAttack, 256, 64, 304), new(TorizoTilePage.RightAttack, 256, 64, 320)],
        [L.ChestHeldOpen] = [new(TorizoTilePage.LeftAttack, 256, 64, 304), new(TorizoTilePage.RightAttack, 256, 64, 320)],
        [L.ChestClosing1] = [new(TorizoTilePage.LeftAttack, 192, 64, 304), new(TorizoTilePage.RightAttack, 192, 64, 320)],
        [L.ChestClosing2] = [new(TorizoTilePage.LeftAttack, 128, 64, 304), new(TorizoTilePage.RightAttack, 128, 64, 320)],
        [L.ChestClosed] = [new(TorizoTilePage.GoldenLeftAttack, 0, 64, 304), new(TorizoTilePage.GoldenRightAttack, 0, 64, 320)],
        [L.EyeBeamGlow1] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyeBeamGlow2] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyeBeamGlow3] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyeBeamGlow4] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
        [L.EyesLightingAfterStun1] = [new(TorizoTilePage.StatueCrumble, 0, 64, 472)],
        [L.EyesLightingAfterStun2] = [new(TorizoTilePage.StatueCrumble, 64, 64, 472)],
        [L.EyesLightingAfterStun3] = [new(TorizoTilePage.StatueCrumble, 128, 64, 472)],
        [L.EyesLightingAfterStun4] = [new(TorizoTilePage.StatueCrumble, 192, 64, 472)],
    };

    /// <summary>
    /// Bomb Torizo's looks, in the order its animations show them: its eyes lighting up as it wakes and flickering back on,
    /// then its gut blowing up, which its health reaches before its face blowing up.
    /// </summary>
    internal static readonly TorizoArtworkLook[] BombSequence =
    [
        L.EyesLightingUp1, L.EyesLightingUp2, L.EyesLightingUp3, L.EyesLightingUp4,
        L.EyesLightingAgain1, L.EyesLightingAgain2, L.EyesLightingAgain3, L.EyesLightingAgain4,
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
        L.EyesLightingUp1, L.EyesLightingUp2, L.EyesLightingUp3, L.EyesLightingUp4,
        L.EyesLightingAgain1, L.EyesLightingAgain2, L.EyesLightingAgain3, L.EyesLightingAgain4,
        L.ChestOpening1, L.ChestOpening2, L.ChestOpen, L.ChestHeldOpen, L.ChestClosing1, L.ChestClosing2, L.ChestClosed,
        L.EyeBeamGlow1, L.EyeBeamGlow2, L.EyeBeamGlow3, L.EyeBeamGlow4,
        L.EyesLightingAfterStun1, L.EyesLightingAfterStun2, L.EyesLightingAfterStun3, L.EyesLightingAfterStun4,
    ];
}
