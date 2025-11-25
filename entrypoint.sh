# ...existing code...
#!/bin/sh
set -e

echo "Waiting for database..."
until pg_isready -h db -p 5432 -U postgres; do
  sleep 1
done

cd /src/CinemaApp

# Ensure dotnet tools from manifest are available and global tool path is on PATH
export PATH="$PATH:/root/.dotnet/tools"
echo "Restoring dotnet tools (if any)..."
dotnet tool restore || echo "dotnet tool restore returned non-zero (continuing)"

# Restore NuGet packages and build so project.assets.json exists for EF tools
echo "Restoring NuGet packages..."
dotnet restore CinemaApp.csproj || { echo "dotnet restore failed"; exit 1; }

echo "Building project to generate assets..."
dotnet build CinemaApp.csproj -c Release || { echo "dotnet build failed"; exit 1; }

# If dotnet-ef is still not available, try installing it globally (safe no-op if already present)
if ! command -v dotnet-ef >/dev/null 2>&1; then
  echo "dotnet-ef not found on PATH, attempting global install..."
  dotnet tool install --global dotnet-ef || echo "global install failed (continuing)"
fi

echo "Runtime: listing migrations visible to EF CLI"
dotnet ef migrations list --project CinemaApp.csproj || echo "dotnet ef migrations list failed"

echo "Generate migrations script (idempotent) to /tmp/migrations.sql"
dotnet ef migrations script --idempotent -o /tmp/migrations.sql --project CinemaApp.csproj || echo "script generation failed"
if [ -f /tmp/migrations.sql ]; then
  echo "---- migrations.sql head ----"
  head -n 80 /tmp/migrations.sql || true
  echo "---- end head ----"
fi

echo "Attempting database update (verbose)"
dotnet ef database update --project CinemaApp.csproj --verbose || {
  echo "dotnet ef database update failed"
  exit 1
}

echo "Running seed..."
dotnet /app/CinemaApp.dll --seed || echo "seed failed"

echo "Starting application..."
exec dotnet /app/CinemaApp.dll