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
            
            Cracker c = new Cracker();
            bool done = false;
            Task.Run(() => {

                Debug.WriteLine("Running tester");
                
                long start = Environment.TickCount;
                int[] values = c.GetSeeds(15, 7, 17, 30, null);
                Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                Debug.WriteLine($"Expected: 81788565 | Actual: {values.Length}");
                if (values.Length == 81788565) {
                    Debug.WriteLine("First run OK");

                    start = Environment.TickCount;
                    int[] values2 = c.GetSeeds(14, 7, 15, 28, values);
                    Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                    Debug.WriteLine($"Expected: 2073151 | Actual: {values2.Length}");
                    if (values2.Length == 2073151) Debug.WriteLine("Second run OK");
                }

                done = true;
            });
            Task.Run(() => {
                while (true) {
                    if (done) return;
                    Debug.WriteLine($"Progress: {(c.Progress*100).ToString("000")}%");
                    Task.Delay(1000).Wait();
                }
            });
        }
    }
}
