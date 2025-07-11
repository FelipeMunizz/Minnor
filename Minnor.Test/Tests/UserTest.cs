using Minnor.Core.Context;
using Minnor.Core.Extensions.ContextExtension;
using Minnor.Core.Extensions.SelectExtension;
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

    [Fact]
    public void FirstOrDefault_ShouldReturnSingleUser()
    {
        var user = CreateContext()
            .Query<User>()
            .FirstOrDefault(u => u.Id == 2);

        var user1 = CreateContext()
            .Query<User>()
            .FirstOrDefault();

        Assert.NotNull(user);
        Assert.NotNull(user1);
        Assert.Equal(1, user1.Id);
        Assert.Equal(2, user.Id);
    }

    [Fact]
    public void Pagination_ShouldReturnPagedResults()
    {
        var pageIndex = 0;
        var pageSize = 2;

        var pagedUsers = CreateContext()
            .Query<User>()
            .Page(pageIndex, pageSize)
            .ToList();

        Assert.NotNull(pagedUsers);
        Assert.True(pagedUsers.Count <= pageSize);
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

    [Fact]
    public void Insert_WithTransaction_ShouldCommit()
    {
        var user = UsuarioGenerator.GerarUsuario();

        using var ctx = CreateContext();
        ctx.BeginTransaction();

        var result = ctx.Insert(user);

        ctx.Commit();

        var inserted = CreateContext()
            .Query<User>()
            .Where(u => u.Id == result.Id)
            .ToList()
            .FirstOrDefault();

        Assert.NotNull(inserted);
        Assert.Equal(user.Nome, inserted.Nome);
    }

    [Fact]
    public void Insert_WithTransaction_ShouldRollback()
    {
        var user = UsuarioGenerator.GerarUsuario();

        using var ctx = CreateContext();
        ctx.BeginTransaction();

        var result = ctx.Insert(user);

        ctx.Rollback(); // <- Não deve persistir

        var fetched = CreateContext()
            .Query<User>()
            .Where(u => u.Id == result.Id)
            .ToList()
            .FirstOrDefault();

        Assert.Null(fetched); // Não foi persistido
    }

    [Fact]
    public void InsertRange_ShouldAddMultipleUsers()
    {
        var users = new List<User>
        {
            UsuarioGenerator.GerarUsuario(),
            UsuarioGenerator.GerarUsuario(),
            UsuarioGenerator.GerarUsuario()
        };
        var results = CreateContext().InsertRange<User>(users);

        Assert.NotNull(results);
        Assert.Equal(users.Count, results.Count());
        Assert.All(results, u => Assert.True(u.Id > 0));
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

    [Fact]
    public void Update_WithTransaction_ShouldCommit()
    {
        using var ctx = CreateContext();
        var user = ctx.Query<User>().FirstOrDefault();
        Assert.NotNull(user);

        var novoNome = UsuarioGenerator.GerarUsuario().Nome;

        ctx.BeginTransaction();
        user.Nome = novoNome;
        ctx.Update(user);
        ctx.Commit();

        var updated = CreateContext().Query<User>().Where(u => u.Id == user.Id).ToList().FirstOrDefault();

        Assert.NotNull(updated);
        Assert.Equal(novoNome, updated.Nome);
    }

    [Fact]
    public void Update_WithTransaction_ShouldRollback()
    {
        using var ctx = CreateContext();
        var user = ctx.Query<User>().FirstOrDefault();
        Assert.NotNull(user);

        var originalNome = user.Nome;
        var novoNome = UsuarioGenerator.GerarUsuario().Nome;

        ctx.BeginTransaction();
        user.Nome = novoNome;
        ctx.Update(user);
        ctx.Rollback();

        var reverted = CreateContext().Query<User>().Where(u => u.Id == user.Id).ToList().FirstOrDefault();

        Assert.NotNull(reverted);
        Assert.Equal(originalNome, reverted.Nome);
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


    [Fact]
    public void Delete_WithTransaction_ShouldCommit()
    {
        var ctx = CreateContext();
        var user = ctx.Query<User>().ToList().LastOrDefault();
        Assert.NotNull(user);

        ctx.BeginTransaction();
        var result = ctx.Delete(user);
        ctx.Commit();

        var deleted = CreateContext().Query<User>().Where(u => u.Id == user.Id).ToList().FirstOrDefault();
        Assert.True(result);
        Assert.Null(deleted);
    }

    [Fact]
    public void Delete_WithTransaction_ShouldRollback()
    {
        var ctx = CreateContext();
        var user = ctx.Query<User>().ToList().LastOrDefault();
        Assert.NotNull(user);

        ctx.BeginTransaction();
        ctx.Delete(user);
        ctx.Rollback();

        var recovered = CreateContext().Query<User>().Where(u => u.Id == user.Id).ToList().FirstOrDefault();
        Assert.NotNull(recovered); // Continua existindo
    }
    #endregion

    private MinnorContext CreateContext() => 
        new MinnorContext(_connectionString);
    
}
