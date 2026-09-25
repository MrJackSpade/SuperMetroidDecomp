namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction streams for both mirrored
/// three-component eye doors. Their executable callbacks, enemy-projectile
/// programs, physical draw layouts, and editable visuals are separate data.
/// </summary>
internal static class EyeDoorPlmProgramDefinitions
{
    /// <summary>First facing-left eye instruction at $84:D81E.</summary>
    internal const ushort FirstAddress = 0xd81e;
    /// <summary>Last facing-right bottom-component instruction at $84:DA8B.</summary>
    internal const ushort LastAddress = 0xda8b;

    private static readonly byte[] Program = Convert.FromHexString(
        "728AE3D80400039C418D060430D8248722D8248A80D8C18650BD08000B9C418D" +
        "010478D84000139C7AD700002000139C7AD700002000139C7AD700004000139C" +
        "06000B9C3000039C3000039C06000B9C418D06043CD8248722D80400039C2487" +
        "3CD8108C099FD79FD7918A03C4D802001B9C0200239C9FD702001B9C0200239C" +
        "02001B9C9FD70200239C04000B9C0800039C90D700003800039C04000B9C0400" +
        "239C24873CD8CA86B6D7B6D79FD79FD7DAD74E870A0300F79B0400A7A93F87D3" +
        "D82487B1C4DAD72487B1C4728A1DD9418D0610FBD808002B9C2487EDD8248A1D" +
        "D9C18653D708002B9C0800319C0800379C0800319C418D061003D92487EDD8BC" +
        "86728A53D9418D061031D908003D9C248723D9248A53D9C18653D708003D9C08" +
        "00439C0800499C0800439C418D061039D9248723D9BC86728A1ADA04005B9C41" +
        "8D060467D9248759D9248AB7D9C18650BD0800639C418D0104AFD940006B9C7A" +
        "D7140020006B9C7AD7140020006B9C7AD7140040006B9C0600639C30005B9C30" +
        "005B9C0600639C418D060473D9248759D904005B9C248773D9108C099FD79FD7" +
        "918A03FBD90200739C02007B9C9FD70200739C02007B9C0200739C9FD702007B" +
        "9C0400639C08005B9C90D7040038005B9C0400639C04007B9C248773D9CA86B6" +
        "D7B6D79FD79FD7C3D74E870A03004F9C0400E3A93F870ADA2487E2C4C3D72487" +
        "E2C4728A54DA418D061032DA0800839C248724DA248A54DAC18653D70600839C" +
        "0600899C06008F9C0600899C418D06103ADA248724DABC86728A8ADA418D0610" +
        "68DA0800959C24875ADA248A8ADAC18653D70600959C06009B9C0600A19C0600" +
        "9B9C418D061070DA24875ADABC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - FirstAddress;
        if ((uint)offset < Program.Length - 1)
        {
            value = (ushort)(Program[offset] | Program[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        int offset = address - FirstAddress;
        if ((uint)offset < Program.Length)
        {
            value = Program[offset];
            return true;
        }
        value = 0;
        return false;
    }
}
