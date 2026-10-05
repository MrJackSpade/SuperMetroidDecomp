using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable direct small-OBJ attributes for Samus's bank-$90 atmospheric effects.</summary>
/// <remarks>
/// Types four, six, and seven share the same retail list. Type two has a null retail
/// pointer and deliberately remains on the mutable-address-space path.
/// </remarks>
public sealed class SamusAtmosphericArtworkCatalog
{
    private readonly Dictionary<int, ushort> typeOne = new();
    private readonly Dictionary<int, ushort> sharedTypeFour = new();

    public SamusAtmosphericArtworkCatalog(ushort[] typeOne, ushort[] sharedTypeFour)
    {
        ArgumentNullException.ThrowIfNull(typeOne);
        ArgumentNullException.ThrowIfNull(sharedTypeFour);
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
        TypeOne = new FrameSequence(this, true);
        SharedTypeFour = new FrameSequence(this, false);
    }

    /// <summary>SHA-256 of both selected atmospheric small-OBJ lists.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusAtmosphericArtworkCatalog), content =>
    {
        content.AppendWords("type one", TypeOne.ToArray());
        content.AppendWords("shared type four", SharedTypeFour.ToArray());
    });

    public IReadOnlyList<ushort> TypeOne { get; }
    public IReadOnlyList<ushort> SharedTypeFour { get; }

    public bool TryResolve(byte type, byte frame, out ushort attributes)
    {
        if (type is not (1 or 4 or 6 or 7) || frame >= SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
        {
            attributes = 0;
            return false;
        }
        attributes = Resolve(type == 1, frame);
        return true;
    }

    private ushort Resolve(bool footstep, int frame) =>
        (footstep ? typeOne : sharedTypeFour).TryGetValue(frame, out ushort attributes)
            ? attributes : SamusAtmosphericArtworkDefinitions.Attributes(footstep, frame);

    private sealed class FrameSequence(SamusAtmosphericArtworkCatalog owner, bool footstep) : IReadOnlyList<ushort>
    {
        public int Count => SamusMovementRomData.Environment.DirectAtmosphericFrameCount;
        public ushort this[int index] => (uint)index < Count
            ? owner.Resolve(footstep, index) : throw new ArgumentOutOfRangeException(nameof(index));
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int frame = 0; frame < Count; frame++) yield return this[frame];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
