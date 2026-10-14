namespace SuperMetroid.Core.Audio;

/// <summary>The four byte-wide bidirectional APU I/O ports, SNES registers <c>$2140-$2143</c>.</summary>
public enum ApuPort : byte
{
    /// <summary>Port zero, <c>$2140</c>, used for music commands and upload handshakes.</summary>
    Music = 0,
    /// <summary>Port one, <c>$2141</c>: sound library one; adding a zero-based library index selects ports one through three.</summary>
    SoundLibrary1 = 1,
    /// <summary>Port two, <c>$2142</c>: sound library two.</summary>
    SoundLibrary2 = 2,
    /// <summary>Port three, <c>$2143</c>: sound library three.</summary>
    SoundLibrary3 = 3,
}
