import os
from llama_cpp.server.app import create_app
from llama_cpp.server.settings import ModelSettings, ServerSettings

# Create settings for your model
model_settings = ModelSettings(
    hf_model_repo_id="ggml-org/SmolVLM2-2.2B-Instruct-GGUF",
    model="SmolVLM2-2.2B-Instruct-f16.gguf",  # Path to your model file
    n_gpu_layers=-1,  # Use all GPU layers
    n_ctx=4096,       # Context size
)

# Create server settings
server_settings = ServerSettings(
    host="0.0.0.0",
    port=int(os.environ.get('PORT', 8080))
)

# Create the app with settings
app = create_app(
    server_settings=server_settings,
    model_settings=[model_settings]  # Note: model_settings should be in a list
)

# If you're running the server directly in this file:
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host=server_settings.host, port=server_settings.port)
