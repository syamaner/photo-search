import { Component, h } from '@stencil/core';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true,
})
export class AppHome {
  render() {
    return (
      <div class="app-home">
 

        <stencil-route-link url="/photos">
          <button>View Photos</button>
        </stencil-route-link>
      </div>
    );
  }
}
