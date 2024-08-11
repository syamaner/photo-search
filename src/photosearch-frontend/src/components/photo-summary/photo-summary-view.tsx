import { Component, Host, Prop, h } from '@stencil/core';
import { PubSub } from 'pubsub-js'
import { EventNames } from '../../utils/event-names'
import { PhotoSummary } from '../../models/PhotoSummary';

@Component({
  tag: 'photo-summary-view',
  styleUrl: 'photo-summary-view.sass',
  shadow: true,
})
export class PhotoSummaryView {
  photoSelectedListenerSubscription: any;
  @Prop() selectedPhoto: PhotoSummary = undefined;
  @Prop() availableModels: Set<string> = new Set<string>();
  @Prop() selectedModel: string;
  @Prop() selectedSummary: string;
  onPhotoSelected = (_msg: string, data: PhotoSummary) => {
    this.selectedPhoto = data;
    
    for (const key in data.Summaries) {
      if (data.Summaries.hasOwnProperty(key)) {
        this.availableModels.add(key);
      }
    }

    this.selectedModel = this.availableModels[0];
    this.selectedSummary = data.Summaries[this.selectedModel];

  }

  handleSelect = (event) => {
    console.log(event.target.value);
    this.selectedModel = event.target.value;
    this.selectedSummary = this.selectedPhoto.Summaries[this.selectedModel];
  }
  componentDidLoad() {
    this.photoSelectedListenerSubscription = PubSub.subscribe(EventNames.PhotoSelected, this.onPhotoSelected);
  }

  render() {

    if (this.selectedPhoto) {
      return (
        <Host>
          <slot>
            <select onInput={(event) => this.handleSelect(event)}>
            <option value='-' selected={true}>Please Select a model.</option>
              {[...this.availableModels].map(model => (
                <option value={model} selected={this.selectedModel===model}>{model}  </option>
              ))}
            </select>
            <div class="p-3">
            <article class="prose prose-lg">{this.selectedSummary}</article>
            </div>
          </slot>
        </Host>
      );
    }
    else {
      return (
        <Host>
          <slot><span>Please select a photo.</span></slot>
        </Host>
      );
    }
  }
}
