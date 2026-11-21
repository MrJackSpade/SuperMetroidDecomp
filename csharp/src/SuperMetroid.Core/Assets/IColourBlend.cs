namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a backend shows a fade (#2563): the simulation names a fade's colours and how far it has come
/// (<see cref="FadeLevel"/>), and the backend that draws the frame blends them, showing every colour in the steps the game
/// shows. The simulation never blends a colour itself.
/// </summary>
public interface IColourBlend
{
    /// <summary>The colour <paramref name="progress"/> of the way from <paramref name="from"/> to <paramref name="to"/>.</summary>
    /// <param name="from">The colour the fade starts from.</param>
    /// <param name="to">The colour the fade ends on.</param>
    /// <param name="progress">How far the fade has come.</param>
    RgbColor Partway(RgbColor from, RgbColor to, FadeLevel progress);

    /// <summary>
    /// <paramref name="colour"/> shown at <paramref name="level"/> between black, a level of none, and itself, every step
    /// taken.
    /// </summary>
    /// <param name="colour">The colour shown at its whole level.</param>
    /// <param name="level">How far up from black the colour shows.</param>
    RgbColor AtLevel(RgbColor colour, FadeLevel level);
}
