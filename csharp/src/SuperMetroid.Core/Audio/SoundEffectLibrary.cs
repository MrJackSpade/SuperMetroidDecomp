namespace SuperMetroid.Core.Audio;

/// <summary>
/// The three mutually exclusive sound-effect queues exposed by the retail cartridge.
/// These values select different SPC input ports and are not combinable flags.
/// </summary>
public enum SoundEffectLibrary
{
    /// <summary>First cartridge sound queue, published through APU port $2141 and host queue index zero.</summary>
    Library1 = 1,
    /// <summary>Second cartridge sound queue, published through APU port $2142 and host queue index one.</summary>
    Library2 = 2,
    /// <summary>Third cartridge sound queue, published through APU port $2143 and host queue index two.</summary>
    Library3 = 3,
}
