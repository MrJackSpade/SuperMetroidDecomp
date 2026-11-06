using SuperMetroid.Core.Rooms.Backgrounds;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's screen (#1358, #1467, #1515): its colours and the text the final page's lower screen holds. Which pictures
/// the screen shows follows from the ending's phase and its scenes' state each frame; nothing records which were loaded.
/// </summary>
internal sealed class EndingScreen
{
    /// <summary>What the ending's colours show: each load, fade, flash and palette-effect record its scenes have shown (#1512).</summary>
    internal EndingColours Colours { get; } = new();

    /// <summary>The text the final page's lower screen shows: the staging page as the white flash's end copies it there.</summary>
    internal ImmutableArray<WrittenPicture> FinalPageLowerText { get; private set; } = [];

    /// <summary>Shows <paramref name="text"/> on the final page's lower screen.</summary>
    internal void ShowFinalLowerText(ImmutableArray<WrittenPicture> text) => FinalPageLowerText = text;
}
