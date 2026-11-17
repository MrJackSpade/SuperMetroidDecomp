namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How long one piece of Ridley's breakup lasts: the updates it lasts, dying on the last.</summary>
internal readonly record struct RidleyExplosionPartDefinition(
    int LifetimeUpdates);
