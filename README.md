![Project Stage][project-stage-shield]
![Maintenance][maintenance-shield]

> 🇧🇷 [Leia em Português](README.pt-br.md)

# Jellyfin Script Runner Plugin

Plugin for [Jellyfin](https://jellyfin.org) that automatically runs a custom script whenever an item is added and/or updated in the library.

Perfect for keeping slideshows, playlists, or any external file always in sync with your server's media.

---

## Installation

1. Open the Jellyfin dashboard
2. Go to **Dashboard → Plugins → Repositories**
3. Click **+** and add the URL: <br/>`https://raw.githubusercontent.com/iHumberto/jellyfin-plugin-scriptrunner/main/manifest.json`
4. Go to **Catalog** and install **ScriptRunner**
5. Restart Jellyfin

---

## Configuration

Go to **Dashboard → Plugins → ScriptRunner** and add a new script entry.

<img width="1627" height="1000" alt="image" src="https://github.com/user-attachments/assets/67457b10-9b28-4f1d-a8bf-a2b06a3812fe" />

<img width="1623" height="1008" alt="image" src="https://github.com/user-attachments/assets/545f728f-f11f-42b7-b755-4ae8755c601a" />


| Field | Description |
|-------|-------------|
| **Name** | A name to identify this script |
| **Content** | The script content to execute |
| **Debounce (seconds)** | How long to wait after the last event before running (avoids multiple triggers during large scans) |
| **Trigger on item added** | Run the script when a new item is added to the library |
| **Trigger on item updated** | Run the script when an existing item is updated |

---

## How It Works
Jellyfin detects a new item (Movie or Series) <br/>
↓<br/>
Plugin waits for the debounce timer (default: 30s)<br/>
↓<br/>
Executes the configured script<br/>
↓<br/>
Script does whatever you need<br/>

---

## Compatibility

| Jellyfin | Status |
|----------|--------|
| 10.10.x  | ✅ Supported |
| 10.9.x   | Not tested |

---

## License

This project is licensed under the [GNU GPL v3](LICENSE).

[maintenance-shield]: https://img.shields.io/maintenance/yes/2026.svg
[project-stage-shield]: https://img.shields.io/badge/project%20stage-production%20ready-brightgreen.svg
   
