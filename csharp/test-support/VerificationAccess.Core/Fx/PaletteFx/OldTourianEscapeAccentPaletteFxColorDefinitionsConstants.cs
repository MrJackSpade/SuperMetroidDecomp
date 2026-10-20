
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="OldTourianEscapeAccentPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeAccentPaletteFxColorDefinitionsConstants
{
    /// <summary>Both complete loops last 64 frames.</summary>
    public const int CycleFrames = 64;

    /// <summary>Provides the Old Tourian escape accent palette cycle period.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.CycleFrames;
    }
    /// <summary>$8D:FBC5-FC59/FC63-FCF7: exact neutral holds16/2/32 and extra-tick phases3/none/1 are authored animation choreography. Identical colors receive different holds; regenerating these choices would invent a different rhythm.</summary>
    internal static readonly FlickerPass[] AuthoredCadence = [new(16, 3), new(2, null), new(32, 1)];
    /// <summary>Each pass contains the neutral palette followed by four tint stages.</summary>
    internal const int TintPhases = 5;
    /// <summary>Applies each five-stage pass's authored neutral hold and extra-tick tint phase.</summary>
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameCount) throw new ArgumentOutOfRangeException(nameof(frame));
        FlickerPass pass = AuthoredCadence[frame / TintPhases];
        int tint = frame % TintPhases;
        return tint == 0 ? pass.NeutralHold : (ushort)(tint == pass.ExtendedTintPhase ? 2 : 1);
    }

    /// <summary>One authored flicker pass: neutral hold and optional tint phase held for an extra native tick.</summary>
    internal readonly record struct FlickerPass(ushort NeutralHold, int? ExtendedTintPhase);
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 10;
    /// <summary>Both loops contain fifteen timed records.</summary>
    public static int FrameCount => OldTourianEscapeAccentPaletteFxColorDefinitions.All[0].Rows.FrameCount;
}
