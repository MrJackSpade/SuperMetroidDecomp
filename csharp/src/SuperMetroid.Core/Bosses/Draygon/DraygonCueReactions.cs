namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What the owner of a <see cref="DraygonCue"/> does on each of its cases (#2553).</summary>
/// <param name="StartPartAnimations">Reacts to a <see cref="DraygonCue.StartPartAnimations"/>.</param>
/// <param name="TrackWithEye">Reacts to a <see cref="DraygonCue.TrackWithEye"/>.</param>
/// <param name="ShiftBodyPicture">Reacts to a <see cref="DraygonCue.ShiftBodyPicture"/>.</param>
/// <param name="QueueSound">Reacts to a <see cref="DraygonCue.QueueSound"/>.</param>
/// <param name="SpawnDyingEffect">Reacts to a <see cref="DraygonCue.SpawnDyingEffect"/>.</param>
/// <param name="ParalyseTailAndArms">Reacts to a <see cref="DraygonCue.ParalyseTailAndArms"/>.</param>
/// <param name="SetBodyIntangible">Reacts to a <see cref="DraygonCue.SetBodyIntangible"/>.</param>
/// <param name="SetBodyPhase">Reacts to a <see cref="DraygonCue.SetBodyPhase"/>.</param>
/// <param name="TailWhipHit">Reacts to a <see cref="DraygonCue.TailWhipHit"/>.</param>
/// <param name="SpawnGoop">Reacts to a <see cref="DraygonCue.SpawnGoop"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record DraygonCueReactions<TContext>(Action<DraygonCue.StartPartAnimations, TContext> StartPartAnimations, Action<DraygonCue.TrackWithEye, TContext> TrackWithEye, Action<DraygonCue.ShiftBodyPicture, TContext> ShiftBodyPicture, Action<DraygonCue.QueueSound, TContext> QueueSound, Action<DraygonCue.SpawnDyingEffect, TContext> SpawnDyingEffect, Action<DraygonCue.ParalyseTailAndArms, TContext> ParalyseTailAndArms, Action<DraygonCue.SetBodyIntangible, TContext> SetBodyIntangible, Action<DraygonCue.SetBodyPhase, TContext> SetBodyPhase, Action<DraygonCue.TailWhipHit, TContext> TailWhipHit, Action<DraygonCue.SpawnGoop, TContext> SpawnGoop);
