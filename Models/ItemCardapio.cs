namespace ComandaFacil.Models;

/// <summary>
/// // [ABSTRAÇÃO] Classe abstrata que modela o conceito de item comercializável no cardápio.
/// Por que existe: Isola os atributos comuns (Id, Nome, Preço) e define os métodos obrigatórios
/// para qualquer produto do restaurante, permitindo que a aplicação manipule coleções homogêneas
/// sem precisar conhecer previamente se o item é uma bebida ou um prato preparado.
/// </summary>
public abstract class ItemCardapio
{
    // [ENCAPSULAMENTO] Id com 'init' imutável para não permitir alteração de chave de identificação
    public int Id { get; init; }
    public string Nome { get; set; }

    // [ENCAPSULAMENTO] Campo de apoio privado (backing field) para proteger a regra de negócio do preço
    private decimal _preco;

    /// <summary>
    /// // [ENCAPSULAMENTO] Propriedade com validação no assessor 'set'.
    /// Por que existe: Impede que qualquer item do cardápio assuma preço negativo,
    /// protegendo a integridade financeira e contábil do restaurante.
    /// </summary>
    public decimal Preco
    {
        get => _preco;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "O preço de um item do cardápio não pode ser negativo.");
            _preco = value;
        }
    }

    /// <summary>
    /// Construtor protegido da classe abstrata.
    /// Por que existe: Garante que os atributos essenciais de qualquer item sejam validados
    /// no momento de sua instanciação pelas subclasses derivadas.
    /// </summary>
    protected ItemCardapio(int id, string nome, decimal preco)
    {
        // [ENCAPSULAMENTO] Validações de integridade no momento da criação
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "O identificador do item deve ser positivo.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do item é obrigatório para exibição no cardápio.", nameof(nome));

        if (preco < 0)
            throw new ArgumentOutOfRangeException(nameof(preco), "O preço inicial do item não pode ser negativo.");

        Id = id;
        Nome = nome.Trim();
        _preco = preco;
    }

    /// <summary>
    /// // [ABSTRAÇÃO] Contrato abstrato para exibição dos detalhes específicos do produto.
    /// Por que existe: Cada tipo de item possui características físicas muito distintas (ex: volume e
    /// temperatura vs ingredientes e tempo de cozimento). A base abstrai a forma de apresentação.
    /// </summary>
    public abstract string ExibirDetalhes();

    /// <summary>
    /// // [ABSTRAÇÃO] Contrato abstrato para serialização dos dados em formato texto delimitado.
    /// Por que existe: Obriga cada produto a saber como gravar suas próprias propriedades específicas em disco.
    /// </summary>
    public abstract string ToTxt();

    /// <summary>
    /// // [POLIMORFISMO] Factory polimórfica que reconstrói a subclasse correta a partir do prefixo da linha.
    /// Por que existe: Permite que a camada de persistência leia um arquivo heterogêneo de cardápio e
    /// instancie dinamicamente Bebida ou PratoPrincipal com base no cabeçalho de cada registro.
    /// </summary>
    public static ItemCardapio FromTxt(string linha)
    {
        var partes = linha.Split(';');
        if (partes.Length < 1)
            throw new FormatException($"Linha de cardápio vazia ou inválida: '{linha}'");

        return partes[0] switch
        {
            "BEBIDA" => Bebida.FromTxt(linha),
            "PRATO_PRINCIPAL" => PratoPrincipal.FromTxt(linha),
            _ => throw new FormatException($"Tipo de item desconhecido para desserialização: '{partes[0]}'")
        };
    }
}
