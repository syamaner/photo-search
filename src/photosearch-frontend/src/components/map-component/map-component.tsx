import { Component, State, h } from '@stencil/core';
import {  Map, Marker, StyleSpecification } from 'maplibre-gl'; 
import { PubSub } from 'pubsub-js'
import { EventNames } from '../../utils/event-names' 

@Component({ tag: 'map-component', styleUrl: 'map-component.css', shadow: true })
export class MapComponent {
    @State() mapElement: HTMLElement;
 
    polygobListenerSubscription: any;

    map: Map | undefined;
    markers: {
        [name: string]: Marker
    } = {};

    onPhotosLoaded = (_msg: string, _data: any) => {
    };

    onAddressSelected = (_msg: string, _data: any)  => {
        // for (const key in this.markers) {
        //     this.markers[key].remove();
        //     delete this.markers[key];
        // }
        // this.map.flyTo({
        //     center: [
        //         data.lon, data.lat
        //     ],
        //     zoom: 15
        // });
        // if (!this.markers[data.place_id]) {
        //     this.markers[data.place_id] = new Marker().setLngLat([data.lon, data.lat]).addTo(this.map);
        //     this.markers[data.place_id].setPopup(new Popup().setHTML(`<h1>${data.display_name}</h1>`));
        // }
    };


 
    disconnectedCallback() {
        if (this.polygobListenerSubscription) {
            PubSub.unsubscribe(this.polygobListenerSubscription);
        }

    }

    componentDidLoad() {
        this.polygobListenerSubscription = PubSub.subscribe(EventNames.AddressSelected, this.onPhotosLoaded);
        const style:StyleSpecification = {
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
                -74.5, 40
            ],
            zoom: 14
        }); 
    }

    render() {
        return  <div id="map" ref={(el) => this.mapElement = el as HTMLElement}></div>       
    }
} 
