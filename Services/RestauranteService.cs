using ComandaFacil.Enums;
using ComandaFacil.Models;
using ComandaFacil.Repositories;

namespace ComandaFacil.Services;

/// <summary>
/// Serviço principal de orquestração de negócios do restaurante.
/// Por que existe: Separa a lógica de negócios da interface de usuário (Console CLI)
/// e da persistência direta (RepositorioTxt), atuando como mediador do domínio
/// e garantindo coesão e baixo acoplamento (Single Responsibility Principle).
/// </summary>
public class RestauranteService
{
    // [ENCAPSULAMENTO] Coleções internas mantidas estritamente privadas para não permitir bypass das regras
    private readonly List<Cliente> _clientes = [];
    private readonly List<ItemCardapio> _cardapio = [];
    private readonly List<Pedido> _pedidos = [];
    private readonly RepositorioTxt _repositorio;

    private const string ArquivoClientes = "clientes.txt";
    private const string ArquivoCardapio = "cardapio.txt";
    private const string ArquivoPedidos = "pedidos.txt";

    /// <summary>
    /// Construtor com injeção opcional de repositório.
    /// Por que existe: Permite injeção de dependência e flexibilidade para testes ou caminhos alternativos.
    /// </summary>
    public RestauranteService(RepositorioTxt? repositorio = null)
    {
        _repositorio = repositorio ?? new RepositorioTxt();
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Cadastra cliente com validação de unicidade de ID e CPF.
    /// Por que existe: Impede a inserção de registros duplicados que gerariam inconsistências fiscais e cadastrais.
    /// </summary>
    public void CadastrarCliente(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente, "O cliente a cadastrar não pode ser nulo.");

        if (_clientes.Any(c => c.Id == cliente.Id))
            throw new InvalidOperationException($"Já existe um cliente cadastrado com o Id #{cliente.Id}.");

        if (_clientes.Any(c => c.Cpf.Equals(cliente.Cpf, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Já existe um cliente cadastrado com o CPF '{cliente.Cpf}'.");

        _clientes.Add(cliente);
    }

    /// <summary>
    /// // [POLIMORFISMO] Cadastra um ItemCardapio genérico recebendo instâncias de Bebida ou PratoPrincipal.
    /// Por que existe: Trata itens de forma polimórfica através da abstração base ItemCardapio,
    /// sem precisar de métodos separados para cada categoria de item vendável.
    /// </summary>
    public void CadastrarItemCardapio(ItemCardapio item)
    {
        ArgumentNullException.ThrowIfNull(item, "O item a ser adicionado ao cardápio não pode ser nulo.");

        if (_cardapio.Any(i => i.Id == item.Id))
            throw new InvalidOperationException($"Já existe um item no cardápio com o Id #{item.Id}.");

        _cardapio.Add(item);
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Criação de novo pedido amarrado a um cliente existente com ID autoincremental.
    /// Por que existe: Assegura que nenhum pedido seja criado sem cliente válido cadastrado.
    /// </summary>
    public Pedido CriarPedido(int idCliente)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == idCliente)
            ?? throw new InvalidOperationException($"Cliente com Id #{idCliente} não encontrado para vinculação do pedido.");

        int novoIdPedido = _pedidos.Count == 0 ? 1 : _pedidos.Max(p => p.Id) + 1;
        var novoPedido = new Pedido(novoIdPedido, cliente);
        _pedidos.Add(novoPedido);

        return novoPedido;
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Retorna cópia rasa da lista de itens do cardápio.
    /// Por que existe: Evita que manipuladores externos alterem a lista interna do cardápio por referência.
    /// </summary>
    public List<ItemCardapio> ObterCardapio() => [.. _cardapio];

    /// <summary>
    /// // [ENCAPSULAMENTO] Retorna cópia rasa da lista de pedidos registrados.
    /// Por que existe: Impede manipulação externa da coleção _pedidos.
    /// </summary>
    public List<Pedido> ObterPedidos() => [.. _pedidos];

    /// <summary>
    /// // [ENCAPSULAMENTO] Retorna cópia rasa da lista de clientes cadastrados.
    /// Por que existe: Impede modificação externa da lista de clientes em memória.
    /// </summary>
    public List<Cliente> ObterClientes() => [.. _clientes];

    /// <summary>
    /// Localiza um pedido específico por seu Id único.
    /// Por que existe: Fornece consulta direta requerida pela interface de usuário.
    /// </summary>
    public Pedido? ObterPedidoPorId(int id) => _pedidos.FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// Localiza um item do cardápio por seu Id único.
    /// Por que existe: Permite seleção rápida para montagem de pedidos e consultas pontuais.
    /// </summary>
    public ItemCardapio? ObterItemCardapioPorId(int id) => _cardapio.FirstOrDefault(i => i.Id == id);

    /// <summary>
    /// // [POLIMORFISMO] Busca itens por nome ou descrição/ingredientes com pattern matching de tipos.
    /// Por que existe: Atende à consulta textual solicitada, aproveitando as propriedades
    /// especializadas de PratoPrincipal (Ingredientes) sem quebrar a coesão.
    /// </summary>
    public List<ItemCardapio> BuscarItensPorDescricao(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return [];

        string termoNormalizado = termo.Trim();

        return _cardapio.Where(item =>
            item.Nome.Contains(termoNormalizado, StringComparison.OrdinalIgnoreCase) ||
            (item is PratoPrincipal prato && prato.Ingredientes.Contains(termoNormalizado, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }

    /// <summary>
    /// Localiza um cliente específico por seu Id único.
    /// Por que existe: Viabiliza consultas diretas e checagem de existência.
    /// </summary>
    public Cliente? ObterClientePorId(int id) => _clientes.FirstOrDefault(c => c.Id == id);

    /// <summary>
    /// // [POLIMORFISMO] Salva todos os clientes, itens e pedidos acionando ToTxt() polimorficamente.
    /// Por que existe: Delega a formatação de cada item para a sua respectiva classe filha (Bebida ou PratoPrincipal),
    /// gravando os dados em arquivos de texto sem dependências externas.
    /// </summary>
    public void SalvarTodosDados()
    {
        // 1. Salvar Clientes
        List<string> linhasClientes = _clientes.Select(c => c.ToTxt()).ToList();
        _repositorio.SalvarLinhas(ArquivoClientes, linhasClientes);

        // 2. Salvar Cardápio polimorficamente
        List<string> linhasCardapio = _cardapio.Select(i => i.ToTxt()).ToList();
        _repositorio.SalvarLinhas(ArquivoCardapio, linhasCardapio);

        // 3. Salvar Pedidos
        List<string> linhasPedidos = _pedidos.Select(p => p.ToTxt()).ToList();
        _repositorio.SalvarLinhas(ArquivoPedidos, linhasPedidos);
    }

    /// <summary>
    /// // [POLIMORFISMO] Carrega os dados dos arquivos texto ou inicializa sementes de exemplo.
    /// Por que existe: Utiliza as factories FromTxt para instanciar a hierarquia de objetos
    /// em tempo de execução sem acoplar a infraestrutura ao serviço.
    /// </summary>
    public void CarregarDadosIniciais()
    {
        _clientes.Clear();
        _cardapio.Clear();
        _pedidos.Clear();

        var linhasClientes = _repositorio.LerLinhas(ArquivoClientes);
        var linhasCardapio = _repositorio.LerLinhas(ArquivoCardapio);
        var linhasPedidos = _repositorio.LerLinhas(ArquivoPedidos);

        if (linhasClientes.Count > 0 || linhasCardapio.Count > 0)
        {
            foreach (var linha in linhasClientes)
            {
                if (!string.IsNullOrWhiteSpace(linha))
                    _clientes.Add(Cliente.FromTxt(linha));
            }

            foreach (var linha in linhasCardapio)
            {
                if (!string.IsNullOrWhiteSpace(linha))
                    _cardapio.Add(ItemCardapio.FromTxt(linha));
            }

            foreach (var linha in linhasPedidos)
            {
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var partes = linha.Split(';');
                if (partes.Length >= 5 && partes[0] == "PEDIDO")
                {
                    int pedidoId = int.Parse(partes[1]);
                    int clienteId = int.Parse(partes[2]);
                    DateTime dataHora = DateTime.Parse(partes[3], null, System.Globalization.DateTimeStyles.RoundtripKind);
                    StatusPedido status = Enum.Parse<StatusPedido>(partes[4]);

                    var cliente = _clientes.FirstOrDefault(c => c.Id == clienteId);
                    if (cliente is not null)
                    {
                        var pedido = new Pedido(pedidoId, cliente, dataHora, status);

                        if (partes.Length >= 6 && !string.IsNullOrWhiteSpace(partes[5]))
                        {
                            var itensSerializados = partes[5].Split('|', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var itemStr in itensSerializados)
                            {
                                var subPartes = itemStr.Split(':');
                                if (subPartes.Length == 2)
                                {
                                    int itemId = int.Parse(subPartes[0]);
                                    int quantidade = int.Parse(subPartes[1]);

                                    var itemObj = _cardapio.FirstOrDefault(i => i.Id == itemId);
                                    if (itemObj is not null)
                                    {
                                        pedido.AdicionarItem(itemObj, quantidade);
                                    }
                                }
                            }
                        }

                        _pedidos.Add(pedido);
                    }
                }
            }
        }
        else
        {
            PopularDadosExemplo();
            SalvarTodosDados();
        }
    }

    /// <summary>
    /// Popula itens e clientes iniciais caso os arquivos .txt estejam vazios.
    /// Por que existe: Facilita testes e demonstrações acadêmicas imediatas após o primeiro 'dotnet run'.
    /// </summary>
    private void PopularDadosExemplo()
    {
        CadastrarCliente(new Cliente(1, "Ana Souza", "(31) 98765-4321", "123.456.789-00"));
        CadastrarCliente(new Cliente(2, "Carlos Oliveira", "(37) 99123-4567", "987.654.321-99"));
        CadastrarCliente(new Cliente(3, "Beatriz Ferreira", "(11) 97654-3210", "456.789.123-55"));

        CadastrarItemCardapio(new Bebida(101, "Suco Natural de Laranja", 12.50m, 500, gelada: true));
        CadastrarItemCardapio(new Bebida(102, "Refrigerante Guaraná", 8.00m, 350, gelada: true));
        CadastrarItemCardapio(new Bebida(103, "Café Espresso Gourmet", 6.50m, 120, gelada: false));

        CadastrarItemCardapio(new PratoPrincipal(
            201,
            "Feijoada Tradicional Completa",
            48.90m,
            "Feijão preto, carnes nobres, arroz, couve refogada, farofa e laranja",
            tempoPreparoMinutos: 25));

        CadastrarItemCardapio(new PratoPrincipal(
            202,
            "Filé de Frango Grelhado com Legumes",
            34.00m,
            "Peito de frango selecionado, brócolis, cenoura no vapor e arroz integral",
            tempoPreparoMinutos: 18));

        CadastrarItemCardapio(new PratoPrincipal(
            203,
            "Lasanha Artesanal à Bolonhesa",
            42.00m,
            "Massa fresca, molho de carne especial, queijo muçarela e parmesão gratinado",
            tempoPreparoMinutos: 30));

        var pedidoExemplo = CriarPedido(1);
        pedidoExemplo.AdicionarItem(ObterItemCardapioPorId(201)!, 1);
        pedidoExemplo.AdicionarItem(ObterItemCardapioPorId(101)!, 2);
        pedidoExemplo.AtualizarStatus(StatusPedido.EmPreparo);
    }
}
