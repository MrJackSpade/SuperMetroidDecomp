using Engine.Shared;

namespace Engine.Actors;

/// <summary>What a piece of equipment adds to a weapon's shots while it is switched on.</summary>
/// <param name="Equipment">The equipment, by name.</param>
/// <param name="Kinds">The kinds its shots also carry.</param>
/// <param name="Damage">The damage it adds.</param>
public sealed record WeaponProperty(string Equipment, IReadOnlyList<HitKind> Kinds, int Damage);
