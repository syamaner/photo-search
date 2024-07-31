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


svc = Florence2Service.Florence2Service(os.environ["FLORENCE_MODEL"])
model = os.environ["FLORENCE_MODEL"]


@app.post('/api/summarise/<filepath>')
def summarise_photo(filepath):
    logger = logging.getLogger(__name__)
    with tracer.start_as_current_span(f"summarise-with-{model}"):   
        content = request.json
        summary = svc.generate_caption(content['base64image'])
        object_response = svc.generate_object_list(content['base64image'])
        logger.info(object_response)
        logger.info(summary)
        result = {"summary": summary['<MORE_DETAILED_CAPTION>'], "objects":object_response["<OD>"]['labels']}
        response = json.dumps(result)     
        logger.info( response)
        return response
if __name__ == "__main__":
    port = int(os.environ.get('PORT', 8111))
    app.run(debug=True,host='0.0.0.0',port=port,use_reloader=False)