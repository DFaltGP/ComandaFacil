namespace ComandaFacil.Models;

/// <summary>
/// // [HERANÇA] Subclasse selada (sealed) que especializa ItemCardapio para bebidas.
/// Por que existe: Reutiliza as propriedades gerais de cardápio (Id, Nome, Preço)
/// e adiciona características pertinentes a bebidas líquidas (VolumeMl e se é servida Gelada).
/// A palavra-chave 'sealed' previne derivações adicionais, melhorando a segurança e devirtualização.
/// </summary>
public sealed class Bebida : ItemCardapio
{
    // [ENCAPSULAMENTO] Atributos específicos encapsulados no tipo Bebida
    public int VolumeMl { get; set; }
    public bool Gelada { get; set; }

    /// <summary>
    /// Construtor de Bebida que delega parâmetros comuns à classe base ItemCardapio.
    /// Por que existe: Reaproveita as checagens e atribuições da classe mãe,
    /// aplicando validações especializadas para o volume em mililitros.
    /// </summary>
    public Bebida(int id, string nome, decimal preco, int volumeMl, bool gelada)
        : base(id, nome, preco)
    {
        // [ENCAPSULAMENTO] Validação de consistência física da bebida
        if (volumeMl <= 0)
            throw new ArgumentOutOfRangeException(nameof(volumeMl), "O volume da bebida em ml deve ser estritamente positivo.");

        VolumeMl = volumeMl;
        Gelada = gelada;
    }

    /// <summary>
    /// // [POLIMORFISMO] Sobrescrita especializada de ExibirDetalhes() para Bebidas.
    /// Por que existe: Apresenta informações exclusivas de bebidas (mililitros e temperatura recomendada),
    /// permitindo que o cardápio formate este item de maneira rica sem acoplamento explícito.
    /// </summary>
    public override string ExibirDetalhes()
    {
        string statusGelada = Gelada ? "Servida Gelada" : "Temperatura Ambiente";
        return $"[Bebida #{Id}] {Nome} ({VolumeMl}ml) - {statusGelada} | Preço: {Preco:C2}";
    }

    /// <summary>
    /// // [POLIMORFISMO] Sobrescrita especializada de ToTxt() para serialização de Bebidas.
    /// Por que existe: Garante que os campos 'VolumeMl' e 'Gelada' sejam gravados no arquivo
    /// texto com o discriminador de tipo 'BEBIDA', usando formato numérico invariante (ponto decimal).
    /// </summary>
    public override string ToTxt()
    {
        return $"BEBIDA;{Id};{Nome};{Preco.ToString(System.Globalization.CultureInfo.InvariantCulture)};{VolumeMl};{Gelada}";
    }

    /// <summary>
    /// Factory method para desserialização de uma linha do arquivo em instância de Bebida.
    /// Por que existe: Converte a linha de texto em objeto tipado validando todos os tokens.
    /// </summary>
    public new static Bebida FromTxt(string linha)
    {
        var partes = linha.Split(';');
        if (partes.Length < 6 || partes[0] != "BEBIDA")
            throw new FormatException($"Linha inválida para conversão em Bebida: '{linha}'");

        int id = int.Parse(partes[1]);
        string nome = partes[2];
        decimal preco = decimal.Parse(partes[3], System.Globalization.CultureInfo.InvariantCulture);
        int volumeMl = int.Parse(partes[4]);
        bool gelada = bool.Parse(partes[5]);

        return new Bebida(id, nome, preco, volumeMl, gelada);
    }
}
