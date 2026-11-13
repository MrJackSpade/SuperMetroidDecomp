using System.Collections.Immutable;
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo decision a random sample takes only when several of its fields all read zero: each field is a chance taken by
/// its lowest value, and the decision is taken when every one of them is, so the fields' bit count sets its odds.
/// </summary>
/// <param name="Chances">The chances the decision needs, each taken when its field reads zero.</param>
internal sealed record TorizoRandomDecision(ImmutableArray<RandomChance> Chances)
{
    /// <summary>The decision taken when every one of <paramref name="fields"/> reads zero.</summary>
    internal static TorizoRandomDecision WhenAllClear(params RandomSampleField[] fields) =>
        new([.. fields.Select(field => RandomChance.Lowest(1, field))]);

    /// <summary>Whether <paramref name="sample"/> takes the decision: every one of its chances is taken.</summary>
    /// <param name="sample">A random sample, 0 through 65535.</param>
    internal bool IsTakenBy(int sample) => Chances.All(chance => chance.IsTakenBy(sample));
}
