using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using ZazBoy.Console;
using ZazBoy.UI.Controls;
using Size = Avalonia.Size;

namespace ZazBoy.UI
{
    public class MainWindow : Window
    {
        private EmulatorDisabledNotice emulatorDisabledNotice;
        private Panel contentHolder;
        private Panel dialogShade;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            contentHolder = this.FindControl<Panel>("ContentHolder");
            dialogShade = this.FindControl<Panel>("DialogShade");
            emulatorDisabledNotice = this.FindControl<EmulatorDisabledNotice>("EmulatorDisabledNotice");
            ShowDialogShade(false);
            ShowEmulatorDisabledNotice(false);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Panel rootPanel = this.FindControl<Panel>("RootPanel");
                rootPanel.Margin = new Thickness(0, 30, 0, 0);
            }
        }

        public void SetActiveContent(Control control)
        {
            contentHolder.Children.RemoveRange(0, contentHolder.Children.Count);
            contentHolder.Children.Add(control);
        }

        public void ShowDialogShade(bool show)
        {
            dialogShade.IsVisible = show;
        }

        public void ShowEmulatorDisabledNotice(bool show)
        {
            emulatorDisabledNotice.IsVisible = show;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Environment.Exit(0);
        }
    }
}
