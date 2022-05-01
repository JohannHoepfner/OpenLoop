using System;
using System.Collections.Generic;
using System.Linq;
using Flee.PublicTypes;

namespace OpenLoopRun
{
    public partial class Runner
    {
        public IDictionary<string, List<double>> VarHistoryT
        {
            get
            {
                var res = new Dictionary<string, List<double>>();
                foreach (var (key, value) in VarHistory.SelectMany(t => t))
                {
                    if (!res.ContainsKey(key))
                    {
                        res.Add(key, new List<double>());
                    }

                    res[key].Add(value);
                }

                return res;
            }
        }
        
        public OpenLoopScript Script { get; init; }
        public ICollection<IDictionary<string, double>> VarHistory { get; }
        
        private readonly ExpressionContext _context;

        /// <summary>
        /// Sets parser options and opens new OpenLoopScript class.
        /// </summary>
        public Runner()
        {
            _context = new ExpressionContext();

            _context.Imports.AddType(typeof(Math));
            _context.ParserOptions.DecimalSeparator = '.';
            _context.ParserOptions.RecreateParser();

            VarHistory = new List<IDictionary<string, double>>();
            Script = new OpenLoopScript();
        }

        /// <summary>
        /// Runs start-script once and loop-script every time-step
        /// </summary>
        public void RunScript()
        {
            foreach (var line in Script.StartCode)
                RunLine(new ScriptLineSrc(line));

            foreach (var line in Script.LoopCode)
                RunLine(new ScriptLineSrc(line));

            UpdateVarHistory(_context);

            var loopCodeCompiled
                = Script.LoopCode
                    .Select(line => new ScriptLineBin((string) line, _context))
                    .ToList();

            for (var i = 0; i < Script.Iterations; i++)
            {
                foreach (var line in loopCodeCompiled)
                {
                    RunLine((ScriptLineBin) line);
                }

                UpdateVarHistory(_context);
            }
        }

        /// <summary>
        /// Compiles line and executes it once.
        /// </summary>
        /// <param name="line"></param>
        private void RunLine(ScriptLineSrc line)
        {
            var lineCompiled = new ScriptLineBin(line, _context);
            RunLine(lineCompiled);
        }

        /// <summary>
        /// Execute line once
        /// </summary>
        /// <param name="line"></param>
        private void RunLine(ScriptLineBin line)
        {
            _context.Variables[line.VariableToStore] = line.ExpressionToEval.Evaluate();
        }

        /// <summary>
        /// renews vars per time-step
        /// </summary>
        /// <param name="context"></param>
        private void UpdateVarHistory(ExpressionContext context)
        {
            var dict =
                context.Variables.Keys.ToDictionary(
                    key => key,
                    key => (double)context.Variables[key]
                );
            VarHistory.Add(dict);
        }
    }
}