import { Component, Env, h } from '@stencil/core';
import { Map, Marker, Popup, StyleSpecification } from 'maplibre-gl';
import { PhotoSummary } from '../../models/PhotoSummary';

// import { PubSub } from 'pubsub-js'
// import { EventNames } from '../../utils/event-names' 

@Component({ tag: 'map-component', styleUrl: 'map-component.css', shadow: true })
export class MapComponent {

  mapElement: HTMLElement;
  data: Array<PhotoSummary> = [];
  map: Map | undefined;

  markers: {
    [name: string]: Marker
  } = {};

  loadPhotos = async () => {
    const response = await fetch(Env.API_BASE_URL + "/photos/abc");
    this.data = await response.json();
  };

  componentWillLoad = async () => {
    await this.loadPhotos();
  }

  disconnectCallback = () => {
    this.markers=null;
    this.map=null;
  }

  componentDidLoad = async () => {
    const style: StyleSpecification = {
      version: 8,
      sources: {
        osm: {
          type: 'raster',
          tiles: ["https://tile.openstreetmap.org/{z}/{x}/{y}.png"],
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
        this.data[0].Longitude, this.data[0].Latitude
      ],
      zoom: 14
    });

    this.data.forEach((photo) => {
      const marker = new Marker({
        //color: "#FFFFFF",
        draggable: false
      })
        .setLngLat([photo.Longitude, photo.Latitude])
        .addTo(this.map);
      console.log(photo.Address);
      const imgUrl = `${Env.API_BASE_URL}/image/${photo.Id}/640/480`;
      marker.setPopup(new Popup({ className: "apple-popup" }).setHTML(`<img src='${imgUrl}' loading="lazy"></img>`));
      this.markers[photo.Id] = marker;

    });

  }

  render() {
    return <div id="map" ref={(el) => this.mapElement = el as HTMLElement}></div>
  }
} 
