using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;
using ZazBoy.UI.Controls.MemoryInspectorControls;

namespace ZazBoy.UI
{
    public class MemoryInspector : UserControl
    {
        private RegistersInspectorCategoryControl registersCategory;
        private MemoryRegionInspectorCategoryControl cartridgeCategory;
        private MemoryRegionInspectorCategoryControl vramCategory;
        private MemoryRegionInspectorCategoryControl exramCategory;
        private MemoryRegionInspectorCategoryControl wramCategory;
        private MemoryRegionInspectorCategoryControl oamCategory;
        private MemoryRegionInspectorCategoryControl ioCategory;
        private MemoryRegionInspectorCategoryControl hramCategory;
        private MemoryRegionInspectorCategoryControl interruptCategory;

        public MemoryInspector()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.registersCategory = this.FindControl<RegistersInspectorCategoryControl>("RegistersCategory");
            this.cartridgeCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("CartridgeCategory");
            this.vramCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("VRAMCategory");
            this.exramCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("EXRAMCategory");
            this.wramCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("WRAMCategory");
            this.oamCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("OAMCategory");
            this.ioCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("IOCategory");
            this.hramCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("HRAMCategory");
            this.interruptCategory = this.FindControl<MemoryRegionInspectorCategoryControl>("InterruptCategory");

            GameBoy gameBoy = GameBoy.Instance();
            registersCategory.Initialise(gameBoy);
            cartridgeCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.CARTRIDGE_ADDRESS), MemoryMap.CARTRIDGE_ADDRESS, (MemoryMap.VRAM_ADDRESS - MemoryMap.CARTRIDGE_ADDRESS));
            vramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.VRAM_ADDRESS), MemoryMap.VRAM_ADDRESS, (MemoryMap.EXRAM_ADDRESS - MemoryMap.VRAM_ADDRESS));
            exramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.EXRAM_ADDRESS), MemoryMap.EXRAM_ADDRESS, (MemoryMap.WRAM_ADDRESS - MemoryMap.EXRAM_ADDRESS));
            wramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.WRAM_ADDRESS), MemoryMap.WRAM_ADDRESS, (MemoryMap.PROHIBITED_ADDRESS - MemoryMap.WRAM_ADDRESS));
            oamCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.OAM_ADDRESS), MemoryMap.OAM_ADDRESS, (MemoryMap.UNUSED_ADDRESS - MemoryMap.OAM_ADDRESS));
            ioCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.IO_ADDRESS), MemoryMap.IO_ADDRESS, (MemoryMap.HRAM_ADDRESS - MemoryMap.IO_ADDRESS));
            hramCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.HRAM_ADDRESS), MemoryMap.HRAM_ADDRESS, (MemoryMap.INTERRUPT_ENABLE_ADDRESS - MemoryMap.HRAM_ADDRESS));
            interruptCategory.Initialise(gameBoy, MemoryMap.GetAddressLocationName(MemoryMap.INTERRUPT_ENABLE_ADDRESS), MemoryMap.INTERRUPT_ENABLE_ADDRESS, 1); //No -1 as we're not comparing against the next region, as there isn't one.
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            GameBoy gameBoy = GameBoy.Instance();
            ((MainWindow)e.Root).ShowEmulatorDisabledNotice(!gameBoy.IsPoweredOn);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            ((MainWindow)e.Root).ShowEmulatorDisabledNotice(false);
        }
    }
}
