namespace SuperMetroid.Core.Game;

/// <summary>The exclusive authored color phases of ordinary Dachora.</summary>
public enum DachoraPalettePhase
{
    /// <summary>The ordinary complete OBJ palette restored outside speed and shine animation.</summary>
    Default,

    /// <summary>The four-frame acceleration palette cycle used at maximum running speed.</summary>
    Speed,

    /// <summary>The four-frame stored-shine palette cycle used while charging and launching.</summary>
    Shine,
}
