FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

WORKDIR /TodoApi

ADD *.sln .
ADD *.csproj .

RUN dotnet restore

ADD . .

RUN dotnet publish -c Release -o out --no-restore


FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0


WORKDIR /TodoApi
COPY --from=build-env /TodoApi/out /TodoApi

EXPOSE 8080

CMD ["dotnet", "TodoApi.dll"]
