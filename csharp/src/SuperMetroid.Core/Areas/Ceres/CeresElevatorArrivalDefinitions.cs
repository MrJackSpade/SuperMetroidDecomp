using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival's two actors, the moving pad that lowers Samus and the stationary platform that hides the
/// landing recess: where they start, how long the pad waits and where Samus lands. Their pictures are
/// <see cref="CeresElevatorArrivalPictures"/>.
/// </summary>
internal static class CeresElevatorArrivalDefinitions
{
    /// <summary>
    /// The graphics both arrival actors start with: the shared sheet, as their setup clears them, with the
    /// level-data concealer's colour cleared to black. A new game clears that colour just before the arrival starts, so
    /// the concealer hides the landing recess until both actors leave at touchdown.
    /// </summary>
    internal static SpriteGraphics StartingGraphics { get; } =
        SpriteGraphics.SharedIn(PictureShading.Shaded(CeresElevatorShades.ConcealerCleared));

    /// <summary>
    /// The updates of the moving pad's wait before it lowers Samus, the one it first lowers her on included: it holds her
    /// still on every update of the wait but the last and moves her on the last.
    /// </summary>
    internal const int MovingPadWaitUpdates = 60;

    /// <summary>How far below Samus the moving pad rides, in pixels.</summary>
    internal const int MovingPadYOffset = 28;

    /// <summary>The room Y the stationary platform stands at.</summary>
    internal const int StationaryPlatformY = 97;

    /// <summary>The room Y Samus stands at once the descent ends.</summary>
    internal const int LandingSamusY = 72;

    /// <summary>
    /// The room Y that ends Samus's descent: the step that takes her down to it sets her back up to
    /// <see cref="LandingSamusY"/>.
    /// </summary>
    internal const int LandingThresholdY = 73;


    /// <summary>How far Samus descends with the moving pad each update of the arrival: one pixel down.</summary>
    internal static Offset16<RoomPixels> SamusDescentStep { get; } = new(0, 1);
}
