import { Component, Env, h } from '@stencil/core';
import { Map, Marker, Popup, StyleSpecification } from 'maplibre-gl';
import { PhotoSummary } from '../../models/PhotoSummary';

import { PubSub } from 'pubsub-js'
import { EventNames } from '../../utils/event-names'

@Component({ tag: 'map-component', styleUrl: 'map-component.css', shadow: true })
export class MapComponent {

  mapElement: HTMLElement;
  photoSummaries: Array<PhotoSummary> = [];
  map: Map | undefined;
  popup = new Popup({ className: "apple-popup",  closeOnClick: true, maxWidth: "450px", closeOnMove: false, closeButton: false });
  markers: {
    [name: string]: Marker
  } = {};

  loadImage = async (url) => {
    return (
      fetch(url)
        .then((resp) => resp.blob())
        .then((blob) => {
          return URL.createObjectURL(blob);
        })
    );
  };

  loadPhotos = async () => {
    const response = await fetch(Env.API_BASE_URL + "/api/photos");
    this.photoSummaries = await response.json();
    this.photoSummaries.forEach((photo) => {
      const marker = new Marker({
        draggable: false,
      }).setLngLat([photo.Longitude, photo.Latitude]);
      this.markers[photo.Id] = marker;
    });
  };

  componentWillLoad = async () => {
    await this.loadPhotos();
  }

  disconnectCallback = () => {
    this.markers = null;
    this.map = null;
  }

  componentDidLoad = async () => {
    const style: StyleSpecification = {
      version: 8,
      sources: {
        osm: {
          type: 'raster',
          tiles: [`${Env.MAP_TILE_SERVER}/tile/{z}/{x}/{y}.png`],
          tileSize: 256,
          attribution: 'Map tiles by <a target="_top" rel="noopener" href="https://tile.openstreetmap.org/">OpenStreetMap tile servers</a>, under the <a target="_top" rel="noopener" href="https://operations.osmfoundation.org/policies/tiles/">tile usage policy</a>. Data by <a target="_top" rel="noopener" href="http://openstreetmap.org">OpenStreetMap</a>'
        }
      },
      layers: [{
        id: 'osm',
        type: 'raster',
        source: 'osm',
      }],
    };

    this.map = new Map({
      container: this.mapElement,
      style: style,
      center: [
        this.photoSummaries[0].Longitude, this.photoSummaries[0].Latitude
      ],
      zoom: 14
    });

    this.photoSummaries.forEach((photo) => {
      let marker = this.markers[photo.Id];
      marker.addTo(this.map);
      let imgUrl = `${Env.API_BASE_URL}/api/image/${photo.Id}/1280/1280`;
      marker.getElement().addEventListener('click', () => { 
          this.loadImage(imgUrl).then((imageData) => {            
            this.popup.setHTML(`<img src='${imageData}' data-id="${photo.Id}"></img>`);
            marker.setPopup(this.popup);
            marker.togglePopup();
          });

          PubSub.publish(EventNames.PhotoSelected, photo);
      });

    });
  }

  render() {
    return <div id="map" ref={(el) => this.mapElement = el as HTMLElement}></div>
  }
}
