# Datadog Instrumentation

This branch contains code for Datadog instrumentation.

By default, hitting an API endpoint will generate a trace, which is sent to CubeAPM. This behavior is controlled via environment variables in [docker-compose.yml](docker-compose.yml).

Refer the project README below for more details.

# .NET Core Instrumentation

This is a sample app to demonstrate how to instrument .NET Core app with **Datadog**, **Elastic**, **New Relic** and **OpenTelemetry**. It contains source code for the .NET Core app which interacts with various services like Redis, MySQL, Kafka, etc. to demonstrate tracing for these services. This repository has a docker compose file to set up all these services conveniently.

The code is organized into multiple branches. The main branch has the Flask app without any instrumentation. Other branches then build upon the main branch to add specific instrumentations as below:

| Branch                                                                                         | Instrumentation | Code changes for instrumentation                                                                                |
| ---------------------------------------------------------------------------------------------- | --------------- | --------------------------------------------------------------------------------------------------------------- |
| [main](https://github.com/cubeapm/sample_app_dotnet_core/tree/main)         | None            | -                                                                                                               |
| [datadog](https://github.com/cubeapm/sample_app_dotnet_core/tree/datadog) | Datadog       | [main...datadog](https://github.com/cubeapm/sample_app_dotnet_core/compare/main...datadog) |
| [elastic](https://github.com/cubeapm/sample_app_dotnet_core/tree/elastic)         | Elastic   | [main...elastic](https://github.com/cubeapm/sample_app_dotnet_core/compare/main...elastic)         |
| [newrelic](https://github.com/cubeapm/sample_app_dotnet_core/tree/newrelic) | New Relic       | [main...newrelic](https://github.com/cubeapm/sample_app_dotnet_core/compare/main...newrelic) |
| [otel](https://github.com/cubeapm/sample_app_dotnet_core/tree/otel)         | OpenTelemetry   | [main...otel](https://github.com/cubeapm/sample_app_dotnet_core/compare/main...otel)         |

## Setup

Clone this repository and go to the project directory. Then run the following commands

```
docker compose up --build
```

.NET Core app will now be available at `http://localhost:8080`.

The app has various API endpoints to demonstrate OpenTelemetry integrations with Redis, MySQL, Kafka, etc. Check out [Controllers/RootController.cs](Controllers/RootController.cs) for the list of API endpoints. 

## Contributing

Please feel free to raise PR for any enhancements - additional service integrations, library version updates, documentation updates, etc.
