using System.Text;
using ComandaFacil.Enums;

namespace ComandaFacil.Models;

/// <summary>
/// // [ENCAPSULAMENTO] Entidade raiz de agregação que gerencia os itens, status e valor do pedido.
/// Por que existe: Centraliza todas as regras de negócio relativas ao ciclo de vida de uma comanda,
/// garantindo que pedidos fechados ou cancelados não sejam violados e que os cálculos financeiros
/// permaneçam estritamente sincronizados com os itens contidos.
/// </summary>
public sealed class Pedido
{
    // [ENCAPSULAMENTO] Propriedades públicas identificadoras com imutabilidade posicional
    public int Id { get; init; }
    public Cliente Cliente { get; init; }
    public DateTime DataHora { get; init; }

    // [ENCAPSULAMENTO] Status com 'private set' para que alterações só ocorram via método com regras
    public StatusPedido Status { get; private set; }

    /// <summary>
    /// // [ENCAPSULAMENTO] Coleção interna estritamente privada.
    /// Por que existe: Oculta o armazenamento físico e impede que qualquer classe externa faça
    /// '_itens.Add()' ou '_itens.Clear()' diretamente, contornando as validações de negócio.
    /// </summary>
    private readonly List<ItemPedido> _itens = [];

    /// <summary>
    /// // [ENCAPSULAMENTO] Exposição controlada somente leitura da lista de itens.
    /// Por que existe: Permite que camadas de apresentação leiam e iterem pelos itens do pedido
    /// com total segurança, sem poder manipular a estrutura da lista.
    /// </summary>
    public IReadOnlyList<ItemPedido> Itens => _itens.AsReadOnly();

    /// <summary>
    /// Construtor de Pedido com injeção obrigatória do cliente solicitante.
    /// Por que existe: Garante que nenhum pedido exista solto ou sem cliente responsável no sistema.
    /// </summary>
    public Pedido(int id, Cliente cliente, DateTime? dataHora = null, StatusPedido status = StatusPedido.Pendente)
    {
        // [ENCAPSULAMENTO] Validação de integridade dos argumentos de inicialização
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "O identificador do pedido deve ser maior que zero.");

        ArgumentNullException.ThrowIfNull(cliente, "Todo pedido deve estar necessariamente vinculado a um Cliente cadastrado.");

        Id = id;
        Cliente = cliente;
        DataHora = dataHora ?? DateTime.Now;
        Status = status;
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Método controlado de adição de itens com checagem de ciclo de vida.
    /// Por que existe: Verifica se o pedido ainda está aberto a modificações (não cancelado ou finalizado)
    /// e trata a agregação de quantidades de itens repetidos, mantendo a comanda organizada.
    /// </summary>
    public void AdicionarItem(ItemCardapio item, int quantidade)
    {
        ArgumentNullException.ThrowIfNull(item, "O item do cardápio adicionado não pode ser nulo.");

        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade a adicionar deve ser um número positivo.");

        // [ENCAPSULAMENTO] Bloqueio de mutação em pedidos já finalizados ou cancelados
        if (Status is StatusPedido.Cancelado or StatusPedido.EntregueEPago)
            throw new InvalidOperationException($"Não é permitido adicionar itens a um pedido que já se encontra como '{Status}'.");

        int indiceExistente = _itens.FindIndex(i => i.Item.Id == item.Id);
        if (indiceExistente >= 0)
        {
            var existente = _itens[indiceExistente];
            _itens[indiceExistente] = new ItemPedido(item, existente.Quantidade + quantidade);
        }
        else
        {
            _itens.Add(new ItemPedido(item, quantidade));
        }
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Método controlado de remoção de itens do pedido por ID.
    /// Por que existe: Protege a integridade do pedido verificando se o status permite alterações
    /// e garantindo que o item realmente existia no pedido antes da exclusão.
    /// </summary>
    public void RemoverItem(int itemId)
    {
        // [ENCAPSULAMENTO] Regra de negócio: pedidos encerrados não permitem estorno de itens
        if (Status is StatusPedido.Cancelado or StatusPedido.EntregueEPago)
            throw new InvalidOperationException($"Não é permitido remover itens de um pedido com status '{Status}'.");

        int removidos = _itens.RemoveAll(i => i.Item.Id == itemId);
        if (removidos == 0)
            throw new InvalidOperationException($"O item com Id #{itemId} não foi encontrado dentro deste pedido.");
    }

    /// <summary>
    /// // [POLIMORFISMO] Método de cálculo do montante financeiro total do pedido.
    /// Por que existe: Itera sobre a coleção de itens delegando para 'CalcularSubtotal()', que acessa
    /// os preços polimorficamente sem necessitar de checagem condicional de tipos.
    /// </summary>
    public decimal CalcularTotal()
    {
        return _itens.Sum(item => item.CalcularSubtotal());
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Máquina de estados para transição controlada do ciclo de vida do pedido.
    /// Por que existe: Garante que um pedido cancelado não seja ressuscitado indevidamente
    /// e que pedidos já pagos não sejam revertidos para pendentes acidentalmente.
    /// </summary>
    public void AtualizarStatus(StatusPedido novoStatus)
    {
        if (Status == StatusPedido.Cancelado)
            throw new InvalidOperationException("Pedidos com status 'Cancelado' estão encerrados definitivamente e não podem ser alterados.");

        if (Status == StatusPedido.EntregueEPago && novoStatus != StatusPedido.EntregueEPago)
            throw new InvalidOperationException("Um pedido que já foi 'EntregueEPago' não pode ter seu status retrocedido.");

        Status = novoStatus;
    }

    /// <summary>
    /// Gera o extrato formatado e humanizado do pedido.
    /// Por que existe: Formata um documento de visualização amigável contendo cliente, data,
    /// cada item do pedido e o valor total consolidado.
    /// </summary>
    public string GerarExtrato()
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================================================");
        sb.AppendLine($"                    EXTRATO DO PEDIDO #{Id}");
        sb.AppendLine("================================================================");
        sb.AppendLine($"Data/Hora: {DataHora:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Status   : {Status}");
        sb.AppendLine($"Cliente  : {Cliente.ExibirIdentificacao()}");
        sb.AppendLine("----------------------------------------------------------------");
        sb.AppendLine("ITENS DO PEDIDO:");

        if (_itens.Count == 0)
        {
            sb.AppendLine("  (Nenhum item adicionado até o momento)");
        }
        else
        {
            foreach (var itemPedido in _itens)
            {
                sb.AppendLine($"  - {itemPedido.Quantidade}x {itemPedido.Item.Nome} " +
                              $"({itemPedido.Item.Preco:C2} un) = {itemPedido.Subtotal:C2}");
                sb.AppendLine($"    Detalhe: {itemPedido.Item.ExibirDetalhes().Replace("\n", "\n    ")}");
            }
        }

        sb.AppendLine("----------------------------------------------------------------");
        sb.AppendLine($"VALOR TOTAL: {CalcularTotal():C2}");
        sb.AppendLine("================================================================");

        return sb.ToString();
    }

    /// <summary>
    /// Serialização do pedido em texto delimitado para armazenamento em disco.
    /// Por que existe: Permite reconstruir o pedido completo na inicialização do sistema
    /// sem precisar de dependências ou bancos de dados externos.
    /// </summary>
    public string ToTxt()
    {
        string itensFormatados = string.Join("|", _itens.Select(i => $"{i.Item.Id}:{i.Quantidade}"));
        return $"PEDIDO;{Id};{Cliente.Id};{DataHora:O};{Status};{itensFormatados}";
    }
}
