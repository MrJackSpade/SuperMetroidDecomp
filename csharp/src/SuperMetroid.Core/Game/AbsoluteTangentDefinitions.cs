namespace SuperMetroid.Core.Game;

/// <summary>Exact native 8.8 absolute ray gradients, including the horizontal infinity substitute.</summary>
public static class AbsoluteTangentDefinitions
{
    /// <summary>$91:C9D4 AbsoluteTangentTable: indices 0..128, reflected around 64.
    /// The regular samples truncate 256*tan(n*3.14159/128), where n=min(index,128-index).
    /// The exact diagonal is 256 and the singular endpoint uses the native finite
    /// infinity substitute 15360. The final index 128 is zero.</summary>
    /// <remarks>Independently matches all 129 NTSC J/U v1.0 words and pinned bank_91.asm.
    /// Reduced pi explains the near-singular 10427 samples; true pi yields 10428.
    /// This establishes the output convention without asserting authoring-tool provenance.
    /// Thirteen decimal Taylor terms on [0,pi/2) avoid platform-dependent libm results.</remarks>
    public static ushort Sample(int index)
    {
        if ((uint)index > 128) throw new ArgumentOutOfRangeException(nameof(index));
        int n = Math.Min(index, 128 - index);
        if (n == 0) return 0;
        if (n == 32) return 256;
        if (n == 64) return 15360;
        decimal x = n * 3.14159m / 128;
        decimal squared = x * x;
        decimal sineTerm = x, sine = x;
        decimal cosineTerm = 1, cosine = 1;
        for (int k = 1; k < 13; k++)
        {
            sineTerm = -sineTerm * squared / ((2 * k) * (2 * k + 1));
            cosineTerm = -cosineTerm * squared / ((2 * k - 1) * (2 * k));
            sine += sineTerm;
            cosine += cosineTerm;
        }
        return (ushort)(256 * sine / cosine);
    }
}
