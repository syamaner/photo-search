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
          <div class="flex mb-4 map-container" >
          <div class="w-1/2 h-120 " >
            <map-component></map-component>
          </div>
          <div  class="w-1/2   h-120 ">
            <photo-summary-view></photo-summary-view>
          </div></div>
        </Host>
      );
  
  }
}
