using Minnor.Core.Context;
using Minnor.Test.Entities;
using Minnor.Test.Helpers;

namespace Minnor.Test.Tests;

public class UserTest
{
    private readonly string _connectionString =
        "Data Source=DESKTOP-RVJ8D0O\\SQLEXPRESS;Initial Catalog=Minnor;Integrated Security=True;Pooling=False;Encrypt=False;TrustServerCertificate=False;";

    #region Select
    [Fact]
    public void GetAll_ShouldReturnUsers()
    {
        // Act
        var users = CreateContext()
            .Query<User>().ToList();

        // Assert
        Assert.NotNull(users);
        Assert.True(users.Count > 0);
    }

    [Fact]
    public void Where_ShouldFilterByNameAndEmail()
    {
        var result = CreateContext()
            .Query<User>()
            .Where(u => u.Nome == "Felipe Muniz" && u.Email == "lipe.baterra@gmail.com")
            .ToList();

        Assert.All(result, u => Assert.Equal("Felipe Muniz", u.Nome));
    }

    [Fact]
    public void OrderByDescending_ShouldReturnSorted()
    {
        var result = CreateContext()
            .Query<User>()
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
    #endregion

    #region Insert
    [Fact]
    public void Insert_ShouldAddUser()
    {
        var user = UsuarioGenerator.GerarUsuario();

        var result = CreateContext()
            .Insert<User>(user);

        if (result is not null && result.Id > 0)
        {
            Assert.Equal(user.Nome, result.Nome);
        }
        else
        {
            Assert.Fail("User was not inserted correctly.");
        }
    }
    #endregion

    private MiniOrmContext CreateContext()
    {
        return new MiniOrmContext(_connectionString);
    }
}
