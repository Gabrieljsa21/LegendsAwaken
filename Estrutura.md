# 📁 Estrutura de Pastas – Legends Awaken

LegendsAwaken/
├── LegendsAwaken.sln
├── README.md
├── TODO.md
├── .gitignore
│
├── LegendsAwaken.Bot/
│ ├── Program.cs
│ ├── BotConfig.cs
│ ├── CommandHandler.cs
│ ├── Commands/
│ │ ├── InvocarCommand.cs
│ │ ├── SubirAndarCommand.cs
│ │ ├── VerHeroiCommand.cs
│ │ └── TreinarCommand.cs
│ └── Helpers/
│ └── EmbedHelper.cs
│
├── LegendsAwaken.Application/
│ ├── Services/
│ │ ├── HeroiService.cs
│ │ ├── TorreService.cs
│ │ ├── GachaService.cs
│ │ ├── TreinamentoService.cs
│ │ └── CidadeService.cs
│ └── DTOs/
│ └── GachaResultadoDTO.cs
│
├── LegendsAwaken.Domain/
│ ├── Heroi.cs
│ ├── Habilidade.cs
│ ├── TorreAndar.cs
│ ├── Cidade.cs
│ ├── Usuario.cs
│ └── Enum/
│ ├── Raridade.cs
│ ├── TipoHabilidade.cs
│ └── TipoAndar.cs
│
├── LegendsAwaken.Infrastructure/
│ ├── LegendsAwakenDbContext.cs
│ ├── Migrations/
│ │ └── .placeholder
│ └── Repositories/
│ ├── HeroiRepository.cs
│ ├── TorreRepository.cs
│ └── CidadeRepository.cs
│
├── LegendsAwaken.Data/
│ ├── herois_base.json
│ ├── habilidades.json
│ └── classes.json
│
└── LegendsAwaken.Tests/
└── .placeholder