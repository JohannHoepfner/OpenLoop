using System.Collections.Generic;

namespace OpenLoopRun
{
    public class OpenLoopScript
    {
        /// <summary>
        /// Number of iterations loop() is executed
        /// </summary>
        public long Iterations { get; set; }
        
        /// <summary>
        /// Script containing initial values
        /// </summary>
        public IEnumerable<string> StartCode { get; set; }
        
        /// <summary>
        /// Script executed every time-step
        /// </summary>
        public IEnumerable<string> LoopCode { get; set; }

        public OpenLoopScript()
        {
            Iterations = 1;
            StartCode = new List<string>();
            LoopCode = new List<string>();
        }
    }
}