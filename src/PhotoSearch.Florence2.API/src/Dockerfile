FROM bitnami/pytorch:2.3.1-debian-12-r4
 

USER root
RUN useradd -m -u 1000 user

USER user

ENV HOME=/home/user \
	PATH=/home/user/.local/bin:$PATH \
    HF_HOME=/home/user/hf/hfcache \
    TRANSFORMERS_CACHE=/home/user/hf/tfcache \
    HUGGINGFACE_HUB_CACHE=/home/user/hf/hubcache \
    PYTHONUNBUFFERED=0 \
    FLORENCE_MODEL=Florence-2-large

WORKDIR $HOME/app

COPY requirements.txt .
RUN pip install -r requirements.txt

COPY --chown=user . $HOME/app


CMD ["python",  "main.py"]