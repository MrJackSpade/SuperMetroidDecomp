namespace SuperMetroid.Core.Rendering;

/// <summary>
/// One shape of a <see cref="ScreenWindow"/> (#1468): a rectangle, an oval or a cone in screen pixels, which the backend
/// tests every pixel against. Shapes are descriptions; nothing in the simulation rasterizes them.
/// </summary>
public abstract record WindowShape;
