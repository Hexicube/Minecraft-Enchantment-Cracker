namespace Minecraft_Enchantment_Cracker.Tasks {
    public interface IProgressiveTask {
        float Progress { get; }
        string ProgressText { get; }
        string ProgressText2 { get; }
        bool Success { get; }
    }
}