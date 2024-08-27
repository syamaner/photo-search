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
  @Prop({ mutable: true }) selectedPhoto: PhotoSummary = undefined;
  @Prop({ mutable: true }) availableModels: Set<string> = new Set<string>();
  @Prop({ mutable: true }) selectedModel: string;
  @Prop({ mutable: true }) selectedSummary: string;
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
          <form class="flex mb-4 map-container p-3" >
            <label  class="w-1/4  align-middle pt-2 ">Model: </label>
            
            <select id="models" onInput={(event) => this.handleSelect(event)} class="w-3/4 bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500">
              <option value='-' selected={true}>Please Select a model.</option>
              {[...this.availableModels].map(model => (
                <option value={model} selected={this.selectedModel === model}>{model}  </option>
              ))}
            </select>
            </form>
            <div class="p-3">
              <p class="prose prose-lg">{this.selectedSummary}</p>
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
