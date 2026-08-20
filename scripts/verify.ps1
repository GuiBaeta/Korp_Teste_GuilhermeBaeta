[CmdletBinding()]
param(
    [switch]$SkipDockerValidation
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot

function Invoke-VerificationStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name"

    & $Action

    if ($LASTEXITCODE -ne 0) {
        throw "$Name falhou com o codigo de saida $LASTEXITCODE."
    }
}

Push-Location $projectRoot

try {
    Invoke-VerificationStep "Restaurando dependencias do backend" {
        dotnet restore "Korp_Teste_GuilhermeBaeta.sln"
    }

    Invoke-VerificationStep "Compilando backend" {
        dotnet build "Korp_Teste_GuilhermeBaeta.sln" --no-restore
    }

    Invoke-VerificationStep "Executando testes do backend" {
        dotnet test "Korp_Teste_GuilhermeBaeta.sln" --no-build
    }

    Push-Location "frontend"

    try {
        Invoke-VerificationStep "Instalando dependencias do frontend" {
            npm ci
        }

        Invoke-VerificationStep "Compilando frontend" {
            npm run build
        }

        Invoke-VerificationStep "Executando testes do frontend" {
            npm test -- --watch=false
        }
    }
    finally {
        Pop-Location
    }

    if (-not $SkipDockerValidation) {
        Invoke-VerificationStep "Validando Docker Compose" {
            docker compose --env-file .env.example config --quiet
        }
    }

    Write-Host ""
    Write-Host "Verificacao concluida com sucesso."
}
finally {
    Pop-Location
}
