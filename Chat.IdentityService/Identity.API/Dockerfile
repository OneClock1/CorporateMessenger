#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Chat.IdentityService/Identity.API/Identity.API.csproj", "Chat.IdentityService/Identity.API/"]
COPY ["Chat.IdentityService/Identity.Domain/Identity.Domain.csproj", "Chat.IdentityService/Identity.Domain/"]
COPY ["Chat.Common/Common/Common.csproj", "Chat.Common/Common/"]
COPY ["Chat.IdentityService/Identity.Infrastructure/Identity.Infrastructure.csproj", "Chat.IdentityService/Identity.Infrastructure/"]
COPY ["Chat.IdentityService/Identity.Persistence/Identity.Persistence.csproj", "Chat.IdentityService/Identity.Persistence/"]
RUN dotnet restore "Chat.IdentityService/Identity.API/Identity.API.csproj"
COPY . .
WORKDIR "/src/Chat.IdentityService/Identity.API"
RUN dotnet build "Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Identity.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Identity.API.dll"]