using SuperMetroid.Desktop;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Game;

/// <summary>The normal reset-to-gameplay desktop shell for the translated game.</summary>
internal sealed class GameForm : Form
{
    /// <summary>Stores game control.</summary>
    private readonly PlayableGameControl gameControl;
    /// <summary>Stores closing renderer.</summary>
    private bool closingRenderer;
    /// <summary>Stores renderer stopped.</summary>
    private bool rendererStopped;

    /// <summary>The normal ROM-independent host path after asset installation.</summary>
    public GameForm(
        GameInstallation installation,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null)
        : this(new PlayableGameControl(installation, gameOptions, replay, errorReporter))
    {
    }

    /// <summary>Initializes a new game form instance.</summary>
    private GameForm(PlayableGameControl gameControl)
    {
        Text = "Super Metroid C#";
        StartPosition = FormStartPosition.CenterScreen;
        // This fits a crisp 3x 256x224 image plus the debugger toolbar/help text on an
        // ordinary 1080p desktop. RuntimeCanvas automatically chooses a smaller integer
        // scale if the user resizes the window.
        ClientSize = new Size(900, 760);
        this.gameControl = gameControl;
        Controls.Add(gameControl);
    }

    /// <summary>On shown.</summary>
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await gameControl.InitializeRendererAsync();
    }

    /// <summary>On form closing.</summary>
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
