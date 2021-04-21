using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
    }
}
