from codeformer.app import inference_app
import os


def create_folder():
    if not os.path.exists(f"{os.getcwd()}/outCode"):
        os.makedirs(f"{os.getcwd()}/outCode")


def run_codeformer(
    image, background_enhance, face_upsample, upscale, codeformer_fidelity
):
    create_folder()
    out = inference_app(
        image=image,
        background_enhance=background_enhance,
        face_upsample=face_upsample,
        upscale=upscale,
        codeformer_fidelity=codeformer_fidelity,
    )
    if not os.path.exists(f"outCode/{os.path.basename(image)}"):
        os.rename(out, f"outCode/{os.path.basename(image)}")
