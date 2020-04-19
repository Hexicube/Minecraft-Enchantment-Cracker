using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft_Enchantment_Cracker
{
    public class ItemFinderTask : IProgressiveTask {


        public float Progress => .5f;
        public string ProgressText => "No player seed";
        public string ProgressText2 => "";
        public bool Success => false;
    }
}
