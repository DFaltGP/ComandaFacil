namespace ComandaFacil.Models;

/// <summary>
/// // [HERANÇA] Subclasse selada (sealed) que especializa ItemCardapio para pratos culinários.
/// Por que existe: Reaproveita os atributos gerais de produto do menu (Id, Nome, Preço)
/// e adiciona aspectos específicos da cozinha (Ingredientes da receita e Tempo de Preparo estimado em minutos).
/// </summary>
public sealed class PratoPrincipal : ItemCardapio
{
    // [ENCAPSULAMENTO] Atributos específicos protegidos e manipulados através de propriedades
    public string Ingredientes { get; set; }
    public int TempoPreparoMinutos { get; set; }

    /// <summary>
    /// Construtor de PratoPrincipal que repassa dados gerais para a base ItemCardapio.
    /// Por que existe: Garante a consistência da inicialização das propriedades básicas,
    /// validando obrigatoriedade da descrição dos ingredientes e tempo de cozinha não-negativo.
    /// </summary>
    public PratoPrincipal(int id, string nome, decimal preco, string ingredientes, int tempoPreparoMinutos)
        : base(id, nome, preco)
    {
        // [ENCAPSULAMENTO] Validações defensivas para proteção de integridade
        if (string.IsNullOrWhiteSpace(ingredientes))
            throw new ArgumentException("A descrição dos ingredientes é obrigatória para orientar o cliente e a cozinha.", nameof(ingredientes));

        if (tempoPreparoMinutos < 0)
            throw new ArgumentOutOfRangeException(nameof(tempoPreparoMinutos), "O tempo de preparo não pode ser negativo.");

        Ingredientes = ingredientes.Trim();
        TempoPreparoMinutos = tempoPreparoMinutos;
    }

    /// <summary>
    /// // [POLIMORFISMO] Sobrescrita especializada de ExibirDetalhes() para Pratos Principais.
    /// Por que existe: Exibe a lista completa de ingredientes e a estimativa de tempo de preparo da cozinha,
    /// permitindo que o cliente tome decisões de pedido com base no tempo de espera e restrições alimentares.
    /// </summary>
    public override string ExibirDetalhes()
    {
        return $"[Prato #{Id}] {Nome} | Preço: {Preco:C2} | Preparo: ~{TempoPreparoMinutos} min\n    Ingredientes: {Ingredientes}";
    }

    /// <summary>
    /// // [POLIMORFISMO] Sobrescrita especializada de ToTxt() para serialização de Pratos.
    /// Por que existe: Persiste os campos 'Ingredientes' e 'TempoPreparoMinutos' identificados
    /// com a tag 'PRATO_PRINCIPAL' para viabilizar a reconstituição em disco.
    /// </summary>
    public override string ToTxt()
    {
        return $"PRATO_PRINCIPAL;{Id};{Nome};{Preco.ToString(System.Globalization.CultureInfo.InvariantCulture)};{Ingredientes};{TempoPreparoMinutos}";
    }

    /// <summary>
    /// Factory method para desserialização de uma linha do arquivo texto em objeto PratoPrincipal.
    /// Por que existe: Reconstrói com segurança a instância tipada e com seus devidos tipos numéricos.
    /// </summary>
    public new static PratoPrincipal FromTxt(string linha)
    {
        var partes = linha.Split(';');
        if (partes.Length < 6 || partes[0] != "PRATO_PRINCIPAL")
            throw new FormatException($"Linha inválida para conversão em PratoPrincipal: '{linha}'");

        int id = int.Parse(partes[1]);
        string nome = partes[2];
        decimal preco = decimal.Parse(partes[3], System.Globalization.CultureInfo.InvariantCulture);
        string ingredientes = partes[4];
        int tempoPreparo = int.Parse(partes[5]);

        return new PratoPrincipal(id, nome, preco, ingredientes, tempoPreparo);
    }
}
