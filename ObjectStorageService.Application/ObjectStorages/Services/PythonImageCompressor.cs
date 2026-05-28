
namespace ObjectStorageService.ObjectStorages.Services;

using Python.Runtime;

public sealed class PythonImageCompressor
{
    public byte[] CompressToWebp(byte[] imageBytes)
    {
        PythonEngine.Initialize();

        try
        {
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                dynamic io = Py.Import("io");

                PythonEngine.Exec("""
from io import BytesIO
from PIL import Image, ImageOps

MAX_OUTPUT_SIZE = 50 * 1024
MIN_QUALITY = 20
MIN_SIDE = 320

def compress_to_webp(image_bytes, target_size=MAX_OUTPUT_SIZE):
    image_file = BytesIO(image_bytes)

    image_file.seek(0)
    image = Image.open(image_file)
    image = ImageOps.exif_transpose(image)

    if image.mode not in ("RGB", "RGBA"):
        image = image.convert("RGBA" if "A" in image.getbands() else "RGB")

    current = image
    scale = 1.0

    while True:
        for quality in range(90, MIN_QUALITY - 1, -5):
            output = BytesIO()
            current.save(
                output,
                format="WEBP",
                quality=quality,
                method=6
            )

            data = output.getvalue()

            if len(data) <= target_size:
                return data

        width, height = current.size
        next_width = max(MIN_SIDE, int(width * 0.9))
        next_height = max(MIN_SIDE, int(height * 0.9))

        if (next_width, next_height) == current.size:
            break

        scale *= 0.9

        current = image.resize(
            (next_width, next_height),
            Image.Resampling.LANCZOS
        )

        if scale < 0.2 and min(current.size) <= MIN_SIDE:
            break

    output = BytesIO()

    current.save(
        output,
        format="WEBP",
        quality=MIN_QUALITY,
        method=6
    )

    data = output.getvalue()

    if len(data) <= target_size:
        return data

    raise ValueError(
        "Could not compress image below target size."
    )
""");

                dynamic main = Py.Import("__main__");
                dynamic compress = main.compress_to_webp;

                using PyObject pyBytes = imageBytes.ToPython();
                dynamic result = compress(pyBytes);

                return result.As<byte[]>();
            }
        }
        finally
        {
            PythonEngine.Shutdown();
        }
    }

}