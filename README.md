# Codeformer .NET Wrapper

This repository contains a .NET wrapper for using Codeformer, a tool for enhancing videos using the Codeformer model. Codeformer is a model designed for enhancing images and videos, and this wrapper provides a convenient UI for interacting with the Codeformer functionality.

## Features

- Provides a user-friendly UI for interacting with Codeformer.
- Allows for enhancing videos using the Codeformer model.
- Supports configuring parameters such as upscaling factor, fidelity, face upsampling, and background enhancement.

## Installation and Usage

1. Ensure you have Python 3.9 installed on your system.
2. Clone this repository to your local machine.
3. Build the solution using Visual Studio or any other preferred IDE.
4. Ensure all dependencies are installed. (Emgu.CV, Python.Runtime, etc.)
5. Install the required Python packages by running `pip install -r requirements.txt` in the `codeformer-DotNetWrapper` directory.
6. Set the `PYTHONNET_PYDLL` environment variable to point to `python39.dll`.
7. Run the application.

## Dependencies

- Emgu.CV: Library for image processing.
- Python.Runtime: Interoperability library for Python and .NET.
- Codeformer: Python package for video enhancement.

## Usage

1. Open the application.
2. Select a video file you want to enhance.
3. Adjust parameters as needed (upscaling factor, fidelity, etc.).
4. Click the "Enhance" button to start the enhancement process.
5. Wait for the process to finish. You can pause/play the video during the process.
6. Once enhancement is complete, you can save the enhanced video.

## Contributors

- Just me 

## License

This project is licensed under the [MIT License](LICENSE).
