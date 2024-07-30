# Photo Search Demo

This is a repository that will eventually support full text search on your own photos locally.

The high level steps are:

- Phase 1
- [ ] Indexing the photos in a database
  -  [x] Extract Exif Data
  - [x] Extract Original Timestamps and GPS data if available
  - [x] Summarise and categorise the photo using multi-modal models
    - [x] In order to evaluate the performance, for each photo there are multiple summaries generated using different local models using a combination of Ollama API as well as models Served by Hugging Face using a simple Python Flask server. 
  - [x] Reverse geocode GPS position if there was GPS data associated
- [ ] Evaluate Model Performance
  - [ ] Option A: Automate using state of the art models from commercial suppliers such as OpenAI
  - [ ] Option B: A custom UI that shows the image and the summaries and a rating component vor manual evaluation by humans.
- Phase 2
- [ ] Build an RAG to search the images using natural language
  - [ ] Import embeddings of the summaries into a vector DB
  - [ ] Query the DB and provide context to a local LLM and show the results
- Phase 3
- [ ] Duplicate finder
  - [ ] Index photo features in a vector database
  - [ ] Cluster the images based on similarity to:
    - Find the original image
    - Find copies and similar photos
      - A copy can be an edited version of the original
      - A similar photo could be same location taken by a different camera or under different conditions / seasons
   - [ ] Visualise similarities and allow users to chose which ones to delete
   - [ ] Provide a delete script to verify and execute to remove duplicates
 - Identify blurry images for deletion



## Running locally:

- Python
  - Install Rye to manage Python Dependencies
  - Once installed `cd src/PhotoSearch.Florence2.API/src` then `rye sync`
    - This will create a virtual environment and install the necessary packages.
- For running docker locally:
  - Ensure Docker is running on the local machine.
  - Select `http-local-host-no-gpu` inside [PhotoSearch.AppHost/Properties/launchSettings.json](./src/PhotoSearch.AppHost/Properties/launchSettings.json)
    - Or a derived configuration where you set ENABLE_NVIDIA_DOCKER to true if:
      - You have Nvidia GPU available locally
      - And have Nvidia Docker installed and working
      - This can work natively in Linux and also runs on WSL 2 in Windows 10 / 11
      - [Nvidia Container Toolkit](https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/latest/install-guide.html)
      - [Cuda support on WSL 2](https://docs.nvidia.com/cuda/wsl-user-guide/index.html)
      - On Linux []Lambda Stack from Lambda Labs(https://lambdalabs.com/lambda-stack-deep-learning-software) makes management of Nvidia drivers and container toolkit a breeze.
    - If no GPU used, it is wort using small models using the following environment variables inside [PhotoSearch.AppHost/Properties/launchSettings.json](./src/PhotoSearch.AppHost/Properties/launchSettings.json)
      - `"OLLAMA_MODEL": "llava-phi3",`
      - `"FLORENCE_MODEL": "Florence-2-base-ft",`
- Once application starts, the following functionality is available using the included [Http file](./src/PhotoSearch.API/PhotoSearch.API.http) in the API project.
  - The environment variables can be edited in [http-client.env.json file](./src/PhotoSearch.API/http-client.env.json)
  - The following calls can be used:
    - `{{API_HostAddress}}/photos/index/photos-geocode`, ``: to index the photos in the database.
    - `GET {{API_HostAddress}}/photos/summarise/{{ollamaModel}}` trigger summarisation of photos using `OLLAMA_MODEL` environment variable using Ollama container.
    - `GET {{API_HostAddress}}/photos/summarise/{{florenceModel}}` trigger summarisation using  `FLORENCE_MODEL` variants that will be handled in the worker making calls to Phyton service.
    - `GET {{Nominatim_HostAddress}}/reverse?format=geojson&namedetails=1&lat=47.378177&lon=8.540192` an example request for Nominatim container for reverse geocoding lookup.

