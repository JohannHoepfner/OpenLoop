namespace OpenLoopRun;

public partial class Runner
{
    private class ScriptLineSrc
    {
        private static (string left, string right) SplitEq(string line) => (
            left: (line[..line.IndexOf('=')]).Trim(),
            right: (line[(line.IndexOf('=') + 1)..]).Trim()
        );

        public ScriptLineSrc(string src)
        {
            (VariableToStore, ExpressionToEval) = SplitEq(src);
        }

        public string VariableToStore { get; set; }
        public string ExpressionToEval { get; set; }
    }
}