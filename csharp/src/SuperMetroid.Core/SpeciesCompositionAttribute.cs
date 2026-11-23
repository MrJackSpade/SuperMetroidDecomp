namespace SuperMetroid.Core;

/// <summary>
/// Marks the composition of a room's species (#2571): the one type that names each species to build its behaviour from the
/// room's shared services and register it with the contracts the shared phases iterate. SME6473 lets only this type name a
/// species' behaviour outside the species' own namespaces, requires that one type at most carries it, and rejects a member
/// of it that hands a species' behaviour out, so no phase reaches a species through it by name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed partial class SpeciesCompositionAttribute : Attribute;
