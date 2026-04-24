using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Enum
{
    public enum Elemento
    {
        Fogo,
        Água,
        Terra,
        Ar,
        Luz,
        Trevas,
        Gelo,
        Raio,
        Natureza,
        Metal
    }
    public enum FuncaoTatica
    {
        Frente,
        Suporte,
        Controle,
        LongoAlcance,
        Curandeiro
    }
    public enum Raridade
    {
        Estrela1 = 1,
        Estrela2 = 2,
        Estrela3 = 3,
        Estrela4 = 4,
        Estrela5 = 5
    }
    public enum TipoAndar
    {
        Subjugacao,      // Elimine todos os inimigos
        Fuga,            // Sobreviva ou fuja por X turnos
        Escolta,         // Proteja um aliado ou carga
        Defesa,          // Resista a ondas inimigas
        Armadilha,       // Lide com efeitos aleatórios e perigosos
        EventoEspecial   // Andares com efeitos únicos ou eventos narrativos
    }

    public enum Raca
    {
        Humano,
        Bestial,
        Anao,
        Elfo,
        Draconato,
        Fada
    }

    public enum Profissao
    {
        Guerreiro,
        Arqueiro,
        Mago,
        Ladino,
        Paladino,
        Clerigo,

        Agricultor,
        Pescador,
        Caçador,
        Lenhador,
        Mineiro,
        Cozinheiro,

        Ferreiro,
        Alfaiate,
        Joalheiro,
        Alquimista,
        Construtor,
        //Engenheiro,

        //Curandeiro,
        //Professor,
        //Instrutor,
        //Artista,
        Pesquisador,

        //Mercador,
        //Diplomata,
        //Espiao,
        //Contrabandista
    }

    public enum Atributo
    {
        Forca,
        Agilidade,
        Vitalidade,
        Inteligencia,
        Percepcao
    }

    public enum TipoHabilidade
    {
        Combate,
        Craft,
        Coleta
    }

    public enum OrigemBonusAtributo
    {
        Racial,
        Profissao,
        Antecedente,
        Equipamento,
        Talento,
        LevelUp,
        Outro
    }

    public enum SlotEquipamento { Arma, Armadura, Acessorio }

    public enum Qualidade { Comum = 1, Bom = 2, Raro = 3, Excepcional = 4, Mestre = 5 }

    public enum TipoResourceNode { Campo, Floresta, Mina, Prado }

    public enum TipoPredio { Fazenda, Serraria, Mina, Forja, Arena, Guilda }

    public enum SlotTipo { Responsabilidade, Operacao }

    public enum TipoFragmento
    {
        Heroi = 1,
        Arquetipo = 2,
        Generico = 3
    }

    public enum TipoUnlock
    {
        Fragmentos = 1,
        MarcoTorre = 2,
        CondicaoUnica = 3
    }

    public enum TipoContrato
    {
        Arquetipo = 1,
        Nomeado = 2
    }

    public enum TipoEventoAlto
    {
        DescobertaBioma = 1,
        HeroiIconicoDesbloqueado = 2
    }

    public enum TipoReward { Micro, Medio, Alto }

    public enum ObjetivoOperacao { FarmRecurso, ExploracaoLeve }

    public enum PerfilRisco { Seguro, Balanceado, Agressivo }

    public enum StatusOperacao { Ativa, Concluida, Expirada }

    public enum EstadoSustento { Ativo, Instavel, Degradado, Inativo }

}
