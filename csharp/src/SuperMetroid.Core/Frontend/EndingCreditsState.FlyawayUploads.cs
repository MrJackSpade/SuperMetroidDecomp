using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private byte[] flyawayCharacters = [];
    private byte[] flyawayMap = [];

    /// <summary>
    /// The ending's first flyaway reuses the Ceres approach's cartridge source.
    /// Rebind only chunks already transferred by the native 16-NMI DMA sequence;
    /// future chunks must still arrive on their original scheduled frames.
    /// </summary>
    internal void BindFlightArtwork(CeresFlightArtworkCatalog? value)
    {
        flightArtwork = value;
        if (value is null || Phase < EndingCreditsPhase.ZebesExplosionTileUpload ||
            Phase >= EndingCreditsPhase.FadeOutToCredits)
            return;
        PrepareFlyawayStreams();
        int completedChunks = Phase == EndingCreditsPhase.ZebesExplosionTileUpload
            ? Math.Clamp(16 - phaseTimer, 0, 16) : 16;
        for (int index = 0; index < completedChunks; index++)
            UploadFlyawayChunk(index);
    }

    private void PrepareFlyawayUploads()
    {
        PrepareFlyawayStreams();
        // Func117 restores the BG half before disabling BG1. OBJ VRAM and palettes
        // remain live while Func118 replaces the two Mode-7 lanes over sixteen NMIs.
        LoadStaticPalette(EndingPaletteId.Explosion, 0, 128, 0);
    }

    private void PrepareFlyawayStreams()
    {
        flyawayCharacters = flightArtwork is null
            ? RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.FlyawayCharacters,
                EndingCreditsRomData.Rendering.DecompressionLimit)
            : flightArtwork.Mode7Characters.ToArray();
        byte[] decodedMap = flightArtwork is null
            ? RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.FlyawayMap,
                EndingCreditsRomData.Rendering.DecompressionLimit)
            : flightArtwork.Mode7Maps.ToArray();
        RequireMinimum(flyawayCharacters, EndingCreditsRomData.Rendering.Mode7Bytes, "flyaway characters");
        RequireMinimum(decodedMap, EndingCreditsRomData.Rendering.FlyawayMapDataBytes, "flyaway map");
        flyawayMap = new byte[EndingCreditsRomData.Rendering.Mode7Bytes];
        decodedMap.AsSpan(0, EndingCreditsRomData.Rendering.FlyawayMapDataBytes).CopyTo(flyawayMap);
        flyawayMap.AsSpan(EndingCreditsRomData.Rendering.FlyawayMapDataBytes,
            EndingCreditsRomData.Rendering.Mode7Bytes - EndingCreditsRomData.Rendering.FlyawayMapDataBytes)
            .Fill(EndingCreditsRomData.Rendering.FlyawayBlankTile);
    }

    private void UploadFlyawayChunk(int index)
    {
        int chunksPerLane = EndingCreditsRomData.Rendering.Mode7Bytes / EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        int offset = index % chunksPerLane * EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        if (index < chunksPerLane)
            vram.LoadMode7CharacterBytes(flyawayCharacters.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), (ushort)offset);
        else
            vram.LoadMode7MapBytes(flyawayMap.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), (ushort)offset);
    }

    private bool EscapeBackgroundEnabled => Phase < EndingCreditsPhase.ZebesExplosionTileUpload
        || Phase >= EndingCreditsPhase.PlanetEscapeFast;
}
