# Área do cliente Felibow

A área do cliente está integrada à aplicação MVC existente e usa o mesmo `ApplicationDbContext`, banco SQLite, produtos, carrinho e pedidos da loja. Ela não cria uma cópia do catálogo.

## Executar localmente

Na raiz do repositório:

```powershell
dotnet run --project .\loja_s\loja_s.csproj
```

Abra `/Conta/Cadastro` para criar uma conta e `/Conta/Login` para entrar. O painel pessoal fica em `/Conta/MinhaConta`. O primeiro acesso cria as tabelas da conta que faltarem e adiciona as colunas de endereço ao banco existente.

## Segurança e funcionamento

- A senha usa `PasswordHasher<Usuario>` do ASP.NET Core Identity; não há senha administrativa ou de cliente predefinida.
- A autenticação usa cookie HttpOnly, SameSite Lax, HTTPS obrigatório fora do ambiente de desenvolvimento e uma sessão revogável por dispositivo.
- Cinco senhas incorretas bloqueiam temporariamente a conta por 15 minutos.
- Tokens de redefinição e confirmação de e-mail são armazenados como hash e expiram; cada token é de uso único.
- Em `Development`, os links de e-mail são escritos no log da aplicação para permitir testar o fluxo. Não use esses logs como mecanismo de envio em produção.
- Em produção, configure o SMTP exclusivamente por variáveis de ambiente:

```powershell
$env:Application__PublicBaseUrl = "https://www.sualoja.com.br"
$env:Email__Smtp__Host = "smtp.seu-provedor"
$env:Email__Smtp__Port = "587"
$env:Email__Smtp__From = "conta@seudominio"
$env:Email__Smtp__EnableSsl = "true"
$env:Email__Smtp__Username = "usuario-smtp"
$env:Email__Smtp__Password = "segredo-fornecido-pelo-provedor"
dotnet run --project .\loja_s\loja_s.csproj
```

- `Application__PublicBaseUrl` deve ser a origem HTTPS pública da loja (sem caminho, query ou fragmento). Os links de redefinição de senha e confirmação de e-mail usam essa origem configurada, evitando confiar no cabeçalho `Host` recebido na requisição. Em `Development`, a origem da requisição é usada somente para facilitar testes locais.
- Formas de pagamento guardam apenas bandeira, últimos quatro dígitos e um token recebido do gateway, protegido com ASP.NET Core Data Protection. A captura e tokenização reais dependem da integração com um provedor; nunca envie número completo ou CVV para esta aplicação.
- Carrinhos de visitantes permanecem na sessão. Ao entrar, os itens são mesclados ao carrinho persistido da conta e ajustados ao estoque disponível.
- Pedidos e detalhes são filtrados pelo usuário autenticado. Endereços de entrega associados a pedidos não podem ser excluídos.
- Autenticação em dois fatores não está implementada nesta etapa.

## Verificação manual

1. Cadastre uma conta com senha forte e aceite dos termos.
2. Entre; visite Meus Dados, Endereços, Favoritos, Notificações, Segurança e Central de Atendimento.
3. Adicione um produto ao carrinho, entre na conta e confirme que o carrinho aparece no painel.
4. Finalize uma compra e confira o pedido em `/Pedido/MeusPedidos`.
5. Teste a recuperação de senha. Em desenvolvimento, copie o link temporário dos logs; em produção, configure SMTP antes de habilitar envio.
