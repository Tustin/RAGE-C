namespace RAGE.Parser.Globals
{
	/// <summary>
	/// Each Global item will have an Id to indicate the order it is used in the global call.
	/// </summary>
	public interface IGlobal
	{
		int Id { get; set; }
		int Index { get; set; }

	}
}