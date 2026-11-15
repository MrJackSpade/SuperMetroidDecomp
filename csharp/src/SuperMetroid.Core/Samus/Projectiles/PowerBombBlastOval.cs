using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The oval of added colour a Power Bomb's blast shows on one frame (#1927): its centre, its radii, and the colour it
/// adds to the scene inside it.
/// </summary>
/// <param name="Centre">The blast's centre in the room.</param>
/// <param name="Radii">The oval's horizontal and vertical radii in whole pixels; an oval with no height shows nothing.</param>
/// <param name="Colour">The colour the oval adds to the scene.</param>
public readonly record struct PowerBombBlastOval(Position16<RoomPixels> Centre, Extent16<RoomPixels> Radii, RgbColor Colour)
{
    /// <summary>
    /// The oval <paramref name="horizontalRadius"/> pixels wide either side of <paramref name="centre"/>, three quarters
    /// of that tall, rounded down.
    /// </summary>
    /// <param name="centre">The blast's centre in the room.</param>
    /// <param name="horizontalRadius">The oval's horizontal radius in whole pixels.</param>
    /// <param name="colour">The colour the oval adds to the scene.</param>
    public static PowerBombBlastOval Of(Position16<RoomPixels> centre, int horizontalRadius, RgbColor colour) =>
        new(centre, new Extent16<RoomPixels>(horizontalRadius,
            horizontalRadius * PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts), colour);
}
