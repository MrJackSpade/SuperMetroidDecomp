namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One wave of the flashback's Rinka launches (#2561): the wait before it and the pair it launches.</summary>
/// <param name="Wait">The updates the wave waits, the one it launches on included.</param>
/// <param name="Pair">The Rinkas it launches, in launch order.</param>
internal sealed record IntroRinkaLaunchWave(int Wait, IReadOnlyList<IntroRinkaLaunch> Pair);
