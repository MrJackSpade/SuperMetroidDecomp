using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Dachora's installed default, speed, and shine RGB5 images, kept as installed. Speed/shine timers and their
/// frame-index arithmetic remain compiled enemy behavior.
/// </summary>
public sealed class DachoraColorCatalog
{
    /// <summary>The installed default palette.</summary>
    private readonly InstalledColourRows normal;
    /// <summary>The installed speed-boost frames.</summary>
    private readonly InstalledColourRows speed;
    /// <summary>The installed stored-shine frames.</summary>
    private readonly InstalledColourRows shine;

    /// <summary>Keeps the complete installed palettes.</summary>
    /// <param name="normal">The default palette's colours.</param>
    /// <param name="speed">Each speed-boost frame's colours.</param>
    /// <param name="shine">Each stored-shine frame's colours.</param>
    /// <exception cref="InvalidDataException">A palette is missing a frame or a colour.</exception>
    internal DachoraColorCatalog(RgbColor[] normal, RgbColor[][] speed, RgbColor[][] shine)
    {
        this.normal = new InstalledColourRows(normal, DachoraColorFormat.ColorsPerFrame, "Dachora normal");
        this.speed = new InstalledColourRows(speed, DachoraColorDefinitions.AnimatedFrameCount, DachoraColorFormat.ColorsPerFrame, "Dachora speed");
        this.shine = new InstalledColourRows(shine, DachoraColorDefinitions.AnimatedFrameCount, DachoraColorFormat.ColorsPerFrame, "Dachora shine");
    }

    /// <summary>Resolves one installed Dachora color.</summary>
    /// <param name="phase">Exclusive default, speed-boost, or stored-shine palette phase selected by enemy behavior.</param>
    /// <param name="frame">Zero for the default phase, or zero-based animated frame 0 through 3 for speed and shine.</param>
    /// <param name="color">Palette color index 0 through 15, including the transparent slot at zero.</param>
    /// <returns>The installed colour; the caller chooses the actor's sprite palette destination.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    public RgbColor Resolve(DachoraColourPhase phase, int frame, int color) => phase switch
    {
        DachoraColourPhase.Default => frame == 0
            ? normal.Colour(color)
            : throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}."),
        DachoraColourPhase.Speed => speed.Colour(frame, color),
        DachoraColourPhase.Shine => shine.Colour(frame, color),
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Dachora palette phase."),
    };
}
