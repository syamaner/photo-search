import logging
from opentelemetry import trace
from PIL import Image
from io import BytesIO
import base64
from transformers import AutoProcessor, AutoModelForCausalLM 
from unittest.mock import patch
from transformers.dynamic_module_utils import get_imports

tracer = trace.get_tracer(__name__)
logging.basicConfig()
logging.root.setLevel(logging.INFO)


def fixed_get_imports(filename: str ):
    """Work around for https://huggingface.co/microsoft/phi-1_5/discussions/72."""
    if not str(filename).endswith("/modeling_florence2.py"):
        return get_imports(filename)
    imports = get_imports(filename)
    imports.remove("flash_attn")
    return imports


class Florence2Service:
    def __init__(self, modelName: str):
        logger = logging.getLogger(__name__)    
        with patch("transformers.dynamic_module_utils.get_imports", fixed_get_imports):
            with tracer.start_as_current_span("florence2_service_init"):
                logger.info(f"Initialising Florence2Service with Model {modelName}")    
                self.model = AutoModelForCausalLM.from_pretrained(f"microsoft/{modelName}", trust_remote_code=True)
                self.processor = AutoProcessor.from_pretrained(f"microsoft/{modelName}", trust_remote_code=True)
                self.detailed_caption_prompt = "<MORE_DETAILED_CAPTION>"
                self.object_detection_prompt = "<OD>"
                logger.info("Florence2Service initialized")
            
    def generate_caption(self, base64_image: str)->str:
        image = Image.open(BytesIO(base64.b64decode(base64_image)))
        inputs = self.processor(text=self.detailed_caption_prompt, images=image, return_tensors="pt")
        logger = logging.getLogger(__name__)   
        with tracer.start_as_current_span("generatecaption"):
            try:
                generated_ids = self.model.generate(
                    input_ids=inputs["input_ids"],
                    pixel_values=inputs["pixel_values"],
                    max_new_tokens=1024,
                    num_beams=3,
                    do_sample=False
                )
                generated_text = self.processor.batch_decode(generated_ids, skip_special_tokens=False)[0]
                parsed_answer = self.processor.post_process_generation(generated_text, task=self.detailed_caption_prompt, image_size=(image.width, image.height))    
                logger.info(parsed_answer)
                return parsed_answer
            except Exception as error:
                logger.error(error)
                pass
            
        
    def generate_object_list(self, base64_image: str)->str:
        image = Image.open(BytesIO(base64.b64decode(base64_image)))
        inputs = self.processor(text=self.object_detection_prompt, images=image, return_tensors="pt")
        with tracer.start_as_current_span("object_detection"):
            logger = logging.getLogger(__name__)        
            try:
                generated_ids = self.model.generate(
                    input_ids=inputs["input_ids"],
                    pixel_values=inputs["pixel_values"],
                    max_new_tokens=1024,
                    num_beams=3,
                    do_sample=False
                )
                generated_text = self.processor.batch_decode(generated_ids, skip_special_tokens=False)[0]
                parsed_answer = self.processor.post_process_generation(generated_text, task=self.object_detection_prompt, image_size=(image.width, image.height))    
                logger.info(parsed_answer)
                
                return parsed_answer
            except Exception as error:
                logger.error(error)
                pass
        
        