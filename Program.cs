using LojaApp.DAO;
using LojaApp.Models;
using System.Globalization;

namespace LojaApp;

internal class Program
{
    private static readonly ProdutoDAO produtoDAO = new();
    private static readonly CategoriaDAO categoriaDAO = new();
    private static readonly ClienteDAO clienteDAO = new();
    private static readonly PedidoDAO pedidoDAO = new();

    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        while (true)
        {
            Console.WriteLine("\n=== LojaApp (SQL Server + ADO.NET) ===");
            Console.WriteLine("1) Listar produtos");
            Console.WriteLine("2) Inserir produto");
            Console.WriteLine("3) Atualizar produto");
            Console.WriteLine("4) Deletar produto");
            Console.WriteLine("5) Buscar produto por ID");
            Console.WriteLine("6) Listar produtos por categoria (JOIN)");
            Console.WriteLine("7) Listar pedidos de um cliente");
            Console.WriteLine("8) Criar pedido com transação");
            Console.WriteLine("9) Desafios: Estoque Baixo / Busca por Nome / Vendas por período");
            Console.WriteLine("0) Sair");
            Console.Write("Escolha: ");
            var opt = Console.ReadLine();

            try
            {
                switch (opt)
                {
                    case "1": ListarProdutos(); break;
                    case "2": InserirProduto(); break;
                    case "3": AtualizarProduto(); break;
                    case "4": DeletarProduto(); break;
                    case "5": BuscarPorId(); break;
                    case "6": ListarPorCategoria(); break;
                    case "7": ListarPedidosDeCliente(); break;
                    case "8": CriarPedidoTransacao(); break;
                    case "9": MenuDesafios(); break;
                    case "0": return;
                    default: Console.WriteLine("Opção inválida."); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }

    static void ListarProdutos()
    {
        var produtos = produtoDAO.GetAll();
        Console.WriteLine("\n--- Produtos ---");
        foreach (var p in produtos)
            Console.WriteLine($"{p.Id} | {p.Nome} | R${p.Preco:0.00} | Estoque: {p.Estoque} | CatId: {p.CategoriaId}");
    }

    static void InserirProduto()
    {
        Console.Write("Nome: ");
        var nome = Console.ReadLine() ?? "";

        Console.Write("Preço: ");
        var preco = decimal.Parse(Console.ReadLine() ?? "0");

        Console.Write("Estoque: ");
        var estoque = int.Parse(Console.ReadLine() ?? "0");

        Console.WriteLine("Categorias:");
        foreach (var c in categoriaDAO.GetAll())
            Console.WriteLine($"{c.Id} - {c.Nome}");

        Console.Write("CategoriaId: ");
        var catId = int.Parse(Console.ReadLine() ?? "0");

        var id = produtoDAO.Insert(new Produto { Nome = nome, Preco = preco, Estoque = estoque, CategoriaId = catId });
        Console.WriteLine($"Inserido com Id = {id}");
    }

    static void AtualizarProduto()
    {
        Console.Write("Id do produto: ");
        var id = int.Parse(Console.ReadLine() ?? "0");
        var p = produtoDAO.GetById(id);
        if (p == null) { Console.WriteLine("Não encontrado."); return; }

        Console.Write($"Nome ({p.Nome}): ");
        var nome = Console.ReadLine();
        Console.Write($"Preço ({p.Preco}): ");
        var precoStr = Console.ReadLine();
        Console.Write($"Estoque ({p.Estoque}): ");
        var estStr = Console.ReadLine();
        Console.Write($"CategoriaId ({p.CategoriaId}): ");
        var catStr = Console.ReadLine();

        p.Nome = string.IsNullOrWhiteSpace(nome) ? p.Nome : nome!;
        p.Preco = string.IsNullOrWhiteSpace(precoStr) ? p.Preco : decimal.Parse(precoStr!);
        p.Estoque = string.IsNullOrWhiteSpace(estStr) ? p.Estoque : int.Parse(estStr!);
        p.CategoriaId = string.IsNullOrWhiteSpace(catStr) ? p.CategoriaId : int.Parse(catStr!);

        var ok = produtoDAO.Update(p);
        Console.WriteLine(ok ? "Atualizado." : "Falhou.");
    }

    static void DeletarProduto()
    {
        Console.Write("Id do produto: ");
        var id = int.Parse(Console.ReadLine() ?? "0");
        var ok = produtoDAO.Delete(id);
        Console.WriteLine(ok ? "Deletado." : "Não encontrado.");
    }

    static void BuscarPorId()
    {
        Console.Write("Id do produto: ");
        var id = int.Parse(Console.ReadLine() ?? "0");
        var p = produtoDAO.GetById(id);
        if (p == null) Console.WriteLine("Não encontrado.");
        else Console.WriteLine($"{p.Id} | {p.Nome} | R${p.Preco:0.00} | Estoque: {p.Estoque} | CatId: {p.CategoriaId}");
    }

    static void ListarPorCategoria()
    {
        Console.WriteLine("Categorias:");
        foreach (var c in categoriaDAO.GetAll())
            Console.WriteLine($"{c.Id} - {c.Nome}");
        Console.Write("Informe CategoriaId: ");
        var catId = int.Parse(Console.ReadLine() ?? "0");

        var lista = produtoDAO.GetByCategory(catId);
        Console.WriteLine("\n--- Produtos por Categoria ---");
        foreach (var (produto, categoria) in lista)
            Console.WriteLine($"{produto.Id} | {produto.Nome} | R${produto.Preco:0.00} | Cat: {categoria.Nome}");
    }

    static void ListarPedidosDeCliente()
    {
        Console.WriteLine("Clientes:");
        foreach (var c in clienteDAO.GetAll())
            Console.WriteLine($"{c.Id} - {c.Nome}");
        Console.Write("ClienteId: ");
        var clienteId = int.Parse(Console.ReadLine() ?? "0");

        var pedidos = pedidoDAO.ListByCliente(clienteId);
        Console.WriteLine("\n--- Pedidos do Cliente ---");
        foreach (var p in pedidos)
            Console.WriteLine($"Pedido {p.Id} | Data: {p.DataPedido:yyyy-MM-dd HH:mm} | Total: R${p.Total:0.00}");
    }

    static void CriarPedidoTransacao()
    {
        Console.WriteLine("Clientes:");
        foreach (var c in clienteDAO.GetAll())
            Console.WriteLine($"{c.Id} - {c.Nome}");
        Console.Write("ClienteId: ");
        var clienteId = int.Parse(Console.ReadLine() ?? "0");

        var itens = new List<PedidoItem>();
        while (true)
        {
            Console.WriteLine("\nProdutos disponíveis:");
            foreach (var p in produtoDAO.GetAll())
                Console.WriteLine($"{p.Id} - {p.Nome} (R${p.Preco:0.00}) Estoque:{p.Estoque}");

            Console.Write("ProdutoId (ou ENTER para finalizar): ");
            var pid = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(pid)) break;

            Console.Write("Quantidade: ");
            var qtd = int.Parse(Console.ReadLine() ?? "1");

            var prod = produtoDAO.GetById(int.Parse(pid));
            if (prod == null) { Console.WriteLine("Produto inválido."); continue; }
            if (qtd <= 0 || qtd > prod.Estoque) { Console.WriteLine("Quantidade inválida ou acima do estoque."); continue; }

            itens.Add(new PedidoItem
            {
                ProdutoId = prod.Id,
                Quantidade = qtd,
                PrecoUnitario = prod.Preco
            });
        }

        if (!itens.Any()) { Console.WriteLine("Nenhum item informado."); return; }

        var pedidoId = pedidoDAO.CreateOrder(clienteId, itens);
        Console.WriteLine($"Pedido criado com sucesso (Id={pedidoId}). Detalhes:");
        var detalhes = pedidoDAO.GetDetalhesPedido(pedidoId);
        foreach (var d in detalhes.Itens)
            Console.WriteLine($" - {d.ProdutoNome} x{d.Quantidade} = R${d.TotalItem:0.00}");
        Console.WriteLine($"TOTAL: R${detalhes.TotalGeral:0.00}");
    }

    static void MenuDesafios()
    {
        Console.WriteLine("\nDesafios:");
        Console.WriteLine("a) Estoque baixo");
        Console.WriteLine("b) Busca por nome (LIKE)");
        Console.WriteLine("c) Total de vendas por período");
        Console.Write("Opção: ");
        var op = Console.ReadLine();

        switch (op?.ToLowerInvariant())
        {
            case "a":
                Console.Write("Limite de estoque (ex: 10): ");
                var lim = int.Parse(Console.ReadLine() ?? "10");
                var baixos = produtoDAO.GetLowStock(lim);
                Console.WriteLine("\n--- Produtos com estoque baixo ---");
                foreach (var p in baixos) Console.WriteLine($"{p.Id} | {p.Nome} | Estoque:{p.Estoque}");
                break;

            case "b":
                Console.Write("Termo de busca: ");
                var termo = Console.ReadLine() ?? "";
                var achados = produtoDAO.SearchByNameLike(termo);
                Console.WriteLine("\n--- Busca por nome ---");
                foreach (var p in achados) Console.WriteLine($"{p.Id} | {p.Nome} | R${p.Preco:0.00}");
                break;

            case "c":
                Console.Write("Data início (yyyy-MM-dd): ");
                var di = DateTime.Parse(Console.ReadLine() ?? "");
                Console.Write("Data fim (yyyy-MM-dd): ");
                var df = DateTime.Parse(Console.ReadLine() ?? "");
                var total = pedidoDAO.TotalVendidoPeriodo(di, df);
                Console.WriteLine($"Total de vendas no período: R${total:0.00}");
                break;

            default:
                Console.WriteLine("Opção inválida.");
                break;
        }
    }
}
