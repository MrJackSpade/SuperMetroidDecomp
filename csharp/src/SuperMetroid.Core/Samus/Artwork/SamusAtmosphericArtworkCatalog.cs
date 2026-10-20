using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable direct small-OBJ attributes for Samus's bank-$90 atmospheric effects.</summary>
/// <remarks>
/// Types four, six, and seven share the same retail list. Type two has a null retail
/// pointer and deliberately remains on the mutable-address-space path.
/// </remarks>
public sealed class SamusAtmosphericArtworkCatalog
{
    /// <summary>Edited type-one footstep OBJ attributes by animation-frame ordinal.</summary>
    internal readonly Dictionary<int, ushort> typeOne = new();
    /// <summary>Edited lava/dust attributes shared by types four, six, and seven.</summary>
    internal readonly Dictionary<int, ushort> sharedTypeFour = new();

    /// <summary>Captures selected four-frame attribute lists, retaining independent differences from calculated stock words without retaining caller arrays.</summary>
    /// <param name="typeOne">Four type-one footstep words in animation-frame order, corresponding to $90:8C0F.</param>
    /// <param name="sharedTypeFour">Four shared lava/dust words for types 4, 6, and 7, corresponding to $90:8C17.</param>
    /// <exception cref="ArgumentNullException">Either input array is null.</exception>
    /// <exception cref="InvalidDataException">Either list does not contain exactly four words. Individual packed attribute values are not restricted.</exception>
    public SamusAtmosphericArtworkCatalog(ushort[] typeOne, ushort[] sharedTypeFour)
    {
        Ensure.NotNull(typeOne);
        Ensure.NotNull(sharedTypeFour);
        if (typeOne.Length != SamusMovementRomData.Environment.DirectAtmosphericFrameCount ||
            sharedTypeFour.Length != SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
            throw new InvalidDataException("Samus atmospheric small-OBJ lists must each contain four frames.");
        for (int frame = 0; frame < typeOne.Length; frame++)
        {
            if (typeOne[frame] != SamusAtmosphericArtworkDefinitions.Attributes(true, frame))
                this.typeOne.Add(frame, typeOne[frame]);
            if (sharedTypeFour[frame] != SamusAtmosphericArtworkDefinitions.Attributes(false, frame))
                this.sharedTypeFour.Add(frame, sharedTypeFour[frame]);
        }
        TypeOne = new FrameSequence(this, AtmosphericFrameList.Footsteps);
        SharedTypeFour = new FrameSequence(this, AtmosphericFrameList.SharedLavaDust);
    }

    /// <summary>SHA-256 of both selected atmospheric small-OBJ lists.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusAtmosphericArtworkCatalog), content =>
    {
        content.AppendWords("type one", TypeOne.ToArray());
        content.AppendWords("shared type four", SharedTypeFour.ToArray());
    });

    /// <summary>Immutable four-word view of AtmosphericGraphics_SpriteTileNumberAttributes_1_Footstep ($90:8C0F), indexed by frame 0..3.</summary>
    public IReadOnlyList<ushort> TypeOne { get; }
    /// <summary>Immutable four-word view of AtmosphericGraphics_SpriteTileNumberAttribute_4_6_7_LavaDust ($90:8C17), shared by types 4, 6, and 7.</summary>
    public IReadOnlyList<ushort> SharedTypeFour { get; }

    /// <summary>Resolves one direct small-OBJ word, with tile bits 0..8, palette bits 9..11, priority bits 12..13, and flip bits 14..15.</summary>
    /// <param name="type">Native atmospheric type: 1 for footsteps, or 4, 6, or 7 for shared lava/dust. Type 2's mirrored-WRAM words and spritemap-based effects are not catalog entries.</param>
    /// <param name="frame">Animation-frame index 0..3; does not select an animation duration.</param>
    /// <param name="attributes">Selected packed tile/attribute word on success; zero on failure.</param>
    /// <returns>True for a supported type and frame; false otherwise without reading cartridge or mutable memory.</returns>
    public bool TryResolve(byte type, int frame, out ushort attributes)
    {
        if (type is not (1 or 4 or 6 or 7) || frame < 0 || frame >= SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
        {
            attributes = 0;
            return false;
        }
        attributes = Resolve(type == 1, frame);
        return true;
    }

    /// <summary>Uses a supplied edit when present, otherwise calculates the stock direct-OBJ word.</summary>
    private ushort Resolve(bool footstep, int frame) =>
        (footstep ? typeOne : sharedTypeFour).TryGetValue(frame, out ushort attributes)
            ? attributes : SamusAtmosphericArtworkDefinitions.Attributes(footstep, frame);

    /// <summary>Read-only four-frame view over one selected atmospheric attribute list.</summary>
    private sealed class FrameSequence(SamusAtmosphericArtworkCatalog owner, AtmosphericFrameList list) : IReadOnlyList<ushort>
    {
        /// <summary>The native direct-attribute list has four animation positions.</summary>
        public int Count => SamusMovementRomData.Environment.DirectAtmosphericFrameCount;
        /// <summary>Resolves a bounded frame from its owning footstep or shared lava/dust list.</summary>
        public ushort this[int index] => (uint)index < Count
            ? owner.Resolve(list == AtmosphericFrameList.Footsteps, index) : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields selected attribute words in animation-frame order.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int frame = 0; frame < Count; frame++) yield return this[frame];
        }
        /// <summary>Exposes the same attribute sequence to non-generic consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
