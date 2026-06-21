FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files for restore
COPY TrainingCenterMS/TrainingCenter.API.csproj TrainingCenterMS/
COPY TrainingCenter.Business/TrainingCenter.Business.csproj TrainingCenter.Business/
COPY TrainingCenter/TrainingCenter.Core.csproj TrainingCenter/
COPY TrainingCenter_DataAccess/TrainingCenter.DataAccess.csproj TrainingCenter_DataAccess/

# Restore dependencies
RUN dotnet restore TrainingCenterMS/TrainingCenter.API.csproj

# Copy everything and build
COPY . .
RUN dotnet publish TrainingCenterMS/TrainingCenter.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
ENTRYPOINT ["dotnet", "TrainingCenter.API.dll"]
