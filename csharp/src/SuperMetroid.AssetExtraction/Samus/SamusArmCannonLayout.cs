using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What the game binds of the arm cannon (#1638): each pose's drawing descriptor within the drawing window, from which each
/// pose's typed placement is decoded for gameplay, which receives only <see cref="Gameplay"/>. The installed
/// <c>samus-arm-cannon.json</c> holds only this; the cover appearances and tile selections the overlay sheet is composed
/// from, whose appearances are packed sprite attributes, are import data (<see cref="SamusArmCannonSourceArtwork"/>).
/// </summary>
/// <remarks>
/// The descriptors are still the cartridge's direction, drawing-mode and displacement bytes, decoded here at load. That is a
/// step: the destination installs each pose's typed placement, once the overread displacements past each descriptor have a
/// bounded domain of their own.
/// </remarks>
public sealed class SamusArmCannonLayout
{
    /// <summary>The descriptor's flag byte bit that makes its direction change after animation frame zero.</summary>
    private const int FrameDependentDirectionFlag = 1 << 7;
    /// <summary>The bits of a descriptor's direction byte that name the direction.</summary>
    private const int DirectionBits = FrameDependentDirectionFlag - 1;
    /// <summary>Where a descriptor's drawing mode lies after its direction byte.</summary>
    private const int DrawingModeByte = 1;
    /// <summary>Where a frame-dependent descriptor's later direction lies after its direction byte.</summary>
    private const int LaterDirectionByte = 2;
    /// <summary>The bytes before a single-direction descriptor's displacements: direction and drawing mode.</summary>
    private const int SingleDirectionHeaderBytes = 2;
    /// <summary>The bytes before a frame-dependent descriptor's displacements: both directions with their mode bytes.</summary>
    private const int FrameDependentHeaderBytes = 4;
    /// <summary>The bytes of one animation frame's displacement: signed X, then signed Y.</summary>
    private const int DisplacementBytes = 2;

    /// <summary>Where each pose's drawing descriptor begins within the drawing data, as installed.</summary>
    private readonly Dictionary<SamusPoseId, int> poseDescriptors;
    /// <summary>The installed drawing window: every descriptor's direction, mode and displacement bytes.</summary>
    private readonly byte[] drawingData;

    /// <summary>Holds validated descriptors and drawing data and decodes each pose's placement.</summary>
    private SamusArmCannonLayout(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors, byte[] drawingData)
    {
        Gameplay = new InstalledSamusArmCannonArtwork(Placements(poseDescriptors, drawingData));
        this.poseDescriptors = poseDescriptors.ToDictionary();
        this.drawingData = [.. drawingData];
    }

    /// <summary>Where each pose draws the cover, which is what the game reads of the arm cannon.</summary>
    public InstalledSamusArmCannonArtwork Gameplay { get; }

    /// <summary>SHA-256 of each pose's descriptor and the drawing window.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-layout-v1", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose descriptor", PoseDescriptor(pose));
        content.Append("drawing data", drawingData);
    });

    /// <summary>Gets where <paramref name="pose"/>'s installed drawing descriptor begins within the drawing data.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    public int PoseDescriptor(SamusPoseId pose) =>
        poseDescriptors.TryGetValue(pose, out int selected) ? selected
            : throw new ArgumentOutOfRangeException(nameof(pose));

    /// <summary>The installed descriptor byte <paramref name="offset"/> bytes into the drawing data.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    /// <exception cref="InvalidDataException">The offset lies outside the installed drawing window.</exception>
    public byte ReadDrawingByte(int offset) => (uint)offset < (uint)drawingData.Length
        ? drawingData[offset]
        : throw new InvalidDataException($"Arm-cannon drawing byte {offset} is not installed.");

    /// <summary>Validates and holds <paramref name="poseDescriptors"/> over <paramref name="drawingData"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The descriptors do not cover exactly the poses with records, a descriptor lies outside the drawing data, the drawing
    /// data is not the whole window, or a drawing value is not a byte.
    /// </exception>
    internal static SamusArmCannonLayout Create(IReadOnlyDictionary<SamusPoseId, int>? poseDescriptors, int[]? drawingData)
    {
        if (poseDescriptors is null || drawingData is null || drawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException("Arm-cannon layout has an invalid table geometry.");
        if (!SamusPoseNumbers.CoversRecordedPoses(poseDescriptors))
            throw new InvalidDataException("Arm-cannon placements must cover exactly the poses with records.");
        foreach (int descriptor in poseDescriptors.Values)
            if (descriptor < 0 || descriptor + 2 >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
                throw new InvalidDataException($"Arm-cannon pose descriptor {descriptor} lies outside drawing data.");
        byte[] data = [.. drawingData.Select((value, index) => (uint)value <= byte.MaxValue
            ? (byte)value
            : throw new InvalidDataException($"Arm-cannon drawing byte {index} is not a byte."))];
        return new SamusArmCannonLayout(poseDescriptors, data);
    }

    /// <summary>Parses the installed layout document and validates its descriptors and drawing window.</summary>
    /// <exception cref="InvalidDataException">The document is malformed, has another version or invalid tables.</exception>
    internal static SamusArmCannonLayout Read(Stream json)
    {
        SamusArmCannonLayoutDocument document = JsonAssetDocument.Read<SamusArmCannonLayoutDocument>(json, Options, "arm-cannon layout");
        if (document.Version != SamusArmCannonArtworkFormat.Version)
            throw new InvalidDataException("Arm-cannon layout has an invalid version.");
        return Create(document.PoseDescriptors, document.DrawingData);
    }

    /// <summary>Serializes the layout document as indented camel-case UTF-8 JSON for <c>samus-arm-cannon.json</c>.</summary>
    internal static byte[] Write(SamusArmCannonLayoutDocument document) => JsonSerializer.SerializeToUtf8Bytes(document, Options);

    /// <summary>
    /// Decodes each pose's descriptor into its typed placement: the descriptor's direction byte, whose top bit makes
    /// the byte after the drawing mode the direction of every later animation frame, then the drawing mode, then one
    /// signed X and Y displacement per animation frame. Poses sharing a descriptor share its placement.
    /// </summary>
    private static Dictionary<SamusPoseId, SamusArmCannonPlacement> Placements(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors, byte[] drawingData)
    {
        var placements = new Dictionary<int, SamusArmCannonPlacement>();
        return SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose => Placement(poseDescriptors[pose]));

        SamusArmCannonPlacement Placement(int descriptor)
        {
            if (placements.TryGetValue(descriptor, out SamusArmCannonPlacement? shared))
                return shared;
            int direction = drawingData[descriptor];
            bool frameDependent = (direction & FrameDependentDirectionFlag) != 0;
            int offsetsStart = descriptor + (frameDependent ? FrameDependentHeaderBytes : SingleDirectionHeaderBytes);
            int frames = Math.Max(0, (drawingData.Length - offsetsStart) / DisplacementBytes);
            // The drawing-mode byte's low four bits choose whether and when the cover draws; each direction byte names an aim.
            var placement = new SamusArmCannonPlacement(SamusArmCannonDrawOrderCodec.FromDrawingMode(drawingData[descriptor + DrawingModeByte]),
                SamusProjectileDirectionCodec.FromAimIndex(direction & DirectionBits),
                frameDependent ? SamusProjectileDirectionCodec.FromAimIndex(drawingData[descriptor + LaterDirectionByte] & DirectionBits) : null,
                Enumerable.Range(0, frames).Select(frame => offsetsStart + frame * DisplacementBytes).Select(at => Offset16<RoomPixels>.Of(
                    unchecked((sbyte)drawingData[at]), unchecked((sbyte)drawingData[at + 1]))));
            placements.Add(descriptor, placement);
            return placement;
        }
    }

    /// <summary>Strict camel-case options for loading and writing the layout document.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
