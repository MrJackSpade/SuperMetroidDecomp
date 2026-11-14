using SuperMetroid.Core.Bosses.Torizo;
using L = SuperMetroid.Core.Bosses.Torizo.TorizoArtworkLook;

namespace SuperMetroid.AssetExtraction.Bosses.Torizo;

/// <summary>
/// The import's knowledge of the Torizos' artwork looks (#1406, #1463, #1538, #1761): what each look copies over the sprite
/// characters, decoded from the Bomb and Golden Torizo instruction lists' character copies, and the order each Torizo's
/// body animations show the looks in, the order in which each look's copy adds to the Torizo's artwork: the import composes
/// every look's finished pictures over the looks before it.
/// </summary>
internal static class TorizoLookUploadDefinitions
{
    /// <summary>What each look copies: its page of installed characters, the page's byte it starts at, its bytes and where it lands.</summary>
    internal static readonly IReadOnlyDictionary<TorizoArtworkLook, TorizoLookUpload> Uploads = new Dictionary<TorizoArtworkLook, TorizoLookUpload>
    {
        [L.GutBlowingUp1] = new(TorizoTilePage.LeftAttack, 0, 64, 304),
        [L.GutBlowingUp2] = new(TorizoTilePage.RightAttack, 0, 64, 320),
        [L.GutBlowingUp3] = new(TorizoTilePage.LeftAttack, 64, 32, 487),
        [L.GutBlownUp] = new(TorizoTilePage.RightAttack, 64, 32, 503),
        [L.FaceBlowingUp1] = new(TorizoTilePage.LeftAttack, 96, 32, 485),
        [L.FaceBlowingUp2] = new(TorizoTilePage.RightAttack, 96, 32, 501),
        [L.FaceBlowingUp3] = new(TorizoTilePage.SharedDeath, 0, 64, 456),
        [L.FaceBlowingUp4] = new(TorizoTilePage.SharedDeath, 0, 64, 458),
        [L.FaceBlowingUp5] = new(TorizoTilePage.SharedDeath, 0, 64, 472),
        [L.FaceBlowingUp6] = new(TorizoTilePage.SharedDeath, 0, 64, 474),
        [L.FaceBlowingUp7] = new(TorizoTilePage.SharedDeath, 0, 64, 488),
        [L.FaceBlowingUp8] = new(TorizoTilePage.SharedDeath, 0, 64, 504),
        [L.FaceBlownUp] = new(TorizoTilePage.SharedDeath, 0, 32, 415),
        [L.EyesLightingUp1] = new(TorizoTilePage.StatueCrumble, 0, 64, 472),
        [L.EyesLightingUp2] = new(TorizoTilePage.StatueCrumble, 64, 64, 472),
        [L.EyesLightingUp3] = new(TorizoTilePage.StatueCrumble, 128, 64, 472),
        [L.EyesLightingUp4] = new(TorizoTilePage.StatueCrumble, 192, 64, 472),
        [L.EyesLightingAgain1] = new(TorizoTilePage.StatueCrumble, 0, 64, 472),
        [L.EyesLightingAgain2] = new(TorizoTilePage.StatueCrumble, 64, 64, 472),
        [L.EyesLightingAgain3] = new(TorizoTilePage.StatueCrumble, 128, 64, 472),
        [L.EyesLightingAgain4] = new(TorizoTilePage.StatueCrumble, 192, 64, 472),
        [L.Awakening] = new(TorizoTilePage.GoldenAwakening, 0, 1536, 208),
        [L.ChestOpening1Top] = new(TorizoTilePage.LeftAttack, 128, 64, 304),
        [L.ChestOpening1] = new(TorizoTilePage.RightAttack, 128, 64, 320),
        [L.ChestOpening2Top] = new(TorizoTilePage.LeftAttack, 192, 64, 304),
        [L.ChestOpening2] = new(TorizoTilePage.RightAttack, 192, 64, 320),
        [L.ChestOpenTop] = new(TorizoTilePage.LeftAttack, 256, 64, 304),
        [L.ChestOpen] = new(TorizoTilePage.RightAttack, 256, 64, 320),
        [L.ChestHeldOpenTop] = new(TorizoTilePage.LeftAttack, 256, 64, 304),
        [L.ChestHeldOpen] = new(TorizoTilePage.RightAttack, 256, 64, 320),
        [L.ChestClosing1Top] = new(TorizoTilePage.LeftAttack, 192, 64, 304),
        [L.ChestClosing1] = new(TorizoTilePage.RightAttack, 192, 64, 320),
        [L.ChestClosing2Top] = new(TorizoTilePage.LeftAttack, 128, 64, 304),
        [L.ChestClosing2] = new(TorizoTilePage.RightAttack, 128, 64, 320),
        [L.ChestClosedTop] = new(TorizoTilePage.GoldenLeftAttack, 0, 64, 304),
        [L.ChestClosed] = new(TorizoTilePage.GoldenRightAttack, 0, 64, 320),
        [L.EyeBeamGlow1] = new(TorizoTilePage.StatueCrumble, 0, 64, 472),
        [L.EyeBeamGlow2] = new(TorizoTilePage.StatueCrumble, 64, 64, 472),
        [L.EyeBeamGlow3] = new(TorizoTilePage.StatueCrumble, 128, 64, 472),
        [L.EyeBeamGlow4] = new(TorizoTilePage.StatueCrumble, 192, 64, 472),
        [L.EyesLightingAfterStun1] = new(TorizoTilePage.StatueCrumble, 0, 64, 472),
        [L.EyesLightingAfterStun2] = new(TorizoTilePage.StatueCrumble, 64, 64, 472),
        [L.EyesLightingAfterStun3] = new(TorizoTilePage.StatueCrumble, 128, 64, 472),
        [L.EyesLightingAfterStun4] = new(TorizoTilePage.StatueCrumble, 192, 64, 472),
    };

    /// <summary>
    /// Bomb Torizo's looks, in the order its animations show them: its eyes lighting up as it wakes and flickering back on,
    /// then its gut blowing up, which its health reaches before its face blowing up.
    /// </summary>
    internal static readonly TorizoArtworkLook[] BombSequence =
    [
        L.EyesLightingUp1, L.EyesLightingUp2, L.EyesLightingUp3, L.EyesLightingUp4,
        L.EyesLightingAgain1, L.EyesLightingAgain2, L.EyesLightingAgain3, L.EyesLightingAgain4,
        L.GutBlowingUp1, L.GutBlowingUp2, L.GutBlowingUp3, L.GutBlownUp,
        L.FaceBlowingUp1, L.FaceBlowingUp2, L.FaceBlowingUp3, L.FaceBlowingUp4,
        L.FaceBlowingUp5, L.FaceBlowingUp6, L.FaceBlowingUp7, L.FaceBlowingUp8, L.FaceBlownUp,
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
        L.ChestOpening1Top, L.ChestOpening1, L.ChestOpening2Top, L.ChestOpening2, L.ChestOpenTop, L.ChestOpen,
        L.ChestHeldOpenTop, L.ChestHeldOpen, L.ChestClosing1Top, L.ChestClosing1, L.ChestClosing2Top, L.ChestClosing2,
        L.ChestClosedTop, L.ChestClosed,
        L.EyeBeamGlow1, L.EyeBeamGlow2, L.EyeBeamGlow3, L.EyeBeamGlow4,
        L.EyesLightingAfterStun1, L.EyesLightingAfterStun2, L.EyesLightingAfterStun3, L.EyesLightingAfterStun4,
    ];
}
