import base64
from io import BytesIO
import logging
from opentelemetry import trace
from PIL import Image
import requests
import Florence2Service
from flask import Flask, Response, json, request, jsonify,make_response
import os
app = Flask(__name__)

tracer = trace.get_tracer(__name__)
logging.basicConfig()
logging.root.setLevel(logging.INFO)

#docker run  -v ./cache:/home/user/hf:rw -p 5051:5000  florencelocal:1

svc = Florence2Service.Florence2Service(os.environ["FLORENCE_MODEL"])

# @app.post("/")
# def home():
#     print("Hello World")
#     url = "https://huggingface.co/datasets/huggingface/documentation-images/resolve/main/transformers/tasks/car.jpg?download=true"
#     image = Image.open(requests.get(url, stream=True).raw)
#     buffered = BytesIO()
#     image.save(buffered, format="JPEG")
#     img_str = base64.b64encode(buffered.getvalue())
    
#     summary = None
#     with tracer.start_as_current_span("summarise-florence2"):        
#         logger = logging.getLogger(__name__)
      
#         summary = svc.generate_caption(img_str)
#         object_response = svc.generate_object_list(img_str)
#         logger.info(summary)
#         result = {"summary": summary['<MORE_DETAILED_CAPTION>'], "objects":object_response["<OD>"]['labels']}
#         response1 = json.dumps(result) # make_response(result, 200)  #['<OD>']['labels']      
#         logger.info( response1)
#         return response1
    
#     return summary


@app.post('/api/summarise/<filepath>')
def summarise_photo(filepath):
    
    with tracer.start_as_current_span("summarise-florence2"):        
        logger = logging.getLogger(__name__)
        content = request.json
        summary = svc.generate_caption(content['base64image'])
        object_response = svc.generate_object_list(content['base64image'])
        logger.info(object_response)
        logger.info(summary)
        result = {"summary": summary['<MORE_DETAILED_CAPTION>'], "objects":object_response["<OD>"]['labels']}
        response1 = json.dumps(result) # make_response(result, 200)  #['<OD>']['labels']      
        logger.info( response1)
      #  logger.error(response)
        return response1
if __name__ == "__main__":
    app.run(debug=True,host='0.0.0.0')