FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

WORKDIR /TodoApi

ADD *.sln .
ADD *.csproj .

RUN dotnet restore

ADD . .

RUN dotnet publish -c Release -o out --no-restore

# Install dependencies
RUN apt-get update && apt-get install -y curl unzip

RUN curl -L -o elastic_apm_profiler_1.31.0.zip https://github.com/elastic/apm-agent-dotnet/releases/download/v1.31.0/elastic_apm_profiler_1.31.0-linux-x64.zip && \
    unzip elastic_apm_profiler_1.31.0.zip -d /elastic_apm_profiler_1.31.0 && \
    rm elastic_apm_profiler_1.31.0.zip


FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0

COPY --from=build-env /elastic_apm_profiler_1.31.0 /elastic_apm_profiler

# Enable the agent
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={FA65FE15-F085-4681-9B20-95E04F6C03CC}
ENV CORECLR_PROFILER_PATH=/elastic_apm_profiler/libelastic_apm_profiler.so
ENV ELASTIC_APM_PROFILER_HOME=/elastic_apm_profiler
ENV ELASTIC_APM_PROFILER_INTEGRATIONS=/elastic_apm_profiler/integrations.yml


WORKDIR /TodoApi
COPY --from=build-env /TodoApi/out /TodoApi

EXPOSE 8080

CMD ["dotnet", "TodoApi.dll"]
