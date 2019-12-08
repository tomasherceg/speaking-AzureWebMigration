FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["PhotoGallery.App/PhotoGallery.App.csproj", "PhotoGallery.App/"]
COPY ["PhotoGallery.Data/PhotoGallery.Data.csproj", "PhotoGallery.Data/"]
RUN dotnet restore "PhotoGallery.App/PhotoGallery.App.csproj"
COPY . .
WORKDIR "/src/PhotoGallery.App"
RUN dotnet build "PhotoGallery.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoGallery.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhotoGallery.App.dll"]