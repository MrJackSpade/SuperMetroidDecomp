namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that takes normal bombs although its header names neither the common nor a translated shot callback.</summary>
internal interface IEnemyNormalBombAdmission : IEnemyEntity
{
    /// <summary>Whether the normal-bomb walker admits the occupant this frame.</summary>
    /// <param name="collidesByFrameHitboxes">Whether the occupant collides by its frames' hitboxes.</param>
    bool AdmitsNormalBombs(bool collidesByFrameHitboxes);
}
