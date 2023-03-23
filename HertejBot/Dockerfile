FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["HertejBot.csproj", "HertejBot.csproj"]
RUN dotnet restore "HertejBot.csproj"
COPY . .
RUN dotnet build "HertejBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HertejBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./HertejBot"]
