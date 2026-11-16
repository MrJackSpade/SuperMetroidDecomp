namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How long one piece of Ridley's breakup lasts: the count its lifetime countdown is loaded with.</summary>
internal readonly record struct RidleyExplosionPartDefinition(
    int Lifetime);
