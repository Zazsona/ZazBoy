using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using System.Threading;
using ZazBoy.Console;
using ZazBoy.Database;
using static ZazBoy.Console.GameBoy;

namespace ZazBoy.UI.Controls
{
    public class DebugControl : UserControl
    {
        private GameBoy gameBoy;
        private OperationBlock[] operationBlocks;
        private InstructionDatabase idb;

        private PauseHandler pauseHandler; 

        public DebugControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            idb = JsonConvert.DeserializeObject<InstructionDatabase>(Properties.Resources.InstructionDatabase);
            this.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            this.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            Grid operationsList = this.FindControl<Grid>("OperationsList");
            operationBlocks = new OperationBlock[10];
            for (int i = 0; i < 10; i++)
            {
                operationBlocks[i] = this.FindControl<OperationBlock>("OperationBlock" + i);
            }
            pauseHandler = (ushort programCounter) => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(programCounter)); };
            Image cpuIcon = this.FindControl<Image>("CPUIcon");
            cpuIcon.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.CPUIcon);

            Button stepButton = this.FindControl<Button>("StepButton");
            //stepButton.PointerReleased += HandleStepOperation;
            stepButton.Click += StepButton_Click;
        }

        public void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null)
                this.gameBoy.onEmulatorPaused -= pauseHandler;

            this.gameBoy = gameBoy;
            gameBoy.onEmulatorPaused += pauseHandler;
        }

        private void UpdateActiveInstructions(ushort programCounter)
        {
            ushort currentPosition = programCounter;
            foreach (OperationBlock operationBlock in operationBlocks)
            {
                bool isPrefixed = false;
                byte opcode = gameBoy.MemoryMap.ReadDirect(currentPosition);
                if (opcode == 0xCB)
                {
                    isPrefixed = true;
                    opcode = gameBoy.MemoryMap.ReadDirect((ushort)(currentPosition + 1));
                }
                string opcodeHex = "0x" + opcode.ToString("X2");
                InstructionEntry instructionEntry = (isPrefixed) ? idb.cbprefixed[opcodeHex] : idb.unprefixed[opcodeHex];
                operationBlock.SetMnemonic(instructionEntry.GetAssemblyLine());
                operationBlock.SetPosition("#" + currentPosition.ToString("X4"));
                currentPosition = (ushort)(currentPosition + instructionEntry.bytes);
            }
        }

        private void StepButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GameBoy.Instance().IsStepping = true;
            GameBoy.Instance().IsPaused = false;
        }
    }
}
