namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Which of the two wall heads a Fune or Namihe placement behaves as (#1519). The placement names it apart from its
/// species, and the behaviour follows the placement: a placement may give one species the other's behaviour.
/// </summary>
public enum FuneNamiheVariant
{
    /// <summary>Spits on a fixed cooldown, whatever Samus does.</summary>
    Fune,

    /// <summary>Waits until Samus comes within its vertical distance, and spits from a mouth four pixels lower.</summary>
    Namihe,
}
