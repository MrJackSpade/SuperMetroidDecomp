using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Ripper.Artwork;

/// <summary>Cartridge values and helpers of <see cref="RipperFrameDefinitions"/> that only verification reads.</summary>
internal static class RipperFrameDefinitionsConstants
{
    /// <summary>The poses.</summary>
    internal const int PoseCount = 12;
    /// <summary><c>Spritemap_GRipper_Ripper2_Frozen_FacingLeft</c> at $A2:E43F, pinned while a GRipper or Ripper II facing left is frozen.</summary>
    internal const ushort FrozenFacingLeftSpritemap = 0xe43f;
}
