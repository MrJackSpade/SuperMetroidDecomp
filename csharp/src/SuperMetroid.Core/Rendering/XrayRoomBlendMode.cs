namespace SuperMetroid.Core.Rendering;

/// <summary>How the X-ray beam blends a room; each room takes one.</summary>
public enum XrayRoomBlendMode
{
    /// <summary>Inside the beam, blocks show what they really are.</summary>
    RevealBlocks,
    /// <summary>The room's backgrounds stay as they are and blocks are not revealed.</summary>
    PreserveBackgrounds,
    /// <summary>Fireflea's darkness stays outside the beam.</summary>
    Fireflea,
}
