namespace Minecraft_Enchantment_Cracker.Tasks
{
    public class VersionTask : IProgressiveTask {
        public static string VERTEXT = $"V{typeof(MainWindow).Assembly.GetName().Version}";

        public float Progress => 1;
        public string ProgressText => "Enchantment Cracker";
        public string ProgressText2 => VERTEXT;
        public bool Success => true;
    }
}
