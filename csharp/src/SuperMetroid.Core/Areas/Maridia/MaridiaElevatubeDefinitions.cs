namespace SuperMetroid.Core.Areas.Maridia;

/// <summary>
/// The Maridia elevatube's ride (#1415): where it holds Samus, where each entry starts her, how fast she starts and
/// accelerates, and the fastest she may travel.
/// </summary>
internal static class MaridiaElevatubeDefinitions
{
    /// <summary>The room X, in whole pixels, the tube holds Samus at on every update.</summary>
    internal const int SamusCenterX = 128;

    /// <summary>The room Y, in whole pixels, a ride entered from Oasis to the south starts at.</summary>
    internal const int SouthStartingY = 2496;

    /// <summary>A south entry's starting velocity: one pixel per update, upward.</summary>
    internal static readonly SignedEightEight SouthStartingVelocity = SignedEightEight.FromSignedRaw(-256);

    /// <summary>A south entry's acceleration: an eighth of a pixel per update per update, upward.</summary>
    internal static readonly SignedEightEight SouthAcceleration = SignedEightEight.FromSignedRaw(-32);

    /// <summary>The room Y, in whole pixels, a ride entered from Plasma Spark to the north starts at.</summary>
    internal const int NorthStartingY = 64;

    /// <summary>A north entry's starting velocity: one pixel per update, downward.</summary>
    internal static readonly SignedEightEight NorthStartingVelocity = SignedEightEight.FromSignedRaw(256);

    /// <summary>A north entry's acceleration: an eighth of a pixel per update per update, downward.</summary>
    internal static readonly SignedEightEight NorthAcceleration = SignedEightEight.FromSignedRaw(32);

    /// <summary>
    /// The fastest the tube carries Samus either way, fourteen and an eighth pixels per update: an acceleration that
    /// would take her past it is not applied, and her speed stays as it was.
    /// </summary>
    internal static readonly SignedEightEight MaximumSpeed = SignedEightEight.FromSignedRaw(3616);

    /// <summary>The block the room spawns the otherwise invisible elevatube PLM at: column one of the top row.</summary>
    internal static readonly Position16<RoomBlocks> PlmBlock = new(1, 0);
}
