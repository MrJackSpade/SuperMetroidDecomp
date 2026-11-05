using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Harness;

/// <summary>
/// Draws the independent scenes of one published frame at once (#1532): the first on the calling thread, each other on its
/// own long-lived companion thread. <see cref="DisplayRendering"/> keeps one renderer per thread, so every thread draws
/// with its own WARP device, and the companions live for the process so each keeps its renderer and its uploaded images
/// from frame to frame. A published scene can still reach state its game goes on changing, so <see cref="Draw"/> returns
/// only once every scene is drawn: the caller advances its game after the draws, exactly as when it drew them one by one,
/// and every scene is drawn and compared as before.
/// </summary>
internal static class ConcurrentSceneDrawing
{
    /// <summary>The companion threads made so far, in order; the scene after the first goes to the first companion.</summary>
    private static readonly List<DrawingCompanion> companions = [];

    /// <summary>
    /// The pixels each of <paramref name="scenes"/> shows, in order, drawn at once. Every draw has finished when this
    /// returns or throws; a failing draw's own exception is rethrown, the earliest scene's first.
    /// </summary>
    internal static RgbaPixel[][] Draw(ReadOnlySpan<LayeredScene> scenes)
    {
        var pixels = new RgbaPixel[scenes.Length][];
        if (scenes.IsEmpty)
            return pixels;
        var drawing = new Task<RgbaPixel[]>[scenes.Length];
        for (int index = 1; index < scenes.Length; index++)
            drawing[index] = Companion(index - 1).Draw(scenes[index]);
        ExceptionDispatchInfo? failure = null;
        try
        {
            pixels[0] = DisplayRendering.Render(scenes[0]);
        }
        catch (Exception exception)
        {
            // Held, not handled: it is rethrown below once no companion still reads the scenes.
            failure = ExceptionDispatchInfo.Capture(exception);
        }
        for (int index = 1; index < scenes.Length; index++)
        {
            try
            {
                pixels[index] = drawing[index].GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                // Held, not handled: the first failure, in scene order, is rethrown once every draw has finished.
                failure ??= ExceptionDispatchInfo.Capture(exception);
            }
        }
        failure?.Throw();
        return pixels;
    }

    /// <summary>The companion at <paramref name="index"/>, made with every companion before it on first use.</summary>
    private static DrawingCompanion Companion(int index)
    {
        lock (companions)
        {
            while (companions.Count <= index)
                companions.Add(new DrawingCompanion(companions.Count));
            return companions[index];
        }
    }

    /// <summary>One scene a companion draws, and where its pixels or its failure go.</summary>
    /// <param name="Scene">The scene to draw.</param>
    /// <param name="Drawn">Completed with the scene's pixels, or with the draw's exception.</param>
    private readonly record struct SceneDraw(LayeredScene Scene, TaskCompletionSource<RgbaPixel[]> Drawn);

    /// <summary>
    /// A background thread that draws the scenes given to it in order, with the renderer it makes on its first scene and
    /// keeps for the process. A draw's exception completes that draw's task, which <see cref="Draw"/> rethrows on the
    /// calling thread; none escapes the thread.
    /// </summary>
    private sealed class DrawingCompanion
    {
        /// <summary>The scenes given to the companion and not yet drawn, in the order they were given.</summary>
        private readonly BlockingCollection<SceneDraw> pending = new(new ConcurrentQueue<SceneDraw>());

        /// <summary>Starts the companion's thread.</summary>
        /// <param name="number">The companion's place among the companions, which names its thread.</param>
        internal DrawingCompanion(int number)
        {
            var thread = new Thread(DrawPending)
            {
                IsBackground = true,
                Name = $"Verification scene drawing {number}",
            };
            thread.Start();
        }

        /// <summary>Gives the companion <paramref name="scene"/> to draw; the task completes with its pixels.</summary>
        internal Task<RgbaPixel[]> Draw(LayeredScene scene)
        {
            var drawn = new TaskCompletionSource<RgbaPixel[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            pending.Add(new SceneDraw(scene, drawn));
            return drawn.Task;
        }

        /// <summary>Draws each scene given to the companion, for the life of the process.</summary>
        private void DrawPending()
        {
            foreach (SceneDraw draw in pending.GetConsumingEnumerable())
            {
                RgbaPixel[] pixels;
                try
                {
                    pixels = DisplayRendering.Render(draw.Scene);
                }
                catch (Exception exception)
                {
                    // Passed to the caller, which rethrows it: a draw's failure fails the check that asked for it.
                    draw.Drawn.SetException(exception);
                    continue;
                }
                draw.Drawn.SetResult(pixels);
            }
        }
    }
}
