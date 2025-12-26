using System.Collections.Generic;

namespace PracticeTimer.Core
{

    public class Preset
    {
        public string Name { get; set; } = "";
        public List<Phase> Phases { get; set; } = new();
    }
}