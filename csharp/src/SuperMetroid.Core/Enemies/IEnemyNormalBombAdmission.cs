namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that takes normal bombs although its header names neither the common nor a translated shot callback.</summary>
internal interface IEnemyNormalBombAdmission : IEnemyEntity
{
    /// <summary>Whether the normal-bomb walker admits the occupant this frame.</summary>
    /// <param name="usesExtendedHitboxes">Whether the occupant's current frame is an extended spritemap.</param>
    bool AdmitsNormalBombs(bool usesExtendedHitboxes);
}
