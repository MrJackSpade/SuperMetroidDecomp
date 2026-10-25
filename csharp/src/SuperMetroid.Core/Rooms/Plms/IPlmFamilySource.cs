namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM family owner that registers its family's step.</summary>
internal interface IPlmFamilySource
{
    /// <summary>Registers the owner's family step in <paramref name="table"/>.</summary>
    void AddPlmFamilies(PlmFamilyTable table);
}
