namespace ComandaFacil.Models;

/// <summary>
/// // [ABSTRAÇÃO] Classe abstrata que modela a entidade conceitual 'Pessoa'.
/// Por que existe: Isola os atributos essenciais comuns a qualquer ser humano no sistema (Id, Nome, Telefone)
/// e impede a instanciação direta de uma pessoa genérica sem papel definido no negócio.
/// </summary>
public abstract class Pessoa
{
    // [ENCAPSULAMENTO] Id com 'init' para garantir integridade e imutabilidade após a criação.
    public int Id { get; init; }

    // [ENCAPSULAMENTO] Propriedades públicas que protegem o estado exigindo validação via construtor.
    public string Nome { get; set; }
    public string Telefone { get; set; }

    /// <summary>
    /// Construtor protegido (protected) da classe abstrata.
    /// Por que existe: Garante que apenas subclasses derivadas possam inicializar uma Pessoa,
    /// validando regras de preenchimento obrigatório para evitar dados inconsistentes em memória.
    /// </summary>
    protected Pessoa(int id, string nome, string telefone)
    {
        // [ENCAPSULAMENTO] Validações defensivas para preservar a consistência do objeto
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "O Id da pessoa deve ser estritamente maior que zero para garantir unicidade.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome não pode ser vazio para que a pessoa possa ser identificada.", nameof(nome));

        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("O telefone de contato é indispensável para comunicação com o restaurante.", nameof(telefone));

        Id = id;
        Nome = nome.Trim();
        Telefone = telefone.Trim();
    }

    /// <summary>
    /// // [ABSTRAÇÃO] Método abstrato que define o contrato obrigatório de apresentação da pessoa.
    /// Por que existe: Cada subclasse (como Cliente ou Funcionário) possui regras de apresentação
    /// e documentos próprios (ex: CPF), logo a base estabelece a interface comum sem acoplar detalhes concretos.
    /// </summary>
    public abstract string ExibirIdentificacao();
}
