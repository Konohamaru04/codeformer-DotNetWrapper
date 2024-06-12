import cv2
import os
import requests
import numpy as np
import torch
from gfpgan import GFPGANer

def download_file(url, local_path):
    response = requests.get(url, stream=True)
    os.makedirs(os.path.dirname(local_path), exist_ok=True)
    with open(local_path, 'wb') as f:
        for chunk in response.iter_content(chunk_size=8192):
            f.write(chunk)

def restore_image(image_path, bg_upsampler='realesrgan', bg_tile=400):
    # Define model name and download URL
    model_name = 'GFPGANv1.4'
    gfpgan_url = 'https://github.com/TencentARC/GFPGAN/releases/download/v1.3.4/GFPGANv1.4.pth'
    realesrgan_url = 'https://github.com/xinntao/Real-ESRGAN/releases/download/v0.2.1/RealESRGAN_x2plus.pth'

    # Determine model path
    model_path = os.path.join('experiments/pretrained_models', model_name + '.pth')
    if not os.path.isfile(model_path):
        model_path = os.path.join('gfpgan/weights', model_name + '.pth')
    if not os.path.isfile(model_path):
        print(f"Model not found locally. Downloading from {gfpgan_url}...")
        download_file(gfpgan_url, model_path)
        print(f"Model downloaded and saved to {model_path}")

    # Set up background upsampler if specified
    if bg_upsampler == 'realesrgan' and torch.cuda.is_available():
        from basicsr.archs.rrdbnet_arch import RRDBNet
        from realesrgan import RealESRGANer
        print("Setting up RealESRGAN for background enhancement...")
        model = RRDBNet(num_in_ch=3, num_out_ch=3, num_feat=64, num_block=23, num_grow_ch=32, scale=2)
        bg_model_path = os.path.join('experiments/pretrained_models', 'RealESRGAN_x2plus.pth')
        if not os.path.isfile(bg_model_path):
            print(f"RealESRGAN model not found locally. Downloading from {realesrgan_url}...")
            download_file(realesrgan_url, bg_model_path)
            print(f"RealESRGAN model downloaded and saved to {bg_model_path}")
        bg_upsampler = RealESRGANer(
            scale=2,
            model_path=bg_model_path,
            model=model,
            tile=bg_tile,
            tile_pad=10,
            pre_pad=0,
            half=True)  # Use half precision to save memory
    else:
        bg_upsampler = None
        if bg_upsampler == 'realesrgan':
            print("RealESRGAN background upsampler is not used because CUDA is not available.")

    # Initialize the GFPGANer
    restorer = GFPGANer(model_path=model_path, upscale=2, arch='clean', channel_multiplier=2, bg_upsampler=bg_upsampler)

    # Load the image
    img = cv2.imread(image_path)

    # Perform restoration
    results = restorer.enhance(img, has_aligned=False, only_center_face=False, paste_back=True)

    # Unpack the results
    cropped_faces, restored_faces, restored_img = results

    # Ensure the output directory exists
    output_dir = os.path.join(os.getcwd(), 'outCode')
    os.makedirs(output_dir, exist_ok=True)

    # Get the base name of the input image
    base_name = os.path.splitext(os.path.basename(image_path))[0]

    # Save the restored full image
    save_path = os.path.join(output_dir, f"{base_name}.png")
    if isinstance(restored_img, np.ndarray):
        cv2.imwrite(save_path, restored_img)
        print(f"Restored full image saved at {save_path}")
    else:
        print("The restored image is not a numpy array.")

# Example usage:
#restore_image('test.png')
