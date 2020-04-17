using System;
using System.Collections.Generic;
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

namespace Minecraft_Enchantment_Cracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();

            float progress = 0f;
            Task.Run(() => {
                Debug.WriteLine("Running tester");
                
                long start = Environment.TickCount;
                progress = 0f;
                int[] values = Cracker.GetSeeds(15, 7, 17, 30, null, ref progress);
                Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                Debug.WriteLine($"Expected: 81788565 | Actual: {values.Length}");
                if (values.Length == 81788565) {
                    Debug.WriteLine("First run OK");

                    start = Environment.TickCount;
                    progress = 0f;
                    int[] values2 = Cracker.GetSeeds(14, 7, 15, 28, values, ref progress);
                    Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                    Debug.WriteLine($"Expected: 2073151 | Actual: {values2.Length}");
                    if (values2.Length == 2073151) Debug.WriteLine("Second run OK");
                }

                progress = -1f;
            });
            Task.Run(() => {
                while (true) {
                    if (progress < 0) return;
                    Debug.WriteLine($"Progress: {(progress*100).ToString("000")}%");
                    Task.Delay(1000).Wait();
                }
            });
        }
    }
}
