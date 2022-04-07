﻿using Flee.PublicTypes;

namespace OpenLoopRun
{
	public class Runner
	{
		public OpenLoopScript Script { get; set; }
		readonly ExpressionContext context;

		public Runner()
		{
			context = new ExpressionContext();
			context.Imports.AddType(typeof(Math));
			context.ParserOptions.DecimalSeparator = '.';
			context.ParserOptions.RecreateParser();
			VarHistory = new();
			Script = new();
		}

		public void RunScript()
		{
			foreach (var line in Script.StartCode)
				RunLine(new ScriptLineSRC(line));
			foreach (var line in Script.LoopCode)
				RunLine(new ScriptLineSRC(line));
			UpdateVarHistory(context);
			var LoopCodeCompiled =
				Script.LoopCode.Select(
					line => new ScriptLineBIN(line, context)
					).ToList();
			for (var i = 0; i < Script.Iterations; i++)
			{
				foreach (var line in LoopCodeCompiled)
				{
					RunLine(line);
				}
				UpdateVarHistory(context);
			}
		}

		void RunLine(ScriptLineSRC line)
		{
			var LineCompiled = new ScriptLineBIN(line, context);
			RunLine(LineCompiled);
		}
		void RunLine(ScriptLineBIN line)
		{
			context.Variables[line.VariableToStore] = line.ExpressionToEval.Evaluate();
		}

		class ScriptLineSRC
		{
			static (string left, string right) SplitEQ(string line) => (
					left: (line[..line.IndexOf('=')]).Trim(),
					right: (line[(line.IndexOf('=') + 1)..]).Trim()
					);
			public ScriptLineSRC(string src)
			{
				(VariableToStore, ExpressionToEval) = SplitEQ(src);
			}
			public string VariableToStore { get; set; }
			public string ExpressionToEval { get; set; }
		}
		class ScriptLineBIN
		{
			public ScriptLineBIN(ScriptLineSRC line, ExpressionContext context)
			{
				VariableToStore = line.VariableToStore;
				ExpressionToEval = context.CompileGeneric<double>(line.ExpressionToEval);
			}
			public ScriptLineBIN(string src, ExpressionContext context)
			{
				var line = new ScriptLineSRC(src);
				VariableToStore = line.VariableToStore;
				ExpressionToEval = context.CompileGeneric<double>(line.ExpressionToEval);
			}
			public string VariableToStore { get; set; }
			public IGenericExpression<double> ExpressionToEval { get; set; }
		}

		void UpdateVarHistory(ExpressionContext context)
		{
			var dict =
				context.Variables.Keys.ToDictionary(
					key => key,
					key => (double)context.Variables[key]
					);
			VarHistory.Add(dict);
		}
		public List<Dictionary<string, double>> VarHistory { get; private set; }
		public Dictionary<string, List<double>> VarHistoryT
		{
			get
			{
				var res = new Dictionary<string, List<double>>();
				foreach (var d in VarHistory.SelectMany(t => t))
				{
					if (!res.ContainsKey(d.Key))
					{
						res.Add(d.Key, new List<double>());
					}
					res[d.Key].Add(d.Value);
				}
				return res;
			}
		}
	}
}