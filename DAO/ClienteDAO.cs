using LojaApp.Models;
using System.Data.SqlClient;

namespace LojaApp.DAO;

public class ClienteDAO
{
    public List<Cliente> GetAll()
    {
        const string sql = @"SELECT Id, Nome, Email FROM Clientes ORDER BY Nome";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var rd = cmd.ExecuteReader();
        var list = new List<Cliente>();
        while (rd.Read())
        {
            list.Add(new Cliente
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Email = rd.GetString(2)
            });
        }
        return list;
    }
}
