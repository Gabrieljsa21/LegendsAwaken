using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Infrastructure.SeedData
{
    public static class HabilidadesSeed
    {
        public static void PopularHabilidades(LegendsAwakenDbContext context)
        {
            if (context.Habilidades.Any()) return;

            var habilidadesIniciais = new List<Habilidade>
            {
                new Habilidade
                {
                    Id = "combate-proficiencia-espadas",
                    Nome = "Proficiência com Espadas",
                    Descricao = "Treinamento básico no uso de espadas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Guerreiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Forca,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "combate-proficiencia-arcos",
                    Nome = "Proficiência com Arcos",
                    Descricao = "Treinamento básico no uso de arcos.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Arqueiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Agilidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "combate-controle-magico-basico",
                    Nome = "Controle Mágico Básico",
                    Descricao = "Permite conjurar feitiços simples.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Mago,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Inteligencia,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "combate-proficiencia-adagas",
                    Nome = "Proficiência com Adagas",
                    Descricao = "Treinamento no uso de adagas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Ladino,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Agilidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        },
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        }
                    }
                },
                new Habilidade
                {
                    Id = "combate-disciplina-sagrada",
                    Nome = "Disciplina Sagrada",
                    Descricao = "Base da fé e do foco espiritual.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Paladino,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Vitalidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "combate-canalizacao-espiritual",
                    Nome = "Canalização Espiritual",
                    Descricao = "Permite canalizar energias curativas básicas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Combate,
                    ProfissaoVinculada = Profissao.Clerigo,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Inteligencia,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-agricultura-basica",
                    Nome = "Agricultura Básica",
                    Descricao = "Permite cultivar e colher plantações simples.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Agricultor,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-pesca-basica",
                    Nome = "Pesca Básica",
                    Descricao = "Permite pescar em águas rasas com técnicas simples.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Pescador,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-rastreamento",
                    Nome = "Rastreamento",
                    Descricao = "Permite rastrear pegadas e sinais de animais.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Caçador,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        },
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Agilidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-corte-madeira",
                    Nome = "Corte de Madeira",
                    Descricao = "Permite cortar árvores para obter madeira bruta.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Lenhador,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Forca,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-mineracao",
                    Nome = "Mineração Básica",
                    Descricao = "Permite extrair minérios de jazidas simples.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Mineiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Forca,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        },
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Vitalidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        }
                    }
                },
                new Habilidade
                {
                    Id = "coleta-preparo-ingredientes",
                    Nome = "Preparo de Ingredientes",
                    Descricao = "Habilidade para limpar, cortar e preparar ingredientes.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Coleta,
                    ProfissaoVinculada = Profissao.Cozinheiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Inteligencia,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        },
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-forja-basica",
                    Nome = "Forja Básica",
                    Descricao = "Permite criar armas e equipamentos simples em forjas rudimentares.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Ferreiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Forca,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-costura-basica",
                    Nome = "Costura Básica",
                    Descricao = "Permite confeccionar roupas simples e reparar trajes danificados.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Alfaiate,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-lapidacao",
                    Nome = "Lapidação",
                    Descricao = "Permite moldar e refinar gemas para criação de joias básicas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Joalheiro,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Percepcao,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-alquimia-basica",
                    Nome = "Alquimia Básica",
                    Descricao = "Permite criar poções simples e manipular substâncias básicas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Alquimista,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Inteligencia,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-construcao-basica",
                    Nome = "Construção Básica",
                    Descricao = "Permite erguer e reparar estruturas simples como casas e muralhas.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Construtor,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Vitalidade,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        },
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Forca,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 1
                        }
                    }
                },
                new Habilidade
                {
                    Id = "craft-pesquisa-basica",
                    Nome = "Pesquisa Básica",
                    Descricao = "Capacidade de realizar estudos para descobrir novas fórmulas, materiais ou tecnologias.",
                    Rank = 1,
                    TipoHabilidade = TipoHabilidade.Craft,
                    ProfissaoVinculada = Profissao.Pesquisador,
                    HabilidadeBonusAtributos = new List<HabilidadeBonusAtributos>
                    {
                        new HabilidadeBonusAtributos {
                            Atributo = Atributo.Inteligencia,
                            BonusTipo = BonusTipo.Atributo,
                            BonusValor = 2
                        }
                    }
                }


            };

            context.Habilidades.AddRange(habilidadesIniciais);
            context.SaveChanges();
        }
    }
}
