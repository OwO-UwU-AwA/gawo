FROM ghcr.io/nixos/nix
WORKDIR /App

RUN nix-channel --update
RUN NIXPKGS_ALLOW_UNFREE=1 nix-env -iA nixpkgs.dotnet-sdk_8 nixpkgs.surrealdb nixpkgs.prometheus nixpkgs.prometheus-node-exporter nixpkgs.grafana nixpkgs.grafana-image-renderer


# Copy everything
COPY . /App
# Restore as distinct layers
RUN dotnet restore
# Build a release
RUN dotnet publish -c Release -o out --no-restore

ENTRYPOINT ["bash", "docker/run.sh"]
#      ASPN PROM NODE GRAF
EXPOSE 1813 1814 1815 1816
