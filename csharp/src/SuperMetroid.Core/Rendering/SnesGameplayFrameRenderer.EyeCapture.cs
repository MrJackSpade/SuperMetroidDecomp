using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

public static partial class SnesGameplayFrameRenderer
{
    /// <summary>Resolves the published eye cone and ROM tangent table into inclusive color windows.</summary>
    public static ScanlineColorAddRenderLayer? CaptureMorphBallEyeBeam(ISnesAddressSpace bus,
        MorphBallEyeBeamRenderSnapshot beam, Position16<RoomPixels> layer1)
    {
        Ensure.NotNull(bus);
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        Offset16<ScreenPixels> origin = beam.WorldPosition.RelativeToCamera(layer1);
        int angle = beam.Angle.TableIndex;
        int angularWidth = unchecked((byte)beam.AngularWidth);
        ushort[] nativeWindows = EyeBeamWindowBuilder.Build(origin, angle, angularWidth);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit(beam.Red), green = ExpandFiveBit(beam.Green), blue = ExpandFiveBit(beam.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            int left = nativeWindows[y] & byte.MaxValue, right = nativeWindows[y] >> 8;
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }
}
