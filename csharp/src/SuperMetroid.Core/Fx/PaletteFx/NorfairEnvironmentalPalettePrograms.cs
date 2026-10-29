using static SuperMetroid.Core.Fx.PaletteFx.NorfairEnvironmentalPaletteFxColorDefinitions;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Materializes the four native Norfair environmental programs (<see cref="NorfairEnvironmentalPaletteFxColorDefinitions"/>) from
/// their shared record geometry, in native object order without storing duplicate records.
/// </summary>
internal sealed class NorfairEnvironmentalPalettePrograms : IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition>
{
    /// <summary>The four adjacent definition identities at $8D:F785 through $F791.</summary>
    public int Count => 4;
    /// <summary>Builds one program's definition from its owner index.</summary>
    public NorfairEnvironmentalPaletteFxColorDefinition this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            NorfairEnvironmentalPaletteOwner owner = Owners[index];
            bool heat = owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase;
            return new()
            {
                Owner = owner,
                Object = Objects[index],
                PublishesHeatPhase = heat,
                Rows = PaletteFxColorRows.Of(NorfairEnvironmentalPaletteFxColorDefinitions.Programs[index], NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame),
            };
        }
    }
    /// <summary>Visits the heat-phase program followed by palette-four, five, and six programs.</summary>
    public IEnumerator<NorfairEnvironmentalPaletteFxColorDefinition> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same native-order sequence through the nongeneric collection contract.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
