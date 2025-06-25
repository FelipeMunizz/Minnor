# Minnor.Core

**Minnor.Core** é um micro ORM (Object-Relational Mapper) minimalista e leve para .NET, projetado para fornecer uma camada simples e expressiva de mapeamento entre objetos C# e bancos de dados relacionais. Ideal para quem busca controle direto sobre SQL, mas com uma estrutura mais organizada que o `Dapper` puro.

---

## 🆕 Versão 1.0.5 - Novidade: Suporte a Injeção de Dependências

### ✨ O que há de novo?

- ✅ Suporte completo à **injeção de dependência (DI)** com `IServiceCollection`
- ✅ Registro fluente via `AddMinnor(...)`
- ✅ Facilita o uso do `MinnorContext` em projetos ASP.NET Core, Worker Services e outros projetos baseados em DI.
- ✅ Suporte completo à **injeção de dependência (DI)** com `IServiceCollection`
- ✅ Registro fluente via `AddMinnor(...)`
- ✅ Facilita o uso do `MinnorContext` em projetos ASP.NET Core, Worker Services e outros projetos baseados em DI.

### Exemplo de uso

Para registrar o `MinnorContext` no seu projeto, basta adicionar o seguinte código no método `ConfigureServices` da sua classe `Startup`:
```csharp
builder.Services.AddMinnor(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
});
```
Na sua classe de serviço:

```csharp
public class ClienteService
{
    private readonly MinnorContext _context;

    public ClienteService(MinnorContext context)
    {
        _context = context;
    }

    public void CriarCliente()
    {
        var cliente = new Cliente { Nome = "Maria" };
        _context.Insert(cliente);
    }
}
```

---

## 🆕 Versão 1.0.4 - Novidade: Suporte a Queries SQL Personalizadas

### ✨ O que há de novo?

A nova versão do **Minnor** agora permite executar **queries SQL personalizadas** diretamente no ORM. Com isso, você tem total controle sobre a consulta executada, o que é ideal para:

- Consultas complexas
- Uso de `JOIN`, `GROUP BY`, `HAVING`, `ORDER BY`
- Subqueries e CTEs
- Chamadas a **views** e **stored procedures**
- Projeções específicas em DTOs ou tipos anônimos

---

### ✅ Exemplo de uso

Você pode fornecer a query completa como string e informar o tipo de retorno esperado (entidade ou DTO):

```csharp
var resultado = context.Query<PedidoDTO>().CustomQuery(
    "SELECT p.Id, p.Data, c.Nome AS ClienteNome " +
    "FROM Pedidos p INNER JOIN Clientes c ON c.Id = p.ClienteId"
).ToList();

public class PedidoDTO
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string ClienteNome { get; set; }
}
```
### ⚙️ Como funciona?
- O método Query<T>(string sql) executa a string SQL diretamente.

- O ORM faz o mapeamento automático dos resultados para o tipo informado (T).

- Ideal para retornos personalizados fora do modelo padrão das entidades mapeadas.

---

## 🆕 Versão 1.0.3 - Novidade: Suporte a `Includes` nos Selects

### ✨ O que há de novo?

A nova versão do Minnor agora oferece suporte à funcionalidade de `Includes`, permitindo carregar entidades relacionadas em uma única consulta, simplificando o mapeamento de relações entre entidades (por exemplo, 1:1, 1:N e N:N).

Essa funcionalidade é especialmente útil para cenários onde você deseja evitar múltiplas chamadas ao banco de dados ao trabalhar com relacionamentos.

---

### ✅ Exemplo de uso

#### Suponha que você tenha as seguintes classes:

```csharp
public class User
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Senha { get; set; }

    public string Nome { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}

public class Document
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; }
    [ForeignKey("User")]
    public int UserId { get; set; }
    public virtual User User { get; set; }
}

```
Agora você pode fazer:

```csharp
var users = orm.Query<User>()
                   .Include(d => d.Documents)
                   .ToList();
```

Essa chamada irá automaticamente incluir os Documentos para cada Usuario retornado.

### ⚙️ Como funciona?
O método Include permite que você especifique propriedades de navegação que devem ser carregadas junto com a entidade principal.

A funcionalidade utiliza internamente `JOINs` automáticos baseados nos atributos de chave estrangeira definidos na sua modelagem de classes.

---

## 🚀 Principais Recursos

- 🔍 Consultas SQL simplificadas com mapeamento automático
- 🧩 Suporte a expressões lambda
- 🧠 Convenções inteligentes para inferência de chaves primárias
- 🏗️ Arquitetura modular e extensível

---

## 📦 Instalação

Adicione o pacote ao seu projeto .NET:

Install-Package Minnor.Core
