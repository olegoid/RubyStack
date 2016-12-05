namespace RubyStack
{
	public interface IRubyExpressionResult
	{
		void Parse (string s);
	}

	public interface IRubyExpressionResult<T> : IRubyExpressionResult
	{
		T Result { get; }
	}
}
