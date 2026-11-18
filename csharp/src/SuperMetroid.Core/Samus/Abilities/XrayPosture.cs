namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Which visor pose the X-ray takes from the way Samus moves, or whether it starts at all. Its values are arbitrary.</summary>
internal enum XrayPosture
{
    /// <summary>Movement type outside the visor's standing and crouching entry paths.</summary>
    Disallowed,
    /// <summary>Uses the standing right or left visor pose.</summary>
    Standing,
    /// <summary>Uses the crouching right or left visor pose.</summary>
    Crouching,
}
