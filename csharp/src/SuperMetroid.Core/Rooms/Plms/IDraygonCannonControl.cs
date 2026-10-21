namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Draygon cannon control (#633).</summary>
public interface IDraygonCannonControl
{
    /// <summary>Disables the Draygon cannon whose enemy-control word is <paramref name="word"/>.</summary>
    void DisableDraygonCannon(DraygonCannonControlWord word);
}
