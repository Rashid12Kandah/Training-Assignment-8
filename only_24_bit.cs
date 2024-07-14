using System;
using System.Drawing;
using System.Drawing.Imaging;

public class ImageProcessing
{
    public static unsafe Bitmap RemoveWhiteBorders(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;
        BitmapData imageData = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        byte* imagePtr = (byte*)imageData.Scan0.ToPointer();
        int stride = imageData.Stride;

        // Function to check if a row is white
        bool IsRowWhite(int row,int thr)
        {
            for (int col = 0; col < width; col++)
            {
                byte* pixelPtr = imagePtr + row * stride + col * 3;
                if (pixelPtr[0] < thr || pixelPtr[1] < thr || pixelPtr[2] < thr)
                {
                    return false;
                }
            }
            return true;
        }

        // Function to check if a column is white
        bool IsColumnWhite(int col,int thr)
        {
            for (int row = 0; row < height; row++)
            {
                byte* pixelPtr = imagePtr + row * stride + col * 3;
                if (pixelPtr[0] < thr || pixelPtr[1] < thr || pixelPtr[2] < thr)
                {
                    return false;
                }
            }
            return true;
        }

        // Find the top border
        int top = 0;
        while (top < height && IsRowWhite(top,200))
        {
            top++;
        }

        // Find the bottom border
        int bottom = height - 1;
        while (bottom > top && IsRowWhite(bottom,200))
        {
            bottom--;
        }

        // Find the left border
        int left = 0;
        while (left < width && IsColumnWhite(left,200))
        {
            left++;
        }

        // Find the right border
        int right = width - 1;
        while (right > left && IsColumnWhite(right,200))
        {
            right--;
        }

        // Create the new cropped image
        int newWidth = right - left + 1;
        int newHeight = bottom - top + 1;
        Bitmap croppedImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
        BitmapData croppedData = croppedImage.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        byte* croppedPtr = (byte*)croppedData.Scan0.ToPointer();
        int croppedStride = croppedData.Stride;

        // Copy the pixels to the new image
        for (int i = 0; i < newHeight; i++)
        {
            for (int j = 0; j < newWidth; j++)
            {
                byte* srcPixelPtr = imagePtr + (top + i) * stride + (left + j) * 3;
                byte* dstPixelPtr = croppedPtr + i * croppedStride + j * 3;
                dstPixelPtr[0] = srcPixelPtr[0];
                dstPixelPtr[1] = srcPixelPtr[1];
                dstPixelPtr[2] = srcPixelPtr[2];
            }
        }

        image.UnlockBits(imageData);
        croppedImage.UnlockBits(croppedData);

        return croppedImage;
    }

    public static void Main(string[] args)
    {
        string inputPath = args[0];
        Console.WriteLine($"Input image: {inputPath}");
        Bitmap image = new Bitmap(inputPath);
        Console.WriteLine($"Image size: {image.Width}x{image.Height}");
        Console.WriteLine($"Image format: {image.PixelFormat}");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        stopwatch.Start();
        Bitmap croppedImage = RemoveWhiteBorders(image);
        stopwatch.Stop();
        Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} ms");
        croppedImage.Save("cropped_image.png", ImageFormat.Png);
    }
}
