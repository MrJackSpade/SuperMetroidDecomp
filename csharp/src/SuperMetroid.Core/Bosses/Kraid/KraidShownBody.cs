using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's body as he last showed it, the sheet it was drawn from and the body published (#1466, #1515).</summary>
/// <param name="Appearance">How his body showed.</param>
/// <param name="Head">The head pose shown over it, or null before his head showed one.</param>
/// <param name="Sheet">The body sheet it was drawn from.</param>
/// <param name="Layer">The body published.</param>
internal sealed record KraidShownBody(KraidBodyAppearance Appearance, KraidHeadFrame? Head, BossBodySheet Sheet, BossBodyLayer Layer);
