#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Chat.ChatService/Chat.API/Chat.API.csproj", "Chat.ChatService/Chat.API/"]
COPY ["Chat.ChatService/Chat.Domain/Chat.Domain.csproj", "Chat.ChatService/Chat.Domain/"]
COPY ["Chat.Common/Common/Common.csproj", "Chat.Common/Common/"]
COPY ["Chat.ChatService/Chat.Infrastructure/Chat.Infrastructure.csproj", "Chat.ChatService/Chat.Infrastructure/"]
COPY ["Chat.ChatService/Chat.Persistence/Chat.Persistence.csproj", "Chat.ChatService/Chat.Persistence/"]
RUN dotnet restore "Chat.ChatService/Chat.API/Chat.API.csproj"
COPY . .
WORKDIR "/src/Chat.ChatService/Chat.API"
RUN dotnet build "Chat.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chat.API.dll"]