using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Enchantment_Cracker
{
    public class VersionTask : IProgressiveTask {
        public static string VERTEXT = $"V{typeof(MainWindow).Assembly.GetName().Version}";

        public float Progress => 1;
        public string ProgressText => "Enchantment Cracker";
        public string ProgressText2 => VERTEXT;
        public bool Success => true;
    }
}
