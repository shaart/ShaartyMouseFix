using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShaartyMouseFix
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon TrayIcon = null;
        private ContextMenu TrayMenu = null;

        public MainWindow()
        {
            InitializeComponent();
            //this.Visibility = Visibility.Collapsed;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                //this.Hide();
                this.Visibility = Visibility.Collapsed;
            }
            else if (WindowState == WindowState.Normal)
            {
                this.Visibility = Visibility.Visible;
            }
            else
            {
                base.OnStateChanged(e);
            }
        }

        private void ShowHideMainWindow(object sender, RoutedEventArgs e)
        {
            TrayMenu.IsOpen = false; // спрячем менюшку, если она вдруг видима
            if (IsVisible)
            {// если окно видно на экране
             // прячем его
                Hide();
                // меняем надпись на пункте меню
                (TrayMenu.Items[0] as MenuItem).Header = "Show";
            }
            else
            { // а если не видно
              // показываем
                Show();
                // меняем надпись на пункте меню
                (TrayMenu.Items[0] as MenuItem).Header = "Hide";
                //WindowState = CurrentWindowState;
                Activate(); // обязательно нужно отдать фокус окну,
                            // иначе пользователь сильно удивится, когда увидит окно
                            // но не сможет в него ничего ввести с клавиатуры
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
