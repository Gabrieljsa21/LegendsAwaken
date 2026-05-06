using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegendsAwaken.Infrastructure.Migrations
{
    public partial class AddBiomasSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT OR IGNORE INTO ""Biomas"" (""Id"", ""AndarFim"", ""AndarInicio"", ""Descricao"", ""Nome"", ""Tag"") VALUES
('B1000000-0000-0000-0000-000000000001', 25,  1,   'Floresta densa habitada por goblins, lobos e espíritos da natureza corrompidos.', 'Floresta das Sombras',     'Floresta'),
('B1000000-0000-0000-0000-000000000002', 50,  26,  'Um pântano de névoa permanente onde os mortos não ficam mortos.',                  'Pântano dos Mortos-Vivos', 'Pantano'),
('B1000000-0000-0000-0000-000000000003', 75,  51,  'Fortaleza de pedra vulcânica. Dragões de fogo e gigantes habitam seus picos.',    'Montanhas do Inferno',     'Montanha'),
('B1000000-0000-0000-0000-000000000004', 100, 76,  'Dimensão de sombras. Os demônios aqui são guardiões de segredos proibidos.',      'Plano das Sombras',        'Sombras'),
('B1000000-0000-0000-0000-000000000005', 125, 101, 'Céu eterno entre planos. Serafins caídos e guardiões celestiais.',               'Paraíso Corrompido',       'Paraiso'),
('B2000000-0000-0000-0000-000000000001', 150, 126, 'Campos abertos onde hordas se formam no horizonte e não há onde se esconder.',    'Planície dos Confrontos',  'Planicie'),
('B2000000-0000-0000-0000-000000000002', 175, 151, 'Penhascos sobre um mar furioso, dominado por sahuagins e dragões da tempestade.', 'Costa de Ferro',           'Costa'),
('B2000000-0000-0000-0000-000000000003', 200, 176, 'Névoa que consome memórias. Yuan-tis e trolls habitam este lugar de podridão.',   'Pântano do Esquecimento',  'Pantano'),
('B2000000-0000-0000-0000-000000000004', 225, 201, 'Areia negra e calor eterno. Múmias e efreetis guardam segredos de eras extintas.','Deserto das Cinzas',       'Deserto'),
('B2000000-0000-0000-0000-000000000005', 250, 226, 'Tempestades eternas de neve onde yetis e dragões brancos reinam sobre o silêncio.','Ártico das Almas',        'Artico'),
('B2000000-0000-0000-0000-000000000006', 275, 251, 'Abismo subaquático sem luz. O Kraken dorme aqui e algo pior o vigia.',           'As Profundezas',           'Profundezas'),
('B2000000-0000-0000-0000-000000000007', 300, 276, 'A Underdark: cidades Drow, devouradores de mentes e liches em seus covos eternas.','O Submundo',             'Submundo'),
('B2000000-0000-0000-0000-000000000008', 325, 301, 'Uma metrópole tomada por vampiros, rakshasas e arquimagos sem escrúpulos.',      'A Cidade Corrompida',      'Cidade')
;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM ""Biomas"" WHERE ""Id"" IN (
  'B1000000-0000-0000-0000-000000000001','B1000000-0000-0000-0000-000000000002',
  'B1000000-0000-0000-0000-000000000003','B1000000-0000-0000-0000-000000000004',
  'B1000000-0000-0000-0000-000000000005','B2000000-0000-0000-0000-000000000001',
  'B2000000-0000-0000-0000-000000000002','B2000000-0000-0000-0000-000000000003',
  'B2000000-0000-0000-0000-000000000004','B2000000-0000-0000-0000-000000000005',
  'B2000000-0000-0000-0000-000000000006','B2000000-0000-0000-0000-000000000007',
  'B2000000-0000-0000-0000-000000000008'
);");
        }
    }
}
