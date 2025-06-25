using Minnor.Core.Context;
using Minnor.Core.Extensions;
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

    [Fact]
    public void Include_ShouldLoadRelatedDocuments()
    {
        var result = CreateContext()
            .Query<User>()
            .Where(u => u.Id == 2)
            .Include(u => u.Documents)
            .ToList();

        Assert.NotNull(result);
        Assert.All(result, u => Assert.NotEmpty(u.Documents));
    }

    [Fact]
    public void CustomQuery_ShouldReturnUsersWithCustomSql()
    {
        var sql = "SELECT [Id], [Nome] FROM [User] WHERE [Id] = 2";
        var result = CreateContext()
            .Query<UserDTO>()
            .CustomQuery(sql);

        Assert.NotNull(result);
        Assert.True(result.Count > 0);
    }
    #endregion

    #region Insert
    [Fact]
    public void Insert_ShouldAddUser()
    {
        var user = UsuarioGenerator.GerarUsuario();

        var result = CreateContext()
            .Insert<User>(user);

        Assert.True(result is not null && result.Id > 0);
    }
    #endregion

    #region Update
    [Fact]
    public void Update_ShouldModifyUser()
    {
        var user = CreateContext()
            .Query<User>()
            .Where(u => u.Id == 1)
            .ToList()
            .FirstOrDefault();

        if (user is null)
        {
            Assert.Fail("User not found for update test.");
            return;
        }

        user.Nome = UsuarioGenerator.GerarUsuario().Nome;
        var result = CreateContext()
            .Update<User>(user);

        Assert.True(result is not null && result.Id > 0);
        Assert.Equal(user.Nome, result.Nome);
    }
    #endregion

    #region Delete
    [Fact]
    public void Delete_ShouldRemoveUser()
    {
        var user = CreateContext()
            .Query<User>()
            .ToList();
        if (user is null)
        {
            Assert.Fail("User not found for delete test.");
            return;
        }
        var result = CreateContext()
            .Delete<User>(user[user.Count - 1]);

        Assert.True(result);
    }
    #endregion

    private MinnorContext CreateContext() => 
        new MinnorContext(_connectionString);
    
}
