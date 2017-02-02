/*
 * Author: Shalaev Artur
 * Year: 2017
 * 
 * TODO:
 * 1. User config: which mouse buttons must be handled
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
// Thanks to: "Application and Global Mouse and Keyboard Hooks .Net Libary in C#" http://globalmousekeyhook.codeplex.com/
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace ShaartyMouseFix
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region HOOK INITIALIZATION
        //private readonly KeyboardHookListener m_KeyboardHookManager;
        private readonly MouseHookListener m_MouseHookManager;
        #endregion

        #region TRAY INITIALIZATION
        private System.Windows.Forms.NotifyIcon nIcon;
        /// <summary>
        /// Menu:
        /// 0 - Close menu |
        /// 1 - Separator |
        /// 2 - Catched No |
        /// 3 - Separator |
        /// 4 - Delay info |
        /// 5 - Delay slider |
        /// 6 - Separator |
        /// 7 - Exit
        /// </summary>
        private ContextMenu TrayMenu = null;
        private const ushort TRAY_MENU_CATCHED_INDEX = 2;
        private const ushort TRAY_MENU_DELAY_INFO_INDEX = 4;
        #endregion

        #region LANGUAGE STRINGS
        private const string STRING_TRAY_TIP_TITLE = "Shaarty MouseFix";
        private string STRING_TRAY_TEXT = STRING_TRAY_TIP_TITLE + " - " + "suppresses your mouse's wrong (fast) clicks";
        private string STRING_PROGRAM_STARTED = "Program started";
        private string STRING_CURRENT_OPTIONS = "Current options";
        private string STRING_CLICK_DELAY = "Click delay";
        private string STRING_CLOSE_MENU = "> " + "Click here to close this menu" + " <";
        private string STRING_CATCHED_WRONG_CLICKS = "Catched";
        private string STRING_EXIT = "Exit";
        private string STRING_TIME_DELAY = "Click time threshold";
        private string STRING_TIME_UNIT = "ms";
        #endregion

        #region APPLICATION CONSTS
        // Config.ini sections
        const string INI_SECT_LANG = "Localization";
        const string INI_SECT_CONFIG = "Config";

        /// <summary>
        /// Time (ms) when tip is visible
        /// </summary>
        private const int TIP_TIMEOUT = 200;

        private const int DEFAULT_CLICK_DELAY = 100;
        #endregion

        private const string CONFIG_FILE = "config.ini";
        IniFile ConfigINI;

        /// <summary>
        /// Time between clicks, ms
        /// </summary>
        private int __clickDelay;
        private int __catchedWrongClicks;

        /// <summary>
        /// Button click timer
        /// </summary>
        Stopwatch timer_LeftClick, timer_RightClick, timer_MiddleClick,
                  timer_X1Click, timer_X2Click;

        /// <summary>
        /// Loads options and lang strings from ini-file
        /// </summary>
        private void LoadConfigINI()
        {
            // Configuration
            if (ConfigINI.KeyExists("CLICK_DELAY", INI_SECT_CONFIG))
            {
                __clickDelay = int.Parse(ConfigINI.ReadINI(INI_SECT_CONFIG, "CLICK_DELAY"));
            }
            // Language strings
            if (ConfigINI.KeyExists("STRING_TRAY_TEXT", INI_SECT_LANG))
            {
                STRING_TRAY_TEXT = STRING_TRAY_TIP_TITLE + " - " +
                    ConfigINI.ReadINI(INI_SECT_LANG, "STRING_TRAY_TEXT");
            }
            if (ConfigINI.KeyExists("STRING_PROGRAM_STARTED", INI_SECT_LANG))
            {
                STRING_PROGRAM_STARTED = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_PROGRAM_STARTED");
            }
            if (ConfigINI.KeyExists("STRING_CURRENT_OPTIONS", INI_SECT_LANG))
            {
                STRING_CURRENT_OPTIONS = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_CURRENT_OPTIONS");
            }
            if (ConfigINI.KeyExists("STRING_CLICK_DELAY", INI_SECT_LANG))
            {
                STRING_CLICK_DELAY = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_CLICK_DELAY");
            }
            if (ConfigINI.KeyExists("STRING_CLOSE_MENU", INI_SECT_LANG))
            {
                STRING_CLOSE_MENU = "> " + ConfigINI.ReadINI(INI_SECT_LANG, "STRING_CLOSE_MENU") + " <";
            }
            if (ConfigINI.KeyExists("STRING_CURRENT_OPTIONS", INI_SECT_LANG))
            {
                STRING_CURRENT_OPTIONS = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_CURRENT_OPTIONS");
            }
            if (ConfigINI.KeyExists("STRING_CATCHED_WRONG_CLICKS", INI_SECT_LANG))
            {
                STRING_CATCHED_WRONG_CLICKS = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_CATCHED_WRONG_CLICKS");
            }
            if (ConfigINI.KeyExists("STRING_EXIT", INI_SECT_LANG))
            {
                STRING_EXIT = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_EXIT");
            }
            if (ConfigINI.KeyExists("STRING_TIME_DELAY", INI_SECT_LANG))
            {
                STRING_TIME_DELAY = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_TIME_DELAY");
            }
            if (ConfigINI.KeyExists("STRING_TIME_UNIT", INI_SECT_LANG))
            {
                STRING_TIME_UNIT = ConfigINI.ReadINI(INI_SECT_LANG, "STRING_TIME_UNIT");
            }
        }

        /// <summary>
        /// Saves options to ini-file
        /// </summary>
        private void SaveOptions()
        {
            // Configuration
            ConfigINI.Write(INI_SECT_CONFIG, "CLICK_DELAY", __clickDelay.ToString());
        }

        /// <summary>
        /// Saves lang strings to ini-file
        /// </summary>
        private void SaveStrings()
        {
            // Language strings
            // STRING_TRAY_TEXT = STRING_TRAY_TIP_TITLE + " - " + "suppresses your mouse's wrong (fast) clicks";
            ConfigINI.Write(INI_SECT_LANG, "STRING_TRAY_TEXT",
                STRING_TRAY_TEXT.Substring(STRING_TRAY_TEXT.IndexOf(" - ") + 3));
            ConfigINI.Write(INI_SECT_LANG, "STRING_PROGRAM_STARTED", STRING_PROGRAM_STARTED);
            ConfigINI.Write(INI_SECT_LANG, "STRING_CURRENT_OPTIONS", STRING_CURRENT_OPTIONS);
            ConfigINI.Write(INI_SECT_LANG, "STRING_CLICK_DELAY", STRING_CLICK_DELAY);
            // STRING_CLOSE_MENU = "> " + "Click here to close this menu" + " <";
            ConfigINI.Write(INI_SECT_LANG, "STRING_CLOSE_MENU",
                STRING_CLOSE_MENU.Substring(2, STRING_CLOSE_MENU.Length - 4));// "> " and " <"

            ConfigINI.Write(INI_SECT_LANG, "STRING_CATCHED_WRONG_CLICKS", STRING_CATCHED_WRONG_CLICKS);
            ConfigINI.Write(INI_SECT_LANG, "STRING_EXIT", STRING_EXIT);
            ConfigINI.Write(INI_SECT_LANG, "STRING_TIME_DELAY", STRING_TIME_DELAY);
            ConfigINI.Write(INI_SECT_LANG, "STRING_TIME_UNIT", STRING_TIME_UNIT);
        }

        public App()
        {
            Application.Current.Exit += Application_Exit;
            #region OPTIONS
            __catchedWrongClicks = 0;
            __clickDelay = DEFAULT_CLICK_DELAY;

            ConfigINI = new IniFile(CONFIG_FILE);
            if (System.IO.File.Exists(ConfigINI.Path))
            {
                LoadConfigINI();
            }
            else
            {
                // Create ini-file
                SaveOptions();
                SaveStrings();
            }
            #endregion

            nIcon = new System.Windows.Forms.NotifyIcon();
            nIcon.Text = STRING_TRAY_TEXT;
            nIcon.Icon = ShaartyMouseFix.Properties.Resources.Icon64;
            nIcon.Visible = true;
            nIcon.ShowBalloonTip(3000,
                STRING_TRAY_TIP_TITLE + " - " + STRING_PROGRAM_STARTED,
                STRING_CURRENT_OPTIONS + "\n> " +
                STRING_CLICK_DELAY + ": " + __clickDelay + " " + STRING_TIME_UNIT,
                System.Windows.Forms.ToolTipIcon.Info);
            nIcon.Click += nIcon_Click;

            #region TRAY MENU INITIALIZATION
            TrayMenu = new ContextMenu();
            TrayMenu.StaysOpen = false;
            // -- CLOSE MENU ----
            MenuItem _miCloseMenu = new MenuItem();
            _miCloseMenu.Header = STRING_CLOSE_MENU;
            _miCloseMenu.HorizontalContentAlignment = HorizontalAlignment.Center;
            _miCloseMenu.Click += NullClick;
            TrayMenu.Items.Add(_miCloseMenu);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- CATCHED ----
            MenuItem _miCatched = new MenuItem();
            _miCatched.Header = STRING_CATCHED_WRONG_CLICKS + " " + __catchedWrongClicks;
            _miCatched.Click += NullClick;
            TrayMenu.Items.Add(_miCatched);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- DELAY INFO ----
            MenuItem _miClickDelay = new MenuItem();
            _miClickDelay.Header = STRING_TIME_DELAY + ": " + __clickDelay + " " + STRING_TIME_UNIT;
            _miClickDelay.Click += NullClick;
            TrayMenu.Items.Add(_miClickDelay);
            // -- DELAY SLIDER ----
            Slider _miClickDelaySlider = new Slider();
            _miClickDelaySlider.Minimum = 1;
            _miClickDelaySlider.Maximum = 1000;
            _miClickDelaySlider.MinWidth = 100;
            _miClickDelaySlider.Width = 200;
            _miClickDelaySlider.IsSnapToTickEnabled = true;
            _miClickDelaySlider.SmallChange = 1;
            _miClickDelaySlider.LargeChange = 100;
            _miClickDelaySlider.Value = __clickDelay;
            _miClickDelaySlider.ValueChanged += Slider_ValueChanged;
            TrayMenu.Items.Add(_miClickDelaySlider);
            // -- SEPARATOR ----
            TrayMenu.Items.Add(new Separator());
            // -- EXIT ----
            MenuItem _miExit = new MenuItem();
            _miExit.Header = STRING_EXIT;
            _miExit.Click += Menu_Exit_Click;
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
        private bool isTimeHasPassed(ref Stopwatch buttonTimer)
        {
            buttonTimer.Stop();
            if (buttonTimer.ElapsedMilliseconds < __clickDelay)
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
                    e.Handled = isTimeHasPassed(ref timer_LeftClick);
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    e.Handled = isTimeHasPassed(ref timer_MiddleClick);
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    e.Handled = isTimeHasPassed(ref timer_RightClick);
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    e.Handled = isTimeHasPassed(ref timer_X1Click);
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    e.Handled = isTimeHasPassed(ref timer_X2Click);
                    break;
                default:
                    break;
            }
            if (e.Handled)
            {
                __catchedWrongClicks++;
                ((MenuItem)TrayMenu.Items[TRAY_MENU_CATCHED_INDEX]).Header =
                    STRING_CATCHED_WRONG_CLICKS + " " + __catchedWrongClicks;
            }
        }
        #endregion

        void NullClick(object sender, EventArgs e)
        { }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            (TrayMenu.Items[TRAY_MENU_DELAY_INFO_INDEX] as MenuItem).Header =
                STRING_TIME_DELAY + ": " + e.NewValue + " " + STRING_TIME_UNIT;
            __clickDelay = Convert.ToInt32(e.NewValue);
        }

        void nIcon_Click(object sender, EventArgs e)
        {
            TrayMenu.IsOpen = true;
        }

        void Menu_Exit_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SaveOptions();
            nIcon.Dispose();
        }
    }
}