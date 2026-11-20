namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One cue of a Torizo projectile family's animations (#2554): a <see cref="TorizoProjectileCue"/> every Torizo projectile's
/// behaviour runs alike, or one of the family's own <typeparamref name="TOwn"/> cues, which only the behaviour its
/// animations are started with runs.
/// </summary>
/// <typeparam name="TOwn">The cues only the family's behaviour runs.</typeparam>
internal abstract record TorizoFamilyCue<TOwn>
    where TOwn : struct, Enum
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private TorizoFamilyCue()
    {
    }

    /// <summary>A cue every Torizo projectile's behaviour runs alike.</summary>
    /// <param name="Cue">What the cue does.</param>
    internal sealed record Shared(TorizoProjectileCue Cue) : TorizoFamilyCue<TOwn>;

    /// <summary>A cue only the family's behaviour runs.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Own(TOwn Kind) : TorizoFamilyCue<TOwn>;
}
