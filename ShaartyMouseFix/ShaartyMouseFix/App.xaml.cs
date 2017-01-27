/*
 * TODO:
 * 1. Cancel click event (How?)
 * 2. Configure click time threshold
 * 3. Catch wrong mouse click
 * 4. Remove Windows Tips (debug)
 * 5. Stopwatch correction
 */

using System;
using System.Windows;
using System.Windows.Controls;
// using "Application and Global Mouse and Keyboard Hooks .Net Libary in C#": http://globalmousekeyhook.codeplex.com/
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

using System.Diagnostics;

namespace ShaartyMouseFix
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region HOOK INITIALIZATION
        private readonly KeyboardHookListener m_KeyboardHookManager;
        private readonly MouseHookListener m_MouseHookManager;
        #endregion

        #region TRAY INITIALIZATION
        private System.Windows.Forms.NotifyIcon nIcon;
        /// <summary>
        /// Menu:
        /// 0 - Catched No |
        /// 1 - Separator |
        /// 2 - Delay info |
        /// 3 - Delay slider |
        /// 4 - Separator |
        /// 5 - Exit
        /// </summary>
        private ContextMenu TrayMenu = null;
        #endregion

        #region STRINGS
        private const string STRING_TIME_DELAY = "Click time threshold";
        private const string STRING_TIME_UNIT = "ms";
        private const string STRING_TRAY_TEXT = "Shaarty MouseFix - fix your mouse double clicks";
        private const string STRING_TRAY_TIP_TITLE = "Shaarty MouseFix";
        private const string STRING_TRAY_TIP_TEXT = "Program started";
        #endregion

        /// <summary>
        /// Time between clicks, ms
        /// </summary>
        private int __timeDelay = 300;
        Stopwatch stopWatch = new Stopwatch();

        public App()
        {
            nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Text = STRING_TRAY_TEXT;
            nIcon.Icon = ShaartyMouseFix.Properties.Resources.IconDef;
            nIcon.Visible = true;
            nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, STRING_TRAY_TIP_TEXT, 
                System.Windows.Forms.ToolTipIcon.Info);
            nIcon.Click += nIcon_Click;

            #region TRAY MENU INITIALIZATION
            TrayMenu = new ContextMenu();
            // -- CATCHED ----
            MenuItem _miCatched = new MenuItem();
            _miCatched.Header = "Catched 0";
            _miCatched.Click += NullClick;
            TrayMenu.Items.Add(_miCatched);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- DELAY INFO ----
            MenuItem _miClickDelay = new MenuItem();
            _miClickDelay.Header = STRING_TIME_DELAY + ": 100 " + STRING_TIME_UNIT;
            _miClickDelay.Click += NullClick;
            TrayMenu.Items.Add(_miClickDelay);
            // -- DELAY SLIDER ----
            Slider _miClickDelaySlider = new Slider();
            _miClickDelaySlider.Minimum = 1;
            _miClickDelaySlider.Maximum = 1000;
            _miClickDelaySlider.MinWidth = 100;
            _miClickDelaySlider.IsSnapToTickEnabled = true;
            _miClickDelaySlider.SmallChange = 1;
            _miClickDelaySlider.LargeChange = 100;
            _miClickDelaySlider.Value = __timeDelay;
            _miClickDelaySlider.ValueChanged += Slider_ValueChanged;
            TrayMenu.Items.Add(_miClickDelaySlider);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- EXIT ----
            MenuItem _miExit = new MenuItem();
            _miExit.Header = "Exit";
            _miExit.Click += App_Exit;
            TrayMenu.Items.Add(_miExit);
            #endregion TRAY MENU INITIALIZATION

            nIcon.ContextMenu = Resources["TrayMenu"] as System.Windows.Forms.ContextMenu;

            #region HOOK INIT
            // -- Hook -------------------------
            /* If you want to handle keyboard clicks
            m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            m_KeyboardHookManager.Enabled = true;
            m_KeyboardHookManager.KeyDown += HookManager_KeyDown;
            m_KeyboardHookManager.KeyUp += HookManager_KeyUp;
            */
            m_MouseHookManager = new MouseHookListener(new GlobalHooker());
            m_MouseHookManager.Enabled = true;
            m_MouseHookManager.MouseDown += HookManager_MouseDown;
            m_MouseHookManager.MouseUp += HookManager_MouseUp;
            m_MouseHookManager.MouseClick += HookManager_MouseClick;
            #endregion

        }

        #region Keyboard Handle (OFF)
        /* If you want to handle keyboard clicks
        private void HookManager_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.KeyData.ToString() + " Pressed",
                System.Windows.Forms.ToolTipIcon.Info);
        }

        private void HookManager_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.KeyData.ToString() + " Released",
                System.Windows.Forms.ToolTipIcon.Info);
        }
        */
        #endregion

        #region Mouse Handle (ON)
        private void HookManager_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            stopWatch.Start();
            //nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() + " Pressed",
            //    System.Windows.Forms.ToolTipIcon.Info);
        }

        private void HookManager_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            stopWatch.Stop();
            int elapsedMs = stopWatch.Elapsed.Milliseconds;

            if (elapsedMs < __timeDelay)
            {
                nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() 
                    + " Released - U. ms: " + elapsedMs,
                    System.Windows.Forms.ToolTipIcon.Info);
                return;
            }
            else
            {
                nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() 
                    + " Released - H. ms: " + elapsedMs,
                    System.Windows.Forms.ToolTipIcon.Info);                
            }
        }

        private void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //stopWatch.Start();
           
            nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, 
                System.DateTime.Now + " "+ e.Button.ToString() + " Click | Sender: " + sender.ToString(),
                System.Windows.Forms.ToolTipIcon.Info);
            //nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() + " Pressed",
            //    System.Windows.Forms.ToolTipIcon.Info);
        }
        #endregion

        void NullClick(object sender, EventArgs e)
        { }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (TrayMenu.Items[2] as MenuItem).Header = 
                STRING_TIME_DELAY + ": " + e.NewValue + " " + STRING_TIME_UNIT;
            __timeDelay = Convert.ToInt32(e.NewValue);
        }

        void nIcon_Click(object sender, EventArgs e)
        {
            TrayMenu.IsOpen = true;
        }

        void App_Exit(object sender, EventArgs e)
        {
            nIcon.Dispose();
            Shutdown();
        }
    }
}