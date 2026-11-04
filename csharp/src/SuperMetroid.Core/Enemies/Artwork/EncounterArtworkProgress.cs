using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The step each of the room's encounters has reached in its artwork sequence (#1513): the frames the encounter changes
/// show the pre-assembled picture of that step. A room starts with none reached, so every frame shows the picture its
/// room's load gives it.
/// </summary>
public sealed class EncounterArtworkProgress : IEncounterArtworkView
{
    /// <summary>The step each sequence the room's encounters have started has reached.</summary>
    private readonly Dictionary<EncounterArtworkSequence, int> reached = [];

    /// <summary>A room load: no encounter of the new room has played any of its sequence yet.</summary>
    internal void Restart() => reached.Clear();

    /// <summary>Records that the encounter playing <paramref name="sequence"/> has reached step <paramref name="step"/>.</summary>
    internal void Show(EncounterArtworkSequence sequence, int step) => reached[sequence] = Ensure.AtLeastZero(step);

    /// <summary>
    /// Records that the encounter playing <paramref name="sequence"/>, whose steps show <paramref name="order"/>, shows
    /// <paramref name="look"/>: the first step at or after the one reached that shows it, or, for an encounter that comes
    /// round to a look again, as a Torizo's glowing eyes do, its first step that shows it.
    /// </summary>
    /// <exception cref="InvalidDataException">No step of the sequence shows the look.</exception>
    internal void Show<TLook>(EncounterArtworkSequence sequence, IReadOnlyList<TLook> order, TLook look)
        where TLook : IEquatable<TLook>
    {
        Ensure.NotNull(order);
        int from = reached.TryGetValue(sequence, out int current) ? current : 0;
        reached[sequence] = Find(order, look, from) ?? Find(order, look, 0)
            ?? throw new InvalidDataException($"No step of {sequence} shows {look}.");
    }

    /// <summary>The step the encounter playing <paramref name="sequence"/> has reached, or none before it shows its first.</summary>
    internal int? StepOf(EncounterArtworkSequence sequence) => reached.TryGetValue(sequence, out int step) ? step : null;

    /// <inheritdoc />
    public bool Reached(EncounterArtworkSequence sequence, int step) => reached.TryGetValue(sequence, out int current) && current >= step;

    /// <summary>The first step from <paramref name="from"/> on that shows <paramref name="look"/>, or none when none does.</summary>
    private static int? Find<TLook>(IReadOnlyList<TLook> order, TLook look, int from)
        where TLook : IEquatable<TLook>
    {
        for (int step = from; step < order.Count; step++)
        {
            if (order[step].Equals(look))
                return step;
        }
        return null;
    }
}
