namespace ComandaFacil.Repositories;

/// <summary>
/// // [ENCAPSULAMENTO] Repositório concreto para persistência de dados em arquivos de texto (.txt).
/// Por que existe: Isola toda a complexidade de manipulação de caminhos em disco, criação de pastas
/// e chamadas diretas às APIs nativas de leitura e gravação do .NET (System.IO.File e Directory),
/// impedindo que regras de infraestrutura e persistência vazem para a camada de negócios.
/// </summary>
public sealed class RepositorioTxt
{
    // [ENCAPSULAMENTO] Campo privado que guarda o caminho físico absoluto do diretório de dados
    private readonly string _caminhoDiretorio;

    /// <summary>
    /// Construtor que inicializa e garante a existência do diretório de armazenamento no disco.
    /// Por que existe: Previne erros em tempo de execução ao tentar salvar arquivos caso a pasta
    /// de destino ainda não tenha sido criada no sistema de arquivos.
    /// </summary>
    public RepositorioTxt(string? caminhoDiretorio = null)
    {
        _caminhoDiretorio = string.IsNullOrWhiteSpace(caminhoDiretorio)
            ? Path.Combine(AppContext.BaseDirectory, "dados_restaurante")
            : caminhoDiretorio;

        if (!Directory.Exists(_caminhoDiretorio))
        {
            Directory.CreateDirectory(_caminhoDiretorio);
        }
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Grava linhas de texto em arquivo específico dentro da pasta gerenciada.
    /// Por que existe: Centraliza a escrita em disco com tratamento de parâmetros defensivos.
    /// </summary>
    public void SalvarLinhas(string nomeArquivo, List<string> linhas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeArquivo, "O nome do arquivo a ser salvo não pode ser nulo ou vazio.");
        ArgumentNullException.ThrowIfNull(linhas, "A lista de linhas a ser persistida não pode ser nula.");

        string caminhoCompleto = Path.Combine(_caminhoDiretorio, nomeArquivo);
        File.WriteAllLines(caminhoCompleto, linhas);
    }

    /// <summary>
    /// // [ENCAPSULAMENTO] Lê linhas de arquivo em disco retornando lista vazia defensiva se não existir.
    /// Por que existe: Elimina o risco de 'FileNotFoundException' quebrar a inicialização do sistema,
    /// permitindo que o serviço popule dados iniciais na primeira inicialização da aplicação.
    /// </summary>
    public List<string> LerLinhas(string nomeArquivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeArquivo, "O nome do arquivo para leitura não pode ser nulo ou vazio.");

        string caminhoCompleto = Path.Combine(_caminhoDiretorio, nomeArquivo);

        if (!File.Exists(caminhoCompleto))
        {
            return [];
        }

        return [.. File.ReadAllLines(caminhoCompleto)];
    }
}
