namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's purple breath animations (#641, #2554), each its segment (<see cref="MotherBrainBreathAnimations"/>).</summary>
internal enum MotherBrainBreathSegment
{
    /// <summary>A large puff of Mother Brain's purple breath, stopping where it is and growing into a plume that disperses and fades.</summary>
    PurpleBreathBig,
    /// <summary>A small puff of Mother Brain's purple breath, stopping where it is and swelling into a cloud that disperses and fades, then marking her small breath ended.</summary>
    PurpleBreathSmall,
}
