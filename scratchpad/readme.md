docker run -it \
  -e ç=https://download.geofabrik.de/europe/monaco-latest.osm.pbf \
  -e PBF_URL=https://download.geofabrik.de/europe/monaco-latest.osm.pbf \
  -e REPLICATION_URL=https://download.geofabrik.de/europe/monaco-updates/ \
  -p 8080:8080 \
  --name nominatim \
  mediagis/nominatim:4.4


 /Users/sertanyamaner/Desktop/maps

  docker run --rm -it -v /Users/sertanyamaner/Desktop/maps:/data stationa/osmium-tool merge  /data/germany-latest.osm.pbf /data/japan-latest.osm.pbf /data/switzerland-latest.osm.pbf /data/united-kingdom-latest.osm.pbf /data/denmark-latest.osm.pbf /data/italy-latest.osm.pbf /data/spain-latest.osm.pbf /data/taiwan-latest.osm.pbf -o /data/merged-all.pbf

