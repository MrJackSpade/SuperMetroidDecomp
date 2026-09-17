namespace SuperMetroid.Core.Assets;

/// <summary>Schema and native identities for the two escape-warning text programs.</summary>
public static class EscapeTypewriterDefinitions
{
    public const int Version = 1;
    public const string FileName = "escape-typewriter.json";
    public const int CeresSourceAddress = 0xa6c450;
    public const int ZebesSourceAddress = 0xa6c49c;
    public const ushort CharacterDelayFrames = 2;
    public const int MaximumLineLength = 32;

    public static int SourceAddress(EscapeTypewriterProgramId id) => id switch
    {
        EscapeTypewriterProgramId.Ceres => CeresSourceAddress,
        EscapeTypewriterProgramId.Zebes => ZebesSourceAddress,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
    };
}
