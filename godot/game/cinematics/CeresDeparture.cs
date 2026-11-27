using Engine.Core;
using Engine.Input;

namespace Game.Cinematics;

/// <summary>
/// Samus leaving Ceres by its elevator. Play goes on beneath it throughout, the shaft still tilting: she stands held
/// on the elevator for a while, then the screen goes dark over the shaft, and once it is black her leaving is saved
/// and the station is seen blowing up behind her.
/// </summary>
/// <remarks>
/// The dark is the screen's own darkness, which the departure shows at its own fade's level on each of its updates:
/// from fully lit, whatever it stood at before, to black, which it is left at.
/// </remarks>
public partial class CeresDeparture : GameMode
{
    /// <summary>How many updates she stands on the elevator before the screen begins to darken, a level each update.</summary>
    private const int HoldUpdates = 60;

    /// <summary>
    /// The terms play goes on beneath the departure on, as the cartridge has it: Samus with no energy left neither
    /// begins to die nor is saved by her reserves, the pause is refused, and nothing of the escape's takes play.
    /// </summary>
    private static readonly PlayTerms BeneathTheDeparture = new() { PlayerMayDie = false, PlayerMayBeSaved = false, MayPause = false, ServicesMayTakeOver = false };

    private readonly PlayingMode _playing;
    private int _elapsed;

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

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        int levels = Session.Darkness.Levels;
        bool last = _elapsed == HoldUpdates + levels - 1;

        // That she is away is saved by the update that reaches black, once it has played.
        if (last)
            Session.RequestSave(LandingRoom, LandingEntry);
        if (_playing.PlayBeneath(input, BeneathTheDeparture) is { } other)
            throw new InvalidOperationException($"Play beneath the departure from Ceres handed over to {other.GetType().Name}; held on the elevator, Samus leads play nowhere.");
        _elapsed++;
        if (_elapsed <= HoldUpdates)
            return this;
        Session.Darkness.Show(_elapsed - HoldUpdates);
        if (!last)
            return this;

        // Black: the countdown is over, the music stops and every library is told to stop.
        Session.Service<Escape>().Stop();
        Session.Sounds.RequestMusic("");
        Session.Sounds.StopAll(SoundPriority.Essential);
        return new CeresDestruction { LandingRoom = LandingRoom, LandingEntry = LandingEntry };
    }
}
