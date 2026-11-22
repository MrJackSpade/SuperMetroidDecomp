namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What each case of a <see cref="FakeKraidCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Spit">Makes a <see cref="FakeKraidCue.Spit"/> into the result.</param>
/// <param name="ChooseAction">Makes a <see cref="FakeKraidCue.ChooseAction"/> into the result.</param>
/// <param name="Move">Makes a <see cref="FakeKraidCue.Move"/> into the result.</param>
/// <param name="PlayCrySound">Makes a <see cref="FakeKraidCue.PlayCrySound"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record FakeKraidCueCases<TContext, TResult>(Func<FakeKraidCue.Spit, TContext, TResult> Spit, Func<FakeKraidCue.ChooseAction, TContext, TResult> ChooseAction, Func<FakeKraidCue.Move, TContext, TResult> Move, Func<FakeKraidCue.PlayCrySound, TContext, TResult> PlayCrySound);
