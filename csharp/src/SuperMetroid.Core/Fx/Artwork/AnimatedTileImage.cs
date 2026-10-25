namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// One distinct picture an animated-tile object's frames show, the identity the installed room-FX atlas holds its
/// characters under (#1415). Frames that show the same picture again (the horizontal spikes' fourth step, the leftward
/// treadmill's reversed loop) name the picture they repeat.
/// </summary>
/// <param name="Object">The object whose art the picture is.</param>
/// <param name="Index">The picture's place among the object's distinct pictures, from zero.</param>
public readonly record struct AnimatedTileImage(AnimatedTileObject Object, int Index);
