using SuperMetroid.Core;
using System.Collections.Frozen;
using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// The bank-$A9 row upload lists of the dead monsters (#1406): the rows a dead Sidehopper, Zoomer, Ripper or Skree uploads
/// from its staged characters after each rot step. Only the extraction reads them, to find the characters the corpse's rows
/// show in, and so where each corpse lies in its pictures (#1539).
/// </summary>
[EncodingOwner("bank-$A9 dead-monster row upload lists")]
internal static partial class DeadMonsterRowUploadDefinitions
{
    /// <summary>Where the cartridge holds a list: the species whose lists hold it and its variant among them.</summary>
    /// <param name="Species">The species whose lists hold it.</param>
    /// <param name="Variant">Its place among the species' lists, the species' corpse variant that names it.</param>
    internal readonly partial record struct NativeList(RottingCorpseSpecies Species, int Variant);

    /// <summary>Each list's place among the cartridge's lists.</summary>
    private static readonly FrozenDictionary<CorpseRowUploadList, NativeList> NativeLists = new Dictionary<CorpseRowUploadList, NativeList>
    {
        [CorpseRowUploadList.SidehopperBabyMetroidVictim] = new(RottingCorpseSpecies.Sidehopper, 0),
        [CorpseRowUploadList.SidehopperAlreadyDead] = new(RottingCorpseSpecies.Sidehopper, 1),
        [CorpseRowUploadList.ZoomerOnItsBack] = new(RottingCorpseSpecies.Zoomer, 0),
        [CorpseRowUploadList.ZoomerUpright] = new(RottingCorpseSpecies.Zoomer, 1),
        [CorpseRowUploadList.SecondZoomerOnItsBack] = new(RottingCorpseSpecies.Zoomer, 2),
        [CorpseRowUploadList.RipperLevel] = new(RottingCorpseSpecies.Ripper, 0),
        [CorpseRowUploadList.RipperTilted] = new(RottingCorpseSpecies.Ripper, 1),
        [CorpseRowUploadList.Skree] = new(RottingCorpseSpecies.Skree, 0),
        [CorpseRowUploadList.SecondSkree] = new(RottingCorpseSpecies.Skree, 1),
        [CorpseRowUploadList.ThirdSkree] = new(RottingCorpseSpecies.Skree, 2),
    }.ToFrozenDictionary();

    /// <summary>Where the cartridge holds <paramref name="list"/>.</summary>
    internal static NativeList Native(CorpseRowUploadList list) => Ensure.Listed(NativeLists, list);

    /// <summary>The rows <paramref name="list"/> uploads, in native descriptor order.</summary>
    /// <exception cref="InvalidDataException">The cartridge has no such row upload list.</exception>
    internal static ImmutableArray<CorpseRowUpload> ForList(CorpseRowUploadList list)
    {
        NativeList native = Native(list);
        return native.Species switch
        {
            RottingCorpseSpecies.Sidehopper => SidehopperRows(native.Variant),
            RottingCorpseSpecies.Zoomer => ZoomerRows(native.Variant),
            RottingCorpseSpecies.Ripper => RipperRows(native.Variant),
            RottingCorpseSpecies.Skree => SkreeRows(native.Variant),
            RottingCorpseSpecies.Torizo => throw new InvalidDataException($"Corpse {list} has no row upload list."),
            _ => throw new InvalidOperationException($"Undefined {nameof(RottingCorpseSpecies)} {native.Species}."),
        };
    }

    /// <summary>The rows of Sidehopper corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<CorpseRowUpload> SidehopperRows(int variant) =>
        Rows(5, 0xa0, (ushort)(0x7000 + variant * 0x90),
            variant == 0 ? (ushort)0x60 : (ushort)0x40, variant == 0 ? (ushort)0x20 : (ushort)0);

    /// <summary>The rows of Zoomer corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<CorpseRowUpload> ZoomerRows(int variant) =>
        Rows(2, 0x60, (ushort)(0x7530 + variant * 0x30));

    /// <summary>The rows of Ripper corpse <paramref name="variant"/>.</summary>
    private static ImmutableArray<CorpseRowUpload> RipperRows(int variant) =>
        Rows(2, 0x60, (ushort)(0x7500 + variant * 0xc0));

    /// <summary>The rows of Skree corpse <paramref name="variant"/>: the first begins on the next VRAM tile row, later ones on row zero.</summary>
    private static ImmutableArray<CorpseRowUpload> SkreeRows(int variant) =>
        Rows(4, 0x40, (ushort)(variant == 0 ? 0x7150 : 0x7000 + variant * 0x70));

    /// <summary>
    /// Consecutive staged tile rows placed in 32-tile VRAM rows, in native descriptor order; sidehoppers clip their
    /// first row to <paramref name="firstSize"/> bytes and shift it by <paramref name="firstDestinationOffset"/>.
    /// </summary>
    private static ImmutableArray<CorpseRowUpload> Rows(int count, int stride, int destination,
        int firstSize = 0, int firstDestinationOffset = 0)
    {
        var rows = ImmutableArray.CreateBuilder<CorpseRowUpload>(count);
        for (int index = 0; index < count; index++)
        {
            rows.Add(new(index == 0 && firstSize != 0 ? firstSize : stride,
                (ushort)(destination + 0x100 * index + (index == 0 ? firstDestinationOffset : 0))));
        }
        return rows.MoveToImmutable();
    }
}
