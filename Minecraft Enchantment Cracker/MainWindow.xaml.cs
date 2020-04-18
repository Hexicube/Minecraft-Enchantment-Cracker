using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace Minecraft_Enchantment_Cracker {
    public partial class MainWindow : Window {
        public IProgressiveTask ProgressTask;

        public MainWindow() {
            InitializeComponent();
            
            Cracker c = new Cracker();
            ProgressTask = c;
            Task.Run(() => {
                Debug.WriteLine("Running tester");
                
                long start = Environment.TickCount;
                int[] values = c.GetSeeds(15, 7, 17, 30, null);
                Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                Debug.WriteLine($"Expected: 81788565 | Actual: {values.Length}");
                if (values.Length == 81788565) {
                    Debug.WriteLine("First run OK");
                    Task.Delay(2500).Wait();

                    start = Environment.TickCount;
                    int[] values2 = c.GetSeeds(14, 7, 15, 28, values);
                    Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                    Debug.WriteLine($"Expected: 2073151 | Actual: {values2.Length}");
                    if (values2.Length == 2073151) Debug.WriteLine("Second run OK");
                }
            });
            Dispatcher.InvokeAsync(async () => {
                bool success;
                SolidColorBrush red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightPink"));
                SolidColorBrush green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
                while (true) {
                    success = ProgressTask.Success;
                    float p = success ? ProgressTask.Progress : 1f;
                    ProgressBar.Value = p;
                    ProgressBar.Foreground = success ? green : red;
                    ProgressText.Text = ProgressTask.ProgressText;
                    ProgressPercent.Text = ProgressTask.ProgressText2;
                    await Task.Delay(50);
                }
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
