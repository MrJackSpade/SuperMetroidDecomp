using System.Collections.Immutable;

namespace Engine.Shared;

/// <summary>What something does about a hit carrying a particular set of kinds.</summary>
/// <param name="Kinds">The kinds a hit must all carry for this reaction to apply.</param>
/// <param name="DamageTimes">The damage taken is the hit's, times this.</param>
/// <param name="DamageOver">And divided by this. With <paramref name="DamageTimes"/>: 0 over 1 is none, 1 over 2 half, 2 over 1 double.</param>
/// <param name="Status">The status the target is put in, by name; empty for none.</param>
/// <param name="StatusWhen">When it is put in that status.</param>
/// <param name="StatusUpdates">How many updates the status lasts.</param>
/// <param name="UnhittableUpdates">How many updates the target cannot be hit again after taking the hit's damage.</param>
/// <param name="StatusUnhittableUpdates">How many updates the target cannot be hit again after the hit put it in the status.</param>
/// <param name="Passes">
/// Whether the hit takes no notice of the target: nothing is done to it, and what delivered the hit goes on unspent.
/// </param>
public sealed record Reaction(
    ImmutableHashSet<HitKind> Kinds,
    int DamageTimes,
    int DamageOver,
    string Status,
    StatusWhen StatusWhen,
    int StatusUpdates,
    int UnhittableUpdates,
    int StatusUnhittableUpdates,
    bool Passes)
{
    /// <summary>The reaction of something with no reaction of its own to a hit: it takes the hit's full damage.</summary>
    public static Reaction FullDamage { get; } = new([], 1, 1, "", StatusWhen.Never, 0, 0, 0, false);

    /// <summary>The damage a hit of <paramref name="damage"/> does under this reaction.</summary>
    public int DamageFrom(int damage) => damage * DamageTimes / DamageOver;
}
