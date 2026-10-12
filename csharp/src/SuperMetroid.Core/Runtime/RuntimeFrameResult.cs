using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Small immutable return value for loggers and debugger watches after a frame step.
/// Mutable detail remains available on <see cref="SuperMetroidRuntime"/> itself.
/// </summary>
public readonly record struct RuntimeFrameResult();
