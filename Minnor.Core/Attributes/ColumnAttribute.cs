namespace Minnor.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute : Attribute
{
    #region Properties
    public string Name { get; set; }
    #endregion

    #region Constructors
    public ColumnAttribute(string name)
    {
        Name = name;
    }
    #endregion
}
