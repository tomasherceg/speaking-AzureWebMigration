FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["PhotoGallery.Worker/PhotoGallery.Worker.csproj", "PhotoGallery.Worker/"]
COPY ["PhotoGallery.Data/PhotoGallery.Data.csproj", "PhotoGallery.Data/"]
RUN dotnet restore "PhotoGallery.Worker/PhotoGallery.Worker.csproj"
COPY . .
WORKDIR "/src/PhotoGallery.Worker"
RUN dotnet build "PhotoGallery.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoGallery.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoGallery.Worker.dll"]