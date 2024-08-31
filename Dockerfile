FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

WORKDIR /TodoApi

ADD *.sln .
ADD *.csproj .

# StackExchangeRedis instrumentation library is in beta, so it is not picked up
# automatically by auto-instrumentation and needs to be added manually.
# Ref: https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.StackExchangeRedis#stackexchangeredis-instrumentation-for-opentelemetry
RUN dotnet add package OpenTelemetry.Instrumentation.StackExchangeRedis --version 1.9.0-beta.1

RUN dotnet restore

ADD . .

RUN dotnet publish -c Release -o out --no-restore


FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0

# install OpenTelemetry .NET Automatic Instrumentation
ARG OTEL_VERSION=1.9.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y curl unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh

# enable OpenTelemetry .NET Automatic Instrumentation
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={918728DD-259F-4A6A-AC2B-B85E1B658318} \
    CORECLR_PROFILER_PATH=/otel-dotnet-auto/linux-x64/OpenTelemetry.AutoInstrumentation.Native.so \
    DOTNET_ADDITIONAL_DEPS=/otel-dotnet-auto/AdditionalDeps \
    DOTNET_SHARED_STORE=/otel-dotnet-auto/store \
    DOTNET_STARTUP_HOOKS=/otel-dotnet-auto/net/OpenTelemetry.AutoInstrumentation.StartupHook.dll \
    OTEL_DOTNET_AUTO_HOME=/otel-dotnet-auto

WORKDIR /TodoApi
COPY --from=build-env /TodoApi/out /TodoApi

EXPOSE 8080

CMD ["dotnet", "TodoApi.dll"]
