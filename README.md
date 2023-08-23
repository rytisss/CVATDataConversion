# CVATDataConversion
CVAT label type conversion to defined label data type.

# Usage  
Check the sample project provided. In general one function that takes the input directory with CVAT v1.1 format (with downloaded images) and outputs defined format.
https://github.com/rytisss/CVATDataConversion/blob/7da205df8c8dad0b3f6182fc53ca91181deca447/ConversionSample/ConversionSample.cs#L17-L30  

## Input directory tree
```md
├── input
│   ├── images/
│   ├── annotations.xml
```
## Output directory tree
```md
├── output
│   ├── Annotations/
│   ├── Photos/
│   ├── PhotosJPG/
```

## Resource
Example resource (2 images) added to: https://github.com/rytisss/CVATDataConversion/tree/main/ConversionSample/res 
