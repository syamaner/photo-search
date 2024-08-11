import { Component, Host, Prop, h } from '@stencil/core';
import { MatchResults } from '@stencil-community/router';

@Component({
  tag: 'app-profile',
  styleUrl: 'app-profile.css',
  shadow: true,
})
export class AppProfile {
  @Prop() match: MatchResults;


  

  render() {
    
      return (
        <Host>
          <div id="map-container">
            <map-component></map-component>
          </div>
          <div >
            <p class="italic ">The quick brown fox ...</p>
          </div>
        </Host>
      );
  
  }
}
