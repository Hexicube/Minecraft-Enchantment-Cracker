using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Minecraft_Enchantment_Cracker {
    public class ItemBackgroundCanvas : Canvas {
        private SolidColorBrush brushOK  = new SolidColorBrush(new Color { R=139, G=139, B=139, A=255 }),
                                brushBAD = new SolidColorBrush(new Color { R= 85, G= 85, B= 85, A=255 }),
                                brushSEL = new SolidColorBrush(new Color { R=139, G=255, B=139, A=255 });
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            for (int y = 0; y < MainWindow.avail.Length; y++) {
                for (int x = 0; x < MainWindow.avail[y].Length; x++) {
                    dc.DrawRectangle(MainWindow.avail[y][x] ? ((x==MainWindow.ItemSelection.x && y==MainWindow.ItemSelection.y) ? brushSEL : brushOK) : brushBAD, null, new Rect(x*36, y*36, 36, 36));
                }
            }
        }
    }

    public partial class MainWindow : Window {
        public static MinecraftData.MinecraftVersion MinecraftVersion = MinecraftData.MinecraftVersion.VERSIONS[5];
        public static bool[][] avail = MinecraftData.GetAvailability(MainWindow.MinecraftVersion);

        public static (int x, int y) ItemSelection = (5,6);
        public static int CurrentTab = 0;
        public static string Seed1, Seed2, FullSeed;

        public static CrackerTask SeedCracker = new CrackerTask();
        public static ItemFinderTask ItemFinder = new ItemFinderTask();
        public static VersionTask Version = new VersionTask();

        public IProgressiveTask ProgressTask = SeedCracker;

        private FixedPage GetTab(int tab) {
            if (tab == 0) return CrackingPage;
            if (tab == 1) return FindingPage;
            if (tab == 2) return InfoPage;
            return null;
        }

        public MainWindow() {
            InitializeComponent();

            Task.Run(() => {
                Debug.WriteLine("Running tester");
                
                long start = Environment.TickCount;
                int[] values = SeedCracker.GetSeeds(15, 7, 17, 30, null);
                Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                Debug.WriteLine($"Expected: 81788565 | Actual: {values.Length}");
                if (values.Length == 81788565) {
                    Debug.WriteLine("First run OK");
                    Task.Delay(2500).Wait();

                    start = Environment.TickCount;
                    int[] values2 = SeedCracker.GetSeeds(14, 7, 15, 28, values);
                    Debug.WriteLine($"Took {Environment.TickCount - start}ms");
                    Debug.WriteLine($"Expected: 2073151 | Actual: {values2.Length}");
                    if (values2.Length == 2073151) Debug.WriteLine("Second run OK");
                }
            });
            Dispatcher.InvokeAsync(async () => {
                bool success;
                SolidColorBrush red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightPink"));
                SolidColorBrush green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
                // sometimes errors when starting up - perhaps UI elements are null?
                while (true) {
                    try {
                        success = ProgressTask.Success;
                        float p = success ? ProgressTask.Progress : 1f;
                        ProgressBar.Value = p;
                        ProgressBar.Foreground = success ? green : red;
                        ProgressText.Text = ProgressTask.ProgressText;
                        ProgressPercent.Text = ProgressTask.ProgressText2;
                    }
                    catch (Exception) { }
                    await Task.Delay(50);
                }
            });
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                Point p = e.GetPosition(sender as IInputElement);
                if (p.Y < 30) this.DragMove();
                else if (p.Y < 58) {
                    int tab = (int)(p.X-1) / 166;
                    if (tab == 3) tab = 2; // last pixel
                    if (tab != CurrentTab) {
                        GetTab(CurrentTab).Visibility = Visibility.Hidden;
                        GetTab(tab).Visibility = Visibility.Visible;
                        CurrentTab = tab;
                        if (tab == 0) ProgressTask = SeedCracker;
                        else if (tab == 1) ProgressTask = ItemFinder;
                        else ProgressTask = Version;
                    }
                }
            }
            Keyboard.ClearFocus();
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e) {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                Point p = e.GetPosition(sender as IInputElement);
                (int x, int y) item = ((int)p.X / 36, (int)p.Y / 36);
                if (avail[item.y][item.x]) {
                    ItemSelection = item;
                    ItemCanvas.InvalidateVisual();
                }
            }
        }
    }
}
