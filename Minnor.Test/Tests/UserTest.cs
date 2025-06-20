using Minnor.Core.Context;
using Minnor.Test.Entities;

namespace Minnor.Test.Tests;

public class UserTest
{
    private readonly string _connectionString =
        "Data Source=DESKTOP-RVJ8D0O\\SQLEXPRESS;Initial Catalog=NineShed;Integrated Security=True;Pooling=False;Encrypt=False;TrustServerCertificate=False;";

    [Fact]
    public void GetAll_ShouldReturnUsers()
    {
        // Arrange
        var context = new MiniOrmContext(_connectionString);

        // Act
        var users = context.Query<User>().ToList();

        // Assert
        Assert.NotNull(users);
        Assert.True(users.Count > 0);
    }

    [Fact]
    public void Where_ShouldFilterByNameAndEmail()
    {
        var context = new MiniOrmContext(_connectionString);

        var result = context.Query<User>()
            .Where(u => u.Nome == "Felipe Muniz" && u.Email == "lipe.baterra@gmail.com")
            .ToList();

        Assert.All(result, u => Assert.Equal("Felipe Muniz", u.Nome));
    }

    [Fact]
    public void OrderByDescending_ShouldReturnSorted()
    {
        var context = new MiniOrmContext(_connectionString);

        var result = context.Query<User>()
            .OrderBy(u => u.Nome, true)
            .ToList();

        if (result.Count >= 2)
        {
            Assert.True(result[0].Id >= result[1].Id);
        }
        else
        {
            Assert.True(result.Count > 0);
        }
    }
}
