using System.Collections.Generic;
using LegendsAwaken.Application.DTOs;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.Services;

public class RewardDistributionService
{
    public RewardPayload GerarMicroPico(FragmentDropResult drop) =>
        new(
            Titulo: "Fragmento obtido!",
            Descricao: $"+{drop.Quantidade} fragmento(s) de **{drop.HeroiNome}** — {drop.QuantidadeTotal} acumulados",
            ImagemUrl: null,
            Tipo: TipoReward.Micro);

    public RewardPayload GerarPicoMedio(HeroiConfig heroi) =>
        new(
            Titulo: $"✨ {heroi.Nome} recrutado!",
            Descricao: $"Após uma longa jornada, **{heroi.Nome}** finalmente se junta à sua equipe.",
            ImagemUrl: null,
            Tipo: TipoReward.Medio,
            Campos: new Dictionary<string, string>
            {
                ["Raridade"]  = $"{(int)heroi.RaridadeBase}⭐",
                ["Arquétipo"] = heroi.Arquetipo.ToString()
            });

    public RewardPayload GerarPicoAlto(TipoEventoAlto tipo, Bioma? bioma = null, HeroiConfig? heroi = null)
    {
        return tipo switch
        {
            TipoEventoAlto.DescobertaBioma when bioma is not null =>
                new RewardPayload(
                    Titulo: $"🗺️ Novo Bioma: {bioma.Nome}",
                    Descricao: bioma.Descricao,
                    ImagemUrl: null,
                    Tipo: TipoReward.Alto),

            TipoEventoAlto.HeroiIconicoDesbloqueado when heroi is not null =>
                new RewardPayload(
                    Titulo: $"⚔️ {heroi.Nome} se manifesta!",
                    Descricao: $"Um guerreiro lendário surge diante de você. **{heroi.Nome}** decide acompanhar sua jornada.",
                    ImagemUrl: null,
                    Tipo: TipoReward.Alto,
                    Campos: new Dictionary<string, string>
                    {
                        ["Raridade"]  = $"{(int)heroi.RaridadeBase}⭐",
                        ["Arquétipo"] = heroi.Arquetipo.ToString()
                    }),

            _ => new RewardPayload("Recompensa", "Você obteve uma recompensa.", null, TipoReward.Alto)
        };
    }
}
