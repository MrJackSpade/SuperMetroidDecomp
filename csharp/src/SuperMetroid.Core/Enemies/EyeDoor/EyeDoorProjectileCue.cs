using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>One cue of the eye door's shot and sweat animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct EyeDoorProjectileCue(EyeDoorProjectileCueKind Kind);
