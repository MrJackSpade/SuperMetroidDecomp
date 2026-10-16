namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy occupying one slot: bound when its identity is installed and discarded when the slot changes occupant.
/// The behaviors it has are the participation interfaces it implements; the dispatcher keeps eligibility and order,
/// and invokes each handler with the slot it resolved the entity from.
/// </summary>
internal interface IEnemyEntity;
