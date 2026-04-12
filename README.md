# Jellyfin Script Runner Plugin

Dispara um script shell sempre que a biblioteca do Jellyfin é atualizada.

## Build

```bash
dotnet build -c Release
```

O arquivo gerado ficará em:
`bin/Release/net8.0/Jellyfin.Plugin.ScriptRunner.dll`

## Instalação

1. Crie a pasta do plugin:
   ```bash
   mkdir -p /var/lib/jellyfin/plugins/ScriptRunner
   ```

2. Copie a DLL e o meta.json:
   ```bash
   cp bin/Release/net8.0/Jellyfin.Plugin.ScriptRunner.dll /var/lib/jellyfin/plugins/ScriptRunner/
   cp meta.json /var/lib/jellyfin/plugins/ScriptRunner/
   ```

3. Reinicie o Jellyfin:
   ```bash
   sudo systemctl restart jellyfin
   ```

4. Acesse **Dashboard → Plugins → Script Runner** e configure:
   - **Caminho do Script**: ex: `/home/user/update_slideshow.sh`
   - **Debounce**: 30 segundos (recomendado)
   - Marque **"Disparar quando adicionado"**

5. Certifique-se que o script tem permissão de execução:
   ```bash
   chmod +x /home/user/update_slideshow.sh
   ```

## Como funciona

- O plugin escuta os eventos `ItemAdded` e `ItemUpdated` do `ILibraryManager`
- Filtra apenas `Movie` e `Series` (ignora episódios individuais, metadados, etc.)
- Aplica um **debounce** configurável — o script só executa após X segundos
  sem novos eventos, evitando execuções em rajada durante um scan grande
- O output e erros do script são registrados no log do Jellyfin
