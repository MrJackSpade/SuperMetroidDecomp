namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The installed key of each <see cref="SamusDeathExplosionFrame"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class SamusDeathExplosionFrameKeys
{
    /// <summary>Each SamusDeathExplosionFrame member's installed key.</summary>
    internal static InstalledKeys<SamusDeathExplosionFrame> Installed { get; } = new(new Dictionary<SamusDeathExplosionFrame, string>
    {
        [SamusDeathExplosionFrame.SuitGlowing] = "SuitGlowing",
        [SamusDeathExplosionFrame.SuitCracking] = "SuitCracking",
        [SamusDeathExplosionFrame.SuitShattering] = "SuitShattering",
        [SamusDeathExplosionFrame.PiecesFlying] = "PiecesFlying",
        [SamusDeathExplosionFrame.PiecesSpreading] = "PiecesSpreading",
        [SamusDeathExplosionFrame.PiecesScattered] = "PiecesScattered",
        [SamusDeathExplosionFrame.SuitlessHairStreaming] = "SuitlessHairStreaming",
        [SamusDeathExplosionFrame.SuitlessHairGleaming] = "SuitlessHairGleaming",
        [SamusDeathExplosionFrame.SuitlessHairDimming] = "SuitlessHairDimming",
    });
}
