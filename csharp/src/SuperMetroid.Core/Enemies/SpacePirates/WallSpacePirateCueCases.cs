namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What each case of a <see cref="WallSpacePirateCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Climb">Makes a <see cref="WallSpacePirateCue.Climb"/> into the result.</param>
/// <param name="ChooseClimb">Makes a <see cref="WallSpacePirateCue.ChooseClimb"/> into the result.</param>
/// <param name="FireLaser">Makes a <see cref="WallSpacePirateCue.FireLaser"/> into the result.</param>
/// <param name="PrepareJump">Makes a <see cref="WallSpacePirateCue.PrepareJump"/> into the result.</param>
/// <param name="Begin">Makes a <see cref="WallSpacePirateCue.Begin"/> into the result.</param>
/// <param name="BeginOnWall">Makes a <see cref="WallSpacePirateCue.BeginOnWall"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="WallSpacePirateCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record WallSpacePirateCueCases<TContext, TResult>(Func<WallSpacePirateCue.Climb, TContext, TResult> Climb, Func<WallSpacePirateCue.ChooseClimb, TContext, TResult> ChooseClimb, Func<WallSpacePirateCue.FireLaser, TContext, TResult> FireLaser, Func<WallSpacePirateCue.PrepareJump, TContext, TResult> PrepareJump, Func<WallSpacePirateCue.Begin, TContext, TResult> Begin, Func<WallSpacePirateCue.BeginOnWall, TContext, TResult> BeginOnWall, Func<WallSpacePirateCue.Signal, TContext, TResult> Signal);
