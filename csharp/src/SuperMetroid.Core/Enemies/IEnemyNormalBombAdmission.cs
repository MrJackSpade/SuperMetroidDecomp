namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that takes normal bombs although its definition names neither the common nor a private shot reaction.</summary>
internal interface IEnemyNormalBombAdmission : IEnemyEntity
{
    /// <summary>Whether the normal-bomb walker admits the occupant this frame.</summary>
    /// <param name="collidesByFrameHitboxes">Whether the occupant collides by its frames' hitboxes.</param>
    bool AdmitsNormalBombs(bool collidesByFrameHitboxes);
}
