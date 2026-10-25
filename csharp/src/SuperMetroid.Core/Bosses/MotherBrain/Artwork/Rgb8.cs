using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>An unquantized eight-bit-per-channel tissue endpoint; interpolation occurs before conversion to RGB5.</summary>
/// <param name="Red">The red channel, 0 through 255.</param>
/// <param name="Green">The green channel, 0 through 255.</param>
/// <param name="Blue">The blue channel, 0 through 255.</param>
internal readonly record struct Rgb8(int Red, int Green, int Blue)
{
    /// <summary>Selects an eight-bit endpoint channel for interpolation.</summary>
    internal int this[ColorChannel channel] => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };
}
