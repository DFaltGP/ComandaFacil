namespace ComandaFacil.Models;

/// <summary>
/// // [HERANÇA] Subclasse concreta selada (sealed) que herda diretamente de Pessoa.
/// Por que existe: Reutiliza a estrutura de atributos de Pessoa (Id, Nome, Telefone)
/// e especializa o modelo adicionando as responsabilidades e dados tributários/cadastrais de um cliente (CPF).
/// O modificador 'sealed' impede extensões desnecessárias, fechando a hierarquia de herança.
/// </summary>
public sealed class Cliente : Pessoa
{
    // [ENCAPSULAMENTO] Propriedade com dados específicos do cliente
    public string Cpf { get; set; }

    /// <summary>
    /// Construtor de Cliente invocando o construtor base da Pessoa.
    /// Por que existe: Repassa Id, Nome e Telefone para a classe base validar e armazenar,
    /// validando localmente as regras de integridade do CPF.
    /// </summary>
    public Cliente(int id, string nome, string telefone, string cpf)
        : base(id, nome, telefone)
    {
        // [ENCAPSULAMENTO] Proteção contra cadastro de clientes sem documento identificador
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("O CPF do cliente é obrigatório para emissão de pedidos e comprovantes.", nameof(cpf));

        Cpf = cpf.Trim();
    }

    /// <summary>
    /// // [POLIMORFISMO] Sobrescrita (override) do método abstrato da classe base Pessoa.
    /// Por que existe: Permite que a identificação do cliente contenha seu CPF e prefixo específico,
    /// respondendo polimorficamente quando referenciado genericamente como Pessoa.
    /// </summary>
    public override string ExibirIdentificacao()
    {
        return $"[Cliente #{Id}] {Nome} | Tel: {Telefone} | CPF: {Cpf}";
    }

    /// <summary>
    /// Serialização dos dados do cliente em linha de texto (.txt).
    /// Por que existe: Desacopla a estrutura de persistência, permitindo exportar os dados
    /// em formato padronizado com delimitadores de ponto e vírgula sem bibliotecas externas.
    /// </summary>
    public string ToTxt()
    {
        return $"CLIENTE;{Id};{Nome};{Telefone};{Cpf}";
    }

    /// <summary>
    /// Factory method para reconstruir a entidade a partir de uma linha persistida.
    /// Por que existe: Centraliza a regra de parsing da persistência na própria classe,
    /// mantendo coesão e facilitando manutenções futuras caso novos campos sejam adicionados.
    /// </summary>
    public static Cliente FromTxt(string linha)
    {
        var partes = linha.Split(';');
        if (partes.Length < 5 || partes[0] != "CLIENTE")
            throw new FormatException($"Linha de texto incompatível com o formato de Cliente: '{linha}'");

        int id = int.Parse(partes[1]);
        string nome = partes[2];
        string telefone = partes[3];
        string cpf = partes[4];

        return new Cliente(id, nome, telefone, cpf);
    }
}
