namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Observable outputs of one solid-enemy collision probe.</summary>
/// <param name="Collided">True when the rounded future box overlaps a qualifying body and the current directional gap is not negative.</param>
/// <param name="Distance">The whole-pixel gap, zero or more, between Samus's edge and the body's on collision, or the requested whole distance on failure; no fraction of a pixel is returned.</param>
/// <param name="Enemy">The enemy met on collision, or null on failure.</param>
/// <param name="WasTouching">True for a successful zero-gap contact, which drops Samus's fraction of a pixel vertically; false for positive gaps and failed probes.</param>
[IndependentFlag("independent collision facts: a collision occurred, and Samus was already touching")]
public readonly record struct SolidEnemyCollisionResult(
    bool Collided,
    int Distance,
    ISolidEnemy? Enemy,
    bool WasTouching);
