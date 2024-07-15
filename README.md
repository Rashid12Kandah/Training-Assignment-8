# White Border Removal

csc -unsafe -out:Border_Removal.exe /main:BorderRemoval.ImageProcessing Border_Removal.cs Task_4.cs Color_To_GS_Conv.cs

mono Border_Removal.exe "<path/to/img>"

## Original Image - Husky image with white background

<img src="https://github.com/Rashid12Kandah/Training_Assignment_8/blob/main/husky.jpeg" alt="Original Husky image with white background" height="652" width="500">

>Image Information

>>Image size: 980x1279

>>Image format: Format24bppRgb

## After White Border removal

<img src="https://github.com/Rashid12Kandah/Training_Assignment_8/blob/main/cropped_image.png" alt="Cropped Husky Image with white background" height="414" width="300">

>Image Information
>
>>Image size: 773x1070

>>Cropped image format: Format24bppRgb

>>Time taken: 50 ms

