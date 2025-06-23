namespace Minnor.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public string NavigationPropertyName { get; set; }

    public ForeignKeyAttribute(string navigationPropertyName)
    {
        NavigationPropertyName = navigationPropertyName;
    }
}
