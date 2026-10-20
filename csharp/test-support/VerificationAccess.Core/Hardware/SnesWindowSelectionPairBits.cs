namespace SuperMetroid.Core.Hardware;

/// <summary>Named bits of <see cref="SnesWindowSelectionPair"/> that tests compose register bytes from.</summary>
internal static class SnesWindowSelectionPairBits
{
    /// <summary>Neither layer selects a window.</summary>
    internal const SnesWindowSelectionPair None = 0;
    /// <summary>The lower layer inverts the first window's interval.</summary>
    internal const SnesWindowSelectionPair LowerInvertFirst = (SnesWindowSelectionPair)0x01;
    /// <summary>The lower layer enables the first window.</summary>
    internal const SnesWindowSelectionPair LowerEnableFirst = (SnesWindowSelectionPair)0x02;
    /// <summary>The lower layer inverts the second window's interval.</summary>
    internal const SnesWindowSelectionPair LowerInvertSecond = (SnesWindowSelectionPair)0x04;
    /// <summary>The lower layer enables the second window.</summary>
    internal const SnesWindowSelectionPair LowerEnableSecond = (SnesWindowSelectionPair)0x08;
    /// <summary>The upper layer inverts the first window's interval.</summary>
    internal const SnesWindowSelectionPair UpperInvertFirst = (SnesWindowSelectionPair)0x10;
    /// <summary>The upper layer enables the first window.</summary>
    internal const SnesWindowSelectionPair UpperEnableFirst = (SnesWindowSelectionPair)0x20;
    /// <summary>The upper layer inverts the second window's interval.</summary>
    internal const SnesWindowSelectionPair UpperInvertSecond = (SnesWindowSelectionPair)0x40;
    /// <summary>The upper layer enables the second window.</summary>
    internal const SnesWindowSelectionPair UpperEnableSecond = (SnesWindowSelectionPair)0x80;
}
