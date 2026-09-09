namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Literal WRAM window/screen-selection cache. This models the native byte layout,
/// including the gameplay_TM byte between TM and TS: a word write must not treat
/// those screen registers as adjacent. It does not infer CPU Y or effect settings.
/// </summary>
public sealed class GameplayWindowRegisterCache
{
    private readonly byte[] _bytes = new byte[
        GameplayWindowRegisterAddresses.SubscreenWindow - GameplayWindowRegisterAddresses.Window12Selection + 1];

    /// <summary>Owned values uploaded by the last accepted NMI, unaffected by later writes.</summary>
    public GameplayWindowRegisterSnapshot Displayed { get; private set; }

    public byte ReadByte(ushort address) => _bytes[Index(address)];

    public void WriteByte(ushort address, byte value) => _bytes[Index(address)] = value;

    /// <summary>
    /// Writes both bytes of a native little-endian store. Validate the whole range
    /// before mutation: callers outside this modeled WRAM region need another owner.
    /// </summary>
    public void WriteWord(ushort address, ushort value)
    {
        int index = Index(address);
        if (index == _bytes.Length - 1)
            throw new ArgumentOutOfRangeException(nameof(address), "Word store crosses the modeled window cache.");
        _bytes[index] = (byte)value;
        _bytes[index + 1] = (byte)(value >> 8);
    }

    /// <summary>
    /// The window/screen subset of InitializeLayerBlending ($88:8075). Window
    /// edges and logic survive this initialization; color math has a separate owner.
    /// This is not a replacement for the full layer-blending dispatcher.
    /// </summary>
    public void InitializeWindowAndScreenSelection()
    {
        WriteByte(GameplayWindowRegisterAddresses.Window12Selection, 0);
        WriteByte(GameplayWindowRegisterAddresses.Window34Selection, 0);
        WriteByte(GameplayWindowRegisterAddresses.ObjectColorSelection, 0);
        WriteByte(GameplayWindowRegisterAddresses.MainScreen,
            (byte)(SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj));
        WriteByte(GameplayWindowRegisterAddresses.Subscreen, (byte)SnesMainScreenLayers.Bg3);
        WriteByte(GameplayWindowRegisterAddresses.MainScreenWindow, 0);
        WriteByte(GameplayWindowRegisterAddresses.SubscreenWindow, 0);
    }

    /// <summary>
    /// Accepted branch of $80:9583 uploads $80:91EE's cached values. Lag NMIs
    /// neither upload them nor refresh gameplay_TM. No main-loop reset happens here.
    /// </summary>
    public void LatchNmi(bool mainLoopRequestedNmi)
    {
        if (!mainLoopRequestedNmi) return;
        WriteByte(GameplayWindowRegisterAddresses.GameplayMainScreen,
            ReadByte(GameplayWindowRegisterAddresses.MainScreen));
        Displayed = new(new(
            ReadByte(GameplayWindowRegisterAddresses.Window12Selection),
            ReadByte(GameplayWindowRegisterAddresses.Window34Selection),
            ReadByte(GameplayWindowRegisterAddresses.ObjectColorSelection),
            ReadByte(GameplayWindowRegisterAddresses.FirstLeft),
            ReadByte(GameplayWindowRegisterAddresses.FirstRight),
            ReadByte(GameplayWindowRegisterAddresses.SecondLeft),
            ReadByte(GameplayWindowRegisterAddresses.SecondRight),
            ReadByte(GameplayWindowRegisterAddresses.BackgroundLogic),
            ReadByte(GameplayWindowRegisterAddresses.ObjectColorLogic)),
            ReadByte(GameplayWindowRegisterAddresses.MainScreen),
            ReadByte(GameplayWindowRegisterAddresses.Subscreen),
            ReadByte(GameplayWindowRegisterAddresses.MainScreenWindow),
            ReadByte(GameplayWindowRegisterAddresses.SubscreenWindow));
    }

    private int Index(ushort address)
    {
        int index = address - GameplayWindowRegisterAddresses.Window12Selection;
        if ((uint)index >= (uint)_bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(address), "Address is outside the modeled window cache.");
        return index;
    }
}

/// <summary>
/// Literal uploaded bytes, including unused high bits. Layer renderers may mask
/// those bits, but a later native read or overwrite must retain the raw values.
/// </summary>
public readonly record struct GameplayWindowRegisterSnapshot(
    SnesWindowRegisters Windows, byte MainScreen, byte Subscreen,
    byte MainScreenWindow, byte SubscreenWindow);
