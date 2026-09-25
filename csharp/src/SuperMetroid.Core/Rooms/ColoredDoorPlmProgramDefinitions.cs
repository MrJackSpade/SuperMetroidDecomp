namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-authored bank-$84 instruction streams for yellow, green, and red
/// resident door caps. Each color owns four facing-specific closing and active
/// lists; their physical draw layouts and editable tile choices live elsewhere.
/// </summary>
internal static class ColoredDoorPlmProgramDefinitions
{
    /// <summary>First yellow closing list at $84:BFFD.</summary>
    internal const ushort YellowStart = 0xbffd;
    /// <summary>Last yellow instruction byte at $84:C184.</summary>
    internal const ushort YellowEnd = 0xc184;
    /// <summary>First green closing list at $84:C185.</summary>
    internal const ushort GreenStart = 0xc185;
    /// <summary>Last green instruction byte at $84:C300.</summary>
    internal const ushort GreenEnd = 0xc300;
    /// <summary>First red closing list at $84:C301.</summary>
    internal const ushort RedStart = 0xc301;
    /// <summary>Last red instruction byte at $84:C488, before blue doors.</summary>
    internal const ushort RedEnd = 0xc488;

    private static readonly byte[] Yellow = Convert.FromHexString(
        "020077A602008BA7198C0802007FA7020073A7010067A7728AB1C4248A2AC0C1" +
        "8626BD010067A7B486248724C0918A014BC00300B3A9040067A70300B3A90400" +
        "67A70300B3A9040067A7248724C0198C07040073A704007FA704008BA75C0077" +
        "A6BC86020083A60200BBA7198C080200AFA70200A3A7010097A7728AE2C4248A" +
        "8DC0C18626BD010097A7B486248787C0918A01AEC00300EFA9040097A70300EF" +
        "A9040097A70300EFA9040097A7248787C0198C070600A3A70600AFA70600BBA7" +
        "010083A6BC8602008FA60200EBA7198C080200DFA70200D3A70100C7A7728A13" +
        "C5248AECC0C18626BD0100C7A7B486918A010DC103002BAA0400C7A703002BAA" +
        "0400C7A703002BAA0400C7A72487EAC0198C070600D3A70600DFA70600EBA701" +
        "008FA6BC8602009BA602001BA8198C0802000FA8020003A80100F7A7728A44C5" +
        "248A4FC1C18626BD0200F7A70100F7A7B486918A0170C1030067AA0400F7A703" +
        "0067AA0400F7A7030067AA0400F7A724874DC1198C07060003A806000FA80600" +
        "1BA801009BA6BC86");
    private static readonly byte[] Green = Convert.FromHexString(
        "020077A602004BA8198C0802003FA8020033A8010027A8728AB1C4248AAEC1C1" +
        "8688BD010027A8B486918A01CFC10300B3A9040027A80300B3A9040027A80300" +
        "B3A9040027A82487ACC1198C07060033A806003FA806004BA8010077A6BC8602" +
        "0083A602007BA8198C0802006FA8020063A8010057A8728AE2C4248A0DC2C186" +
        "88BD010057A8B486918A012EC20300EFA9040057A80300EFA9040057A80300EF" +
        "A9040057A824870BC2198C07060063A806006FA806007BA8010083A6BC860200" +
        "8FA60200ABA8198C0802009FA8020093A8010087A8728A13C5248A6CC2C18688" +
        "BD010087A8B486918A018DC203002BAA040087A803002BAA040087A803002BAA" +
        "040087A824876AC2198C07060093A806009FA80600ABA801008FA6BC8602009B" +
        "A60200DBA8198C080200CFA80200C3A80100B7A8728A44C5248ACBC2C18688BD" +
        "0100B7A8B486918A01ECC2030067AA0400B7A8030067AA0400B7A8030067AA04" +
        "00B7A82487C9C2198C070600C3A80600CFA80600DBA801009BA6BC86");
    private static readonly byte[] Red = Convert.FromHexString(
        "020077A602000BA9198C080200FFA80200F3A80100E7A8728AB1C4248A2AC3C1" +
        "8650BD0100E7A8B486918A054EC3198C090300B3A90400E7A80300B3A90400E7" +
        "A80300B3A90400E7A8248728C3198C070600F3A80600FFA806000BA9010077A6" +
        "BC86020083A602003BA9198C0802002FA9020023A9010017A9728AE2C4248A8C" +
        "C3C18650BD010017A9B486918A05B0C3198C090300EFA9040017A90300EFA904" +
        "0017A90300EFA9040017A924878AC3198C07060023A906002FA906003BA90100" +
        "83A6BC8602008FA602006BA9198C0802005FA9020053A9010047A9728A13C524" +
        "8AEEC3C18650BD010047A9B486918A0512C4198C0903002BAA040047A903002B" +
        "AA040047A903002BAA040047A92487ECC3198C07060053A906005FA906006BA9" +
        "01008FA6BC8602009BA602009BA9198C0802008FA9020083A9010077A9728A44" +
        "C5248A50C4C18650BD010077A9B486918A0574C4198C09030067AA040077A903" +
        "0067AA040077A9030067AA040077A924874EC4198C07060083A906008FA90600" +
        "9BA901009BA6BC86");

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset) && offset + 1 < bytes.Length)
        {
            value = (ushort)(bytes[offset] | bytes[offset + 1] << 8);
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (TryGetBytes(address, out byte[] bytes, out int offset))
        {
            value = bytes[offset];
            return true;
        }
        value = 0;
        return false;
    }

    private static bool TryGetBytes(ushort address, out byte[] bytes, out int offset)
    {
        if (address >= YellowStart && address <= YellowEnd)
        {
            bytes = Yellow;
            offset = address - YellowStart;
            return true;
        }
        if (address >= GreenStart && address <= GreenEnd)
        {
            bytes = Green;
            offset = address - GreenStart;
            return true;
        }
        if (address >= RedStart && address <= RedEnd)
        {
            bytes = Red;
            offset = address - RedStart;
            return true;
        }
        bytes = Array.Empty<byte>();
        offset = 0;
        return false;
    }
}
