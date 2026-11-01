using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// One movie's replay as a frame sequence a host drives. <see cref="Frames"/> yields <see langword="null"/> at each frame
/// boundary (once the opening import has been made, and after each completed frame has been compared) and the controller
/// input of the next frame to run before it; the host runs that frame between the two. A frame is one game step: an
/// update's input, or one message-box frame inside an update.
/// </summary>
internal interface IConvertedMovieReplay : IDisposable
{
    /// <summary>The game the host steps; a console reset in the movie replaces it.</summary>
    SuperMetroidGame Game { get; }

    /// <summary>
    /// The replay as a sequence: the opening import, then each frame's expectations and input, then that frame's
    /// completion and comparison. Each boundary is <see langword="null"/>; each frame to run is its input.
    /// </summary>
    IEnumerable<ControllerButton?> Frames();

    /// <summary>
    /// The final report once <see cref="Frames"/> has ended, after asserting the native checkpoints are exhausted and
    /// running the movie's own closing checks.
    /// </summary>
    string Finish();

    /// <summary>
    /// The failure the replay reports for <paramref name="exception"/>, thrown by production code while the host ran the
    /// pending frame, or <see langword="null"/> when the replay names none and the original propagates unchanged.
    /// </summary>
    Exception? NamedProductionFailure(Exception exception);
}
