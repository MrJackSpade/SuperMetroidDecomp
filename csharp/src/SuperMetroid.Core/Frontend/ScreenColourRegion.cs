using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A run of one frontend screen owner's colours (#1464): <paramref name="ColourCount"/> colours from colour
/// <paramref name="FirstColour"/> of set <paramref name="Set"/> of <paramref name="Owner"/>, which may continue into the
/// owner's following sets. Installed screen palettes name the colours a relationship covers this way.
/// </summary>
/// <param name="Owner">The owner that holds the colours.</param>
/// <param name="Set">The owner's set the run starts in.</param>
/// <param name="FirstColour">The run's first colour within that set.</param>
/// <param name="ColourCount">The number of colours in the run.</param>
public sealed record ScreenColourRegion(ScreenColourOwner Owner, int Set, int FirstColour, int ColourCount)
{
    /// <summary>Whether <paramref name="cell"/> is one of the run's colours.</summary>
    public bool Contains(ScreenColourCell cell)
    {
        int first = Set * ColourSets.SetColours + FirstColour;
        int colour = cell.Set * ColourSets.SetColours + cell.Colour;
        return cell.Owner == Owner && colour >= first && colour < first + ColourCount;
    }
}
