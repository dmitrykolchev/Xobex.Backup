using System;
using System.Collections.Generic;
using System.Text;

namespace Xobex.Console;

[Flags]
public enum InputTokenType
{
    Separator = -1,
    NIL = 0x00,
    SOH = 0x01,
    STX = 0x02,
    ETX = 0x03,
    EOT = 0x04,
    ENQ = 0x05,
    ACK = 0x06,
    BEL = 0x07,
    BS = 0x08,
    HT = 0x09,
    LF = 0x0A,
    VT = 0x0B,
    FF = 0x0C,
    CR = 0x0D,
    SO = 0x0E,
    SI = 0x0F,
    DLE = 0x10,
    DC1 = 0x11,
    DC2 = 0x12,
    DC3 = 0x13,
    DC4 = 0x14,
    NAK = 0x15,
    SYN = 0x16,
    ETB = 0x17,
    CAN = 0x18,
    EM = 0x19,
    SUB = 0x1A,
    ESC = 0x1B,
    IS4 = 0x1C,
    IS3 = 0x1D,
    IS2 = 0x1E,
    IS1 = 0x1F,
    SP = 0x20,

    DEL = 0x7F,

    Char7Bit = 0x100,
    Char8Bit = 0x200,
    Digit = 0x101,
    UpperCase = 0x102,
    LowerCase = 0x103,
    Symbol = 0x104
}
