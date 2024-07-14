using System;
using System.Drawing;
using System.Drawing.Imaging;
using dipp;
using conversion;

namespace BorderRemoval
{
    public class ImageProcessing
    {
        public static Bitmap To_8Bit(Bitmap image)
        {
            if (image.PixelFormat != PixelFormat.Format24bppRgb)
            {
                throw new Exception("Image format is not supported only 24-bit and 8-bit images are supported");
            }
            
            return GrayScaleConvert.Conv_24to8bits(image);
        
        }

        public static unsafe Bitmap RemoveWhiteBorders(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;

            Bitmap rmvWhite;
            if (image.PixelFormat != PixelFormat.Format8bppIndexed)
            {
                Console.WriteLine("---------------------------------");
                Console.WriteLine($"Image pixel frormat: {image.PixelFormat}");
                rmvWhite = To_8Bit(image);
                Console.WriteLine($"Converted pixel format: {rmvWhite.PixelFormat}");
            }
            else
            {
                rmvWhite = (Bitmap)image.Clone();
            }

            BitmapData imageData = rmvWhite.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            byte* imagePtr = (byte*)imageData.Scan0.ToPointer();
            int stride = imageData.Stride;

            bool IsRowWhite(int row, int thr)
            {
                for (int col = 0; col < width; col++)
                {
                    byte* pixelPtr = imagePtr + row * stride + col;
                    if (*pixelPtr < thr)
                    {
                        return false;
                    }
                }
                return true;
            }

            bool IsColumnWhite(int col, int thr)
            {
                for (int row = 0; row < height; row++)
                {
                    byte* pixelPtr = imagePtr + row * stride + col;
                    if (*pixelPtr < thr)
                    {
                        return false;
                    }
                }
                return true;
            }

            int thr = 210;

            int top = 0;
            while (top < height && IsRowWhite(top, thr))
            {
                top++;
            }

            int bottom = height - 1;
            while (bottom > top && IsRowWhite(bottom, thr))
            {
                bottom--;
            }

            int left = 0;
            while (left < width && IsColumnWhite(left, thr))
            {
                left++;
            }

            int right = width - 1;
            while (right > left && IsColumnWhite(right, thr))
            {
                right--;
            }

            int newWidth = right - left + 1;
            int newHeight = bottom - top + 1;
/// THERE IS A PROBLEM HERE FIX IT
            // Bitmap croppedImage = new Bitmap(newWidth, newHeight, image.PixelFormat);
            // BitmapData croppedData = croppedImage.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, image.PixelFormat);

            BitmapData oldImgData = null;
            if (image.PixelFormat == PixelFormat.Format24bppRgb)
            {
                oldImgData = image.LockBits(new Rectangle(left, top, newWidth, newHeight), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            }
            else if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                oldImgData = image.LockBits(new Rectangle(left, top, newWidth, newHeight), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            }
            Bitmap croppedImage = null;

            if (image.PixelFormat == PixelFormat.Format24bppRgb)
            {
                byte* oldImgPtr = (byte*)oldImgData.Scan0.ToPointer();
                int oldImgStride = oldImgData.Stride;
                croppedImage = RemoveBorders24bpp(oldImgPtr, oldImgStride, newWidth, newHeight);
            }
            else if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                byte* oldImgPtr = (byte*)oldImgData.Scan0.ToPointer();
                int oldImgStride = oldImgData.Stride;
                croppedImage = RemoveBorders8bpp(oldImgPtr, oldImgStride, newWidth, newHeight);
            }

            rmvWhite.UnlockBits(imageData);
            image.UnlockBits(oldImgData);
            

            return croppedImage;

            unsafe Bitmap RemoveBorders24bpp(byte* oldImgPtr, int oldImgStride, int newWidth, int newHeight)
            {
                Bitmap croppedImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
                BitmapData croppedData = croppedImage.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                byte* croppedPtr = (byte*)croppedData.Scan0.ToPointer();
                int croppedStride = croppedData.Stride;

                for (int i = 0; i < newHeight; i++)
                {
                    for (int j = 0; j < newWidth; j++)
                    {
                        byte* srcPixelPtr = oldImgPtr + i * oldImgStride + j * 3;
                        byte* dstPixelPtr = croppedPtr + i * croppedStride + j * 3;
                        dstPixelPtr[0] = srcPixelPtr[0];
                        dstPixelPtr[1] = srcPixelPtr[1];
                        dstPixelPtr[2] = srcPixelPtr[2];
                    }
                }
                croppedImage.UnlockBits(croppedData);

                return croppedImage;
            }

            unsafe Bitmap RemoveBorders8bpp(byte* oldImgPtr, int oldImgStride, int newWidth, int newHeight)
            {
                Bitmap croppedImage = new Bitmap(newWidth, newHeight, PixelFormat.Format8bppIndexed);
                BitmapData croppedData = croppedImage.LockBits(new Rectangle(0, 0, newWidth, newHeight), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
    
                byte* croppedPtr = (byte*)croppedData.Scan0.ToPointer();
                int croppedStride = croppedData.Stride;
                ColorPalette palette = croppedImage.Palette;

                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                croppedImage.Palette = palette;

                for (int i = 0; i < newHeight; i++)
                {
                    for (int j = 0; j < newWidth; j++)
                    {
                        byte* srcPixelPtr = oldImgPtr + i * oldImgStride + j;
                        byte* dstPixelPtr = croppedPtr + i * croppedStride + j;
                        *dstPixelPtr = *srcPixelPtr;
                    }
                }
                croppedImage.UnlockBits(croppedData);

                return croppedImage;
            }
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
}
