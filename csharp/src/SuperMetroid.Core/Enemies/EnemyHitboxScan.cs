namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whether the engine tests a contact against an enemy's extended hitboxes. The collision walkers for Samus's touch and
/// for shots and bombs test the definition's reaction before they read any hitbox: a definition that names the engine's
/// own shared empty reaction withholds the contact, while a definition that names an empty reaction of the species'
/// own still lets the hitboxes decide (Kraid's arm and feet name one and still select the arm's shot reaction). The
/// import decides it from the definition; it matters only for an enemy whose contact enters the extended-hitbox walker.
/// </summary>
public enum EnemyHitboxScan
{
    /// <summary>The walker tests the contact against the enemy's extended hitboxes.</summary>
    Scanned,

    /// <summary>The walker withholds the contact before it reads any hitbox: no overlap, no quake, no collision mark.</summary>
    Withheld,
}
