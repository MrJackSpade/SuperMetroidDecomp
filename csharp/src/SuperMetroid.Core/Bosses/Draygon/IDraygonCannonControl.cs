
namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon cannon control (#633).</summary>
public interface IDraygonCannonControl
{
    /// <summary>Disables the Draygon cannon whose enemy-control word is at <paramref name="variablePointer"/>.</summary>
    void DisableDraygonCannon(DraygonCannonControlWord word);
}
