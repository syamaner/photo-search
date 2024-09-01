import { Component, Host, Prop, h } from '@stencil/core';
import { PubSub } from 'pubsub-js'
import { EventNames } from '../../utils/event-names'
import { PhotoSummary, ModelResponse } from '../../models/PhotoSummary';

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
  @Prop({ mutable: true }) categories: Array<string> = [];
  @Prop({ mutable: true }) objects: Array<string> = [];

  onPhotoSelected = (_msg: string, data: PhotoSummary) => {
    this.selectedPhoto = data;

    for (const key in data.Summaries) {
      if (data.Summaries.hasOwnProperty(key)) {
        this.availableModels.add(key);
      }
    }

    this.selectedModel="";
    this.selectedSummary="";
    this.categories.length = 0;
    this.objects.length = 0;
    this.selectedModel = this.availableModels[0];
    // let photo = data.Summaries[this.selectedModel];
    this.selectedSummary = data.Summaries[this.selectedModel].Summary;
    // console.log(photo);

    // this.categories = [... data.Summaries[this.selectedModel].Categories];
    // this.objects = [... data.Summaries[this.selectedModel].DetectedObjects];

  }

  handleSelect = (event) => {
    console.log(event.target.value);
    this.selectedModel = event.target.value;
    this.selectedSummary = this.selectedPhoto.Summaries[this.selectedModel].Summary;

    let photo = this.selectedPhoto.Summaries[this.selectedModel];

    console.log(photo);
    this.categories = [...photo.Categories];
    this.objects = [...photo.DetectedObjects];
  }
  componentDidLoad() {
    this.photoSelectedListenerSubscription = PubSub.subscribe(EventNames.PhotoSelected, this.onPhotoSelected);
  }

  render() {
    if (this.selectedPhoto) {
      return (
        <Host>
            <form class="flex mb-4 map-container p-3" >
              <label class="w-1/4  align-middle pt-2 ">Model: </label>
              <select id="models" onInput={(event) => this.handleSelect(event)} class="w-3/4 bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500">
                <option value='-' selected={true}>Please Select a model.</option>
                {[...this.availableModels].map(model => (
                  <option value={model} selected={this.selectedModel === model}>{model}  </option>
                ))}
              </select>
            </form>

            <div class="p-3">
              <label class="font-bold">Address:</label>
              <p class="prose prose-lg">{this.selectedPhoto.Address}</p>
              <p><a class="font-medium text-blue-600 dark:text-blue-500 hover:underline" target='blank' href={`https://maps.google.com/?q=${this.selectedPhoto.Latitude},${this.selectedPhoto.Longitude}`}>Google Maps Link</a> </p>
            </div>  
            <div style={{display: this.selectedModel ? 'block':'none'}}>
            
            <div class="p-3">
            <label class="font-bold">Photo Description</label>
              <p class="prose prose-lg">{this.selectedSummary}</p>
            </div>
            <div class="p-3">
              <div class="grid grid-rows-1 grid-flow-col gap-4">
                <div class="col-span-1">
                  <label class="font-bold">Photo Categories</label>
                  {this.categories.map((category) => (
                    <li key={category}>
                      <ul>{category}</ul>
                    </li>
                  ))}
                </div>
                <div class="col-span-1">
                  <label class="font-bold">Photo Contents</label>
                  {this.objects.map((item) => (
                    <li key={item}>
                      <ul>{item}</ul>
                    </li>
                  ))}
                </div>
              </div>
              </div>
            </div>
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
