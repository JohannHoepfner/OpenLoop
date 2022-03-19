namespace OpenLoopRun
{
	public class OpenLoopProgram
	{
		public long Iterations { get; set; }
		public List<String> StartCode { get; set; }
		public List<String> LoopCode { get; set; }

		public OpenLoopProgram()
		{
			Iterations = 1;
			StartCode = new List<String>();
			LoopCode = new List<String>();
		}
	}
}
