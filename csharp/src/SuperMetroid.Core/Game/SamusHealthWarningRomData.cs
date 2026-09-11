using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>Cartridge definitions for LowEnergyCheck and its critical-energy latch.</summary>
public static class SamusHealthWarningRomData
{
    /// <summary>$90:EA82 compares energy with 31 using the subtraction's negative flag.</summary>
    public const ushort HealthyThreshold = 31;
    /// <summary>$90:EAA0/$EA92 use QueueSound_Lib3_Max6.</summary>
    public const byte MaximumQueuedSounds = 6;
    /// <summary>$90:EA9D, library-three $02 starts the critical-energy warning.</summary>
    public static readonly SoundEffectId Start = new(SoundEffectLibrary.Library3, 0x02);
    /// <summary>$90:EA8F, library-three $01 stops the warning through the native cancel command.</summary>
    public static readonly SoundEffectId Stop = new(SoundEffectLibrary.Library3, 0x01);
}
