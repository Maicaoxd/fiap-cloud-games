# Fiap Cloud Games

Projeto acadêmico desenvolvido para a FIAP Pos Tech em Arquitetura de Sistemas .NET, no contexto do Tech Challenge da plataforma Cloud Games.

## Contexto

O objetivo da fase 1 é entregar um MVP backend para gerenciamento de usuários, jogos e biblioteca de jogos adquiridos, com foco em:

- modelagem orientada a domínio
- arquitetura limpa
- autenticação e autorização
- persistência relacional
- qualidade de código e testes

Observação: o desafio original fazia referência a .NET 8, porém este projeto foi desenvolvido em .NET 10 com autorização do professor.

## Objetivo da solução

Disponibilizar uma API REST para:

- cadastro e autenticação de usuários
- manutenção de perfil de usuário
- gestão administrativa de usuários
- gestão administrativa de jogos
- aquisição e consulta da biblioteca de jogos do usuário autenticado

## Escopo implementado

### Usuários

- cadastro de usuário
- autenticação com JWT
- troca de senha pelo próprio usuário autenticado
- redefinição de senha via fluxo de recuperação
- atualização do próprio perfil
- atualização administrativa de usuário
- desativação de usuário por administrador
- listagem de usuários por administrador

### Jogos

- cadastro de jogo por administrador
- listagem de jogos ativos
- consulta de jogo por identificador
- atualização de jogo por administrador
- desativação de jogo por administrador

### Biblioteca

- aquisição de jogo pelo usuário autenticado
- listagem da biblioteca do usuário autenticado

## Decisões arquiteturais

### Arquitetura

O projeto foi estruturado como um monólito modular com separação em camadas:

- `FCG.Domain`: regras de negócio e modelo de domínio
- `FCG.Application`: casos de uso e contratos de aplicação
- `FCG.Infrastructure`: persistência, segurança e integrações técnicas
- `FCG.Api`: camada HTTP, autenticação, middleware, contratos e documentação

### Padrões e práticas adotados

- Clean Architecture
- DDD
- TDD
- Entity Framework Core com abordagem Code First
- Fluent API para mapeamento relacional
- autenticação com JWT
- autorização por role (`User` e `Administrator`)
- middleware global para tratamento de exceções
- logging estruturado com Serilog
- Swagger para documentação e execução dos endpoints

### Modelagem DDD

O domínio foi organizado em três contextos centrais:

- `Users`
- `Games`
- `Libraries`

Foram utilizados conceitos táticos de DDD como:

- Entities
- Value Objects
- Repositories
- Use Cases / Application Services

## Event Storming e documentação DDD

A documentação de Event Storming e modelagem DDD está disponível no Miro:

- [Board de Event Storming / DDD](https://miro.com/app/live-embed/uXjVGj7nr7s=/?embedMode=view_only_without_ui&moveToViewport=-673%2C-374%2C2165%2C1027&embedId=38068290877)

## Estrutura da solução

```text
src/
  FCG.Domain/
  FCG.Application/
  FCG.Infrastructure/
  FiapCloudGames/   -> projeto da API

tests/
  FCG.Tests/
```

## Tecnologias utilizadas

- .NET 10
- ASP.NET Core
- Entity Framework Core
- SQL Server / LocalDB
- JWT Bearer Authentication
- Serilog
- Swashbuckle / Swagger
- xUnit
- NSubstitute
- Shouldly

## Pré-requisitos

Para executar o projeto localmente, recomenda-se o seguinte ambiente:

- .NET 10 SDK
- SQL Server LocalDB ou SQL Server
- `dotnet-ef` instalado globalmente

Comando para instalar o `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef
```

Caso o comando já exista na sua máquina:

```powershell
dotnet ef --version
```

## Configuração de ambiente

O projeto utiliza, por padrão, a seguinte connection string em [appsettings.json](src/FiapCloudGames/appsettings.json):

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FiapCloudGames;Trusted_Connection=True;TrustServerCertificate=True"
```

Se você quiser usar outra instância de SQL Server, ajuste esse valor no arquivo `appsettings.json`.

### JWT

As configurações de JWT também estão no `appsettings.json`:

- `Issuer`
- `Audience`
- `Secret`
- `ExpirationMinutes`

Observação: a secret atual é apenas para uso acadêmico e desenvolvimento local.

## Como preparar o banco de dados

### 1. Restaurar pacotes

```powershell
dotnet restore FiapCloudGames.slnx
```

### 2. Aplicar as migrations

```powershell
dotnet ef database update --project src\FCG.Infrastructure\FCG.Infrastructure.csproj --startup-project src\FiapCloudGames\FCG.Api.csproj
```

### Observação importante sobre a migration de recuperação de usuário

A migration [20260425201013_AddUserRecoveryData.cs](src/FCG.Infrastructure/Persistence/Migrations/20260425201013_AddUserRecoveryData.cs) assume uma base limpa ou contendo apenas o usuário administrador seedado.

Se você já tiver usuários antigos criados antes dessa migration, o caminho mais seguro é:

- recriar a base local, ou
- tratar/backfill desses dados antes de aplicar a migration

## Seed do administrador

O projeto possui seed de usuário administrador na migration [20260421171937_SeedAdministratorUser.cs](src/FCG.Infrastructure/Persistence/Migrations/20260421171937_SeedAdministratorUser.cs).

Dados relevantes:

- e-mail: `admin@email.com`

Além disso, a migration de dados de recuperação define:

- CPF: `52998224725`
- data de nascimento: `1990-01-01`

Caso você precise definir uma senha para o administrador em ambiente local, pode usar o endpoint de recuperação de senha com esses dados.

## Como executar a aplicação

### Via terminal

```powershell
dotnet run --project src\FiapCloudGames\FCG.Api.csproj
```

### URLs padrão

Conforme [launchSettings.json](src/FiapCloudGames/Properties/launchSettings.json):

- HTTP: [http://localhost:5205/swagger](http://localhost:5205/swagger)
- HTTPS: [https://localhost:7164/swagger](https://localhost:7164/swagger)

O Swagger abre automaticamente em ambiente de desenvolvimento.

## Como executar os testes

```powershell
dotnet test tests\FCG.Tests\FCG.Tests.csproj -m:1
```

Estado atual da suíte no momento desta documentação:

- 215 testes unitários passando

## Logging

O projeto utiliza Serilog com escrita em:

- console
- arquivo em `logs/`

Os logs são úteis para acompanhar:

- requisições HTTP
- exceções tratadas
- eventos técnicos da aplicação

## Principais endpoints

### Autenticação

- `POST /api/auth/login`
- `POST /api/auth/forgot-password`

### Usuários

- `GET /api/users` (admin)
- `POST /api/users`
- `PUT /api/users/me`
- `PATCH /api/users/me/password`
- `PUT /api/users/{userId}` (admin)
- `PATCH /api/users/{userId}/deactivate` (admin)

### Jogos

- `GET /api/games`
- `GET /api/games/{gameId}`
- `POST /api/games` (admin)
- `PUT /api/games/{gameId}` (admin)
- `PATCH /api/games/{gameId}/deactivate` (admin)

### Biblioteca

- `GET /api/library/games`
- `POST /api/library/games/{gameId}`

## Segurança e autorização

O projeto trabalha com dois perfis:

- `User`
- `Administrator`

Regras principais:

- cadastro é público
- login é público
- gestão de jogos é administrativa
- gestão administrativa de usuários é restrita a administradores
- biblioteca exige usuário autenticado

## Estratégia de testes

O projeto foi desenvolvido com foco em TDD e atualmente possui cobertura unitária para:

- domínio
- casos de uso da aplicação
- controllers
- middleware
- extensões da API
- serviços de segurança

As bibliotecas de teste utilizadas são:

- xUnit
- NSubstitute
- Shouldly

## Possíveis evoluções futuras

Algumas evoluções que podem ser aplicadas em fases seguintes:

- testes de integração e testes HTTP
- paginação e filtros em listagens
- observabilidade expandida
- política de refresh token
- versionamento de API
- automação de pipeline CI/CD

## Autor

Projeto desenvolvido por Maicon Guedes no contexto acadêmico da FIAP Pos Tech em Arquitetura de Sistemas .NET.
