FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
# INSTALL NEWRELIC
ENV CORECLR_ENABLE_PROFILING=1 \
CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so \
NEW_RELIC_LICENSE_KEY=195cb6e62921d83376172a83bb2389a5c9bedaac \
NEW_RELIC_APP_NAME=JAMTech.AIO.Docker
COPY /newrelic ./newrelic
RUN dpkg -i ./newrelic/newrelic-netcore20-agent_8.14.*.deb
ENV TZ America/Santiago
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "JAMTech.dll"]