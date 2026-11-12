# Pisco

App pessoal em Blazor Server (.NET 8) com PostgreSQL.

- [1. Instalar os requisitos](#1-instalar-os-requisitos)
- [2. Obter e configurar o projeto](#2-obter-e-configurar-o-projeto)
- [3. Correr](#3-correr)
- [4. Atalhos do VS Code](#4-atalhos-do-vs-code)
- [5. Primeiro acesso](#5-primeiro-acesso)
- [Base de dados](#base-de-dados)
- [Problemas comuns](#problemas-comuns)
- [Estrutura do projeto](#estrutura-do-projeto)

Detalhes sobre as imagens, os volumes e o debug em Docker: [docs/docker.md](docs/docker.md).

## 1. Instalar os requisitos

| Ferramenta | Para quê |
|---|---|
| **Git** | Obter o projeto |
| **.NET 8 SDK** | Compilar e correr a app fora do Docker |
| **Docker** (com Docker Compose) | Correr o PostgreSQL e, se quiseres, a própria app |
| **VS Code** | Editar e fazer debug (opcional) |

### Linux (Ubuntu / Debian) e WSL

```bash
sudo apt update
```

```bash
sudo apt install -y git dotnet-sdk-8.0
```

Docker: segue https://docs.docker.com/engine/install/ubuntu/ (ou, no WSL, instala o **Docker Desktop** no Windows e ativa *Settings → Resources → WSL integration*).

Para usares o Docker sem `sudo` (depois fecha e volta a abrir o terminal):

```bash
sudo usermod -aG docker $USER
```

### Windows / macOS

- .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Docker Desktop: https://www.docker.com/products/docker-desktop/
- Git: https://git-scm.com/downloads

### VS Code

Ao abrires a pasta, o VS Code sugere as extensões do projeto (`.vscode/extensions.json`). Aceita:

- **C# Dev Kit** — compilar e fazer debug
- **Container Tools** — ver contentores, imagens e logs
- **PostgreSQL** — ver a base de dados dentro do VS Code

### Confirmar a instalação

```bash
dotnet --version
```

```bash
docker compose version
```

O `dotnet --version` deve mostrar `8.x`.

## 2. Obter e configurar o projeto

```bash
git clone git@github.com:afbc04/pisco.git
```

```bash
cd pisco
```

```bash
cp .env.example .env
```

O `.env` não vai para o git. É lido pela app (quando corre fora do Docker) e pelo Docker Compose:

| Variável | Por omissão | Para que serve |
|---|---|---|
| `POSTGRES_HOST` | `localhost` | Onde está o PostgreSQL (a app em Docker usa sempre `db`) |
| `POSTGRES_PORT` | `5432` | Porta do PostgreSQL no teu PC |
| `POSTGRES_DB` | `pisco` | Nome da base de dados |
| `POSTGRES_USER` | `pisco` | Utilizador da base de dados |
| `POSTGRES_PASSWORD` | `pisco` | Palavra-passe da base de dados |
| `PISCO_PORT` | `5080` | Porta da app quando corre em Docker |

## 3. Correr

Há três formas. Todas usam o PostgreSQL em Docker.

| Forma | Endereço | Quando usar |
|---|---|---|
| **a) Local** — a app corre no teu PC | http://localhost:5000 | Dia a dia, debug rápido |
| **b) Docker** — tudo em contentores | http://localhost:5080 | Ver a app como fica "em produção" |
| **c) Docker em debug** — contentor com debugger | http://localhost:5080 | Investigar algo que só acontece no contentor |

### a) Local

No VS Code: **F5** com **Pisco (local)**. Ou no terminal:

```bash
docker compose up -d --wait db
```

```bash
dotnet run
```

### b) Docker

```bash
docker compose up -d --build --wait
```

### c) Docker em debug

No VS Code: **F5** com **Pisco (Docker)**. O VS Code constrói a imagem de debug, arranca o contentor e liga o debugger — os breakpoints funcionam como em local. Ao parar o debug, o contentor continua a correr.

Sem o VS Code, o contentor de debug arranca com:

```bash
docker compose -f docker-compose.yml -f docker-compose.debug.yml up -d --build --wait
```

Para voltar à imagem normal, corre outra vez o comando da forma **b)**.

## 4. Atalhos do VS Code

| Atalho | O que faz |
|---|---|
| **F5** | Começa o debug com a configuração escolhida no painel *Run and Debug* (**Ctrl+Shift+D**) |
| **Shift+F5** | Para o debug |
| **Ctrl+Shift+F5** | Reinicia o debug |
| **F9** | Põe / tira um breakpoint na linha |
| **F10** / **F11** | Avança uma linha / entra na função |
| **Ctrl+Shift+B** | Compila o projeto |
| **Ctrl+Shift+P** → *Tasks: Run Task* | Escolhe uma das tarefas abaixo |

### Configurações de debug (`.vscode/launch.json`)

| Nome | O que faz |
|---|---|
| **Pisco (local)** | Arranca o PostgreSQL, compila, corre a app e abre o browser em http://localhost:5000 |
| **Pisco (Docker)** | Constrói e arranca o contentor de debug e liga o debugger à app dentro dele |

### Tarefas (`.vscode/tasks.json`)

| Tarefa | Comando |
|---|---|
| `build` | Compila o projeto |
| `start` | `db: up` + `build` (usada pelo **Pisco (local)**) |
| `db: up` | Arranca só o PostgreSQL |
| `db: stop` | Para o PostgreSQL (os dados ficam) |
| `db: psql` | Abre a consola `psql` dentro do contentor |
| `db: reset (apaga os dados)` | Pede confirmação e apaga a base de dados e todos os dados |
| `docker: up` | Constrói e arranca tudo em Docker (imagem de produção) |
| `docker: debug` | Constrói e arranca o contentor de debug |
| `docker: down` | Para e remove os contentores (os dados ficam) |
| `docker: logs` | Mostra os logs da app em Docker, em tempo real |

## 5. Primeiro acesso

No primeiro arranque a app cria as tabelas e também:

- a conta **`admin`** com a palavra-passe **`admin`** (na tabela `pisco_config.config`);
- a workspace **`demo`**, pública e definida como workspace inicial.

Muda a palavra-passe do `admin` se a app ficar acessível a outras pessoas.

Sem sessão iniciada vês só as workspaces públicas (a barra lateral mostra **Público**); com sessão vês todas e podes geri-las em **Parâmetros** (a barra lateral mostra **Restrito**).

## Base de dados

O PostgreSQL só aceita ligações do próprio PC (`127.0.0.1`). Para ligar com o DBeaver ou outra app:

```
jdbc:postgresql://localhost:5432/pisco?user=pisco&password=pisco
```

(Troca os valores se mudaste o `.env`.) Tudo fica no schema `pisco_config`, que tem só duas tabelas:

| Tabela | Conteúdo |
|---|---|
| `workspaces` | Nome e se é pública |
| `config` | Uma única linha: utilizador e palavra-passe (hash), workspace inicial e última workspace escolhida |

Os dados de cada workspace (quando existirem) ficam em tabelas com uma coluna `workspace_id`.

A base de dados **só guarda dados**. Toda a lógica de negócio (valores por omissão, datas, validações, o que acontece ao apagar) está nos objetos C# (`Domain/`) e nos serviços (`Services/`). Nos ficheiros `Data/Sql/*.sql` só há tabelas e restrições de integridade (chaves, `UNIQUE`, `NOT NULL`) como rede de segurança — sem `DEFAULT`, `ON DELETE CASCADE`, triggers, funções ou procedures.

## Problemas comuns

- **`port is already allocated` / porta 5432 ocupada** — já tens outro PostgreSQL a correr. Muda `POSTGRES_PORT` no `.env` (por exemplo para `5433`).
- **`permission denied` ao usar o Docker** — falta o passo do `usermod`, ou ainda não reabriste o terminal.
- **A app não liga à base de dados** — vê se o contentor está a correr com `docker compose ps` e se o `.env` existe.
- **O debug em Docker não para nos breakpoints** — confirma que o contentor `pisco` está a usar a imagem `pisco:debug` (`docker compose ps`). Se estiver `pisco:latest`, corre a tarefa `docker: debug`.
- **Depois de recriar o contentor pede login outra vez** — não devia: as chaves dos cookies ficam no volume `pisco-keys`. Se apagaste os volumes (`db: reset`), é normal.

## Estrutura do projeto

```
Components/        páginas e layout (Blazor)
  Layout/          layout principal, menu lateral, modal de login e presença
  Pages/           páginas (Início, Demo, Parâmetros, Erro)
  Parameters/      secções e mini modais da página Parâmetros
Data/              acesso à BD (Entity Framework Core), SQL e seed
  Sql/             config.sql (schema pisco_config)
Domain/            entidades (Config, Workspace)
Services/          configuração, autenticação, workspaces, toasts
Resources/         traduções (pt / en)
wwwroot/           css, js, fontes, imagens
docs/              documentação e notas
```

Convenção de nomes: pastas, tipos e membros públicos em `PascalCase`; tudo o que é privado começa por minúscula (`camelCase`).

**Base de dados no código:** a configuração e as workspaces são objetos C# (`Domain/Config.cs`, `Domain/Workspace.cs`) guardados com Entity Framework Core (`Data/PiscoDbContext.cs`), através do `ConfigService` e do `WorkspaceService`. Cada objeto valida os seus próprios dados e regras (por exemplo, o `Config` trata da palavra-passe com PBKDF2-SHA256 e salt e garante que a workspace inicial é pública). As tabelas são criadas por `Data/Sql/config.sql`; o EF só lê e grava.

Páginas: Início (`/`), Demo (`/demo`, mostra os componentes e estilos partilhados para ver e testar), Parâmetros (`/parameters`, com sessão iniciada). Não há página de login: clicar no gato ou em "Pisco" abre o modal de login, e abrir uma página que exige sessão volta ao início com o modal aberto (`/?login=1&returnUrl=…`).
