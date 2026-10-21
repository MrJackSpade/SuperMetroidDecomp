using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Title;

/// <summary>The lines and zoom of one capture installed title gradient result.</summary>
/// <param name="Lines">The lines.</param>
/// <param name="Zoom">The zoom.</param>
internal readonly record struct CaptureInstalledTitleGradientResult(TitleGradientLine[] Lines, ushort Zoom);
