
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainContactHitboxDefinitions"/> members production does not use.</summary>
internal static class MotherBrainContactHitboxDefinitionsAccess
{
    /// <summary>Maps a Mother Brain contact part to the source address of its native hitbox record.</summary>
    extension(MotherBrainContactHitboxDefinitions)
    {
        /// <summary>Returns the pinned list address used only for parity diagnostics.</summary>
        internal static int GetSourceAddress(MotherBrainContactPart part) =>
            part switch
            {
                MotherBrainContactPart.Body => MotherBrainContactHitboxDefinitions.BodySourceAddress,
                MotherBrainContactPart.Brain => MotherBrainContactHitboxDefinitions.BrainSourceAddress,
                MotherBrainContactPart.Neck => MotherBrainContactHitboxDefinitions.NeckSourceAddress,
                _ => throw new ArgumentOutOfRangeException(nameof(part)),
            };
    }
}
