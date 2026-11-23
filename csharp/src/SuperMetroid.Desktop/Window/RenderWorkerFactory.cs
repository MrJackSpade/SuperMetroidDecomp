using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11.Frames;

namespace SuperMetroid.Desktop.Window;

/// <summary>
/// Makes the render worker the game control starts for its window: the hardware worker, or one a check makes fail its
/// startup (#2570). The window handle stays in the native host; a check's worker names it only through this delegate.
/// </summary>
/// <param name="window">The window the worker presents to.</param>
/// <param name="clientSize">The window's client size when the worker starts.</param>
/// <param name="generation">The renderer startup the worker belongs to.</param>
internal delegate D3D11RenderWorker RenderWorkerFactory(nint window, Extent16<DisplayPixels> clientSize, long generation);
