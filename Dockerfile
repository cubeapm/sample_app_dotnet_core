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

COPY newrelic.config /TodoApi/newrelic.config

# ==========================================
# NEW RELIC AUTO-INSTRUMENTATION INITIALIZATION
# ==========================================
# These environment variables initialize the New Relic agent without requiring code changes.
# When the container starts, the .NET runtime sees CORECLR_ENABLE_PROFILING=1.
# It then looks at the CORECLR_PROFILER_PATH and injects the New Relic profiler library
# into the application's memory before the app runs.
# The agent then automatically hooks into functions like HTTP requests and DB calls.
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36CAA32E-61B4-436A-B604-A3E55359CB79}
ENV CORECLR_NEWRELIC_HOME=/TodoApi/newrelic
ENV CORECLR_PROFILER_PATH=/TodoApi/newrelic/libNewRelicProfiler.so

CMD ["dotnet", "TodoApi.dll"]
