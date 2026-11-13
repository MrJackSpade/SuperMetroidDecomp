using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The arm cannon's drawing descriptors as the import reads them (#1638): each pose's descriptor within the extracted
/// drawing window, the cartridge's direction, drawing-mode and displacement bytes. Only the import decodes them, into each
/// pose's typed placement (<see cref="Gameplay"/>), and installs the placements the game draws with
/// (<see cref="Installed"/>).
/// </summary>
public sealed class SamusArmCannonDescriptors
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

    /// <summary>Where each pose's drawing descriptor begins within the drawing data.</summary>
    private readonly Dictionary<SamusPoseId, int> poseDescriptors;
    /// <summary>The extracted drawing window: every descriptor's direction, mode and displacement bytes.</summary>
    private readonly byte[] drawingData;

    /// <summary>Holds validated descriptors and drawing data and decodes each pose's placement.</summary>
    private SamusArmCannonDescriptors(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors, byte[] drawingData)
    {
        this.poseDescriptors = poseDescriptors.ToDictionary();
        this.drawingData = [.. drawingData];
        Gameplay = new InstalledSamusArmCannonArtwork(WindowPlacements());
    }

    /// <summary>
    /// Each pose's placement as its descriptor reads it: the displacement of every animation frame the drawing window holds,
    /// continuing through the descriptors stored after the pose's own as the game's per-frame read does.
    /// </summary>
    public InstalledSamusArmCannonArtwork Gameplay { get; }

    /// <summary>Gets where <paramref name="pose"/>'s drawing descriptor begins within the drawing data.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    public int PoseDescriptor(SamusPoseId pose) =>
        poseDescriptors.TryGetValue(pose, out int selected) ? selected
            : throw new ArgumentOutOfRangeException(nameof(pose));

    /// <summary>The descriptor byte <paramref name="offset"/> bytes into the drawing data.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    /// <exception cref="InvalidDataException">The offset lies outside the extracted drawing window.</exception>
    public byte ReadDrawingByte(int offset) => (uint)offset < (uint)drawingData.Length
        ? drawingData[offset]
        : throw new InvalidDataException($"Arm-cannon drawing byte {offset} is not extracted.");

    /// <summary>Validates and holds <paramref name="poseDescriptors"/> over <paramref name="drawingData"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The descriptors do not cover exactly the poses with records, a descriptor lies outside the drawing data, the drawing
    /// data is not the whole window, or a drawing value is not a byte.
    /// </exception>
    internal static SamusArmCannonDescriptors Create(IReadOnlyDictionary<SamusPoseId, int>? poseDescriptors, int[]? drawingData)
    {
        if (poseDescriptors is null || drawingData is null || drawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException("Arm-cannon descriptors have an invalid table geometry.");
        if (!SamusPoseNumbers.CoversRecordedPoses(poseDescriptors))
            throw new InvalidDataException("Arm-cannon descriptors must cover exactly the poses with records.");
        foreach (int descriptor in poseDescriptors.Values)
            if (descriptor < 0 || descriptor + LaterDirectionByte >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
                throw new InvalidDataException($"Arm-cannon pose descriptor {descriptor} lies outside drawing data.");
        byte[] data = [.. drawingData.Select((value, index) => (uint)value <= byte.MaxValue
            ? (byte)value
            : throw new InvalidDataException($"Arm-cannon drawing byte {index} is not a byte."))];
        return new SamusArmCannonDescriptors(poseDescriptors, data);
    }

    /// <summary>
    /// The placements the installation holds (#1638): each pose's drawing order and directions, and the displacement of
    /// every animation frame <paramref name="shown"/> says the pose shows, which are the frames the cover is drawn at, whose
    /// displacement the drawing window holds. A displacement read past the pose's own descriptor is the value the game reads
    /// there. A shown frame whose displacement lies past the window has none, as the window's placements have none there.
    /// </summary>
    internal SamusArmCannonLayout Installed(SamusShownFrames shown)
    {
        Ensure.NotNull(shown);
        return new SamusArmCannonLayout(SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
            Decode(poseDescriptors[pose], Enumerable.Range(0, InstalledSamusBody.AnimationFrames)
                .Where(frame => shown.TryPictureFrame(pose, frame, out _) && WithinWindow(poseDescriptors[pose], frame)))));
    }

    /// <summary>Each pose's placement over every animation frame the drawing window holds; poses sharing a descriptor share it.</summary>
    private Dictionary<SamusPoseId, SamusArmCannonPlacement> WindowPlacements()
    {
        var placements = new Dictionary<int, SamusArmCannonPlacement>();
        return SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
        {
            int descriptor = poseDescriptors[pose];
            if (!placements.TryGetValue(descriptor, out SamusArmCannonPlacement? placement))
            {
                int frames = Math.Max(0, (drawingData.Length - DisplacementsStart(descriptor)) / DisplacementBytes);
                placement = Decode(descriptor, Enumerable.Range(0, frames));
                placements.Add(descriptor, placement);
            }
            return placement;
        });
    }

    /// <summary>
    /// Decodes <paramref name="descriptor"/> into a typed placement over <paramref name="frames"/>: the descriptor's direction
    /// byte, whose top bit makes the byte after the drawing mode the direction of every later animation frame, then the
    /// drawing mode, then one signed X and Y displacement per animation frame.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame's displacement lies past the drawing window.</exception>
    private SamusArmCannonPlacement Decode(int descriptor, IEnumerable<int> frames)
    {
        int direction = drawingData[descriptor];
        bool frameDependent = (direction & FrameDependentDirectionFlag) != 0;
        int displacements = DisplacementsStart(descriptor);
        // The drawing-mode byte's low four bits choose whether and when the cover draws; each direction byte names an aim.
        return new SamusArmCannonPlacement(SamusArmCannonDrawOrderCodec.FromDrawingMode(drawingData[descriptor + DrawingModeByte]),
            SamusProjectileDirectionCodec.FromAimIndex(direction & DirectionBits),
            frameDependent ? SamusProjectileDirectionCodec.FromAimIndex(drawingData[descriptor + LaterDirectionByte] & DirectionBits) : null,
            frames.ToDictionary(frame => frame, frame => Displacement(displacements + frame * DisplacementBytes, frame)));
    }

    /// <summary>Where <paramref name="descriptor"/>'s displacements begin: after its one or two direction and mode pairs.</summary>
    private int DisplacementsStart(int descriptor) => descriptor +
        ((drawingData[descriptor] & FrameDependentDirectionFlag) != 0 ? FrameDependentHeaderBytes : SingleDirectionHeaderBytes);

    /// <summary>Whether animation frame <paramref name="frame"/>'s displacement of <paramref name="descriptor"/> lies within the drawing window.</summary>
    private bool WithinWindow(int descriptor, int frame) =>
        DisplacementsStart(descriptor) + frame * DisplacementBytes + 1 < drawingData.Length;

    /// <summary>The signed X and Y displacement stored at <paramref name="at"/> for animation frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The displacement lies past the drawing window.</exception>
    private Offset16<RoomPixels> Displacement(int at, int frame) => at + 1 < drawingData.Length
        ? Offset16<RoomPixels>.Of(unchecked((sbyte)drawingData[at]), unchecked((sbyte)drawingData[at + 1]))
        : throw new InvalidDataException($"Arm-cannon animation frame {frame}'s displacement lies past the drawing window.");
}
