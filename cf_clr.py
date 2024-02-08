import os
import glob
import argparse
from codeformer.app import inference_app

def create_folder():
    if not os.path.exists(f"{os.getcwd()}/outCode"):
        os.makedirs(f"{os.getcwd()}/outCode")

def main(args):
    # Parse command-line arguments
    create_folder()
    parser = argparse.ArgumentParser(description="Run inference with codeformer.app")
    parser.add_argument("image", help="Path to the input image")
    parser.add_argument("--background_enhance", action="store_true", help="Enhance background (default: False)", default=True)
    parser.add_argument("--face_upsample", action="store_true", help="Upsample faces (default: False)", default=True)
    parser.add_argument("--upscale", type=int, default=2, help="Upscale factor (default: 2)")
    parser.add_argument("--codeformer_fidelity", type=int, default=1, help="Codeformer fidelity (default: 1)")
    args = parser.parse_args(args)

    if os.path.isfile(args.image):
        out = inference_app(
                image=args.image,
                background_enhance=args.background_enhance,
                face_upsample=args.face_upsample,
                upscale=args.upscale,
                codeformer_fidelity=args.codeformer_fidelity,
            )
        os.rename(out, f"outCode/{os.path.basename(args.image)}")
    elif os.path.isdir(args.image):
        png_files = glob.glob(os.path.join(args.image, '*.png'))
        for file_path in png_files:
            out = inference_app(
                image=file_path,
                background_enhance=args.background_enhance,
                face_upsample=args.face_upsample,
                upscale=args.upscale,
                codeformer_fidelity=args.codeformer_fidelity,
            )
            os.rename(out, f"outCode/{os.path.basename(file_path)}")
    else:
        print(f"{args.image} does not exist or is neither a file nor a directory.")

    

if __name__ == "__main__":
    import sys
    main(sys.argv[1:])