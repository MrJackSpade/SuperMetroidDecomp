namespace SuperMetroid.Core.Audio;

/// <summary>The two operations the 65816 can publish to Super Metroid's audio CPU.</summary>
public enum CartridgeAudioCommandKind : byte
{
    /// <summary>Upload one cartridge music/SFX data stream into SPC RAM.</summary>
    Upload,

    /// <summary>Write one byte to SNES APU input port $2140-$2143.</summary>
    WritePort,
}
