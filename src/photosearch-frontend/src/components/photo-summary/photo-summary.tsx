import { Component, Host, h } from '@stencil/core';

@Component({
  tag: 'photo-summary',
  styleUrl: 'photo-summary.sass',
  shadow: true,
})
export class PhotoSummary {
  render() {
    return (
      <Host>
        <slot></slot>
      </Host>
    );
  }
}
