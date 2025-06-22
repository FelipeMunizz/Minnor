namespace Minnor.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute : Attribute
{
    #region Properties
    public string Name { get; set; }
    #endregion

    #region Constructors
    public TableAttribute(string name)
    {
        Name = name;
    }
    #endregion
}
