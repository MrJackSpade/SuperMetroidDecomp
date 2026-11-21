using System.Collections.Frozen;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// The shades the import pre-colours the game-over Baby in (#1512): its idle colours and each phase of its cry. Which inks
/// each recolours, and to what, is the import's.
/// </summary>
public static class GameOverBabyShades
{
    /// <summary>
    /// The installed name of each colouring's shade (#2511), written out so renaming a colouring's identity never changes a
    /// name on disk.
    /// </summary>
    private static readonly IdentityTable<GameOverBabyColouring, string> ColourKeys =
        new(new Dictionary<GameOverBabyColouring, string>
        {
            [GameOverBabyColouring.Idle] = "GameOverBabyIdle",
            [GameOverBabyColouring.ClosedCry] = "GameOverBabyClosedCry",
            [GameOverBabyColouring.MiddleCry] = "GameOverBabyMiddleCry",
            [GameOverBabyColouring.OpenCry] = "GameOverBabyOpenCry",
        });

    /// <summary>The Baby shown in its <paramref name="colours"/> colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring has no installed name.</exception>
    public static PictureShade Colours(GameOverBabyColouring colours) => new(ColourKeys[colours], 0);
}
