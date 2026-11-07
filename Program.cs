using System.Globalization;
using ComandaFacil.Enums;
using ComandaFacil.Models;
using ComandaFacil.Services;

namespace ComandaFacil;

/// <summary>
/// Ponto de entrada da aplicação Console de Gestão de Restaurante.
/// Implementa a interface de usuário (CLI) modular, limpa e interativa.
/// Conta com limpeza automática de tela (Console.Clear) após cada execução
/// para manter o terminal despoluído.
/// </summary>
public static class Program
{
    private static readonly RestauranteService _service = new();

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");

        _service.CarregarDadosIniciais();

        bool executando = true;
        while (executando)
        {
            Console.Clear();
            ExibirCabecalho();
            ExibirMenuPrincipal();

            string? opcao = Console.ReadLine()?.Trim();

            if (opcao == "0")
            {
                _service.SalvarTodosDados();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nDados salvos automaticamente nos arquivos txt. Encerrando aplicação... Até logo!");
                Console.ResetColor();
                break;
            }

            try
            {
                switch (opcao)
                {
                    case "1":
                        ListarCardapio();
                        break;
                    case "2":
                        ConsultarItemCardapio();
                        break;
                    case "3":
                        CadastrarNovoItemCardapio();
                        break;
                    case "4":
                        ListarClientes();
                        break;
                    case "5":
                        CadastrarNovoCliente();
                        break;
                    case "6":
                        CriarNovoPedido();
                        break;
                    case "7":
                        ConsultarPedidoPorId();
                        break;
                    case "8":
                        AdicionarItemAoPedido();
                        break;
                    case "9":
                        RemoverItemDoPedido();
                        break;
                    case "10":
                        AtualizarStatusDoPedido();
                        break;
                    case "11":
                        ListarPedidos();
                        break;
                    case "12":
                        SalvarDados();
                        break;
                    default:
                        AvisarErro("Opção inválida. Digite um número de 0 a 12 correspondente ao menu.");
                        break;
                }
            }
            catch (Exception ex)
            {
                AvisarErro($"Erro durante a execução: {ex.Message}");
            }

            // Opção de "continuar" obrigatória após qualquer uma das 12 opções
            SolicitarContinuar();
        }
    }

    private static void ExibirCabecalho()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""
        ========================================================================
                      SISTEMA DE GESTÃO DE RESTAURANTE - .NET 10 / C# 15
                       Demonstração Prática dos 4 Pilares da POO
        ========================================================================
        """);
        Console.ResetColor();
    }

    private static void ExibirMenuPrincipal()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("""
        ---------------------------- MENU PRINCIPAL ----------------------------
        1.  Listar Todo o Cardápio (Polimorfismo: Bebidas e Pratos)
        2.  Consultar Item do Cardápio (por ID ou Descrição/Nome)
        3.  Cadastrar Novo Item no Cardápio (Bebida / Prato Principal)
        4.  Listar Clientes Cadastrados
        5.  Cadastrar Novo Cliente
        6.  Criar Novo Pedido
        7.  Consultar Pedido por ID (Visualizar Extrato Detalhado)
        8.  Adicionar Item ao Pedido
        9.  Remover Item do Pedido
        10. Atualizar Status do Pedido (Pendente, EmPreparo, Pronto, etc.)
        11. Listar Todos os Pedidos Registrados
        12. Salvar Dados no Repositório (.txt)
        0.  Sair da Aplicação
        ------------------------------------------------------------------------
        """);
        Console.ResetColor();
        Console.Write("Selecione uma opção (0 a 12): ");
    }

    /// <summary>
    /// Aguarda o usuário pressionar ENTER para continuar, permitindo que a próxima iteração
    /// limpe o console e exiba o menu sem poluição de telas anteriores.
    /// </summary>
    private static void SolicitarContinuar()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n------------------------------------------------------------------------");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Pressione [ENTER] para continuar e retornar ao menu...");
        Console.ResetColor();
        Console.ReadLine();
    }

    // 1. Listar Cardápio
    private static void ListarCardapio()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- CARDÁPIO COMPLETO DO RESTAURANTE ---");
        Console.ResetColor();

        var cardapio = _service.ObterCardapio();
        if (cardapio.Count == 0)
        {
            Console.WriteLine("O cardápio está vazio no momento.");
            return;
        }

        // Demonstração de Polimorfismo: chamada dinâmica de ExibirDetalhes()
        foreach (var item in cardapio)
        {
            Console.WriteLine(item.ExibirDetalhes());
        }
    }

    // 2. Consultar Item do Cardápio (por ID ou Descrição)
    private static void ConsultarItemCardapio()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- CONSULTAR ITENS DO CARDÁPIO ---");
        Console.ResetColor();
        Console.WriteLine("Escolha o tipo de consulta:");
        Console.WriteLine("1. Consultar por ID do item");
        Console.WriteLine("2. Consultar por Descrição / Nome / Ingredientes");
        Console.Write("Opção (1 ou 2): ");

        string? tipoConsulta = Console.ReadLine()?.Trim();
        switch (tipoConsulta)
        {
            case "1":
            {
                Console.Write("\nDigite o ID do item desejado: ");
                int idItem = LerInteiroObrigatorio();

                var item = _service.ObterItemCardapioPorId(idItem);
                if (item is null)
                {
                    AvisarErro($"Nenhum item com o ID #{idItem} foi localizado no cardápio.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✓ Item localizado com sucesso:");
                Console.ResetColor();
                Console.WriteLine(item.ExibirDetalhes());
                break;
            }
            case "2":
            {
                Console.Write("\nDigite o termo para busca (nome ou ingredientes): ");
                string termo = LerTextoObrigatorio();

                var resultados = _service.BuscarItensPorDescricao(termo);
                if (resultados.Count == 0)
                {
                    AvisarErro($"Nenhum item do cardápio encontrado correspondente ao termo '{termo}'.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ Foram localizado(s) {resultados.Count} item(ns):");
                Console.ResetColor();
                foreach (var item in resultados)
                {
                    Console.WriteLine(item.ExibirDetalhes());
                }
                break;
            }
            default:
                AvisarErro("Tipo de consulta inválido. Por favor, escolha 1 ou 2.");
                break;
        }
    }

    // 3. Cadastrar Novo Item no Cardápio
    private static void CadastrarNovoItemCardapio()
    {
        Console.WriteLine("\nQual tipo de item deseja cadastrar?");
        Console.WriteLine("1. Bebida (volume, temperatura)");
        Console.WriteLine("2. Prato Principal (ingredientes, tempo de preparo)");
        Console.Write("Escolha (1 ou 2): ");
        string? tipo = Console.ReadLine()?.Trim();

        var cardapio = _service.ObterCardapio();
        int novoId = cardapio.Count == 0 ? 101 : cardapio.Max(i => i.Id) + 1;

        Console.Write("Nome do item: ");
        string nome = LerTextoObrigatorio();

        Console.Write("Preço (ex: 29,90): ");
        decimal preco = LerDecimalObrigatorio();

        if (tipo == "1")
        {
            Console.Write("Volume em ml (ex: 350): ");
            int volume = LerInteiroObrigatorio();

            Console.Write("É servida gelada? (S/N): ");
            bool gelada = Console.ReadLine()?.Trim().ToUpper() == "S";

            var bebida = new Bebida(novoId, nome, preco, volume, gelada);
            _service.CadastrarItemCardapio(bebida);
            Sucesso($"Bebida #{novoId} cadastrada com sucesso!");
        }
        else if (tipo == "2")
        {
            Console.Write("Ingredientes principais: ");
            string ingredientes = LerTextoObrigatorio();

            Console.Write("Tempo estimado de preparo em minutos: ");
            int tempo = LerInteiroObrigatorio();

            var prato = new PratoPrincipal(novoId, nome, preco, ingredientes, tempo);
            _service.CadastrarItemCardapio(prato);
            Sucesso($"Prato Principal #{novoId} cadastrado com sucesso!");
        }
        else
        {
            AvisarErro("Tipo de item inválido.");
        }
    }

    // 4. Listar Clientes
    private static void ListarClientes()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- CLIENTES CADASTRADOS ---");
        Console.ResetColor();

        var clientes = _service.ObterClientes();
        if (clientes.Count == 0)
        {
            Console.WriteLine("Nenhum cliente cadastrado.");
            return;
        }

        // Polimorfismo: ExibirIdentificacao sobrescrito em Cliente
        foreach (var cliente in clientes)
        {
            Console.WriteLine(cliente.ExibirIdentificacao());
        }
    }

    // 5. Cadastrar Novo Cliente
    private static void CadastrarNovoCliente()
    {
        Console.WriteLine("\n--- NOVO CADASTRO DE CLIENTE ---");
        var clientes = _service.ObterClientes();
        int novoId = clientes.Count == 0 ? 1 : clientes.Max(c => c.Id) + 1;

        Console.Write("Nome completo: ");
        string nome = LerTextoObrigatorio();

        Console.Write("Telefone de contato: ");
        string telefone = LerTextoObrigatorio();

        Console.Write("CPF (ex: 123.456.789-00): ");
        string cpf = LerTextoObrigatorio();

        var cliente = new Cliente(novoId, nome, telefone, cpf);
        _service.CadastrarCliente(cliente);
        Sucesso($"Cliente #{novoId} cadastrado com sucesso!");
    }

    // 6. Criar Novo Pedido
    private static void CriarNovoPedido()
    {
        Console.WriteLine("\n--- ABERTURA DE NOVO PEDIDO ---");
        ListarClientes();

        Console.Write("\nDigite o Id do Cliente para o pedido: ");
        int idCliente = LerInteiroObrigatorio();

        var pedido = _service.CriarPedido(idCliente);
        Sucesso($"Pedido #{pedido.Id} criado com sucesso para o cliente '{pedido.Cliente.Nome}'. Status: {pedido.Status}");
    }

    // 7. Consultar Pedido por ID (Detalhes e Extrato)
    private static void ConsultarPedidoPorId()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- CONSULTAR PEDIDO POR ID ---");
        Console.ResetColor();

        Console.Write("Digite o Id do Pedido que deseja consultar: ");
        int idPedido = LerInteiroObrigatorio();

        var pedido = _service.ObterPedidoPorId(idPedido);
        if (pedido is null)
        {
            AvisarErro($"Pedido #{idPedido} não foi localizado no sistema.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(pedido.GerarExtrato());
        Console.ResetColor();
    }

    // 8. Adicionar Item ao Pedido
    private static void AdicionarItemAoPedido()
    {
        Console.WriteLine("\n--- ADICIONAR ITEM AO PEDIDO ---");
        Console.Write("Digite o Id do Pedido: ");
        int idPedido = LerInteiroObrigatorio();

        var pedido = _service.ObterPedidoPorId(idPedido);
        if (pedido is null)
        {
            AvisarErro($"Pedido #{idPedido} não encontrado.");
            return;
        }

        ListarCardapio();
        Console.Write("\nDigite o Id do Item do cardápio a adicionar: ");
        int idItem = LerInteiroObrigatorio();

        var item = _service.ObterItemCardapioPorId(idItem);
        if (item is null)
        {
            AvisarErro($"Item #{idItem} não encontrado no cardápio.");
            return;
        }

        Console.Write($"Quantidade de '{item.Nome}': ");
        int qtd = LerInteiroObrigatorio();

        pedido.AdicionarItem(item, qtd);
        Sucesso($"{qtd}x '{item.Nome}' adicionado(s) ao Pedido #{idPedido}. Novo total: {pedido.CalcularTotal():C2}");
    }

    // 9. Remover Item do Pedido
    private static void RemoverItemDoPedido()
    {
        Console.WriteLine("\n--- REMOVER ITEM DO PEDIDO ---");
        Console.Write("Digite o Id do Pedido: ");
        int idPedido = LerInteiroObrigatorio();

        var pedido = _service.ObterPedidoPorId(idPedido);
        if (pedido is null)
        {
            AvisarErro($"Pedido #{idPedido} não encontrado.");
            return;
        }

        if (pedido.Itens.Count == 0)
        {
            Console.WriteLine("Este pedido não possui itens adicionados.");
            return;
        }

        Console.WriteLine("Itens atuais no pedido:");
        foreach (var itemPedido in pedido.Itens)
        {
            Console.WriteLine($"  [ID #{itemPedido.Item.Id}] {itemPedido.Quantidade}x {itemPedido.Item.Nome} = {itemPedido.Subtotal:C2}");
        }

        Console.Write("Digite o Id do Item a remover: ");
        int idItem = LerInteiroObrigatorio();

        pedido.RemoverItem(idItem);
        Sucesso($"Item #{idItem} removido com sucesso. Total atualizado: {pedido.CalcularTotal():C2}");
    }

    // 10. Atualizar Status do Pedido
    private static void AtualizarStatusDoPedido()
    {
        Console.WriteLine("\n--- ATUALIZAR STATUS DO PEDIDO ---");
        Console.Write("Digite o Id do Pedido: ");
        int idPedido = LerInteiroObrigatorio();

        var pedido = _service.ObterPedidoPorId(idPedido);
        if (pedido is null)
        {
            AvisarErro($"Pedido #{idPedido} não encontrado.");
            return;
        }

        Console.WriteLine($"Status atual do pedido #{idPedido}: {pedido.Status}");
        Console.WriteLine("Selecione o novo status:");
        Console.WriteLine("1. Pendente");
        Console.WriteLine("2. EmPreparo");
        Console.WriteLine("3. Pronto");
        Console.WriteLine("4. EntregueEPago");
        Console.WriteLine("5. Cancelado");
        Console.Write("Opção: ");

        string? opc = Console.ReadLine()?.Trim();
        StatusPedido novoStatus = opc switch
        {
            "1" => StatusPedido.Pendente,
            "2" => StatusPedido.EmPreparo,
            "3" => StatusPedido.Pronto,
            "4" => StatusPedido.EntregueEPago,
            "5" => StatusPedido.Cancelado,
            _ => throw new ArgumentException("Opção de status inválida.")
        };

        pedido.AtualizarStatus(novoStatus);
        Sucesso($"Status do Pedido #{idPedido} atualizado com sucesso para: {novoStatus}");
    }

    // 11. Listar Pedidos
    private static void ListarPedidos()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- LISTA GERAL DE PEDIDOS ---");
        Console.ResetColor();

        var pedidos = _service.ObterPedidos();
        if (pedidos.Count == 0)
        {
            Console.WriteLine("Nenhum pedido registrado.");
            return;
        }

        foreach (var pedido in pedidos)
        {
            Console.WriteLine($"Pedido #{pedido.Id:D3} | Data: {pedido.DataHora:dd/MM/yyyy HH:mm} | Status: {pedido.Status,-13} | Cliente: {pedido.Cliente.Nome} | Total: {pedido.CalcularTotal():C2} ({pedido.Itens.Count} itens)");
        }
    }

    // 12. Salvar Dados
    private static void SalvarDados()
    {
        Console.WriteLine("\nPersistindo todos os dados em arquivos de texto (.txt)...");
        _service.SalvarTodosDados();
        Sucesso("Todos os clientes, itens de cardápio e pedidos foram salvos no disco com sucesso!");
    }

    private static string LerTextoObrigatorio()
    {
        while (true)
        {
            string? entrada = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(entrada))
                return entrada;

            Console.Write("Campo obrigatório. Por favor, digite novamente: ");
        }
    }

    private static int LerInteiroObrigatorio()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int valor) && valor >= 0)
                return valor;

            Console.Write("Valor numérico inteiro inválido. Digite novamente: ");
        }
    }

    private static decimal LerDecimalObrigatorio()
    {
        while (true)
        {
            string? entrada = Console.ReadLine()?.Trim().Replace('.', ',');
            if (decimal.TryParse(entrada, NumberStyles.Any, new CultureInfo("pt-BR"), out decimal valor) && valor >= 0)
                return valor;

            Console.Write("Valor decimal inválido. Digite novamente (ex: 25,50): ");
        }
    }

    private static void Sucesso(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ {msg}");
        Console.ResetColor();
    }

    private static void AvisarErro(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ {msg}");
        Console.ResetColor();
    }
}
