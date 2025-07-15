# 🌟 Legends Awaken

**Legends Awaken** é um bot para Discord que combina elementos de RPG tático, gacha e progressão em torre infinita.

Neste projeto, o jogador assume o papel de **Mestre**, invocando heróis via gacha, treinando-os e montando grupos para enfrentar os desafios da torre infinita.

---

## ⚔️ Funcionalidades Planejadas

- Comandos `/invocar`, `/treinar`, `/subir_andar`, `/ver_heroi` e outros.
- Sistema de **invocação gacha** com raridades variadas.
- Heróis com atributos, habilidades ativas/passivas e progressão individual.
- **Combate automático** com lógica simples de IA.
- Torre infinita com desafios variados:
  - Subjugação, fuga, escolta, etc.
  - Boss fácil a cada 5 andares, médio a cada 10 e difícil a cada 25.
- Gestão de **cidade** com heróis alocados em profissões não-combatentes.
- Armazenamento em **SQLite** local com suporte a múltiplos usuários (por ID do Discord).

---

## 🧱 Arquitetura e Estrutura

### 🧠 Metodologia Adotada

O projeto segue os princípios da **Clean Architecture** com influência de **Domain-Driven Design (DDD)**. Essa estrutura favorece a separação de responsabilidades, testabilidade e escalabilidade.

### 📦 Camadas do Projeto

| Camada           | Responsabilidade                                                      |
| ---------------- | --------------------------------------------------------------------- |
| `Domain`         | Regras de negócio puras, entidades, enums e invariantes               |
| `Application`    | Orquestra casos de uso, serviços de aplicação e DTOs                  |
| `Infrastructure` | Acesso ao banco de dados, implementações de repositórios              |
| `Bot`            | Interface com o Discord (comandos, handlers, interação com usuários)  |
| `Data`           | Arquivos JSON estáticos para base inicial (heróis, habilidades, etc.) |
| `Tests`          | Testes automatizados de funcionalidades e lógica de negócio           |

---

## 🛠️ Tecnologias e Ferramentas

- C# (.NET 7 ou 8)
- Visual Studio 2022 Community
- Discord.Net (ou outra lib C# para bots)
- SQLite (via `Microsoft.EntityFrameworkCore.Sqlite`)
- GitHub (repositório público)

---

## 📂 Estrutura de Pastas (sugerida)

```
LegendsAwaken/
├── LegendsAwaken.sln
├── README.md
├── TODO.md
├── .gitignore
│
├── LegendsAwaken.Bot/            # Projeto principal do bot (Discord)
│   ├── Program.cs
│   ├── BotConfig.cs
│   ├── CommandHandler.cs
│   ├── Commands/
│   └── Helpers/
│
├── LegendsAwaken.Application/    # Casos de uso e regras de aplicação
│   ├── Services/
│   └── DTOs/
│
├── LegendsAwaken.Domain/         # Entidades e regras de negócio
│   ├── Enum/
│
├── LegendsAwaken.Infrastructure/ # Banco de dados e repositórios
│   ├── Migrations/
│   └── Repositories/
│
├── LegendsAwaken.Data/           # Arquivos JSON com dados base
│
├── LegendsAwaken.Tests/          # Projeto de testes automatizados
└──
```

---

## 💾 Banco de Dados

A base será um arquivo `.db` SQLite contendo:

- `Heroi`: informações do herói invocado
- `Habilidade`: habilidades ligadas ao herói
- `TorreAndar`: dados dos andares e histórico
- `Cidade`: dados da cidade e profissões alocadas
- `Usuario`: dados básicos por ID de Discord

Você pode inspecionar ou editar o banco com ferramentas como [DB Browser for SQLite](https://sqlitebrowser.org/).

---

## 🔧 Como rodar

1. Clone o repositório:

   ```bash
   git clone https://github.com/seu-usuario/LegendsAwaken.git
   ```

2. Instale os pacotes:

   ```bash
   dotnet restore
   ```

3. Crie o banco de dados:

   ```bash
   dotnet ef database update
   ```

4. Rode o projeto:

   ```bash
   dotnet run --project LegendsAwaken.Bot
   ```

5. Adicione o bot ao Discord com o token gerado no [Discord Developer Portal](https://discord.com/developers/applications)

---

## 📦 Próximos passos

- ✅ Definir estrutura JSON dos heróis
- ✅ Criar estrutura base da torre
- ⏳ Implementar sistema de combate automático
- ⏳ Criar lógica de gacha
- ⏳ Implementar treinamento e evolução
- ⏳ Adicionar profissões e cidade
- ⏳ Refinar comandos e feedback no Discord

