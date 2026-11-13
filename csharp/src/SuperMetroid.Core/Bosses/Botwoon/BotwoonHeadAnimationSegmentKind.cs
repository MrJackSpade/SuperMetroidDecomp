namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What Botwoon's head animation is doing.</summary>
internal enum BotwoonHeadAnimationSegmentKind
{
    /// <summary>Travelling inside the wall: nothing drawn.</summary>
    Hidden,
    /// <summary>Travelling outside: the closed mouth faces the movement.</summary>
    Moving,
    /// <summary>Aiming at Samus, then opening to spit.</summary>
    Spitting,
}
