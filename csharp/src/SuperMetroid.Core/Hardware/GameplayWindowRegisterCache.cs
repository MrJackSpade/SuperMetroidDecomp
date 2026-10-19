namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Literal WRAM window/screen-selection cache. This models the native byte layout,
/// including the gameplay_TM byte between TM and TS: a word write must not treat
/// those screen registers as adjacent. It does not infer CPU Y or effect settings.
/// </summary>
[PpuLayoutBoundary("backend: gameplay window/screen-selection register cache")]
public sealed class GameplayWindowRegisterCache
{
    /// <summary>Raw WRAM shadow bytes from W12SEL through TSW; intervening gameplay_TM is retained.</summary>
    private readonly byte[] _bytes = new byte[
        GameplayWindowRegisterAddresses.SubscreenWindow - GameplayWindowRegisterAddresses.Window12Selection + 1];

    /// <summary>Owned values uploaded by the last accepted NMI, unaffected by later writes.</summary>
    public GameplayWindowRegisterSnapshot Displayed { get; private set; }

    /// <summary>Reads one raw byte from the modeled WRAM window/screen-register cache.</summary>
    /// <param name="address">Native WRAM offset from <c>$69</c> through <c>$74</c>.</param>
    /// <returns>The current main-loop-owned byte; this need not equal the last NMI-latched value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="address"/> lies outside the modeled cache.</exception>
    [PpuLayoutAccess]
    public byte ReadByte(ushort address) => _bytes[Index(address)];

    /// <summary>Writes one raw byte to the main-loop-owned WRAM cache without changing <see cref="Displayed"/>.</summary>
    /// <param name="address">Native WRAM offset from <c>$69</c> through <c>$74</c>.</param>
    /// <param name="value">Unmasked byte to preserve until an accepted NMI uploads it.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="address"/> lies outside the modeled cache.</exception>
    [PpuLayoutAccess]
    public void WriteByte(ushort address, byte value) => _bytes[Index(address)] = value;

    /// <summary>
    /// Writes both bytes of a native little-endian store. Validate the whole range
    /// before mutation: callers outside this modeled WRAM region need another owner.
    /// </summary>
    [PpuLayoutAccess]
    public void WriteWord(ushort address, ushort value)
    {
        int index = Index(address);
        if (index == _bytes.Length - 1)
            throw new ArgumentOutOfRangeException(nameof(address), "Word store crosses the modeled window cache.");
        _bytes[index] = (byte)value;
        _bytes[index + 1] = SnesLittleEndian.HighByte(value);
    }

    /// <summary>The main-loop-owned cached value of <paramref name="register"/>.</summary>
    [PpuLayoutAccess]
    public byte Read(PpuLayoutCell register) => ReadByte(GameplayWindowRegisterLayout.ShadowAddress(register));

    /// <summary>Writes <paramref name="register"/>'s cached value without changing <see cref="Displayed"/>.</summary>
    [PpuLayoutAccess]
    public void Write(PpuLayoutCell register, byte value) => WriteByte(GameplayWindowRegisterLayout.ShadowAddress(register), value);

    /// <summary>The cached main-screen designation (TM) the next accepted NMI uploads.</summary>
    public SnesMainScreenLayers MainScreen
    {
        get => (SnesMainScreenLayers)Read(GameplayWindowRegisterLayout.MainScreen);
        set => Write(GameplayWindowRegisterLayout.MainScreen, value.ToNativeByte());
    }

    /// <summary>The cached subscreen designation (TS).</summary>
    public SnesMainScreenLayers Subscreen
    {
        get => (SnesMainScreenLayers)Read(GameplayWindowRegisterLayout.Subscreen);
        set => Write(GameplayWindowRegisterLayout.Subscreen, value.ToNativeByte());
    }

    /// <summary>
    /// The misaligned <c>STA $60,X</c> word store of the Chainsaw callback at <c>$90:B0AC</c>:
    /// <paramref name="value"/> lands <paramref name="projectileByteIndex"/> bytes past W12SEL's
    /// shadow, overwriting whichever two cached registers sit there.
    /// </summary>
    public void StoreProjectileIndexedWord(int projectileByteIndex, ushort value) =>
        WriteWord(checked((ushort)(GameplayWindowRegisterLayout.ShadowAddress(GameplayWindowRegisterLayout.Window12Selection) +
            projectileByteIndex)), value);

    /// <summary>
    /// The window/screen subset of InitializeLayerBlending ($88:8075). Window
    /// edges and logic survive this initialization; color math has a separate owner.
    /// This is not a replacement for the full layer-blending dispatcher.
    /// </summary>
    public void InitializeWindowAndScreenSelection()
    {
        Write(GameplayWindowRegisterLayout.Window12Selection, 0);
        Write(GameplayWindowRegisterLayout.Window34Selection, 0);
        Write(GameplayWindowRegisterLayout.ObjectColorSelection, 0);
        MainScreen = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;
        Subscreen = SnesMainScreenLayers.Bg3;
        Write(GameplayWindowRegisterLayout.MainScreenWindow, 0);
        Write(GameplayWindowRegisterLayout.SubscreenWindow, 0);
    }

    /// <summary>
    /// The accepted NMI ($80:9583) uploads $80:91EE's cached values and refreshes gameplay_TM. No main-loop reset
    /// happens here.
    /// </summary>
    public void LatchNmi()
    {
        Write(GameplayWindowRegisterLayout.GameplayMainScreen, Read(GameplayWindowRegisterLayout.MainScreen));
        Displayed = new(new(
            (SnesWindowSelectionPair)Read(GameplayWindowRegisterLayout.Window12Selection),
            (SnesWindowSelectionPair)Read(GameplayWindowRegisterLayout.Window34Selection),
            (SnesWindowSelectionPair)Read(GameplayWindowRegisterLayout.ObjectColorSelection),
            Read(GameplayWindowRegisterLayout.FirstLeft),
            Read(GameplayWindowRegisterLayout.FirstRight),
            Read(GameplayWindowRegisterLayout.SecondLeft),
            Read(GameplayWindowRegisterLayout.SecondRight),
            Read(GameplayWindowRegisterLayout.BackgroundLogic),
            Read(GameplayWindowRegisterLayout.ObjectColorLogic)),
            (SnesMainScreenLayers)Read(GameplayWindowRegisterLayout.MainScreen),
            (SnesMainScreenLayers)Read(GameplayWindowRegisterLayout.MainScreenWindow));
    }

    /// <summary>Validates and converts a native shadow address to the cache's zero-based offset.</summary>
    private int Index(ushort address)
    {
        int index = address - GameplayWindowRegisterAddresses.Window12Selection;
        if ((uint)index >= (uint)_bytes.Length)
            throw new ArgumentOutOfRangeException(nameof(address), "Address is outside the modeled window cache.");
        return index;
    }
}
