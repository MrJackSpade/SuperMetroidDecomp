using SuperMetroid.Desktop;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Game;

/// <summary>The normal reset-to-gameplay desktop shell for the translated game.</summary>
internal sealed class GameForm : Form
{
    private readonly PlayableGameControl gameControl;
    private bool closingRenderer;
    private bool rendererStopped;
    public GameForm(
        string romPath,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null,
        string? audioDirectory = null,
        string? dataDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentNullException.ThrowIfNull(gameOptions);

        Text = "Super Metroid C#";
        StartPosition = FormStartPosition.CenterScreen;
        // This fits a crisp 3x 256x224 image plus the debugger toolbar/help text on an
        // ordinary 1080p desktop. RuntimeCanvas automatically chooses a smaller integer
        // scale if the user resizes the window.
        ClientSize = new Size(900, 760);
        gameControl = new PlayableGameControl(romPath, gameOptions, replay, errorReporter, audioDirectory, dataDirectory);
        Controls.Add(gameControl);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await gameControl.InitializeRendererAsync();
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel || rendererStopped) return;
        e.Cancel = true;
        if (closingRenderer) return;
        closingRenderer = true;
        try { await gameControl.StopRendererAsync(); }
        catch (Exception error) { Console.Error.WriteLine($"Renderer shutdown failed: {error}"); }
        finally
        {
            rendererStopped = true;
            Close();
        }
    }
}
