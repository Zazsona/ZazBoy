using ZazBoy.Console;
using static ZazBoy.Console.CPU;

namespace ZazBoy.UI.Controls.MemoryInspectorControls
{
    public class RegistersInspectorCategoryControl : InspectorCategoryControl
    {
        public RegistersInspectorCategoryControl() : base()
        {
            SetCategoryItemType(typeof(RegistersInspectorCategoryItem));
        }

        public void Initialise(GameBoy gameBoy)
        {
            Initialise(gameBoy, "Registers", 14);
        }

        protected override void ShowPage(int pageNo)
        {
            base.ShowPage(pageNo);
            for (int i = 0; i < ITEMS_PER_PAGE; i++)
            {
                RegistersInspectorCategoryItem memoryItem = (RegistersInspectorCategoryItem) categoryItems[i];
                if (i < length)
                {
                    memoryItem.IsVisible = true;
                    if (i < 8)
                        memoryItem.SetRegister((RegisterType)i);
                    else
                        memoryItem.SetRegister((RegisterPairType)(i - 8));
                }
                else
                    memoryItem.IsVisible = false;
            }
        }
    }
}
