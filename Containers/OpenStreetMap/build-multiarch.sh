#!/bin/bash
docker buildx build --push \
    --platform  linux/amd64,linux/arm64/v8 \
    --tag syamaner/osm-tile-server:2.3.0 \
    .