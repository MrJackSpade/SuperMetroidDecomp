using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The message the message box's window exposes in the most recently completed frame (#2505): what it shows and how far
/// the window has opened about its centre line.
/// </summary>
/// <param name="Message">The message the window shows.</param>
/// <param name="RadiusPixels">The window's half-height in pixels, from one radius step up to the fully open window's.</param>
public sealed partial record ExposedMessage(ShownMessage Message, int RadiusPixels);
