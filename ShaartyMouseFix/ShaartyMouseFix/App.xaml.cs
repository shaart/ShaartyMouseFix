/*
 * TODO:
 * 1. Configure click time threshold
 * 2. Remove Windows Tips (debug)
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
        private const string STRING_CATCHED_WRONG_CLICKS = "Catched";
        private const string STRING_EXIT = "Exit";
        #endregion

        /// <summary>
        /// Time between clicks, ms
        /// </summary>
        private int __timeDelay = 100;
        private int __catchedWrongClicks;
        /// <summary>
        /// Time (ms) when tip is visible
        /// </summary>
        private const int TIP_TIMEOUT = 200;
        Stopwatch stopWatch = new Stopwatch();
        Stopwatch timer_LeftClick, timer_RightClick, timer_MiddleClick, 
            timer_X1Click, timer_X2Click;

        public App()
        {
            __catchedWrongClicks = 0;

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
            _miCatched.Header = STRING_CATCHED_WRONG_CLICKS + " " + 0;
            _miCatched.Click += NullClick;
            TrayMenu.Items.Add(_miCatched);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- DELAY INFO ----
            MenuItem _miClickDelay = new MenuItem();
            _miClickDelay.Header = STRING_TIME_DELAY + ": " + __timeDelay + " " + STRING_TIME_UNIT;
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
            _miExit.Header = STRING_EXIT;
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
            m_MouseHookManager.MouseUp += HookManager_MouseUp;
            m_MouseHookManager.MouseClick += HookManager_MouseClick;
            m_MouseHookManager.MouseDownExt += HookManager_MouseDownExt;
            #endregion

            #region TIMERS INIT AND START
            timer_LeftClick = new Stopwatch();
            timer_RightClick = new Stopwatch();
            timer_MiddleClick = new Stopwatch();
            timer_X1Click = new Stopwatch();
            timer_X2Click = new Stopwatch();

            timer_LeftClick.Start();
            timer_MiddleClick.Start();
            timer_RightClick.Start();
            timer_X1Click.Start();
            timer_X2Click.Start();
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
        private bool isHandleButton(ref Stopwatch buttonTimer, ref MouseEventExtArgs e)
        {
            buttonTimer.Stop();
            if (buttonTimer.ElapsedMilliseconds < __timeDelay)
            {
                buttonTimer.Start();
                return true;
            }
            else
            {
                buttonTimer.Restart();
                return false;
            }
        }

        private void HookManager_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            e.Handled = false;
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    //e.Handled = isHandleButton(ref timer_LeftClick, ref e);
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    e.Handled = isHandleButton(ref timer_MiddleClick, ref e);
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    e.Handled = isHandleButton(ref timer_RightClick, ref e);
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    e.Handled = isHandleButton(ref timer_X1Click, ref e);                    
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    e.Handled = isHandleButton(ref timer_X2Click, ref e);                    
                    break;
                default:
                    break;
            }
            if (e.Handled)
            {
                __catchedWrongClicks++;
                ((MenuItem)TrayMenu.Items[0]).Header = STRING_CATCHED_WRONG_CLICKS + " " + __catchedWrongClicks;
                nIcon.ShowBalloonTip(TIP_TIMEOUT, STRING_TRAY_TIP_TITLE,
                    System.DateTime.Now + " [Handled] " + e.Button.ToString() + " down",
                    System.Windows.Forms.ToolTipIcon.Info);
            }
            else
            {
                if (e.Button != System.Windows.Forms.MouseButtons.Left)
                {
                    nIcon.ShowBalloonTip(TIP_TIMEOUT, STRING_TRAY_TIP_TITLE,
                        System.DateTime.Now + " " + e.Button.ToString() + " down",
                        System.Windows.Forms.ToolTipIcon.Info);
                }
            }
        }

        private void HookManager_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        //    stopWatch.Stop();
        //    int elapsedMs = stopWatch.Elapsed.Milliseconds;

        //    if (elapsedMs < __timeDelay)
        //    {
        //        nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() 
        //            + " Released - U. ms: " + elapsedMs,
        //            System.Windows.Forms.ToolTipIcon.Info);
        //        return;
        //    }
        //    else
        //    {
        //        nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE, e.Button.ToString() 
        //            + " Released - H. ms: " + elapsedMs,
        //            System.Windows.Forms.ToolTipIcon.Info);                
        //    }
        }

        private void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //stopWatch.Start();
            //if (e.Button != System.Windows.Forms.MouseButtons.Left)
            //{
            //    nIcon.ShowBalloonTip(3000, STRING_TRAY_TIP_TITLE,
            //        System.DateTime.Now + " " + e.Button.ToString() + " Click | Sender: " + sender.ToString(),
            //        System.Windows.Forms.ToolTipIcon.Info);
            //}
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