using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;
using ZazBoy.UI.Controls.MemoryInspectorControls;

namespace ZazBoy.UI
{
    public class MemoryInspector : Window
    {
        private MemoryCategoryControl cartridgeCategory;
        private MemoryCategoryControl vramCategory;
        private MemoryCategoryControl exramCategory;
        private MemoryCategoryControl wramCategory;
        private MemoryCategoryControl oamCategory;
        private MemoryCategoryControl ioCategory;
        private MemoryCategoryControl hramCategory;
        private MemoryCategoryControl interruptCategory;

        public MemoryInspector()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.cartridgeCategory = this.FindControl<MemoryCategoryControl>("CartridgeCategory");
            this.vramCategory = this.FindControl<MemoryCategoryControl>("VRAMCategory");
            this.exramCategory = this.FindControl<MemoryCategoryControl>("EXRAMCategory");
            this.wramCategory = this.FindControl<MemoryCategoryControl>("WRAMCategory");
            this.oamCategory = this.FindControl<MemoryCategoryControl>("OAMCategory");
            this.ioCategory = this.FindControl<MemoryCategoryControl>("IOCategory");
            this.hramCategory = this.FindControl<MemoryCategoryControl>("HRAMCategory");
            this.interruptCategory = this.FindControl<MemoryCategoryControl>("InterruptCategory");

            GameBoy gameBoy = GameBoy.Instance();
            cartridgeCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.CARTRIDGE_ADDRESS), MemoryMap.CARTRIDGE_ADDRESS, (MemoryMap.VRAM_ADDRESS - MemoryMap.CARTRIDGE_ADDRESS));
            vramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.VRAM_ADDRESS), MemoryMap.VRAM_ADDRESS, (MemoryMap.EXRAM_ADDRESS - MemoryMap.VRAM_ADDRESS));
            exramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.EXRAM_ADDRESS), MemoryMap.EXRAM_ADDRESS, (MemoryMap.WRAM_ADDRESS - MemoryMap.EXRAM_ADDRESS));
            wramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.WRAM_ADDRESS), MemoryMap.WRAM_ADDRESS, (MemoryMap.PROHIBITED_ADDRESS - MemoryMap.WRAM_ADDRESS));
            oamCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.OAM_ADDRESS), MemoryMap.OAM_ADDRESS, (MemoryMap.UNUSED_ADDRESS - MemoryMap.OAM_ADDRESS));
            ioCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.IO_ADDRESS), MemoryMap.IO_ADDRESS, (MemoryMap.HRAM_ADDRESS - MemoryMap.IO_ADDRESS));
            hramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.HRAM_ADDRESS), MemoryMap.HRAM_ADDRESS, (MemoryMap.INTERRUPT_ENABLE_ADDRESS - MemoryMap.HRAM_ADDRESS));
            interruptCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.INTERRUPT_ENABLE_ADDRESS), MemoryMap.INTERRUPT_ENABLE_ADDRESS, 1); //No -1 as we're not comparing against the next region, as there isn't one.
        }
    }
}
