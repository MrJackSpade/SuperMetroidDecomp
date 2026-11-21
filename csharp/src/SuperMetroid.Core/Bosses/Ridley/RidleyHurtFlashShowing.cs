using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Which of Ridley's drawn parts show his hurt flash's colours, as his step picks them (#1512, #2562): his body on its phase
/// of his body's update count, his tail and wings on the phase one update ahead. His step picks them after it runs and
/// before he moves, only while his movement runs, and a hit picks them again with the update before's phase; his draw
/// shows the step's last pick, which it could not work out again from the flash as it stands when it draws.
/// </summary>
/// <param name="Body">Whether his body shows the flash's colours.</param>
/// <param name="TailAndWings">Whether his tail and wings show the flash's colours.</param>
[IndependentFlag("his body and his tail and wings each show the flash on their own phase")]
public readonly partial record struct RidleyHurtFlashShowing(bool Body, bool TailAndWings)
{
    /// <summary>What <paramref name="hurtFlash"/> shows on <paramref name="bodyUpdateCount"/>'s phase.</summary>
    internal static RidleyHurtFlashShowing Of(UpdateCountdown hurtFlash, int bodyUpdateCount) => new(
        Body: hurtFlash.IsRunning && EnemyDrawPhaseDefinitions.HurtFlashShown.IsInSecondHalfAt(bodyUpdateCount),
        // His drawn parts take the phase one pass ahead of his body's colours.
        TailAndWings: hurtFlash.UpdatesLeft > 1 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsInSecondHalfAt(bodyUpdateCount + 1));
}
