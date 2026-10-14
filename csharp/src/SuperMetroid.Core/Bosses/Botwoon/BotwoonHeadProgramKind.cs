namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What Botwoon's head animation is doing.</summary>
internal enum BotwoonHeadProgramKind
{
    /// <summary>Travelling inside the wall: nothing drawn (<c>InstList_Botwoon_Hide</c>).</summary>
    Hidden,
    /// <summary>Travelling outside: the closed mouth faces the movement (<c>InstList_Botwoon_MouthClosed_*</c>).</summary>
    Moving,
    /// <summary>Aiming at Samus, then opening to spit (<c>InstList_Botwoon_Spit_*</c>).</summary>
    Spitting,
}
