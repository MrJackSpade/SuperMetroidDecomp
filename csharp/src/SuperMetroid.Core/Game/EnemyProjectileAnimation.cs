using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// A projectile family's typed animation, run in place of the bank-$86 instruction interpreter (#641).
/// The slot keeps the cartridge's timer gate, so each frame lasts exactly as long as the native frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its native timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another program); the pass
    /// then stops, as the native instruction that did so would.
    /// </summary>
    internal void Advance(RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus)
    {
        ushort oldTimer = projectile.InstructionTimer.RawValue;
        projectile.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
            return;
        bool shown = TryShowNextFrame(projectile, handlers, samus, out int duration, out ushort presentationKey);
        if (projectile.Animation != this)
            return;
        if (!shown)
        {
            // A held timeline is the native sleep: the timer stays zero and wraps, never expiring again.
            projectile.InstructionTimer = NativeCountdown.Start(0);
            return;
        }
        projectile.InstructionTimer = NativeCountdown.FromRaw((ushort)duration);
        projectile.PresentationOperandAddress = presentationKey;
        projectile.SpritemapPointer = EnemyProjectileSpritemapDefinitions.BlankSpritemap;
    }

    /// <summary>Advances the timeline to its next frame, or reports that it holds.</summary>
    /// <param name="projectile">The slot the animation runs on, for cues that act on it.</param>
    /// <param name="handlers">The engine's handlers, which carry out published cues.</param>
    /// <param name="samus">Samus, for cues that act on her; null in fixtures without her.</param>
    /// <param name="duration">How many projectile passes the frame lasts.</param>
    /// <param name="presentationKey">The installed artwork key of the frame.</param>
    private protected abstract bool TryShowNextFrame(
        RoomEnemyProjectileSlot projectile, EnemyProjectileHandlerTable handlers, SamusState? samus, out int duration, out ushort presentationKey);
}
