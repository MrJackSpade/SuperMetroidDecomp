using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>The first gradient frame of an installed title: its gradient, the zoom it was selected at and the frame.</summary>
/// <param name="Gradient">The gradient the frame draws.</param>
/// <param name="Zoom">The title floor's zoom on that frame.</param>
/// <param name="Scene">The frame as the title captured it.</param>
internal readonly record struct CaptureInstalledTitleGradientResult(ScreenGradient Gradient, ushort Zoom, FloorScene Scene);
