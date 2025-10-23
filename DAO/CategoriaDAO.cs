using LojaApp.Models;
using System.Data.SqlClient;

namespace LojaApp.DAO;

public class CategoriaDAO
{
    public List<Categoria> GetAll()
    {
        const string sql = @"SELECT Id, Nome FROM Categorias ORDER BY Nome";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var rd = cmd.ExecuteReader();
        var list = new List<Categoria>();
        while (rd.Read())
        {
            list.Add(new Categoria
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1)
            });
        }
        return list;
    }
}
