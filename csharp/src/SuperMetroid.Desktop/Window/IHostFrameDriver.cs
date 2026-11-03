using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop.Window;

/// <summary>
/// Drives the host's frames from a recorded timeline instead of the player. The host keeps its own frame
/// loop, presentation and audio; a driver only chooses the game and each frame's input. Live play is the
/// absence of a driver.
/// </summary>
internal interface IHostFrameDriver
{
    /// <summary>Creates the game the host runs; called once per (re)start, after the host loaded its installed content.</summary>
    /// <param name="start">The installed content, assists and identity the host prepared for this session.</param>
    /// <returns>The game the host steps first.</returns>
    SuperMetroidGame Start(HostSessionStart start);

    /// <summary>The game to step now; a driver may replace it between frames (a console reset in a movie).</summary>
    SuperMetroidGame Game { get; }

    /// <summary>The next frame's input, or null when the timeline is finished.</summary>
    ControllerButton? NextInput();

    /// <summary>Called after the host ran and presented the frame with the last input; not called for a frame that threw.</summary>
    /// <param name="frame">The frame the host just completed.</param>
    void FrameCompleted(FrontendFrame frame);

    /// <summary>How many inputs the timeline has supplied since the last start; the last supplied input's zero-based index is one less.</summary>
    int InputsSupplied { get; }

    /// <summary>The timeline's position for the host's status text.</summary>
    string Progress { get; }

    /// <summary>How the host spaces this timeline's frames.</summary>
    HostFramePacing Pacing { get; }
}
