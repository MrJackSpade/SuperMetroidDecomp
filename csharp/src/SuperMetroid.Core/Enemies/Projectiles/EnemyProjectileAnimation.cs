using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile family's typed animation, run in place of the bank-$86 instruction interpreter (#641).
/// The slot keeps the cartridge's timer gate, so each frame lasts exactly as long as the native frame did.
/// </summary>
internal abstract class EnemyProjectileAnimation
{
    /// <summary>
    /// The frame key of a frame that shows the empty projectile spritemap ($8D:8000) rather than an installed program
    /// frame, as a native list operand selecting that spritemap does.
    /// </summary>
    internal const ushort EmptyFrameKey = 0;

    /// <summary>
    /// Advances one projectile pass: the frame expires on the update its native timer would. A cue published
    /// on the way may end the animation (deleting the projectile or handing it to another program); the pass
    /// then stops, as the native instruction that did so would.
    /// </summary>
    internal void Advance(EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers)
    {
        RoomEnemyProjectileSlot projectile = context.Projectile;
        ushort oldTimer = projectile.InstructionTimer.RawValue;
        projectile.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
            return;
        bool shown = TryShowNextFrame(context, handlers, out int duration, out EnemyProjectileFramePresentation presentation);
        if (projectile.Animation != this)
            return;
        if (!shown)
        {
            // A held timeline is the native sleep: the timer stays zero and wraps, never expiring again.
            projectile.InstructionTimer = NativeCountdown.Start(0);
            return;
        }
        projectile.InstructionTimer = NativeCountdown.FromRaw((ushort)duration);
        projectile.PresentationOperandAddress = presentation.PresentationKey;
        projectile.SpritemapPointer = presentation.SpritemapPointer;
    }

    /// <summary>Advances the timeline to its next frame, or reports that it holds.</summary>
    /// <param name="context">The slot the animation runs on, Samus and the pass's camera, for cues that act on them.</param>
    /// <param name="handlers">The engine's handlers, which carry out published cues.</param>
    /// <param name="duration">How many projectile passes the frame lasts.</param>
    /// <param name="presentation">How the frame is drawn.</param>
    private protected abstract bool TryShowNextFrame(
        EnemyProjectileCueContext context, EnemyProjectileHandlerTable handlers, out int duration, out EnemyProjectileFramePresentation presentation);
}
