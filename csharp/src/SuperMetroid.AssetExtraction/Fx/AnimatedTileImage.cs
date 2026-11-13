using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// One distinct picture an animated-tile object's frames show, the identity the installed room-FX atlas holds its
/// characters under (#1415). Frames that show the same picture again (the horizontal spikes' fourth step, the leftward
/// treadmill's reversed loop) name the picture they repeat. The picture is the atlas's storage ordinal (#1527): the game
/// selects frames, and only the import folds a frame into the picture the atlas stores
/// (<see cref="RoomFxAnimatedTileLoops"/>).
/// </summary>
/// <param name="Object">The object whose art the picture is.</param>
/// <param name="Index">The picture's place among the object's distinct pictures, from zero.</param>
public readonly record struct AnimatedTileImage(BlockAnimation Object, int Index);
