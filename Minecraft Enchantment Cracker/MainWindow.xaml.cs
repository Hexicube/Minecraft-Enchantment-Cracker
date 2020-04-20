using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        public static TextBox[][] shelves = new TextBox[16][];

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
            
            shelves[0] = new TextBox[] { Shelf0Slot1, Shelf0Slot2, Shelf0Slot3 };
            shelves[1] = new TextBox[] { Shelf1Slot1, Shelf1Slot2, Shelf1Slot3 };
            shelves[2] = new TextBox[] { Shelf2Slot1, Shelf2Slot2, Shelf2Slot3 };
            shelves[3] = new TextBox[] { Shelf3Slot1, Shelf3Slot2, Shelf3Slot3 };
            shelves[4] = new TextBox[] { Shelf4Slot1, Shelf4Slot2, Shelf4Slot3 };
            shelves[5] = new TextBox[] { Shelf5Slot1, Shelf5Slot2, Shelf5Slot3 };
            shelves[6] = new TextBox[] { Shelf6Slot1, Shelf6Slot2, Shelf6Slot3 };
            shelves[7] = new TextBox[] { Shelf7Slot1, Shelf7Slot2, Shelf7Slot3 };
            shelves[8] = new TextBox[] { Shelf8Slot1, Shelf8Slot2, Shelf8Slot3 };
            shelves[9] = new TextBox[] { Shelf9Slot1, Shelf9Slot2, Shelf9Slot3 };
            shelves[10] = new TextBox[] { Shelf10Slot1, Shelf10Slot2, Shelf10Slot3 };
            shelves[11] = new TextBox[] { Shelf11Slot1, Shelf11Slot2, Shelf11Slot3 };
            shelves[12] = new TextBox[] { Shelf12Slot1, Shelf12Slot2, Shelf12Slot3 };
            shelves[13] = new TextBox[] { Shelf13Slot1, Shelf13Slot2, Shelf13Slot3 };
            shelves[14] = new TextBox[] { Shelf14Slot1, Shelf14Slot2, Shelf14Slot3 };
            shelves[15] = new TextBox[] { Shelf15Slot1, Shelf15Slot2, Shelf15Slot3 };

            Task.Run(() => {
                Task.Delay(2500).Wait();
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
                while (true) {
                    success = ProgressTask.Success;
                    float p = success ? ProgressTask.Progress : 1f;
                    if (float.IsNaN(p)) p = 0f;
                    ProgressBar.Value = p;
                    ProgressBar.Foreground = success ? green : red;
                    ProgressText.Text = ProgressTask.ProgressText;
                    ProgressPercent.Text = ProgressTask.ProgressText2;
                    await Task.Delay(10);
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

        public static bool IsNumeric(string text) {
            return text.All(c => Char.IsDigit(c));
        }
        
        private void TextEntryNumeric(object sender, TextCompositionEventArgs e) {
            e.Handled = !IsNumeric(e.Text);
        }

        private void TextPasteNumeric(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(string))) {
                if (!IsNumeric((string)e.DataObject.GetData(typeof(string)))) e.CancelCommand();
            }
            else e.CancelCommand();
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
