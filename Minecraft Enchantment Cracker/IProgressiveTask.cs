namespace Minecraft_Enchantment_Cracker {
    public interface IProgressiveTask {
        float Progress { get; }
        string ProgressText { get; }
        string ProgressText2 { get; }
        bool Success { get; }
    }
}