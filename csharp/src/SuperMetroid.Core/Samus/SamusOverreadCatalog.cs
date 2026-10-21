using SuperMetroid.Core.Assets;

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
    /// <summary>The bytes past the trail coordinate data that a trail can reach.</summary>
    private readonly byte[] _trailCoordinateAdjacentCode;

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
        _trailCoordinateAdjacentCode = Decode(document.TrailCoordinateAdjacentCode,
            SamusOverreadDefinitions.TrailCoordinateAdjacentByteCount, nameof(document.TrailCoordinateAdjacentCode));
    }

    /// <summary>The SpaceTime palette-copy overrun's source byte <paramref name="offset"/> bytes past the first one it reads.</summary>
    internal byte SpacetimeCopySourceByte(int offset) => Read(_spacetimeCopySource, offset, "SpaceTime copy source");

    /// <summary>Colour word <paramref name="index"/> the Speed Booster palette overrun reads from the grapple handler's code.</summary>
    internal ushort SpeedBoosterGrappleCodeWord(int index) => ReadWord(_speedBoosterGrappleCode, index, "Speed Booster grapple-code colour");

    /// <summary>The byte the HUD's posture-transition admission reads for a pose <paramref name="index"/> poses before the real flags start.</summary>
    internal byte HudPrecedingPostureByte(int index) => Read(_hudPrecedingPostureBytes, index, "HUD preceding posture byte");

    /// <summary>The byte the HUD's posture-transition admission reads for a pose <paramref name="index"/> poses after the real flags end.</summary>
    internal byte HudFollowingPostureByte(int index) => Read(_hudFollowingPostureBytes, index, "HUD following posture byte");

    /// <summary>The byte <paramref name="offset"/> bytes past the trail coordinate data.</summary>
    internal byte TrailCoordinateAdjacentByte(int offset) => Read(_trailCoordinateAdjacentCode, offset, "trail coordinate adjacent byte");

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
