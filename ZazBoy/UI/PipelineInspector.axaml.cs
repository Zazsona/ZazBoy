using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ZazBoy.Console;
using ZazBoy.Database;
using ZazBoy.UI.Controls;
using ZazBoy.UI.Controls.Pipeline;

namespace ZazBoy.UI
{
    public class PipelineInspector : UserControl
    {
        private InstructionEditor instructionEditor;
        private OperationQueue instructionPipeline;
        private Button stepButton;
        private Button skipButton;
        private ushort[] addresses;

        public PipelineInspector()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            instructionEditor = this.FindControl<InstructionEditor>("InstructionEditor");
            instructionEditor.onInstructionEdited += (ushort address) => { UpdateActiveInstructions(GameBoy.Instance(), GameBoy.Instance().CPU.programCounter); };
            instructionPipeline = this.FindControl<OperationQueue>("InstructionPipeline");
            stepButton = this.FindControl<Button>("StepButton");
            skipButton = this.FindControl<Button>("SkipButton");
            stepButton.Content = "   ^\nStep";
            skipButton.Content = "  ^^\nSkip";
            stepButton.Click += HandleStep;
            skipButton.Click += HandleSkip;
            addresses = new ushort[instructionPipeline.GetOperationBlocks().Length];
            foreach (OperationBlock operationBlock in instructionPipeline.GetOperationBlocks())
            {
                operationBlock.PointerPressed += LoadInstructionIntoEditor;
            }
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            GameBoy gameBoy = GameBoy.Instance();
            ((MainWindow)e.Root).ShowEmulatorDisabledNotice(!gameBoy.IsPoweredOn);
            if (gameBoy.IsPoweredOn)
            {
                UpdateActiveInstructions(gameBoy, gameBoy.CPU.programCounter);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            ((MainWindow)e.Root).ShowEmulatorDisabledNotice(false);
        }

        private void LoadInstructionIntoEditor(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            OperationBlock instructionBlock = (OperationBlock)sender;
            if (instructionPipeline.GetSelectedBlock() != instructionBlock)
            {
                GameBoy gameBoy = GameBoy.Instance();
                int blockIndex = 0;
                for (int i = 0; i < instructionPipeline.GetOperationBlocks().Length; i++)
                {
                    if (instructionBlock == instructionPipeline.GetOperationBlocks()[i])
                    {
                        blockIndex = i;
                        break;
                    }
                }
                ushort address = addresses[blockIndex];
                instructionEditor.SetInstruction(gameBoy, address, (gameBoy.MemoryMap.ReadDirect(address) == 0xCB));
            }
            else
            {
                instructionEditor.ResetInstruction();
            }

        }

        private void UpdateActiveInstructions(GameBoy gameBoy, ushort programCounter)
        {
            ushort currentPosition = programCounter;
            for (int i = 0; i<instructionPipeline.GetOperationBlocks().Length; i++)
            {
                OperationBlock operationBlock = instructionPipeline.GetOperationBlocks()[i];
                InstructionEntry instructionEntry = UIUtil.GetInstructionEntry(gameBoy, currentPosition);
                operationBlock.SetMnemonic(instructionEntry.GetAssemblyLine());
                operationBlock.SetPosition("#" + currentPosition.ToString("X4"));
                addresses[i] = currentPosition;
                currentPosition = (ushort)(currentPosition + instructionEntry.bytes);
            }
        }

        private void HandleStep(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GameBoy gameBoy = GameBoy.Instance();
            if (gameBoy.IsPaused)
            {
                gameBoy.IsStepping = true;
                gameBoy.IsPaused = false;
                UpdateActiveInstructions(gameBoy, gameBoy.CPU.programCounter);
            }
        }

        private void HandleSkip(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            GameBoy gameBoy = GameBoy.Instance();
            if (gameBoy.IsPaused)
            {
                InstructionEntry instructionEntry = UIUtil.GetInstructionEntry(gameBoy, gameBoy.CPU.programCounter);
                for (int i = 0; i < instructionEntry.bytes; i++)
                    gameBoy.CPU.IncrementProgramCounter();
                UpdateActiveInstructions(gameBoy, gameBoy.CPU.programCounter);
            }
        }
    }
}
