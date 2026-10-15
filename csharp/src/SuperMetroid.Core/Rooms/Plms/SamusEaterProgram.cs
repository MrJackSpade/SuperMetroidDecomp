namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Brinstar plants' PLM programs (#1296), in place of their bank-$84 lists at $84:ACB8..AD37.</summary>
internal enum SamusEaterProgram
{
    /// <summary>The floor plant bites: holds Samus and seeds four chews.</summary>
    FloorBite,
    /// <summary>One floor chew, repeated while the counter runs, then the release.</summary>
    FloorChew,
    /// <summary>The ceiling plant bites: holds Samus and seeds eight chews.</summary>
    CeilingBite,
    /// <summary>One ceiling chew, repeated while the counter runs, then the release.</summary>
    CeilingChew,
}
