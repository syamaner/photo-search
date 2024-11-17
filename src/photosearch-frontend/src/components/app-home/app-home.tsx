import { Component, Env, h, Prop } from '@stencil/core';

@Component({
  tag: 'app-home',
  styleUrl: 'app-home.css',
  shadow: true,
})
export class AppHome {


  @Prop({ mutable: true }) availableModels: Array<string> = new Array<string>();
  @Prop({ mutable: true }) selectedModel: string;

  @Prop({ mutable: true }) showMessage: boolean = false;
  private handleClick = async () => {
    const response = await fetch(Env.API_BASE_URL + "/api/photos/summarise/batch?ModelNames=" + this.selectedModel);
    if (response.status == 200) {
      this.showMessage = true;
      setTimeout(() => {
        this.showMessage = false;
      }, 6000);
    }
  }

  async componentDidLoad() {
    const response = await fetch(Env.API_BASE_URL + "/api/models/list");

    const results: Array<string> = await response.json();
    this.availableModels = [...results];}

  handleSelect = (event) => {
    console.log(event.target.value);
    this.selectedModel = event.target.value;
  }


  render() {
    return (
      <div class="app-home">

        <stencil-route-link url="/photos">
          <button>View Photos</button>
        </stencil-route-link>
        <div>

        </div>
        <form class="flex mb-4 map-container p-3" >
          <label class="w-1/4  align-middle pt-2 ">Model: </label>
          <select id="models" onInput={(event) => this.handleSelect(event)} class="w-3/4 bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-blue-500 focus:border-blue-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white dark:focus:ring-blue-500 dark:focus:border-blue-500">
            <option value='-' selected={true}>Please Select a model.</option>
            {[...this.availableModels].map(model => (
              <option value={model} selected={this.selectedModel === model}>{model}  </option>
            ))}
          </select>
        </form>
        <div class="grid items-end gap-6 mb-6 md:grid-cols-3">
          <button disabled={!this.selectedModel} onClick={this.handleClick}>Regenerate summaries</button>
          <div hidden={!this.showMessage} class="bg-blue-100 border-t border-b border-blue-500 text-blue-700 px-4 py-3" role="alert">
            <p class="font-bold">Message sent.</p>
            <p class="text-sm">Photos will be summarised using model {this.selectedModel}.</p>
          </div>
        </div>
      </div>
    );
  }
}
