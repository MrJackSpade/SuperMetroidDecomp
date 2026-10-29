namespace SuperMetroid.Core.Assets;

/// <summary>
/// An owner's eight colour sets of sixteen (#1464): gameplay's or a screen's background sets, or their sprite sets. An
/// image colour group the owner colours shows the set of the same identity.
/// </summary>
public interface IColourSets
{
    /// <summary>Set <paramref name="set"/>, zero through seven.</summary>
    ColourRun Set(int set);
}
