using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace LegendsAwaken.Application.Services;

/// <summary>
/// Acessa imagens armazenadas no Cloudflare R2 via API S3 privada.
/// Credenciais lidas de variáveis de ambiente: R2_ACCESS_KEY_ID e R2_SECRET_KEY.
/// </summary>
public class R2ImageService
{
    private readonly AmazonS3Client _s3;
    private readonly string _bucket;

    public R2ImageService(IConfiguration config)
    {
        var endpoint  = config["R2:Endpoint"]
            ?? throw new InvalidOperationException("R2:Endpoint não configurado em appsettings.json.");
        var accessKey = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("Variável de ambiente R2_ACCESS_KEY_ID não definida.");
        var secretKey = Environment.GetEnvironmentVariable("R2_SECRET_KEY")
            ?? throw new InvalidOperationException("Variável de ambiente R2_SECRET_KEY não definida.");

        _bucket = config["R2:Bucket"] ?? "game-assets";

        _s3 = new AmazonS3Client(
            new BasicAWSCredentials(accessKey, secretKey),
            new AmazonS3Config
            {
                ServiceURL     = endpoint,
                ForcePathStyle = true,
            });
    }

    /// <summary>
    /// Retorna o stream da imagem para a chave R2 informada (ex: "heroes/display/001.webp").
    /// Retorna null se o objeto não existir no bucket.
    /// O chamador é responsável por descartar o stream.
    /// </summary>
    public async Task<Stream?> GetAsync(string r2Key)
    {
        try
        {
            var response = await _s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucket,
                Key        = r2Key,
            });
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when ((int)ex.StatusCode == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// Atalho: busca a imagem display ou thumb de um herói pelo ID numérico (zero-padded 3 dígitos).
    /// </summary>
    public Task<Stream?> GetHeroImageAsync(int heroNumericId, bool thumb = false)
    {
        var key = thumb
            ? $"heroes/thumb/{heroNumericId:D3}.webp"
            : $"heroes/display/{heroNumericId:D3}.webp";
        return GetAsync(key);
    }
}
