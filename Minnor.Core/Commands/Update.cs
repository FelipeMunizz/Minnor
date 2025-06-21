namespace Minnor.Core.Commands;

public class Update<T> where T : class, new()
{
	#region Properties
	private readonly string _connectionString;
    #endregion

    #region Constructors
    public Update(string connectionString)
    {
        _connectionString = connectionString;
    }
    #endregion

    #region Methods

    #endregion
}
