using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console;
using ZazBoy.Database;

namespace ZazBoy.UI
{
    /// <summary>
    /// Common utilities for UI.
    /// </summary>
    public static class UIUtil
    {
        public static readonly Avalonia.Media.Color validTextBoxBorderColor = Avalonia.Media.Color.FromRgb(152, 152, 152);
        public static readonly Avalonia.Media.Color invalidTextBoxBorderColor = Avalonia.Media.Color.FromRgb(255, 0, 0);

        private static InstructionDatabase idb;

        /// <summary>
        /// Converts a System.Drawing Bitmap to a Avalonia.Media.Imaging Bitmap
        /// </summary>
        /// <param name="drawingBitmap">The System.Drawing Bitmap to convert.</param>
        /// <returns>The Avalonia.Media.Imaging Bitmap</returns>
        public static Avalonia.Media.Imaging.Bitmap ConvertDrawingBitmapToUIBitmap(Bitmap drawingBitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            drawingBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            Avalonia.Media.Imaging.Bitmap uiBitmap = new Avalonia.Media.Imaging.Bitmap(memoryStream);
            memoryStream.Close();
            return uiBitmap;
        }

        /// <summary>
        /// Gets the InstructionDatabase
        /// </summary>
        /// <returns>The instruction database</returns>
        public static InstructionDatabase GetInstructionDatabase()
        {
            if (idb == null)
                idb = JsonConvert.DeserializeObject<InstructionDatabase>(Properties.Resources.InstructionDatabase);
            return idb;
        }

        /// <summary>
        /// Gets the InstructionEntry for the instruction stored at the specified address.
        /// </summary>
        /// <param name="memoryAddress"></param>
        /// <returns></returns>
        public static InstructionEntry GetInstructionEntry(GameBoy gameBoy, ushort memoryAddress)
        {
            bool isPrefixed = false;
            byte opcode = gameBoy.MemoryMap.ReadDirect(memoryAddress);
            if (opcode == 0xCB)
            {
                isPrefixed = true;
                opcode = gameBoy.MemoryMap.ReadDirect((ushort)(memoryAddress + 1));
            }
            string opcodeHex = "0x" + opcode.ToString("X2");
            InstructionEntry instructionEntry = (isPrefixed) ? GetInstructionDatabase().cbprefixed[opcodeHex] : GetInstructionDatabase().unprefixed[opcodeHex];
            return instructionEntry;
        }

        /// <summary>
        /// Checks if the string denotes a valid ushort, ignoring formatting terms such as "0x" or "#"
        /// </summary>
        /// <param name="ushortString">The string to validate</param>
        /// <returns>bool on valid value</returns>
        public static bool IsHexUShortValid(string ushortString)
        {
            try
            {
                if (ushortString == null)
                    throw new ArgumentNullException("String is null");
                ushortString = ushortString.Replace("0x", "").Replace("#", "");
                int value = int.Parse(ushortString, System.Globalization.NumberStyles.HexNumber);
                return (value >= 0 && value <= ushort.MaxValue);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a ushort value from a string, ignoring formatting terms such as "0x" or "#"
        /// </summary>
        /// <param name="ushortString">The string to parse.</param>
        /// <returns>The ushort value</returns>
        public static ushort ParseHexUShort(string ushortString)
        {
            ushortString = ushortString.Replace("0x", "").Replace("#", "");
            ushort value = ushort.Parse(ushortString, NumberStyles.HexNumber);
            return value;
        }

        /// <summary>
        /// Checks if the string denotes a valid byte, ignoring formatting terms such as "0x" or "#"
        /// </summary>
        /// <param name="byteString">The string to validate</param>
        /// <returns>bool on valid value</returns>
        public static bool IsHexByteValid(string byteString)
        {
            try
            {
                if (byteString == null)
                    throw new ArgumentNullException("String is null");
                byteString = byteString.Replace("0x", "").Replace("#", "");
                int value = int.Parse(byteString, System.Globalization.NumberStyles.HexNumber);
                return (value >= 0 && value <= byte.MaxValue);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a byte value from a hexadecimal string, ignoring formatting terms such as "0x" or "#"
        /// </summary>
        /// <param name="byteString">The string to parse.</param>
        /// <returns>The byte value</returns>
        public static byte ParseHexByte(string byteString)
        {
            byteString = byteString.Replace("0x", "").Replace("#", "");
            byte value = byte.Parse(byteString, NumberStyles.HexNumber);
            return value;
        }
    }
}
