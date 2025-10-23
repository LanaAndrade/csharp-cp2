using LojaApp.Models;
using System.Data;
using System.Data.SqlClient;

namespace LojaApp.DAO;

public class ProdutoDAO
{
    public List<Produto> GetAll()
    {
        const string sql = @"SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos ORDER BY Id";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        using var rd = cmd.ExecuteReader();
        var list = new List<Produto>();
        while (rd.Read())
        {
            list.Add(new Produto
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Preco = rd.GetDecimal(2),
                Estoque = rd.GetInt32(3),
                CategoriaId = rd.GetInt32(4)
            });
        }
        return list;
    }

    public Produto? GetById(int id)
    {
        const string sql = @"SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos WHERE Id=@id";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = id });
        using var rd = cmd.ExecuteReader();
        if (!rd.Read()) return null;
        return new Produto
        {
            Id = rd.GetInt32(0),
            Nome = rd.GetString(1),
            Preco = rd.GetDecimal(2),
            Estoque = rd.GetInt32(3),
            CategoriaId = rd.GetInt32(4)
        };
    }

    public int Insert(Produto p)
    {
        const string sql = @"
INSERT INTO Produtos (Nome, Preco, Estoque, CategoriaId)
VALUES (@nome, @preco, @estoque, @cat);
SELECT SCOPE_IDENTITY();";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nome", p.Nome);
        cmd.Parameters.AddWithValue("@preco", p.Preco);
        cmd.Parameters.AddWithValue("@estoque", p.Estoque);
        cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return id;
    }

    public bool Update(Produto p)
    {
        const string sql = @"
UPDATE Produtos SET Nome=@nome, Preco=@preco, Estoque=@estoque, CategoriaId=@cat
WHERE Id=@id";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@nome", p.Nome);
        cmd.Parameters.AddWithValue("@preco", p.Preco);
        cmd.Parameters.AddWithValue("@estoque", p.Estoque);
        cmd.Parameters.AddWithValue("@cat", p.CategoriaId);
        cmd.Parameters.AddWithValue("@id", p.Id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        const string sql = @"DELETE FROM Produtos WHERE Id=@id";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public List<(Produto produto, Categoria categoria)> GetByCategory(int categoriaId)
    {
        const string sql = @"
SELECT p.Id, p.Nome, p.Preco, p.Estoque, p.CategoriaId, c.Id, c.Nome
FROM Produtos p
JOIN Categorias c ON c.Id = p.CategoriaId
WHERE p.CategoriaId = @cat
ORDER BY p.Id;";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cat", categoriaId);
        using var rd = cmd.ExecuteReader();
        var list = new List<(Produto, Categoria)>();
        while (rd.Read())
        {
            var p = new Produto
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Preco = rd.GetDecimal(2),
                Estoque = rd.GetInt32(3),
                CategoriaId = rd.GetInt32(4)
            };
            var c = new Categoria
            {
                Id = rd.GetInt32(5),
                Nome = rd.GetString(6)
            };
            list.Add((p, c));
        }
        return list;
    }

    // Desafio: Estoque Baixo
    public List<Produto> GetLowStock(int threshold)
    {
        const string sql = @"SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos WHERE Estoque <= @t ORDER BY Estoque ASC";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@t", threshold);
        using var rd = cmd.ExecuteReader();
        var list = new List<Produto>();
        while (rd.Read())
        {
            list.Add(new Produto
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Preco = rd.GetDecimal(2),
                Estoque = rd.GetInt32(3),
                CategoriaId = rd.GetInt32(4)
            });
        }
        return list;
    }

    // Desafio: Busca por Nome (LIKE)
    public List<Produto> SearchByNameLike(string termo)
    {
        const string sql = @"SELECT Id, Nome, Preco, Estoque, CategoriaId FROM Produtos WHERE Nome LIKE @q ORDER BY Nome";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@q", "%" + termo + "%");
        using var rd = cmd.ExecuteReader();
        var list = new List<Produto>();
        while (rd.Read())
        {
            list.Add(new Produto
            {
                Id = rd.GetInt32(0),
                Nome = rd.GetString(1),
                Preco = rd.GetDecimal(2),
                Estoque = rd.GetInt32(3),
                CategoriaId = rd.GetInt32(4)
            });
        }
        return list;
    }

    // Utilitário para atualizar estoque após pedido
    public void DebitarEstoque(int produtoId, int quantidade, SqlConnection conn, SqlTransaction tx)
    {
        const string sql = @"UPDATE Produtos SET Estoque = Estoque - @q WHERE Id=@id AND Estoque >= @q";
        using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@q", quantidade);
        cmd.Parameters.AddWithValue("@id", produtoId);
        var rows = cmd.ExecuteNonQuery();
        if (rows == 0)
            throw new InvalidOperationException("Estoque insuficiente para o produto " + produtoId);
    }
}
