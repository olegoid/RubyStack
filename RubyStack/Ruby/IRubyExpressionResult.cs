namespace RubyStack
{
	public interface IRubyExpressionResult
	{
		bool Parse (string s);
	}

	public interface IRubyExpressionResult<T> : IRubyExpressionResult
	{
		T Result { get; }
	}
}
