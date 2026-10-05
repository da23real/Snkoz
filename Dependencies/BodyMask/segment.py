"""Local, offline person masks. Binary RGB256 input / uint8 mask256 output.
Google MediaPipe SelfieSegmenter; no camera, network, files or user uploads.
"""
import os
os.environ.setdefault('TF_CPP_MIN_LOG_LEVEL', '2')
import sys
import msvcrt
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

msvcrt.setmode(sys.stdin.fileno(), os.O_BINARY)
msvcrt.setmode(sys.stdout.fileno(), os.O_BINARY)
options = vision.ImageSegmenterOptions(
    base_options=python.BaseOptions(model_asset_path=os.path.join(os.path.dirname(__file__), 'selfie_segmenter.tflite')),
    running_mode=vision.RunningMode.IMAGE,
    output_category_mask=False,
    output_confidence_masks=True,
)
def read_exact(size):
    data = bytearray()
    while len(data) < size:
        part = sys.stdin.buffer.read(size-len(data))
        if not part:
            if data:
                raise EOFError('Incomplete RGB frame')
            return None
        data.extend(part)
    return data

with vision.ImageSegmenter.create_from_options(options) as segmenter:
    sys.stdout.buffer.write(b'SKMASK2\n')
    sys.stdout.buffer.flush()
    while True:
        data = read_exact(256*256*3)
        if data is None:
            break
        rgb = np.frombuffer(data, dtype=np.uint8).reshape(256,256,3).copy()
        result = segmenter.segment(mp.Image(image_format=mp.ImageFormat.SRGB, data=rgb))
        masks = result.confidence_masks
        if not masks:
            raise RuntimeError('Person confidence mask absent')
        probability = np.squeeze(masks[1 if len(masks)>1 else 0].numpy_view())
        if probability.shape != (256,256):
            raise RuntimeError('Unexpected mask dimensions: '+str(probability.shape))
        # Soft transition; no person means no effect, never an all-frame fallback.
        mask = np.clip((probability-.25)*2,0,1)
        sys.stdout.buffer.write((mask*255).astype(np.uint8).tobytes())
        sys.stdout.buffer.flush()
