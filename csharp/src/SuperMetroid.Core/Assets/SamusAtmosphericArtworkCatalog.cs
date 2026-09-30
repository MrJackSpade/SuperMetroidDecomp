using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable direct small-OBJ attributes for Samus's bank-$90 atmospheric effects.</summary>
/// <remarks>
/// Types four, six, and seven share the same retail list. Type two has a null retail
/// pointer and deliberately remains on the mutable-address-space path.
/// </remarks>
public sealed class SamusAtmosphericArtworkCatalog
{
    private readonly ushort[] typeOne;
    private readonly ushort[] sharedTypeFour;

    public SamusAtmosphericArtworkCatalog(ushort[] typeOne, ushort[] sharedTypeFour)
    {
        ArgumentNullException.ThrowIfNull(typeOne);
        ArgumentNullException.ThrowIfNull(sharedTypeFour);
        if (typeOne.Length != SamusMovementRomData.Environment.DirectAtmosphericFrameCount ||
            sharedTypeFour.Length != SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
            throw new InvalidDataException("Samus atmospheric small-OBJ lists must each contain four frames.");
        this.typeOne = (ushort[])typeOne.Clone();
        this.sharedTypeFour = (ushort[])sharedTypeFour.Clone();
    }

    /// <summary>SHA-256 of both selected atmospheric small-OBJ lists.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusAtmosphericArtworkCatalog), content =>
    {
        content.AppendWords("type one", this.typeOne);
        content.AppendWords("shared type four", this.sharedTypeFour);
    });

    public ReadOnlySpan<ushort> TypeOne => typeOne;
    public ReadOnlySpan<ushort> SharedTypeFour => sharedTypeFour;

    public bool TryResolve(byte type, byte frame, out ushort attributes)
    {
        ushort[]? list = type switch
        {
            1 => typeOne,
            4 or 6 or 7 => sharedTypeFour,
            _ => null,
        };
        if (list is null || frame >= list.Length)
        {
            attributes = 0;
            return false;
        }
        attributes = list[frame];
        return true;
    }
}
