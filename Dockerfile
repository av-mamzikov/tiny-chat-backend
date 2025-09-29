FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY global.json ./
COPY WebSocketChat.sln ./
COPY WebSocketChat.Backend/ ./WebSocketChat.Backend/
RUN dotnet restore WebSocketChat.Backend/WebSocketChat.Backend.csproj
WORKDIR /app
RUN dotnet build WebSocketChat.Backend/WebSocketChat.Backend.csproj -c Release -o /app/build
RUN dotnet publish WebSocketChat.Backend/WebSocketChat.Backend.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "WebSocketChat.Backend.dll"]
