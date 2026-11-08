namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The exclusive authored color phases of ordinary Dachora.</summary>
public enum DachoraColourPhase
{
    /// <summary>Its normal colours, restored outside its speed and shine glows.</summary>
    Default,

    /// <summary>The four-frame speed glow it shows while running at full speed.</summary>
    Speed,

    /// <summary>The four-frame stored-shine glow it shows while charging and launching.</summary>
    Shine,
}
