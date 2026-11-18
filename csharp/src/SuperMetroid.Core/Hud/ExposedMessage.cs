using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The message the message box's window exposes in the most recently completed frame (#2505): what it shows and how far
/// the window has opened about its centre line.
/// </summary>
/// <param name="Message">The message the window shows.</param>
/// <param name="RadiusPixels">The window's half-height in pixels, from two through 24.</param>
public sealed record ExposedMessage(ShownMessage Message, int RadiusPixels);
