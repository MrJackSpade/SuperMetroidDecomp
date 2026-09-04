using SuperMetroid.Desktop;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Game;

/// <summary>The normal reset-to-gameplay desktop shell for the translated game.</summary>
internal sealed class GameForm : Form
{
    public GameForm(
        string romPath,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentNullException.ThrowIfNull(gameOptions);

        Text = "Super Metroid C#";
        StartPosition = FormStartPosition.CenterScreen;
        // This fits a crisp 3x 256x224 image plus the debugger toolbar/help text on an
        // ordinary 1080p desktop. RuntimeCanvas automatically chooses a smaller integer
        // scale if the user resizes the window.
        ClientSize = new Size(900, 760);
        Controls.Add(new PlayableGameControl(romPath, gameOptions, replay, errorReporter));
    }
}
