using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.DTOs;

public record FragmentDropResult(
    Guid HeroiId,
    string HeroiNome,
    TipoFragmento Tipo,
    int Quantidade,
    int QuantidadeTotal
);

public record RecruitmentResult(
    bool Sucesso,
    HeroiConfig? Heroi,
    string Mensagem
);

public record RewardPayload(
    string Titulo,
    string Descricao,
    string? ImagemUrl,
    TipoReward Tipo,
    Dictionary<string, string>? Campos = null
);
