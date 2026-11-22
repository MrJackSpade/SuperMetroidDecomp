namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What the owner of a <see cref="MagdolliteCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaySoundIfOnScreen">Reacts to a <see cref="MagdolliteCue.PlaySoundIfOnScreen"/>.</param>
/// <param name="Move">Reacts to a <see cref="MagdolliteCue.Move"/>.</param>
/// <param name="MoveWithPillar">Reacts to a <see cref="MagdolliteCue.MoveWithPillar"/>.</param>
/// <param name="ShiftFromThrowOrigin">Reacts to a <see cref="MagdolliteCue.ShiftFromThrowOrigin"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MagdolliteCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record MagdolliteCueReactions<TContext>(Action<MagdolliteCue.PlaySoundIfOnScreen, TContext> PlaySoundIfOnScreen, Action<MagdolliteCue.Move, TContext> Move, Action<MagdolliteCue.MoveWithPillar, TContext> MoveWithPillar, Action<MagdolliteCue.ShiftFromThrowOrigin, TContext> ShiftFromThrowOrigin, Action<MagdolliteCue.Signal, TContext> Signal);
