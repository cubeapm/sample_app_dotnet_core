FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

WORKDIR /TodoApi

ADD *.sln .
ADD *.csproj .

RUN dotnet restore

ADD . .

RUN dotnet publish -c Release -o out --no-restore


FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0

# Install dependencies
RUN apt-get update && apt-get install -y curl

# Install Datadog .NET Tracer
RUN curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v3.13.0/datadog-dotnet-apm_3.13.0_amd64.deb \
    && dpkg -i datadog-dotnet-apm_3.13.0_amd64.deb \
    && rm datadog-dotnet-apm_3.13.0_amd64.deb


# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8} \
CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so \
DD_DOTNET_TRACER_HOME=/opt/datadog


WORKDIR /TodoApi
COPY --from=build-env /TodoApi/out /TodoApi

EXPOSE 8080

CMD ["dotnet", "TodoApi.dll"]
