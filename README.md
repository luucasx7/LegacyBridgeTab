# Legacy Bridge

Extensão para Chrome com um auxiliar para Windows. Ela abre a página da aba atual em uma janela separada com o controle WebBrowser/MSHTML, para ajudar no acesso a alguns sistemas legados.

## Requisitos

- Windows 10 ou 11 com .NET Framework e o componente WebBrowser/MSHTML disponíveis.
- Google Chrome.
- Windows PowerShell.

## Instalação

1. Baixe ou clone este repositório no computador Windows.
2. No Chrome, abra `chrome://extensions`, ative **Modo do desenvolvedor** e clique em **Carregar sem compactação**. Selecione a pasta `extension`.
3. Abra o PowerShell na pasta do projeto e execute, uma linha por vez:

   ```powershell
   .\build.ps1
   .\install.ps1
   ```

4. Se o sistema Hikvision solicitar o plug-in Web Components, obtenha-o diretamente do fornecedor e instale a versão adequada ao seu equipamento. O instalador proprietário não está incluído neste repositório.
5. Reinicie o Chrome, abra o site e clique no ícone **Legacy Bridge**.

## Scripts

- `build.ps1`: compila os programas auxiliares a partir do código em `native/`.
- `install.ps1`: instala o auxiliar no perfil atual do Windows e registra a extensão para comunicação local.
- `uninstall.ps1`: remove o registro e os arquivos instalados.

## Privacidade

A extensão solicita as permissões `activeTab` e `nativeMessaging`. Depois do clique, ela envia ao auxiliar o endereço da aba atual. O auxiliar aceita somente endereços HTTP e HTTPS. O projeto não lê arquivos locais nem envia o conteúdo da página.

## Limitações

- A página abre em uma janela própria, fora da aba do Chrome.
- A compatibilidade depende da versão do Windows e do site.
- Pop-ups são bloqueados nesta versão.
- ActiveX, Java e Silverlight não são incluídos nem garantidos.
- O MSHTML é antigo. Use apenas com sites confiáveis e de acordo com as regras da sua organização.
- Projeto independente, sem afiliação com a Hikvision.
