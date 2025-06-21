namespace Minnor.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class TableAttribute : Attribute
{
    #region Properties
    internal string Name { get; set; }
    #endregion

    #region Constructors
    internal TableAttribute(string name)
    {
        Name = name;
    }
    #endregion
}
