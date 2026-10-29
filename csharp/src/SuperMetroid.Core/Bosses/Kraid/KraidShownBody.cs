using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's last shown body frame, the sheet it was drawn from and the layer drawn (#1466).</summary>
/// <param name="Frame">The body frame.</param>
/// <param name="Sheet">The body sheet it was drawn from.</param>
/// <param name="Layer">The layer drawn.</param>
internal sealed record KraidShownBody(KraidBodyFrame Frame, BossBodySheet Sheet, BossBodyLayer Layer);
