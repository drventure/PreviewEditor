﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2020
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;

namespace WpfHexaEditor.Core
{
    /// <summary>
    /// ByteAction used for ByteModified class
    /// </summary>
    public enum ByteAction
    {
        Nothing,
        Added,
        Deleted,
        Modified,

        /// <summary>
        /// Used in ByteProvider for get list
        /// </summary>
        All
    }

    /// <summary>
    /// Used for coloring mode of selection
    /// </summary>
    public enum FirstColor
    {
        HexByteData,
        StringByteData
    }

    /// <summary>
    /// Mode of Copy/Paste
    /// </summary>
    public enum CopyPasteMode
    {
        Byte,
        HexaString,
        AsciiString,
        TblString,
        CSharpCode,
        VbNetCode,
        JavaCode,
        CCode,
        FSharpCode,
        PascalCode
    }

    /// <summary>
    /// Used with Copy to code fonction for language are similar to C.
    /// </summary>
    internal enum CodeLanguage
    {
        C,
        CSharp,
        Java,
        FSharp,
        Vbnet,
        Pascal
    }

    /// <summary>
    /// Used for check label are selected et next label to select...
    /// </summary>
    public enum KeyDownLabel
    {
        FirstChar,
        SecondChar,
        ThirdChar,
        FourthChar,
        FifthChar,
        SixthChar,
        SeventhChar,
        EighthChar,
        Ninth,
        Tenth,
        Eleventh,
        Twelfth,
        Thirteenth,
        Fourteenth,
        Fifteenth,
        Sixteenth,
        Seventeenth,
        Eighteenth,
        Ninteenth,
        Twentieth,
        TwentyFirst,
        TwentySecond,
        TwentyThird,
        TwentyFourth,
        TwentyFifth,
        TwentySixth,
        TwentySeventh,
        TwentyEighth,
        TwentyNinth,
        Thirtieth,
        ThirtyFirst,
        ThirtySecond,
        NextPosition
    }

    public enum ByteToString
    {
        /// <summary>
        /// Build-in convertion mode. (recommended)
        /// </summary>
        ByteToCharProcess,

        /// <summary>
        /// System.Text.Encoding.ASCII string encoder
        /// </summary>
        AsciiEncoding
    }

    /// <summary>
    /// Scrollbar marker
    /// </summary>
    public enum ScrollMarker
    {
        Nothing,
        SearchHighLight,
        Bookmark,
        SelectionStart,
        ByteModified,
        ByteDeleted,
        TblBookmark
    }

    /// <summary>
    /// Type are opened in byteprovider
    /// </summary>
    public enum ByteProviderStreamType
    {
        File,
        MemoryStream,
        Nothing
    }

    /// <summary>
    /// Type of character are used
    /// </summary>
    public enum CharacterTableType
    {
        Ascii,
        TblFile
    }

    /// <summary>
    /// Used for control the speed of mouse wheel
    /// </summary>
    public enum MouseWheelSpeed
    {
        VerySlow = 1,
        Slow = 3,
        Normal = 5,
        Fast = 7,
        VeryFast = 9,
        System
    }

    /// <summary>
    /// IByteControl spacer width
    /// </summary>
    public enum ByteSpacerWidth
    {
        VerySmall = 1,
        Small = 3,
        Normal = 6,
        Large = 9,
        VeryLarge = 12
    }

    [Flags]
    public enum ByteSpacerGroup
    {
        TwoByte = 2,
        FourByte = 4,
        SixByte = 6,
        EightByte = 8
    }

    public enum ByteSpacerPosition
    {
        HexBytePanel,
        StringBytePanel,
        Both,
        Nothing
    }

    public enum ByteSpacerVisual
    {
        Empty,
        Line,
        Dash
    }

    /// <summary>
    /// Used with the view mode of HexByte, header or position.
    /// </summary>
    public enum DataVisualType
    {
        Hexadecimal,    //Editable
        Decimal,        //Editable
        Binary        //Editable
    }

    /// <summary>
    /// Used with the view mode of HexByte, header or position.
    /// </summary>
    public enum DataVisualState
    {
        Default,
        Origin,
        Changes,
        ChangesPercent
    }

    public enum ByteSizeType
    {
        Bit8,       // editable   
        Bit16,      // editable
        Bit32       // editable
    }

    public enum ByteOrderType
    {
        LoHi,
        HiLo //not editable
    }

    /// <summary>
    /// Used to select the visual of the offset panel
    /// </summary>
    public enum OffSetPanelType
    {
        OffsetOnly,
        LineOnly,
        Both
    }

    /// <summary>
    /// Used to fix the wigth of the offset panel
    /// </summary>
    public enum OffSetPanelFixedWidth
    {
        Dynamic,
        Fixed
    }

    /// <summary>
    /// Used to set the the caret mode
    /// </summary>
    public enum CaretMode
    {
        Insert,
        Overwrite
    }

    /// <summary>
    /// Used to set how many line will be preloaded at control creation
    /// </summary>
    public enum PreloadByteInEditor
    {
        /// <summary>
        /// Load nothing at control start
        /// </summary>
        None,

        /// <summary>
        /// Load maximum of visible line in control view
        /// </summary>
        MaxVisibleLine,

        /// <summary>
        /// Add 10 lines to MaxVisible line
        /// </summary>
        MaxVisibleLineExtended,

        /// <summary>
        /// Load the maximum of line to fit to the screen
        /// </summary>
        MaxScreenVisibleLine,

        /// <summary>
        /// Load MaxScreenVisibleLine at control creation and the others lines will be loaded at first load of file/stream 
        /// </summary>
        MaxScreenVisibleLineAtDataLoad
    }
}