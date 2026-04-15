![Project Stage][project-stage-shield]
![Maintenance][maintenance-shield]


# Jellyfin Script Runner Plugin

Plugin para o [Jellyfin](https://jellyfin.org) que executa um script customizado automaticamente sempre que um item for adicionado ou atualizado na biblioteca.

Ideal para manter slideshows, listas ou qualquer arquivo externo sempre sincronizado com as mídias do servidor.

---

## Instalação via Repositório (Recomendado)

1. Acesse o painel do Jellyfin
2. Vá em **Dashboard → Plugins → Repositórios**
3. Clique em **+** e adicione a URL: 
```https://raw.githubusercontent.com/iHumberto/jellyfin-plugin-scriptrunner/main/manifest.json```
4. Vá para **Catálogo** e instale o **Script Runner**
5. Reinicie o Jellyfin
6. Crie o arquivo `config.json` (veja abaixo)

---

## Instalação Manual

1. Baixe a DLL mais recente na aba [Releases](https://github.com/iHumberto/jellyfin-plugin-scriptrunner/releases)
2. Crie a pasta `/config/plugins/ScriptRunner/`
3. Copie a DLL para essa pasta
4. Crie o arquivo `config.json` (veja abaixo)
5. Reinicie o Jellyfin

---

## Configuração

Crie o arquivo `config.json` dentro da pasta do plugin: ```config/plugins/ScriptRunner/config.json```


```json
{
  "ScriptPath": "/caminho/do_script.sh",
  "ScriptArguments": "",
  "DebounceSeconds": 30,
  "TriggerOnItemAdded": true,
  "TriggerOnItemUpdated": false
}
```

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `ScriptPath` | string | Caminho absoluto do script a executar |
| `ScriptArguments` | string | Argumentos opcionais passados ao script |
| `DebounceSeconds` | int | Tempo em segundos após o último evento antes de executar |
| `TriggerOnItemAdded` | bool | Dispara quando um item é adicionado |
| `TriggerOnItemUpdated` | bool | Dispara quando um item é atualizado |

> **Nota:** Alterações no `config.json` passam a valer após reiniciar o Jellyfin.

---

## Uso com Docker

Exemplo de `docker-compose.yaml`:

```yaml
services:
  jellyfin:
    image: jellyfin/jellyfin
    volumes:
      - ./data:/config
      - ./plugins:/config/plugins
      - ./meu-script.sh:/jellyfin/meu-script.sh
```

No host, crie os arquivos:

```bash
mkdir -p ./plugins/ScriptRunner

cat > ./plugins/ScriptRunner/config.json << EOF
{
  "ScriptPath": "/jellyfin/meu-script.sh",
  "ScriptArguments": "",
  "DebounceSeconds": 30,
  "TriggerOnItemAdded": true,
  "TriggerOnItemUpdated": false
}
EOF
```

Certifique-se que o script tem permissão de execução:

```bash
chmod +x ./meu-script.sh
```

---

## Como funciona
Jellyfin detecta novo item (Filme ou Série) </br>
↓ </br>
Plugin aguarda o debounce (default: 30s) </br>
↓ </br>
Executa o script configurado </br>
↓ </br>
Script faz o que você quiser

O debounce evita múltiplas execuções durante scans grandes — se vários itens forem adicionados em sequência, o script rodará apenas uma vez.

---

## Compatibilidade

| Jellyfin | Status |
|----------|--------|
| 10.10.x  | ✅ Suportado |
| 10.9.x   | Não testado |

---

## Licença

Este projeto está licenciado sob a [GNU GPL v3](LICENSE).

[maintenance-shield]: https://img.shields.io/maintenance/yes/2026.svg
[project-stage-shield]: https://img.shields.io/badge/project%20stage-production%20ready-brightgreen.svg
