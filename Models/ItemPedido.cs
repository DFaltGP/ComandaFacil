namespace ComandaFacil.Models;

/// <summary>
/// // [ENCAPSULAMENTO] Item individual associado a um pedido, implementado como sealed record.
/// Por que existe: Representa uma linha de pedido imutável com semântica de igualdade por valor,
/// agregando uma referência para um ItemCardapio e a respectiva quantidade solicitada.
/// Como record, garante que o estado da linha de pedido não sofra alterações colaterais indesejadas.
/// </summary>
public sealed record ItemPedido
{
    // [ENCAPSULAMENTO] Propriedades com assessores init-only garantindo imutabilidade após a criação
    public ItemCardapio Item { get; init; }
    public int Quantidade { get; init; }

    /// <summary>
    /// Propriedade calculada de subtotal.
    /// Por que existe: Elimina o risco de inconsistência entre quantidade, preço e subtotal,
    /// calculando dinamicamente o valor total desta linha de pedido.
    /// </summary>
    public decimal Subtotal => CalcularSubtotal();

    /// <summary>
    /// Construtor de ItemPedido com validação rigorosa dos parâmetros de entrada.
    /// Por que existe: Impede a existência de itens nulos no pedido ou quantidades nulas/negativas.
    /// </summary>
    public ItemPedido(ItemCardapio item, int quantidade)
    {
        // [ENCAPSULAMENTO] Barreira de integridade contra estados inválidos
        ArgumentNullException.ThrowIfNull(item, "O item do cardápio não pode ser nulo ao compor um pedido.");

        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade de itens no pedido deve ser estritamente maior que zero.");

        Item = item;
        Quantidade = quantidade;
    }

    /// <summary>
    /// // [POLIMORFISMO] Método de cálculo de subtotal que opera sobre a abstração ItemCardapio.
    /// Por que existe: Multiplica a quantidade pelo preço unitário polimorficamente,
    /// sem precisar saber se o item subjacente é uma Bebida ou um PratoPrincipal.
    /// </summary>
    public decimal CalcularSubtotal() => Item.Preco * Quantidade;
}
