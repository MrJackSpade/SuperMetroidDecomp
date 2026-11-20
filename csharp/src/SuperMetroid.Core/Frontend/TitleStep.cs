namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of the title sequence (<see cref="Progression.SuperMetroidGameState.Title"/>) runs.</summary>
internal enum TitleStep
{
    /// <summary>
    /// Power-on or a soft reset: the random generator is reseeded, every gameplay owner and kept explored map is
    /// discarded, and the title opening is shown.
    /// </summary>
    Starting,
    /// <summary>The title opening and title screen, until a file select or an idle attract demo is requested.</summary>
    Showing,
}
