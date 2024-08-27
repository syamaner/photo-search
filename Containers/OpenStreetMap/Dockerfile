FROM overv/openstreetmap-tile-server:2.3.0

COPY ./scripts/start.sh /start.sh
RUN chmod +x /start.sh
ENTRYPOINT [ "/start.sh"]

EXPOSE 80 5432