namespace SuperMetroid.Core.Assets;

/// <summary>
/// Stable visual-frame identities for the two one-frame bank-$86 Skree/Metaree
/// projectile programs. The compositions themselves live in editable asset JSON.
/// </summary>
internal static class SkreeMetareeParticleVisualDefinitions
{
    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABF.</summary>
    internal const ushort SkreeOperand = 0x8abf;

    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC7.</summary>
    internal const ushort MetareeOperand = 0x8ac7;

    /// <summary>Bank-$8D Skree-debris composition selected by $86:8ABF.</summary>
    internal const ushort SkreeComposition = 0x8023;

    /// <summary>Bank-$8D Metaree-debris composition selected by $86:8AC7.</summary>
    internal const ushort MetareeComposition = 0x8435;

    /// <summary>
    /// Maps a Skree or Metaree bank-$86 projectile spritemap operand to its bank-$8D debris
    /// composition pointer.
    /// </summary>
    /// <param name="operandAddress">Bank-local instruction operand for the Skree or Metaree particle.</param>
    /// <returns>The selected debris composition pointer.</returns>
    /// <exception cref="InvalidDataException">The operand is not one of the two catalogued particle selectors.</exception>
    internal static ushort Resolve(ushort operandAddress) => operandAddress switch
    {
        SkreeOperand => SkreeComposition,
        MetareeOperand => MetareeComposition,
        _ => throw new InvalidDataException(
            $"Skree/Metaree visual operand $86:{operandAddress:X4} is not catalogued."),
    };
}
