#!/usr/bin/env bash

cd /App

node_exporter & disown

prometheus --config.file /App/docker/prometheus.yml & disown

cp -r "$(find /nix/store -maxdepth 1 -type d -regex ".*grafana-[0-10].*" | head -n 1)/share/grafana/" /App/grafana/

grafana server --config /App/docker/grafana.ini --homepath /App/grafana & disown

surreal start file:users.sdb & disown

dotnet out/gawo.dll
