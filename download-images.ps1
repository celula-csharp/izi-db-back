Write-Host "=== DESCARGANDO IMAGENES DE DOCKER ===" -ForegroundColor Cyan

$images = @(
    "postgres:15-alpine",
    "mysql:8.0",
    "mongo:6.0",
    "redis:alpine",
    "mcr.microsoft.com/mssql/server:2019-latest"
)

foreach ($image in $images) {
    Write-Host "Descargando $image..." -ForegroundColor Yellow
    docker pull $image
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK: $image descargado correctamente" -ForegroundColor Green
    } else {
        Write-Host "ERROR: No se pudo descargar $image" -ForegroundColor Red
    }
    Write-Host "---" -ForegroundColor Gray
}

Write-Host "Todas las imágenes han sido procesadas!" -ForegroundColor Cyan

# Verificar imágenes descargadas
Write-Host "`nImágenes disponibles en el sistema:" -ForegroundColor Cyan
docker images --format "table {{.Repository}}\t{{.Tag}}\t{{.Size}}"