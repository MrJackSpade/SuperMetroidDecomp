namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The behaviour Fune's and Namihe's main AI runs each frame. The two idle returns stay separate members
/// because Fune and Namihe park on different ones while their mouth animation owns the active part of the attack cycle.
/// Its values are arbitrary.
/// </summary>
public enum FuneNamiheEnemyFunction
{
    /// <summary>Increments the wrapping cooldown before a signed comparison with its cooldown time; on expiry it clears the counter, selects the active mouth animation, and parks at <see cref="FuneActivityNoOp"/>.</summary>
    FuneWaitForCooldown = 1,
    /// <summary>Selects the active mouth animation only when Samus's modular vertical distance is strictly below its whole-pixel vertical proximity, then parks at <see cref="NamiheActivityNoOp"/> without a cooldown.</summary>
    NamiheWaitForSamus,
    /// <summary>Fune's inert main-AI target while animation cues own mouth motion, projectile/sound production, and the finishing cue that restores its cooldown wait.</summary>
    FuneActivityNoOp,
    /// <summary>Namihe's separately observable inert main-AI target while its animation owns the attack and eventually restores its Samus-proximity wait.</summary>
    NamiheActivityNoOp,
}
