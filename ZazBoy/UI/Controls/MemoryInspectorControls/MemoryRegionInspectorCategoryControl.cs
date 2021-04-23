using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZazBoy.Console;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class MemoryRegionInspectorCategoryControl : InspectorCategoryControl
    {
        private ushort startAddress;

        public void Initialise(GameBoy gameBoy, string categoryName, ushort startAddress, int length)
        {
            Initialise(gameBoy, categoryName, length);
            this.startAddress = startAddress;
        }

        protected override void ShowPage(int pageNo)
        {
            base.ShowPage(pageNo);
            ushort pageStartAddress = (ushort)(startAddress + (pageNo * ITEMS_PER_PAGE));
            int pageStartIndex = Math.Abs(pageStartAddress - startAddress);
            int itemsToDisplay = Math.Min(ITEMS_PER_PAGE, length - pageStartIndex);

            for (int i = 0; i < ITEMS_PER_PAGE; i++)
            {
                ushort address = (ushort)unchecked(pageStartAddress + i); //Allow it to overflow so that length can be achieved.

                MemoryBlockItem memoryItem = memoryItems[i];
                if (i < itemsToDisplay)
                {
                    memoryItem.IsVisible = true;
                    memoryItem.SetAddress(address);
                    memoryItem.SetData(gameBoy.MemoryMap.ReadDirect(address));
                }
                else
                    memoryItem.IsVisible = false;
            }
        }
    }
}
