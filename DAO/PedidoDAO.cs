using LojaApp.Models;
using System.Data;
using System.Data.SqlClient;

namespace LojaApp.DAO;

public class PedidoDAO
{
    private readonly ProdutoDAO produtoDAO = new();

    public int CreateOrder(int clienteId, List<PedidoItem> itens)
    {
        using var conn = Database.GetOpenConnection();
        using var tx = conn.BeginTransaction();

        try
        {
            // 1) Insere pedido
            const string sqlPedido = @"INSERT INTO Pedidos (ClienteId) VALUES (@cli); SELECT SCOPE_IDENTITY();";
            using var cmdPedido = new SqlCommand(sqlPedido, conn, tx);
            cmdPedido.Parameters.AddWithValue("@cli", clienteId);
            var pedidoId = Convert.ToInt32(cmdPedido.ExecuteScalar());

            // 2) Insere itens e debita estoque
            const string sqlItem = @"
INSERT INTO PedidoItens (PedidoId, ProdutoId, Quantidade, PrecoUnitario)
VALUES (@p, @prod, @q, @preco);";
            foreach (var item in itens)
            {
                // Debitar estoque primeiro (garantir integridade)
                produtoDAO.DebitarEstoque(item.ProdutoId, item.Quantidade, conn, tx);

                using var cmdItem = new SqlCommand(sqlItem, conn, tx);
                cmdItem.Parameters.AddWithValue("@p", pedidoId);
                cmdItem.Parameters.AddWithValue("@prod", item.ProdutoId);
                cmdItem.Parameters.AddWithValue("@q", item.Quantidade);
                cmdItem.Parameters.AddWithValue("@preco", item.PrecoUnitario);
                cmdItem.ExecuteNonQuery();
            }

            tx.Commit();
            return pedidoId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public List<Pedido> ListByCliente(int clienteId)
    {
        const string sql = @"
SELECT p.Id, p.ClienteId, p.DataPedido,
       SUM(pi.Quantidade * pi.PrecoUnitario) AS Total
FROM Pedidos p
LEFT JOIN PedidoItens pi ON pi.PedidoId = p.Id
WHERE p.ClienteId = @cli
GROUP BY p.Id, p.ClienteId, p.DataPedido
ORDER BY p.DataPedido DESC;";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cli", clienteId);
        using var rd = cmd.ExecuteReader();
        var list = new List<Pedido>();
        while (rd.Read())
        {
            list.Add(new Pedido
            {
                Id = rd.GetInt32(0),
                ClienteId = rd.GetInt32(1),
                DataPedido = rd.GetDateTime(2),
                Total = rd.IsDBNull(3) ? 0m : rd.GetDecimal(3)
            });
        }
        return list;
    }

    public (List<(string ProdutoNome, int Quantidade, decimal PrecoUnitario, decimal TotalItem)> Itens, decimal TotalGeral)
        GetDetalhesPedido(int pedidoId)
    {
        const string sql = @"
SELECT pr.Nome, pi.Quantidade, pi.PrecoUnitario, (pi.Quantidade * pi.PrecoUnitario) AS TotalItem
FROM PedidoItens pi
JOIN Produtos pr ON pr.Id = pi.ProdutoId
WHERE pi.PedidoId = @p
ORDER BY pr.Nome;";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@p", pedidoId);
        using var rd = cmd.ExecuteReader();
        var itens = new List<(string, int, decimal, decimal)>();
        decimal total = 0;
        while (rd.Read())
        {
            var nome = rd.GetString(0);
            var qtd = rd.GetInt32(1);
            var preco = rd.GetDecimal(2);
            var totalItem = rd.GetDecimal(3);
            itens.Add((nome, qtd, preco, totalItem));
            total += totalItem;
        }
        return (itens, total);
    }

    // Desafio: total vendido por período
    public decimal TotalVendidoPeriodo(DateTime inicio, DateTime fim)
    {
        const string sql = @"
SELECT SUM(pi.Quantidade * pi.PrecoUnitario) AS Total
FROM Pedidos p
JOIN PedidoItens pi ON pi.PedidoId = p.Id
WHERE p.DataPedido >= @ini AND p.DataPedido < DATEADD(DAY, 1, @fim);";
        using var conn = Database.GetOpenConnection();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@ini", SqlDbType.DateTime) { Value = inicio });
        cmd.Parameters.Add(new SqlParameter("@fim", SqlDbType.DateTime) { Value = fim });
        var result = cmd.ExecuteScalar();
        return result == DBNull.Value || result == null ? 0m : Convert.ToDecimal(result);
    }
}
