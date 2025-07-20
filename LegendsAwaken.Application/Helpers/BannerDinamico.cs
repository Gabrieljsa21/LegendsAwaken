using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Helpers
{
    public class BannerDinamico
    {
        private readonly List<Raca> _todasRacas = new()
    {
        Raca.Humano,
        Raca.Bestial,
        Raca.Anao,
        Raca.Elfo,
        Raca.Draconato,
        Raca.Fada
    };

        private readonly Dictionary<Raca, int> _chancesIniciais;
        private int _chanceHumanoInicial;

        private Dictionary<Raca, int> _chances;
        private int _indexParaIncremento = 0;

        private Raca? _racaEspecial; // null para banner padrão

        public BannerDinamico(Raca? racaEspecial = null)
        {
            _racaEspecial = racaEspecial;

            // Inicializa chances iniciais e atuais
            if (_racaEspecial == null)
            {
                // Banner padrão
                _chanceHumanoInicial = 90;
                _chancesIniciais = _todasRacas.ToDictionary(
                    r => r,
                    r => r == Raca.Humano ? _chanceHumanoInicial : 2
                );
            }
            else
            {
                // Banner especial
                _chanceHumanoInicial = 90;
                _chancesIniciais = _todasRacas.ToDictionary(r =>
                    r,
                    r => r == Raca.Humano ? 90 :
                         r == _racaEspecial ? 10 : 0
                );
            }

            _chances = new Dictionary<Raca, int>(_chancesIniciais);
        }

        public Dictionary<Raca, int> GetChances() => new Dictionary<Raca, int>(_chances);

        public Raca SortearRaca()
        {
            int soma = _chances.Values.Sum();
            int roll = new Random().Next(1, soma + 1);
            int acumulado = 0;

            foreach (var kvp in _chances)
            {
                acumulado += kvp.Value;
                if (roll <= acumulado)
                    return kvp.Key;
            }

            return Raca.Humano; // fallback seguro
        }

        public void AjustarChancesDepoisDeSorteio(Raca racaSorteada)
        {
            if (racaSorteada == Raca.Humano)
            {
                // Diminui chance Humano em 1 (mínimo 0)
                if (_chances[Raca.Humano] > 0)
                    _chances[Raca.Humano]--;

                if (_racaEspecial == null)
                {
                    // Banner padrão: aumenta +1 em uma raça não Humano, em ordem cíclica
                    var racasNaoHumanas = _todasRacas.Where(r => r != Raca.Humano).ToList();
                    var racaParaIncrementar = racasNaoHumanas[_indexParaIncremento];

                    _chances[racaParaIncrementar] = _chances.GetValueOrDefault(racaParaIncrementar, 0) + 1;

                    _indexParaIncremento = (_indexParaIncremento + 1) % racasNaoHumanas.Count;
                }
                else
                {
                    // Banner especial: aumenta +1 somente na raça especial
                    _chances[_racaEspecial.Value] = _chances.GetValueOrDefault(_racaEspecial.Value, 0) + 1;
                }
            }
            else
            {
                // Saiu raça especial: reseta tudo para o estado inicial
                ResetarChances();
            }
        }

        public void ResetarChances()
        {
            _chances = new Dictionary<Raca, int>(_chancesIniciais);
            _indexParaIncremento = 0;
        }
    }

}
