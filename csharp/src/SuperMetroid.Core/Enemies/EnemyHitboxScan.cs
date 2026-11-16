namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whether the engine tests a contact against an enemy's extended hitboxes. The collision tests for Samus's touch and
/// for shots and bombs read the definition's reaction before they read any hitbox: a definition that names the engine's
/// own shared empty reaction withholds the contact, while a definition that names an empty reaction of the species'
/// own still lets the hitboxes decide (Kraid's arm and feet name one and still select the arm's shot reaction). The
/// import decides it from the definition; it matters only for an enemy whose contact is tested against extended hitboxes.
/// </summary>
public enum EnemyHitboxScan
{
    /// <summary>The contact is tested against the enemy's extended hitboxes.</summary>
    Scanned,

    /// <summary>The contact is withheld before any hitbox is read: no overlap, no quake, no collision mark.</summary>
    Withheld,
}
