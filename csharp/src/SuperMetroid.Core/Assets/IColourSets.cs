namespace SuperMetroid.Core.Assets;

/// <summary>
/// An owner's eight colour sets of sixteen, told apart by their order (#1464): gameplay's sprite sets, or a frontend
/// screen's background or sprite sets. An image colour group the owner colours shows the set of the same order.
/// Gameplay's background sets are not among them: each is named by its <see cref="BackgroundColourSet"/> and held by
/// <see cref="BackgroundColourSets"/>.
/// </summary>
public interface IColourSets
{
    /// <summary>Set <paramref name="set"/>, zero through seven.</summary>
    ColourRun Set(int set);
}
