using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Cartridge bytes that Samus's glitch paths read as data: program code and data past the end of the tables the
/// cartridge meant to read. The import pipeline reads them from the player's cartridge (owner question 3), so no
/// cartridge byte is part of the port; each reader fails when its path is reached without this catalog installed.
/// </summary>
public sealed class SamusOverreadCatalog
{
    /// <summary>The bytes the SpaceTime palette-copy overrun can read.</summary>
    private readonly byte[] _spacetimeCopySource;
    /// <summary>The grapple-handler code the Speed Booster palette overrun reads as colours, as little-endian words.</summary>
    private readonly byte[] _speedBoosterGrappleCode;
    /// <summary>The bytes before the HUD's posture-transition flags.</summary>
    private readonly byte[] _hudPrecedingPostureBytes;
    /// <summary>The bytes after the HUD's posture-transition flags.</summary>
    private readonly byte[] _hudFollowingPostureBytes;
    /// <summary>What a trail reads past each placement list's records, by list and frame; null where the read reaches direct-page scratch.</summary>
    private readonly FrozenDictionary<ProjectileTrailRead, ProjectileTrailPlacement?> _trailOverruns;

    /// <summary>Keeps validated ranges decoded from an installed document.</summary>
    private SamusOverreadCatalog(SamusOverreadDocument document)
    {
        _spacetimeCopySource = Decode(document.SpacetimeCopySource, SamusOverreadDefinitions.SpacetimeCopySourceByteCount,
            nameof(document.SpacetimeCopySource));
        _speedBoosterGrappleCode = Decode(document.SpeedBoosterGrappleCode,
            SamusOverreadDefinitions.SpeedBoosterGrappleCodeColorCount * sizeof(ushort), nameof(document.SpeedBoosterGrappleCode));
        _hudPrecedingPostureBytes = Decode(document.HudPrecedingPostureBytes, SamusOverreadDefinitions.HudPrecedingPostureByteCount,
            nameof(document.HudPrecedingPostureBytes));
        _hudFollowingPostureBytes = Decode(document.HudFollowingPostureBytes, SamusOverreadDefinitions.HudFollowingPostureByteCount,
            nameof(document.HudFollowingPostureBytes));
        ProjectileLists = DecodeProjectileLists(document.ProjectileLists);
        _trailOverruns = DecodeTrailOverruns(document.TrailOverruns, ProjectileLists);
    }

    /// <summary>Where each projectile program's list stands, which the glitch paths' list arithmetic and fresh trails read.</summary>
    internal SamusProjectileListLayout ProjectileLists { get; }

    /// <summary>The SpaceTime palette-copy overrun's source byte <paramref name="offset"/> bytes past the first one it reads.</summary>
    internal byte SpacetimeCopySourceByte(int offset) => Read(_spacetimeCopySource, offset, "SpaceTime copy source");

    /// <summary>Colour word <paramref name="index"/> the Speed Booster palette overrun reads from the grapple handler's code.</summary>
    internal ushort SpeedBoosterGrappleCodeWord(int index) => ReadWord(_speedBoosterGrappleCode, index, "Speed Booster grapple-code colour");

    /// <summary>The byte the HUD's posture-transition admission reads for a pose <paramref name="index"/> poses before the real flags start.</summary>
    internal byte HudPrecedingPostureByte(int index) => Read(_hudPrecedingPostureBytes, index, "HUD preceding posture byte");

    /// <summary>The byte the HUD's posture-transition admission reads for a pose <paramref name="index"/> poses after the real flags end.</summary>
    internal byte HudFollowingPostureByte(int index) => Read(_hudFollowingPostureBytes, index, "HUD following posture byte");

    /// <summary>
    /// The placement a trail reads past <paramref name="list"/>'s records at animation frame <paramref name="frame"/>; null
    /// where the read reaches direct-page scratch.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">No trail reads <paramref name="list"/> at <paramref name="frame"/>.</exception>
    internal ProjectileTrailPlacement? TrailOverrun(ProjectileTrailPlacementList list, int frame) =>
        _trailOverruns.TryGetValue(new(list, frame), out ProjectileTrailPlacement? placement)
            ? placement
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, $"No trail reads {list} past its placements at that frame.");

    /// <summary>Loads and validates an installed overread document.</summary>
    public static SamusOverreadCatalog Load(Stream json)
    {
        SamusOverreadDocument document = JsonAssetDocument.Read<SamusOverreadDocument>(
            json, SamusOverreadFormat.JsonOptions, "Samus overread");
        if (document.Version != SamusOverreadFormat.Version)
            throw new InvalidDataException("Samus overread file has an unsupported version.");
        return new SamusOverreadCatalog(document);
    }

    /// <summary>Validates and serializes an overread document as JSON.</summary>
    public static byte[] Write(SamusOverreadDocument document) =>
        JsonAssetDocument.WriteValidated(document, SamusOverreadFormat.JsonOptions, json => _ = Load(json));

    /// <summary>Decodes one base-64 range and requires its exact length.</summary>
    private static byte[] Decode(string? text, int length, string name)
    {
        if (text is null)
            throw new InvalidDataException($"Samus overread file has no {name}.");
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(text);
        }
        catch (FormatException error)
        {
            throw new InvalidDataException($"Samus overread {name} is not base-64 text.", error);
        }
        if (bytes.Length != length)
            throw new InvalidDataException($"Samus overread {name} holds {bytes.Length} bytes, not {length}.");
        return bytes;
    }

    /// <summary>Decodes every list's reads past its placements, requiring one entry for each frame a trail can read there.</summary>
    private static FrozenDictionary<ProjectileTrailRead, ProjectileTrailPlacement?> DecodeTrailOverruns(
        IReadOnlyDictionary<string, string>? lists, SamusProjectileListLayout layout)
    {
        if (lists is null)
            throw new InvalidDataException("Samus overread file has no trail overruns.");
        ProjectileTrailPlacementList[] known = Enum.GetValues<ProjectileTrailPlacementList>();
        if (lists.Count != known.Length)
            throw new InvalidDataException($"Samus overread file holds trail overruns for {lists.Count} lists, not {known.Length}.");
        var result = new Dictionary<ProjectileTrailRead, ProjectileTrailPlacement?>();
        foreach (ProjectileTrailPlacementList list in known)
        {
            if (!lists.TryGetValue(list.ToString(), out string? text))
                throw new InvalidDataException($"Samus overread file has no trail overruns for {list}.");
            IReadOnlyList<int> frames = ProjectileTrailPlacements.OverrunFrames(list, layout);
            byte[] bytes = Decode(text, frames.Count * SamusOverreadDefinitions.TrailOverrunEntryByteCount, $"{list} trail overruns");
            for (int index = 0; index < frames.Count; index++)
                result.Add(new(list, frames[index]), DecodeTrailOverrun(bytes.AsSpan(
                    index * SamusOverreadDefinitions.TrailOverrunEntryByteCount, SamusOverreadDefinitions.TrailOverrunEntryByteCount), list));
        }
        return result.ToFrozenDictionary();
    }

    /// <summary>Decodes every program's list position, requiring one for each program.</summary>
    private static SamusProjectileListLayout DecodeProjectileLists(IReadOnlyDictionary<string, SamusProjectileListPosition>? lists)
    {
        if (lists is null)
            throw new InvalidDataException("Samus overread file has no projectile lists.");
        SamusProjectileProgram[] known = Enum.GetValues<SamusProjectileProgram>();
        if (lists.Count != known.Length)
            throw new InvalidDataException($"Samus overread file holds {lists.Count} projectile lists, not {known.Length}.");
        return new(known.ToDictionary(program => program, program => lists.TryGetValue(program.ToString(), out SamusProjectileListPosition position)
            ? position
            : throw new InvalidDataException($"Samus overread file has no projectile list for {program}.")));
    }

    /// <summary>Decodes one entry: its kind, then for a placement the left and right sparkles' signed X and Y offsets.</summary>
    private static ProjectileTrailPlacement? DecodeTrailOverrun(ReadOnlySpan<byte> entry, ProjectileTrailPlacementList list) => (TrailOverrunEntryKind)entry[0] switch
    {
        TrailOverrunEntryKind.Scratch => null,
        TrailOverrunEntryKind.Placement => new(
            new(unchecked((sbyte)entry[1]), unchecked((sbyte)entry[2])), new(unchecked((sbyte)entry[3]), unchecked((sbyte)entry[4]))),
        var kind => throw new InvalidDataException($"Samus overread {list} trail overrun has unknown kind {kind}."),
    };

    /// <summary>Reads one byte of a range, rejecting an offset outside it.</summary>
    private static byte Read(byte[] range, int offset, string name) =>
        (uint)offset < (uint)range.Length
            ? range[offset]
            : throw new ArgumentOutOfRangeException(nameof(offset), offset, $"The {name} range holds {range.Length} bytes.");

    /// <summary>Reads one little-endian word of a range, rejecting an index outside it.</summary>
    private static ushort ReadWord(byte[] range, int index, string name) =>
        (uint)index < (uint)(range.Length / sizeof(ushort))
            ? SnesLittleEndian.Word(range[index * sizeof(ushort)], range[index * sizeof(ushort) + 1])
            : throw new ArgumentOutOfRangeException(nameof(index), index, $"The {name} range holds {range.Length / sizeof(ushort)} words.");
}
