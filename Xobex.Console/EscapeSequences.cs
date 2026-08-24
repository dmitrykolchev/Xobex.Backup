using System;
using System.Collections.Generic;
using System.Text;

namespace Xobex.Console;

internal class EscapeSequences
{
    private static Dictionary<string, KeyCode> _escapes = new Dictionary<string, KeyCode>()
    {
        // [@"\u001b\[(\d+)(?:;(\d+(?::\d+)*))?~"]
        //                  Mouse move              - "\u001b[<35;{col};{row}M"
        //          Alt     Mouse Move              - "\u001b[<43;{col};{row}M"
        // Ctrl             Mouse Move              - "\u001b[<51;{col};{row}M"
        // Ctrl     Alt     Mouse Move              - "\u001b[<59;{col};{row}M"
        //                  Mouse move+LButton      - "\u001b[<32;{col};{row}M"
        //          Alt     Mouse Move+LButton      - "\u001b[<40;{col};{row}M"
        // Ctrl             Mouse Move+LButton      - "\u001b[<48;{col};{row}M"
        // Ctrl     Alt     Mouse Move+LButton      - "\u001b[<56;{col};{row}M"
        //                  Mouse move+MButton      - "\u001b[<33;{col};{row}M"
        //          Alt     Mouse Move+MButton      - "\u001b[<41;{col};{row}M"
        // Ctrl             Mouse Move+MButton      - "\u001b[<49;{col};{row}M"
        // Ctrl     Alt     Mouse Move+MButton      - "\u001b[<57;{col};{row}M"
        //                  Mouse move+RButton      - "\u001b[<34;{col};{row}M"
        //          Alt     Mouse Move+RButton      - "\u001b[<42;{col};{row}M"
        // Ctrl             Mouse Move+RButton      - "\u001b[<50;{col};{row}M"
        // Ctrl     Alt     Mouse Move+RButton      - "\u001b[<58;{col};{row}M"

        //                  Mouse Vert Wheel Up     - "\u001b[<64;{col};{row}M"
        //          Alt     Mouse Vert Wheel Up     - "\u001b[<72;{col};{row}M"
        // Ctrl             Mouse Vert Wheel Up     - "\u001b[<80;{col};{row}M"
        // Ctrl     Alt     Mouse Vert Wheel Up     - "\u001b[<88;{col};{row}M"

        //                  Mouse Vert Wheel Down   - "\u001b[<65;{col};{row}M"
        //          Alt     Mouse Vert Wheel Down   - "\u001b[<73;{col};{row}M"
        // Ctrl             Mouse Vert Wheel Down   - "\u001b[<81;{col};{row}M"
        // Ctrl     Alt     Mouse Vert Wheel Down   - "\u001b[<89;{col};{row}M"

        //                  Mouse Vert Wheel Left   - "\u001b[<67;{col};{row}M"
        //          Alt     Mouse Vert Wheel Left   - "\u001b[<75;{col};{row}M"
        // Ctrl             Mouse Vert Wheel Left   - "\u001b[<83;{col};{row}M"
        // Ctrl     Alt     Mouse Vert Wheel Left   - "\u001b[<91;{col};{row}M"

        //                  Mouse Vert Wheel Right  - "\u001b[<66;{col};{row}M"
        //          Alt     Mouse Vert Wheel Right  - "\u001b[<74;{col};{row}M"
        // Ctrl             Mouse Vert Wheel Right  - "\u001b[<82;{col};{row}M"
        // Ctrl     Alt     Mouse Vert Wheel Right  - "\u001b[<90;{col};{row}M"

        //                  Mouse L Down            - "\u001b[<0;{col};{row}M"
        //                  Mouse L Up              - "\u001b[<0;{col};{row}m"
        // Ctrl             Mouse L Down            - "\u001b[<16;{col};{row}M"
        // Ctrl             Mouse L Up              - "\u001b[<16;{col};{row}m"
        //          Alt     Mouse L Down            - "\u001b[<8;{col};{row}M"
        //          Alt     Mouse L Up              - "\u001b[<8;{col};{row}m"
        // Ctrl     Alt     Mouse L Down            - "\u001b[<24;{col};{row}M"
        // Ctrl     Alt     Mouse L Up              - "\u001b[<24;{col};{row}m"
        //                  Mouse M Down            - "\u001b[<1;{col};{row}M"
        //                  Mouse M Up              - "\u001b[<1;{col};{row}m"
        // Ctrl             Mouse M Down            - "\u001b[<17;{col};{row}M"
        // Ctrl             Mouse M Up              - "\u001b[<17;{col};{row}m"
        //          Alt     Mouse M Down            - "\u001b[<9;{col};{row}M"
        //          Alt     Mouse M Up              - "\u001b[<9;{col};{row}m"
        // Ctrl     Alt     Mouse M Down            - "\u001b[<25;{col};{row}M"
        // Ctrl     Alt     Mouse M Up              - "\u001b[<25;{col};{row}m"
        //                  Mouse R Down            - "\u001b[<2;{col};{row}M"
        //                  Mouse R Up              - "\u001b[<2;{col};{row}m"
        // Ctrl             Mouse R Down            - "\u001b[<18;{col};{row}M"
        // Ctrl             Mouse R Up              - "\u001b[<18;{col};{row}m"
        //          Alt     Mouse R Down            - "\u001b[<10;{col};{row}M"
        //          Alt     Mouse R Up              - "\u001b[<10;{col};{row}m"
        // Ctrl     Alt     Mouse R Down            - "\u001b[<26;{col};{row}M"
        // Ctrl     Alt     Mouse R Up              - "\u001b[<26;{col};{row}m"

    };
}


public enum KeyCode
{

}