using System.Collections.Generic;

namespace OpenLoopRun
{
    public class OpenLoopScript
    {
        public long Iterations { get; set; }
        public IEnumerable<string> StartCode { get; set; }
        public IEnumerable<string> LoopCode { get; set; }

        public OpenLoopScript()
        {
            Iterations = 1;
            StartCode = new List<string>();
            LoopCode = new List<string>();
        }
    }
}