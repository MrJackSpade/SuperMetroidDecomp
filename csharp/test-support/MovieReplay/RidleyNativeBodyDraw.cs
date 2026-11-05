namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// What the cartridge's body draw oracle derives from a native checkpoint (#1500): the spritemap records the native
/// memory holds, and the screen row the body is drawn at. They differ only on a landing frame, whose spritemap-position
/// word carries the next table byte that the drawn origin never shows.
/// </summary>
/// <param name="Records">The spritemap records the native memory holds.</param>
/// <param name="DrawnY">The screen row the body is drawn at.</param>
internal readonly record struct RidleyNativeBodyDraw(RidleyNativeBodyRecordResult Records, ushort DrawnY);
