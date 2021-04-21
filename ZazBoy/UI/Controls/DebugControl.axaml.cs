using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using ZazBoy.Console;
using ZazBoy.Database;
using static ZazBoy.Console.GameBoy;

namespace ZazBoy.UI.Controls
{
    public class DebugControl : UserControl
    {
        private GameBoy gameBoy;
        private BreakpointManager breakpointManager;
        private InstructionEditor instructionEditor;
        private Dictionary<OperationBlock, ushort> operationBlocks;
        private InstructionDatabase idb;
        private OperationBlock selectedOperationBlock;

        private PauseHandler pauseHandler; 
        private ResumeHandler resumeHandler;

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
            operationBlocks = new Dictionary<OperationBlock, ushort>();
            for (int i = 0; i < 10; i++)
            {
                OperationBlock operationBlock = this.FindControl<OperationBlock>("OperationBlock" + i);
                operationBlocks.Add(operationBlock, 0);
                operationBlock.PointerReleased += HandleOperationBlockSelected;
            }
            pauseHandler = (ushort programCounter) => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(programCounter, false)); };
            resumeHandler = () => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(gameBoy.CPU.programCounter, true)); };
            Image cpuIcon = this.FindControl<Image>("CPUIcon");
            cpuIcon.Source = UIUtil.ConvertDrawingBitmapToUIBitmap(Properties.Resources.CPUIcon);

            Button stepButton = this.FindControl<Button>("StepButton");
            //stepButton.PointerReleased += HandleStepOperation;
            stepButton.Click += HandleStep;
            Button skipButton = this.FindControl<Button>("SkipButton");
            skipButton.Click += HandleSkip;
            Button editButton = this.FindControl<Button>("EditButton");
            editButton.Click += HandleEditInstruction;
            Button disableButton = this.FindControl<Button>("DisableButton");
            disableButton.Click += HandleDisable;

            Button breakpointsButton = this.FindControl<Button>("BreakpointsButton");
            breakpointsButton.Click += HandleBreakpointsSelected;
        }

        public void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null)
            {
                this.gameBoy.onEmulatorPaused -= pauseHandler;
                this.gameBoy.onEmulatorResumed -= resumeHandler;
            }

            this.gameBoy = gameBoy;
            gameBoy.onEmulatorPaused += pauseHandler;
            gameBoy.onEmulatorResumed += resumeHandler;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdateActiveInstructions(gameBoy.CPU.programCounter, !gameBoy.IsPaused);
        }

        private void UpdateActiveInstructions(ushort programCounter, bool blankOut)
        {
            if (!blankOut)
            {
                ushort currentPosition = programCounter;
                foreach (OperationBlock operationBlock in operationBlocks.Keys)
                {
                    InstructionEntry instructionEntry = GetInstructionEntry(currentPosition);
                    operationBlock.SetMnemonic(instructionEntry.GetAssemblyLine());
                    operationBlock.SetPosition("#" + currentPosition.ToString("X4"));
                    operationBlocks[operationBlock] = currentPosition;
                    currentPosition = (ushort)(currentPosition + instructionEntry.bytes);
                }
            }
            else
            {
                foreach (OperationBlock operationBlock in operationBlocks.Keys)
                {
                    operationBlock.SetMnemonic("----");
                    operationBlock.SetPosition("----");
                    operationBlocks[operationBlock] = 0;
                }
            }
        }

        private InstructionEntry GetInstructionEntry(ushort memoryAddress)
        {
            bool isPrefixed = false;
            byte opcode = gameBoy.MemoryMap.ReadDirect(memoryAddress);
            if (opcode == 0xCB)
            {
                isPrefixed = true;
                opcode = gameBoy.MemoryMap.ReadDirect((ushort)(memoryAddress + 1));
            }
            string opcodeHex = "0x" + opcode.ToString("X2");
            InstructionEntry instructionEntry = (isPrefixed) ? idb.cbprefixed[opcodeHex] : idb.unprefixed[opcodeHex];
            return instructionEntry;
        }

        private void HandleOperationBlockSelected(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            if (selectedOperationBlock != null)
                selectedOperationBlock.SetSelected(false);

            OperationBlock operationBlock = (OperationBlock)sender;
            if (operationBlock == selectedOperationBlock)
                selectedOperationBlock = null;
            else
                selectedOperationBlock = (OperationBlock)sender;
        }

        private void HandleStep(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                GameBoy.Instance().IsStepping = true;
                GameBoy.Instance().IsPaused = false;
            }
        }

        private void HandleSkip(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                InstructionEntry instructionEntry = GetInstructionEntry(gameBoy.CPU.programCounter);
                for (int i = 0; i<instructionEntry.bytes; i++)
                    gameBoy.CPU.IncrementProgramCounter();
                UpdateActiveInstructions(gameBoy.CPU.programCounter, false);
            }
        }

        private void HandleDisable(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                gameBoy.MemoryMap.WriteDirect(gameBoy.CPU.programCounter, 0x00); //NOP
                UpdateActiveInstructions(gameBoy.CPU.programCounter, false);
            }
        }

        private void HandleBreakpointsSelected(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused && (breakpointManager == null || !breakpointManager.IsVisible))
            {
                breakpointManager = new BreakpointManager();
                breakpointManager.Show();
            }
        }

        private void HandleEditInstruction(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused && selectedOperationBlock != null && operationBlocks[selectedOperationBlock] != 0 && (instructionEditor == null || !instructionEditor.IsVisible))
            {
                ushort memoryAddress = operationBlocks[selectedOperationBlock];
                instructionEditor = new InstructionEditor();
                instructionEditor.Initialise(gameBoy, idb, GetInstructionEntry(memoryAddress), memoryAddress, (memoryAddress == 0xCB));
                instructionEditor.Show();
            }
        }
    }
}
