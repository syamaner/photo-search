
from os import environ
import transformers
transformers.logging.set_verbosity_error()
import logging
from opentelemetry import trace

from PIL import Image
from io import BytesIO
import base64

import requests 
from PIL import Image
from transformers import AutoProcessor, AutoModelForCausalLM 

from unittest.mock import patch
from transformers.dynamic_module_utils import get_imports


tracer = trace.get_tracer(__name__)
logging.basicConfig()
logging.root.setLevel(logging.NOTSET)

#docker run  -v ./cache:/home/user/hf:rw   florencelocal:1

def fixed_get_imports(filename: str ):
    """Work around for https://huggingface.co/microsoft/phi-1_5/discussions/72."""
    if not str(filename).endswith("/modeling_florence2.py"):
        return get_imports(filename)
    imports = get_imports(filename)
    imports.remove("flash_attn")
    return imports


with patch("transformers.dynamic_module_utils.get_imports", fixed_get_imports):
    print("loading model")
    model = AutoModelForCausalLM.from_pretrained("microsoft/Florence-2-large", trust_remote_code=True)
    print("loaded model")
    #model = torch.compile(model)
    
    print("loading processor")
    processor = AutoProcessor.from_pretrained("microsoft/Florence-2-large", trust_remote_code=True)
    print("loaded processor")
    prompt = "<MORE_DETAILED_CAPTION>"

    url = "https://huggingface.co/datasets/huggingface/documentation-images/resolve/main/transformers/tasks/car.jpg?download=true"
    image = Image.open(requests.get(url, stream=True).raw)

    inputs = processor(text=prompt, images=image, return_tensors="pt")
    with tracer.start_as_current_span("main"):
        logger = logging.getLogger(__name__)
    
        try:
            generated_ids = model.generate(
                input_ids=inputs["input_ids"],
                pixel_values=inputs["pixel_values"],
                max_new_tokens=1024,
                num_beams=3,
                do_sample=False
            )
            generated_text = processor.batch_decode(generated_ids, skip_special_tokens=False)[0]
            parsed_answer = processor.post_process_generation(generated_text, task="<MORE_DETAILED_CAPTION>", image_size=(image.width, image.height))
            print(parsed_answer)
            logger.info(parsed_answer)
        except:
            print("error")
            pass
