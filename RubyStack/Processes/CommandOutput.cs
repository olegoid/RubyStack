namespace RubyStack
{
	public class CommandOutput
	{
		public long OffsetMilliseconds { get; private set; }

		public string Data { get; private set; }

		public CommandOutput (string data, long offsetMilliseconds)
		{
			Data = data;
			OffsetMilliseconds = offsetMilliseconds;
		}
	}
}
