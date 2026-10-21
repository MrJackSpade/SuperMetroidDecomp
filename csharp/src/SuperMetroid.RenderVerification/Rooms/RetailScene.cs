using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.RenderVerification.Rooms;

/// <summary>A retail scene command and how to render it.</summary>
/// <param name="Command">The command.</param>
/// <param name="Run">The run.</param>
internal readonly record struct RetailScene(string Command, Action<D3D11RenderDevice, D3D11FrameRenderer> Run);
