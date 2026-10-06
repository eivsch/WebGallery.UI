param(
[string]$HostName = "127.0.0.1",
[int]$Port = 8000
)

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

if (-not (Test-Path "..venv\Scripts\python.exe")) {
py -3.14 -m venv .venv
}

python.exe -m pip install --upgrade pip
python.exe -m pip install "setuptools<81"
python.exe -m pip install dlib-20.0.99-cp314-cp314-win_amd64.whl
python.exe -m pip install -r .\requirements.txt

python.exe -m uvicorn main:app --reload --host $HostName --port $Port