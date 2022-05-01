using Flee.PublicTypes;

namespace OpenLoopRun;

public partial class Runner
{
    private class ScriptLineBin
    {
        public ScriptLineBin(ScriptLineSrc line, ExpressionContext context)
        {
            VariableToStore = line.VariableToStore;
            ExpressionToEval = context.CompileGeneric<double>(line.ExpressionToEval);
        }

        public ScriptLineBin(string src, ExpressionContext context)
        {
            var line = new ScriptLineSrc(src);
            VariableToStore = line.VariableToStore;
            
            ExpressionToEval = context.CompileGeneric<double>(line.ExpressionToEval);
        }

        public string VariableToStore { get; }
        public IGenericExpression<double> ExpressionToEval { get; }
    }
}