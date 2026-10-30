# WebGallery

A self-hosted web gallery for organising and browsing media in per-user albums. The monorepo contains three .NET 10 (ASP.NET Core) services:

| Folder | Project | Role | Default URLs |
| ------ | ------- | ---- | ------------ |
| `UI/` | `WebGallery.UI` (+ `Infrastructure`) | MVC web front end | `https://localhost:5050`, `http://localhost:49992` |
| `API/` | `MinimalGallery.API` | Minimal API for users, albums, media, tags, search. Stores index files on disk. Swagger enabled. | `https://localhost:5041`, `http://localhost:5042` |
| `FileServer/` | `WebGallery.FileServer` | Serves/stores the media files (optional encryption, ffmpeg) | `https://localhost:5031`, `http://localhost:5032` |

The UI talks to the API for metadata and to the FileServer for files.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows (default paths and `Start-WebGallery.ps1` assume it)
- ffmpeg (path set in `FfmpegPath`)
- HTTPS dev certificate: `dotnet dev-certs https --trust`

## Configuration

- `API/appsettings.json`: `IndexFilesStoragePath` (required, default `C:\WebGallery\Data`).
- `FileServer/appsettings.json`: `ConnectionStrings:FileSystemRoot` (media root), `UseEncryption` / `EncryptionCertificate`, `FfmpegPath`.
- `UI/WebGallery.UI/appsettings.json`: `ConnectionStrings:ApiEndpoint` and `FileServerUrl`, `Auth:Username` / `Auth:Password` (change the defaults), `Display:*` paging/search limits.

## Run (development)

Each in its own terminal:

```powershell
dotnet run --project FileServer
dotnet run --project API
dotnet run --project UI/WebGallery.UI
```

Open `https://localhost:5050`. Swagger is at `https://localhost:5041/swagger`; `API/MinimalGallery.API.http` has sample requests. Front-end libraries use LibMan (`UI/WebGallery.UI/libman.json`).

## Run published builds

Publish each project into `FileServer`, `MinimalGallery.API` and `UI` subfolders of one root (default `C:\WebGallery`), then:

```powershell
.\Start-WebGallery.ps1 [-RootPath C:\WebGallery]
```

This opens one PowerShell window per service.

## Side-by-side dev mode (manual checks)

Run the current checkout next to the live app without touching it:

```powershell
.\Start-WebGallery-Dev.ps1            # build + start on alternate ports
.\Start-WebGallery-Dev.ps1 -Refresh   # re-create the dev data copy from live data first
.\Start-WebGallery-Dev.ps1 -Stop      # stop only the dev processes it started
```

- Ports: UI `5150`/`5151`, API `5142`/`5141`, FileServer `5132`/`5131` (override with `-UiHttpPort -UiPort -ApiPort -FileServerPort`). Services read `WEBGALLERY_HTTP_PORT` / `WEBGALLERY_HTTPS_PORT`; unset, they keep the standard ports.
- Data: the live data folder (from the live API `appsettings.json`, or `-LiveDataPath`) is mirrored to `<LiveData>-Dev` (`-DevDataPath`): index files (`*.dat` etc.) are copied, media are NTFS hard links (no extra disk space, same drive required). Re-copied only with `-Refresh`.
- The script refuses to run on live ports or if the dev data path overlaps the live data or the repo. PIDs, logs and state live beside the dev data folder.

## API overview

Under `/users/{username}`: user create/delete, `albums` (list, create, rename, merge, delete, rebuild index/tags), media (add, get, delete, rename, move, likes, tags), `search`, `saved-searches`, `ad-hoc-jobs/{jobName}`. See `API/Program.cs` or Swagger.

## Notes

The API allows any CORS origin and has no authentication of its own; don't expose it publicly.
