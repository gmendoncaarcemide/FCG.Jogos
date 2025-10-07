# 🛒 FCG Compras

Microserviço responsável pelo gerenciamento de compras de jogos no sistema FIAP Cloud Games.  
Integra com o serviço de pagamentos e realiza controle de estoque, geração de código de ativação e histórico de transações por usuário ou jogo.

---

## 🚀 Como rodar o projeto

### ✅ Pré-requisitos

- .NET 8 SDK  
- PostgreSQL (via Supabase)  
- EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef

### 📦 Restauração de Pacotes
Após clonar o repositório, navegue até a pasta do projeto e execute:

```bash
cd FCG_COMPRAS
dotnet restore
```

### 🛠️ Configuração do Banco de Dados

#### 🔄 **Supabase + PostgreSQL**
O projeto utiliza **Supabase** como provedor de PostgreSQL em nuvem:

**Connection String:**
```
Host=db.elcvczlnnzbgcpsbowkg.supabase.co  
Port=5432  
Database=postgres  
Username=postgres  
Password=Fiap@1234
```

#### 🗄️ **Aplicando Migrations**
Para aplicar as migrations no Supabase:
```bash
cd FCG.Compras.API
dotnet ef database update --project ../FCG.Compras.Infrastructure --startup-project .
```

#### 📝 **Criando Novas Migrations**
```bash
cd FCG.Compras.API
dotnet ef migrations add NomeDaMigration --project ../FCG.Compras.Infrastructure --startup-project .
```

## 🏗️ Arquitetura
### 📂 Estrutura do Projeto
```
FCG_COMPRAS/
├── FCG.Compras.API/             # Camada de API e Controllers
│   ├── Controllers/             # Controllers REST
│   │   └── CompraController.cs
│   ├── Program.cs               # Configuração da aplicação
│   └── appsettings.json         # Configurações
├── FCG.Compras.Application/     # Regras de negócio e serviços
│   ├── Jogos/
│   │   ├── Interfaces/          # Interfaces dos serviços
│   │   ├── Services/            # Implementação dos serviços
│   │   └── ViewModels/          # DTOs e ViewModels
│   └── ServiceCollectionExtensions.cs
├── FCG.Compras.Domain/          # Entidades e interfaces
│   └── Jogos/
│       ├── Entities/            # Entidades do domínio
│       └── Interfaces/          # Interfaces dos repositórios
└── FCG.Compras.Infrastructure/  # EF Core + Repositórios
    ├── Jogos/
    │   ├── Repositories/        # Implementação dos repositórios
    │   └── Context/             # DbContext Factory
    ├── Migrations/              # Scripts de migração EF Core
    ├── JogosDbContext.cs        # Contexto do EF Core
    └── ServiceCollectionExtensions.cs

```

### 🔧 Tecnologias Utilizadas
- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados (via Supabase)
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação da API


## 📡 Endpoints da API
### 💳 **🛒 Compras de Jogos**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET`  | `/api/compras` | Listar todas as compras |
| `GET`  | `/api/compras/{id}` | Obter compra por ID |
| `POST` | `/api/compras` | Criar nova compra (com integração de pagamento) |
| `GET`  | `/api/compras/usuario/{usuarioId}` | Listar compras por usuário |
| `GET`  | `/api/compras/jogo/{jogoId}` | Listar compras por jogo |
| `PUT`  | `/api/compras/{id}/status` | Atualizar status da compra |
| `POST` | `/api/compras/{id}/cancelar` | Cancelar compra e restaurar estoque |
| `GET`  | `/api/compras/{id}/codigo-ativacao` | Gerar código de ativação para compra aprovada |


## 🗄️ Modelo de Dados
### 📊 **Tabela: Compras**

- `Id` (UUID)
- `UsuarioId` (UUID)
- `JogoId` (UUID)
- `PrecoPago` (Decimal)
- `Status` (Enum): Pendente, Aprovada, Cancelada, Reembolsada, Processando, Ativada
- `CodigoAtivacao` (String)
- `DataCompra` (DateTimeOffset)
- `DataAtivacao` (DateTimeOffset?)
- `Observacoes` (String)
- `DataCriacao` (DateTime)
- `DataAtualizacao` (DateTime?)

## 🐞 Logs e Monitoramento
### 📝 **Serilog**
Logs estruturados com Serilog
Arquivos de log por data em /logs/
Logs de console para desenvolvimento
Formato: compras-api-YYYY-MM-DD.txt

### 🔍 **Swagger**
Documentação automática da API
Interface interativa para testes
Disponível em /swagger quando em desenvolvimento

## 🚀 Deploy e Produção

### ☁️ **Supabase**
Banco de dados PostgreSQL gerenciado
Migrations aplicadas automaticamente na inicialização 
