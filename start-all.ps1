# Script para iniciar los 3 microservicios simultáneamente
Write-Host "Iniciando Microservicios Distribuidos..." -ForegroundColor Cyan

$root = $PSScriptRoot

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root/src/Catalogo.Api'; dotnet run --urls 'http://localhost:5001'" -WindowStyle Normal
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root/src/Inventario.Api'; dotnet run --urls 'http://localhost:5002'" -WindowStyle Normal
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root/src/Pedidos.Api'; dotnet run --urls 'http://localhost:5003'" -WindowStyle Normal

Write-Host "Microservicios iniciados:" -ForegroundColor Green
Write-Host "1. Catalogo.Api   -> http://localhost:5001" -ForegroundColor Yellow
Write-Host "2. Inventario.Api -> http://localhost:5002" -ForegroundColor Yellow
Write-Host "3. Pedidos.Api    -> http://localhost:5003" -ForegroundColor Yellow
Write-Host "Documentación Swagger disponible en la raíz de cada puerto." -ForegroundColor White
