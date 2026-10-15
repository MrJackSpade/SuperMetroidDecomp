namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The three non-sidehopper dead-monster graphics families.</summary>
public enum DeadTourianCorpseSpecies
{
    /// <summary>Three 3-by-2-tile Zoomer corpse variants; native wait/pre-rot/rotting functions are $A9:DA69/$DA94/$DAD0.</summary>
    Zoomer,
    /// <summary>Two 3-by-2-tile Ripper corpse variants; native wait/pre-rot/rotting functions are $A9:DA73/$DA99/$DAE6.</summary>
    Ripper,
    /// <summary>Three 2-by-4-tile Skree corpse variants; native wait/pre-rot/rotting functions are $A9:DA6E/$DA9E/$DAFC.</summary>
    Skree,
}
