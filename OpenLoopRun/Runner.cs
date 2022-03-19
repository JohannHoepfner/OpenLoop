using Flee.PublicTypes;

namespace OpenLoopRun
{
	public class Runner
	{
		public OpenLoopProgram Program { get; set; }

		readonly ExpressionContext context;

		public List<Dictionary<string, double>> VarHistory { get; private set; }
		public Dictionary<string, List<double>> VarHistoryT()
		{
			var res = new Dictionary<string, List<double>>();
			for (int i = 0; i < VarHistory.Count; i++)
			{
				foreach (var d in VarHistory[i])
				{
					if (!res.ContainsKey(d.Key))
					{
						res.Add(d.Key, new List<double>());
					}
					res[d.Key].Add(d.Value);

				}
			}
			return res;
		}
		public Runner()
		{
			context = new ExpressionContext();
			context.Imports.AddType(typeof(Math));
			context.ParserOptions.DecimalSeparator = '.';
			context.ParserOptions.RecreateParser();
			VarHistory = new();
			Program = new();
		}

		public void RunLine(string line)
		{
			var leftSide = line[..line.IndexOf('=')];
			var rightSide = line[(line.IndexOf('=') + 1)..];
			IGenericExpression<double> rightTerm = context.CompileGeneric<double>(rightSide);
			context.Variables[leftSide] = rightTerm.Evaluate();
		}
		public void Start()
		{
			foreach (var line in Program.StartCode)
			{
				RunLine(line);
			}
		}
		public void Step()
		{
			foreach (var line in Program.LoopCode)
			{
				RunLine(line);
			}
			var dict = new Dictionary<string, double>();

			foreach (var key in context.Variables.Keys)
			{
				dict.Add(key, (double)context.Variables[key]);
			}
			VarHistory.Add(dict);
		}
	}
}
