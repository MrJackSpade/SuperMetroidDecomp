using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private byte[] flyawayCharacters = [];
    private byte[] flyawayMap = [];

    private void PrepareFlyawayUploads()
    {
        flyawayCharacters = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.FlyawayCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] decodedMap = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.FlyawayMap,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        RequireMinimum(flyawayCharacters, EndingCreditsRomData.Rendering.Mode7Bytes, "flyaway characters");
        RequireMinimum(decodedMap, EndingCreditsRomData.Rendering.FlyawayMapDataBytes, "flyaway map");
        flyawayMap = new byte[EndingCreditsRomData.Rendering.Mode7Bytes];
        decodedMap.AsSpan(0, EndingCreditsRomData.Rendering.FlyawayMapDataBytes).CopyTo(flyawayMap);
        flyawayMap.AsSpan(EndingCreditsRomData.Rendering.FlyawayMapDataBytes,
            EndingCreditsRomData.Rendering.Mode7Bytes - EndingCreditsRomData.Rendering.FlyawayMapDataBytes)
            .Fill(EndingCreditsRomData.Rendering.FlyawayBlankTile);
        // Func117 restores the BG half before disabling BG1. OBJ VRAM and palettes
        // remain live while Func118 replaces the two Mode-7 lanes over sixteen NMIs.
        cgram.LoadFromBus(bus, EndingCreditsRomData.Assets.ExplosionPalette, 128);
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
