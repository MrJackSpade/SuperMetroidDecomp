namespace SuperMetroid.Core.Assets;

/// <summary>
/// An owner of some of gameplay's colours, which the colour router reaches by position while writers not yet moved to
/// the owner still address colours by position (#1405).
/// </summary>
public interface IGameplayColourOwner
{
    /// <summary>All of the owner's colours, as runs that each cover one of its stores.</summary>
    IReadOnlyList<ColourRun> Stores { get; }

    /// <summary>Whether the owner holds the colour at <paramref name="position"/>.</summary>
    bool Holds(int position);

    /// <summary>The owner's colour at <paramref name="position"/>, which it holds.</summary>
    RgbColor At(int position);

    /// <summary>Changes the owner's colour at <paramref name="position"/>, which it holds.</summary>
    void Set(int position, RgbColor colour);
}
