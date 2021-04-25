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
        /*private GameBoy gameBoy;
        private Grid blockingPanel;
        private BreakpointManager breakpointManager;
        private InstructionEditor instructionEditor;
        private MemoryInspector memoryInspector;

        private PauseHandler pauseHandler; 
        private ResumeHandler resumeHandler;
        private PowerStateChangeHandler powerHandler;
        private bool isStep;

        public DebugControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            this.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
            blockingPanel = this.FindControl<Grid>("BlockingPanel");

            pauseHandler = (ushort programCounter) => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(programCounter, false));};
            resumeHandler = () => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(gameBoy.CPU.programCounter, true));};
            powerHandler = (bool powered) => { Dispatcher.UIThread.Post(() => UpdateActiveInstructions(0, true)); };
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
            Button inspectorButton = this.FindControl<Button>("InspectorButton");
            inspectorButton.Click += HandleInspectorSelected;
        }

        public void HookToGameBoy(GameBoy gameBoy)
        {
            if (this.gameBoy != null)
            {
                this.gameBoy.onEmulatorPaused -= pauseHandler;
                this.gameBoy.onEmulatorResumed -= resumeHandler;
                this.gameBoy.onEmulatorPowerStateChanged -= powerHandler;
            }

            this.gameBoy = gameBoy;
            gameBoy.onEmulatorPaused += pauseHandler;
            gameBoy.onEmulatorResumed += resumeHandler;
            gameBoy.onEmulatorPowerStateChanged += powerHandler;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            UpdateActiveInstructions(gameBoy.CPU.programCounter, !gameBoy.IsPaused);
        }

        private void UpdateActiveInstructions(ushort programCounter, bool blankOut)
        {
            if (gameBoy == null)
                return;

            if (!isStep) //Check for if it's a step, as otherwise the panel will just flash on screen, which is not particularly comfortable.
                blockingPanel.IsVisible = blankOut; 
            isStep = false;

            if (!blankOut && gameBoy.IsPoweredOn)
            {
                ushort currentPosition = programCounter;
                foreach (OperationBlock operationBlock in operationBlocks.Keys)
                {
                    InstructionEntry instructionEntry = UIUtil.GetInstructionEntry(gameBoy, currentPosition);
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
                    if (selectedOperationBlock != null)
                    {
                        selectedOperationBlock.SetSelected(false);
                        selectedOperationBlock = null;
                    }
                }
            }
        }

        private void HandleStep(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                isStep = true;
                GameBoy.Instance().IsStepping = true;
                GameBoy.Instance().IsPaused = false;
            }
        }

        private void HandleSkip(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (gameBoy.IsPaused)
            {
                InstructionEntry instructionEntry = UIUtil.GetInstructionEntry(gameBoy, gameBoy.CPU.programCounter);
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
            /*if (gameBoy.IsPaused && (breakpointManager == null || !breakpointManager.IsVisible))
            {
                breakpointManager = new BreakpointManager();
                breakpointManager.Closed += HandleDialogClosed;
                MainWindow mainWindow = (MainWindow)this.VisualRoot;
                mainWindow.ShowDialogShade(true);
                breakpointManager.ShowDialog(mainWindow);
            }*/
        }

        //private void HandleInspectorSelected(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        //{
            /*if (gameBoy.IsPaused && (memoryInspector == null || !memoryInspector.IsVisible))
            {
                memoryInspector = new MemoryInspector();
                memoryInspector.Closed += HandleDialogClosed;
                MainWindow mainWindow = (MainWindow)this.VisualRoot;
                mainWindow.ShowDialogShade(true);
                memoryInspector.ShowDialog(mainWindow);
            }*/
        //}

        //private void HandleEditInstruction(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        //{
            /*if (gameBoy.IsPaused && selectedOperationBlock != null && operationBlocks[selectedOperationBlock] != 0 && (instructionEditor == null || !instructionEditor.IsVisible))
            {
                ushort memoryAddress = operationBlocks[selectedOperationBlock];
                instructionEditor = new InstructionEditor();
                instructionEditor.Initialise(gameBoy, memoryAddress, gameBoy.MemoryMap.ReadDirect(memoryAddress) == 0xCB);
                instructionEditor.Closed += HandleDialogClosed;
                MainWindow mainWindow = (MainWindow)this.VisualRoot;
                mainWindow.ShowDialogShade(true);
                instructionEditor.ShowDialog(mainWindow);
            }*/
        //}

        //private void HandleDialogClosed(object? sender, System.EventArgs e)
        //{
         //   UpdateActiveInstructions(gameBoy.CPU.programCounter, !gameBoy.IsPaused);
        //    MainWindow mainWindow = (MainWindow)this.VisualRoot;
        //    mainWindow.ShowDialogShade(false);
        //}
    //}
}
