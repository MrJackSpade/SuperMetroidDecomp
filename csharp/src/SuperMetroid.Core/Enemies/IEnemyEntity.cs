namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy holding one place: bound when its identity is installed and discarded when the place changes occupant.
/// The behaviors it has are the participation interfaces it implements; the step keeps eligibility and order,
/// and asks the entity, which acts on the place it is bound to.
/// </summary>
internal interface IEnemyEntity
{
    /// <summary>The collision paths the enemy takes beyond the common ones.</summary>
    EnemyCapabilities Capabilities { get; }
}
