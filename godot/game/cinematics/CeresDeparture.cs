using Engine.Core;
using Engine.Ui;

namespace Game.Cinematics;

/// <summary>
/// Samus leaving Ceres by its elevator: the screen goes dark over the shaft as it stood, and the station is then
/// seen blowing up behind her.
/// </summary>
public partial class CeresDeparture : Cinematic
{
    private readonly PlayingMode _playing;
    private ScreenFade? _dark;

    /// <summary>Samus leaving from <paramref name="playing"/>.</summary>
    public CeresDeparture(PlayingMode playing)
    {
        ArgumentNullException.ThrowIfNull(playing);
        _playing = playing;
    }

    /// <summary>The room play goes on in afterwards, by its scene.</summary>
    public string LandingRoom { get; set; } = "";

    /// <summary>The entry point she arrives at there.</summary>
    public string LandingEntry { get; set; } = "";

    /// <summary>How many updates the screen takes to go dark.</summary>
    private const int DarkeningUpdates = 15;

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    protected override bool CanBeSkipped => false;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _dark = new ScreenFade { Name = "Dark", Layer = 10 };
        AddChild(_dark);
        _dark.FadeOut(DarkeningUpdates);
    }

    /// <inheritdoc/>
    protected override bool Play(int elapsed)
    {
        ScreenFade dark = _dark ?? throw new InvalidOperationException("The departure from Ceres has not begun.");
        dark.Step();
        if (!dark.IsDone)
            return false;
        Session.Sounds.RequestMusic("");
        return true;
    }

    /// <inheritdoc/>
    protected override GameMode Next() => new CeresDestruction { LandingRoom = LandingRoom, LandingEntry = LandingEntry };
}
