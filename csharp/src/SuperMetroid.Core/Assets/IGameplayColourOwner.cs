namespace SuperMetroid.Core.Assets;

/// <summary>
/// An owner of some of the colours gameplay's sprites show, besides the sprite colour sets (#1405, #1464): Samus's. A
/// sprite colour set shows, over its own colours, the colours the owner holds for the image colour group of its place, and
/// the starting colours give the owner its own.
/// </summary>
public interface IGameplayColourOwner : IImageGroupColours
{
    /// <summary>All of the owner's colours, as runs that each cover one of its stores.</summary>
    IReadOnlyList<ColourRun> Stores { get; }

    /// <summary>Gives the owner its colours among <paramref name="starting"/>.</summary>
    void LoadStarting(GameplayStartingColours starting);
}
