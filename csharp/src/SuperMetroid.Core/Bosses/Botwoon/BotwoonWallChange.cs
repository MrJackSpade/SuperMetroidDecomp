using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon asked for his wall's room object (#2562): a defeated room's clear wall or the live crumble.</summary>
/// <param name="Mutation">The wall's change.</param>
public sealed record BotwoonWallChange(BotwoonWallMutation Mutation) : EnemyPassEvent;
