using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>The phase and resumes of one game over menu indexes expected.</summary>
/// <param name="Phase">The phase.</param>
/// <param name="Resumes">The resumes.</param>
internal readonly record struct GameOverMenuIndexesExpected(GameOverMenuPhase Phase, bool Resumes);
