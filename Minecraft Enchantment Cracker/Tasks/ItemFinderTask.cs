namespace Minecraft_Enchantment_Cracker.Tasks
{
    public class ItemFinderTask : IProgressiveTask {


        public float Progress => .5f;
        public string ProgressText => "No player seed";
        public string ProgressText2 => "";
        public bool Success => false;
    }
}
