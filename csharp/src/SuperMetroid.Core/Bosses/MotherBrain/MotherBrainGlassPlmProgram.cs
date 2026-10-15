namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's glass PLM programs (#1296), in place of its bank-$84 lists at $84:D202..D2F8.</summary>
internal enum MotherBrainGlassPlmProgram
{
    /// <summary>Picks the glass's state on entry and arms the hit wake.</summary>
    Entry,
    /// <summary>The intact glass, held until two hits.</summary>
    Stage0,
    /// <summary>The first damage, held until four hits.</summary>
    Stage1,
    /// <summary>The second damage, held until six hits; then the first burst of shards.</summary>
    Stage2,
    /// <summary>The pane transition, held until eight hits.</summary>
    Stage3,
    /// <summary>The first shifted pane, held until ten hits.</summary>
    Stage4,
    /// <summary>The second shifted pane, held until twelve hits; then a burst.</summary>
    Stage5,
    /// <summary>The third shifted pane, held until fourteen hits; then a burst.</summary>
    Stage6,
    /// <summary>The first shatter, held until sixteen hits; then a burst.</summary>
    Stage7,
    /// <summary>The second shatter, held until eighteen hits; then the last burst and the destroyed event.</summary>
    Stage8,
    /// <summary>Mother Brain is dead: the cleared glass, then deletion.</summary>
    BossDead,
    /// <summary>The glass was destroyed on an earlier visit: the shattered glass, then deletion.</summary>
    NoGlass,
}
