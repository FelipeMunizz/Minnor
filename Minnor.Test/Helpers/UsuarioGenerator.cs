using Minnor.Test.Entities;
using System.Text;

namespace Minnor.Test.Helpers;

public static class UsuarioGenerator
{
    private static readonly string[] Nomes = { "Ana", "Bruno", "Carlos", "Daniela", "Eduardo", "Fernanda", "Gabriel", "Helena", "Igor", "Juliana" };
    private static readonly string[] Sobrenomes = { "Silva", "Souza", "Oliveira", "Pereira", "Lima", "Costa", "Almeida", "Nascimento" };
    private static readonly string[] Dominios = { "gmail.com", "outlook.com", "yahoo.com", "teste.com" };

    private static readonly Random random = new();

    public static User GerarUsuario()
    {
        string nome = $"{Nomes[random.Next(Nomes.Length)]} {Sobrenomes[random.Next(Sobrenomes.Length)]}";
        string emailUser = nome.ToLower().Replace(" ", ".") + random.Next(100, 999);
        string email = $"{emailUser}@{Dominios[random.Next(Dominios.Length)]}";
        string senha = GerarSenha(12); // senha com 12 caracteres

        return new User
        {
            Nome = nome,
            Email = email,
            Senha = senha
        };
    }

    private static string GerarSenha(int comprimento)
    {
        const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*";
        StringBuilder sb = new();
        for (int i = 0; i < comprimento; i++)
        {
            sb.Append(caracteres[random.Next(caracteres.Length)]);
        }
        return sb.ToString();
    }
}
