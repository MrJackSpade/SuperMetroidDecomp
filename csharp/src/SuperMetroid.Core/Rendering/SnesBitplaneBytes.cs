using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Rendering;

/// <summary>One character row's bitplane bytes, indexed by bitplane: bitplane N supplies bit N of each pixel's colour.</summary>
[InlineArray(4)]
internal struct SnesBitplaneBytes
{
    /// <summary>Bitplane 0's byte; the higher bitplanes' follow it.</summary>
    private int first;
}
