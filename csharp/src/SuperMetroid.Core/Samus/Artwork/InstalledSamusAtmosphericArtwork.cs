using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable single-piece appearances for Samus's bank-$90 atmospheric effects.</summary>
/// <remarks>
/// Types four, six, and seven share the same retail list. Type two has a null retail
/// pointer and deliberately remains on the mutable-address-space path.
/// </remarks>
public sealed class InstalledSamusAtmosphericArtwork
{
    /// <summary>Edited type-one footstep appearances by animation-frame ordinal.</summary>
    internal readonly Dictionary<int, SpritePart> typeOne = new();
    /// <summary>Edited lava/dust appearances shared by types four, six, and seven.</summary>
    internal readonly Dictionary<int, SpritePart> sharedTypeFour = new();

    /// <summary>Captures selected four-frame appearance lists, retaining independent differences from calculated stock without retaining caller arrays.</summary>
    /// <param name="typeOne">Four type-one footstep appearances in animation-frame order, corresponding to $90:8C0F.</param>
    /// <param name="sharedTypeFour">Four shared lava/dust appearances for types 4, 6, and 7, corresponding to $90:8C17.</param>
    /// <exception cref="ArgumentNullException">Either input array is null.</exception>
    /// <exception cref="InvalidDataException">Either list does not contain exactly four appearances.</exception>
    public InstalledSamusAtmosphericArtwork(SpritePart[] typeOne, SpritePart[] sharedTypeFour)
    {
        Ensure.NotNull(typeOne);
        Ensure.NotNull(sharedTypeFour);
        if (typeOne.Length != SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount ||
            sharedTypeFour.Length != SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount)
            throw new InvalidDataException("Samus atmospheric small-OBJ lists must each contain four frames.");
        for (int frame = 0; frame < typeOne.Length; frame++)
        {
            if (typeOne[frame] != SamusAtmosphericAttributes.Attributes(true, frame))
                this.typeOne.Add(frame, typeOne[frame]);
            if (sharedTypeFour[frame] != SamusAtmosphericAttributes.Attributes(false, frame))
                this.sharedTypeFour.Add(frame, sharedTypeFour[frame]);
        }
        TypeOne = [.. Enumerable.Range(0, SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount).Select(frame => Resolve(true, frame))];
        SharedTypeFour = [.. Enumerable.Range(0, SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount).Select(frame => Resolve(false, frame))];
    }

    /// <summary>SHA-256 of both selected atmospheric appearance lists.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledSamusAtmosphericArtwork) + ".v2", content =>
    {
        content.Append("type one", TypeOne.Count);
        foreach (SpritePart part in TypeOne)
            part.AppendIdentity(content);
        content.Append("shared type four", SharedTypeFour.Count);
        foreach (SpritePart part in SharedTypeFour)
            part.AppendIdentity(content);
    });

    /// <summary>Immutable four-frame view of AtmosphericGraphics_SpriteTileNumberAttributes_1_Footstep ($90:8C0F), indexed by frame 0..3.</summary>
    public IReadOnlyList<SpritePart> TypeOne { get; }
    /// <summary>Immutable four-frame view of AtmosphericGraphics_SpriteTileNumberAttribute_4_6_7_LavaDust ($90:8C17), shared by types 4, 6, and 7.</summary>
    public IReadOnlyList<SpritePart> SharedTypeFour { get; }

    /// <summary>Resolves one single-piece appearance: character, palette, priority and mirroring.</summary>
    /// <param name="type">Native atmospheric type: 1 for footsteps, or 4, 6, or 7 for shared lava/dust. Type 2's mirrored-WRAM words and spritemap-based effects are not catalog entries.</param>
    /// <param name="frame">Animation-frame index 0..3; does not select an animation duration.</param>
    /// <param name="attributes">Selected appearance on success; the default piece on failure.</param>
    /// <returns>True for a supported type and frame; false otherwise without reading cartridge or mutable memory.</returns>
    public bool TryResolve(int type, int frame, out SpritePart attributes)
    {
        if (type is not (1 or 4 or 6 or 7) || frame < 0 || frame >= SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount)
        {
            attributes = default;
            return false;
        }
        attributes = Resolve(type == 1, frame);
        return true;
    }

    /// <summary>Uses a supplied edit when present, otherwise calculates the stock appearance.</summary>
    private SpritePart Resolve(bool footstep, int frame)
    {
        Dictionary<int, SpritePart> edits = footstep ? typeOne : sharedTypeFour;
        return edits.ContainsKey(frame) ? edits[frame] : SamusAtmosphericAttributes.Attributes(footstep, frame);
    }
}
