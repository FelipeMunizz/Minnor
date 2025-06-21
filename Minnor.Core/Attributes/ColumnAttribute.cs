namespace Minnor.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
internal class ColumnAttribute : Attribute
{
    #region Properties
    internal string Name { get; set; }
    #endregion

    #region Constructors
    internal ColumnAttribute(string name)
    {
        Name = name;
    }
    #endregion
}
