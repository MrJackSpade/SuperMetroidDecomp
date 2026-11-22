namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// How far around a death explosion the effect actor one of its cues spawns may land (#2151): a random sample's two
/// scatter fields place it right of and below the explosion, and the whole offset is centred back by the radius, so the
/// actor lands up to the radius away each way.
/// </summary>
/// <param name="HorizontalScatter">The random sample's field that places the actor right of the explosion before centring.</param>
/// <param name="VerticalScatter">The random sample's field that places the actor below the explosion before centring.</param>
/// <param name="Radius">The pixels both offsets are centred back by.</param>
internal readonly partial record struct EnemyDeathSpriteSpread(RandomSampleField HorizontalScatter, RandomSampleField VerticalScatter, int Radius);
