namespace SuperMetroid.Core.Game;

/// <summary>Native endpoint animation at $9B:BFBD, distinct from the rope's angle selection.</summary>
internal static class GrapplePointAnimationDefinitions
{
    /// <summary>$9B:C342, GrappleBeamStartTilesBeginEndPointers_0: first endpoint tile in bank $9A.</summary>
    internal const ushort BeginPointer = 0x8200;
    /// <summary>$9B:C344, GrappleBeamStartTilesBeginEndPointers_1: exclusive endpoint animation limit.</summary>
    internal const ushort EndPointer = 0x8a00;
    /// <summary>$9B:BFD0 advances the tile source by $0200 bytes on timer expiry.</summary>
    internal const ushort FrameStride = 0x0200;
    /// <summary>$9B:BFC6/C645 NTSC delay: five, decremented through zero for a six-call interval.</summary>
    internal const ushort Delay = 5;
    internal const int FrameCount = (EndPointer - BeginPointer) / FrameStride;

    internal static int SourceAddress(byte frame) => SamusGrappleRomData.Banks.CharacterData |
        unchecked((ushort)(BeginPointer + frame * FrameStride));

    internal static void Advance(SamusGrappleState state)
    {
        state.PointAnimationTimer = unchecked((ushort)(state.PointAnimationTimer - 1));
        if ((short)state.PointAnimationTimer >= 0) return;
        state.PointAnimationTimer = Delay;
        ushort pointer = unchecked((ushort)(SourceAddress(state.PointAnimationFrame) + FrameStride));
        if (unchecked((short)(pointer - EndPointer)) >= 0) pointer = BeginPointer;
        // Keep the existing saved ordinal field, but do the native 16-bit pointer
        // arithmetic before converting back. This also retains wrapped pointer states.
        state.PointAnimationFrame = (byte)(unchecked((ushort)(pointer - BeginPointer)) / FrameStride);
    }
}
