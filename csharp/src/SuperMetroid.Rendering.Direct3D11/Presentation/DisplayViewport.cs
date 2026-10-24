using SuperMetroid.Core;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Rendering.Direct3D11.Presentation;

/// <summary>The desktop window's display rectangle: the picture at 4:3 with a whole-number vertical scale; small clients clip the centered minimum-size picture.</summary>
/// <param name="Offset">The picture's top-left corner relative to the client's; negative when a small client clips it.</param>
/// <param name="Size">The drawn picture size.</param>
public readonly record struct DisplayViewport(Offset16<DisplayPixels> Offset, Extent16<DisplayPixels> Size)
{
    /// <summary>
    /// The host's client size. Window-system and view APIs report it as two signed integers;
    /// a negative or oversized dimension is rejected rather than wrapped.
    /// </summary>
    public static Extent16<DisplayPixels> ClientSize(int width, int height)
    {
        Ensure.AtLeastZero(width);
        Ensure.AtLeastZero(height);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, ushort.MaxValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, ushort.MaxValue);
        return new((ushort)width, (ushort)height);
    }

    /// <summary>Preserves the desktop's 4:3 TV correction and integral vertical scaling.</summary>
    public static DisplayViewport ForClient(Extent16<DisplayPixels> client)
    {
        int unitWidth = (D3D11Screen.Height * DisplayAspect.Numerator + DisplayAspect.Denominator / 2)
            / DisplayAspect.Denominator;
        int scale = Math.Max(1, Math.Min(client.X / unitWidth, client.Y / D3D11Screen.Height));
        return Centered(client, Scaled(unitWidth, scale));
    }

    /// <summary>A native-height picture <paramref name="unitWidth"/> pixels wide, multiplied by <paramref name="scale"/>.</summary>
    private static Extent16<DisplayPixels> Scaled(int unitWidth, int scale) =>
        new(checked((ushort)(unitWidth * scale)), checked((ushort)(D3D11Screen.Height * scale)));

    /// <summary>Positions the picture symmetrically, allowing negative offsets when the client clips it.</summary>
    private static DisplayViewport Centered(Extent16<DisplayPixels> client, Extent16<DisplayPixels> picture) => new(
        new Offset16<DisplayPixels>(checked((short)((client.X - picture.X) / 2)), checked((short)((client.Y - picture.Y) / 2))),
        picture);
}
