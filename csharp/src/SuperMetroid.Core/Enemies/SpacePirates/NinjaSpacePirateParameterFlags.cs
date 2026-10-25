namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Confirmed meanings in ninja-Pirate population parameter one. The cartridge currently
/// tests only bit zero; preserving the remaining bits as raw data avoids assigning names to
/// behavior that the retail routine never actually proves.
/// </summary>
[Flags]
public enum NinjaSpacePirateParameterFlags : ushort
{
    /// <summary>No named parameter meaning is set.</summary>
    None = 0,

    /// <summary>Starts at the left post and initially faces toward the right post.</summary>
    StartsAtLeftPostFacingRight = 0x0001,
}
