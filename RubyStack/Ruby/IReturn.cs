namespace RubyStack
{
	/// <summary>
	/// Common interface for results of ruby expression execution.
	/// </summary>
	public interface IReturn { }

	/// <summary>
	/// Use this interface to specify class which handles results of ruby expression execution.
	/// </summary>
	public interface IReturn<T> : IReturn { }

	/// <summary>
	/// Use this interface if you're not interested in ruby expression result.
	/// </summary>
	public interface IReturnVoid : IReturn { }
}
