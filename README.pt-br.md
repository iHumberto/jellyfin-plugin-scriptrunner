![Project Stage][project-stage-shield]
![Maintenance][maintenance-shield]

> 🇺🇸 [Read in English](README.md)

# Jellyfin Script Runner Plugin

Plugin para o [Jellyfin](https://jellyfin.org) que executa um script customizado automaticamente sempre que um item for adicionado e/ou atualizado na biblioteca.

Ideal para manter slideshows, listas ou qualquer arquivo externo sempre sincronizado com as mídias do servidor.

---

## Instalação

1. Acesse o painel do Jellyfin
2. Vá em **Dashboard → Plugins → Repositórios**
3. Clique em **+** e adicione a URL: `https://raw.githubusercontent.com/iHumberto/jellyfin-plugin-scriptrunner/main/manifest.json`<br/>
4. Vá para **Catálogo** e instale o **ScriptRunner**
5. Reinicie o Jellyfin

---

## Configuração

Acesse **Dashboard → Plugins → ScriptRunner** e adicione uma nova entrada de script.

<img width="1627" height="1000" alt="{7203524E-FB4A-4657-92ED-83EF7AEC100E}" src="https://github.com/user-attachments/assets/ac30a19c-a327-4e1f-a26b-fa7ea1c49a96" />
<img width="1623" height="1008" alt="{87EF8388-80F8-4A4B-BE77-0CF3D38A29CC}" src="https://github.com/user-attachments/assets/dd3a92bb-5d19-40f2-919e-5dab94a5a9b4" />


| Campo | Descrição |
|-------|-----------|
| **Name** | Um nome para identificar o script |
| **Content** | O conteúdo do script a ser executado |
| **Debounce (segundos)** | Tempo de espera após o último evento antes de executar (evita múltiplos disparos durante scans grandes) |
| **Trigger on item added** | Executa o script quando um novo item é adicionado à biblioteca |
| **Trigger on item updated** | Executa o script quando um item existente é atualizado |

---

## Como Funciona
Jellyfin detecta novo item (Filme ou Série)<br/>
↓<br/>
Plugin aguarda o debounce (padrão: 30s)<br/>
↓<br/>
Executa o script configurado<br/>
↓<br/>
Script faz o que você precisar<br/>

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
