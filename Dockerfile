FROM mcr.microsoft.com/dotnet/core/runtime:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["src/OnePlusBot/OnePlusBot.csproj", "src/OnePlusBot/"]
RUN dotnet restore "src/OnePlusBot/OnePlusBot.csproj"
COPY . .
WORKDIR "/src/src/OnePlusBot"
RUN dotnet build "OnePlusBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnePlusBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ADD https://github.com/ufoscout/docker-compose-wait/releases/download/2.6.0/wait /wait
RUN chmod +x /wait
COPY --from=publish /app/publish .

CMD /wait && dotnet OnePlusBot.dll
# without wait
#ENTRYPOINT ["dotnet", "OnePlusBot.dll"]
