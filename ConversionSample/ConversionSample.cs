using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

public class ConversionSample
{
    static void Main(string[] args)
    {
        string inputDir = "res/";
        string outputDir = "output/";
        CVAT2SCIILLabels(inputDir, outputDir);
    }

    /// <summary>
    /// Convert CVAT (Computer Vision Annotation Tool) v1.1 to SCIIL defined structure
    /// </summary>
    /// <param name="inputDir">
    /// This directory needs to consist of 'images/' folder with image and 'annotations.xml' file
    /// </param>
    /// <param name="outputDir">
    /// In this folder 'Annotations/', 'Photos/', 'PhotosJPG/' subfolder will be created if they do not exist.
    /// If exist, they will be appended with created images and labels
    /// </param>
    /// <param name="divider">
    /// In this case 1000
    /// </param>
    static public void CVAT2SCIILLabels(string inputDir, string outputDir, double divider = 1000)
    {
        string xmlData = inputDir + "annotations.xml";

        string annotationDir = outputDir + "Annotations//";
        CreateDirectory(annotationDir);
        string photosJPGDir = outputDir + "PhotosJPG//";
        CreateDirectory(photosJPGDir);
        string photosDir = outputDir + "Photos//";
        CreateDirectory(photosDir);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlData);

        XmlNodeList imageNodes = xmlDoc.SelectNodes("//image");
        foreach (XmlNode imageNode in imageNodes)
        {
            string imageName = imageNode.Attributes["name"].Value;
            double imageWidth = (double)(int.Parse(imageNode.Attributes["width"].Value));
            double imageHeight = (double)(int.Parse(imageNode.Attributes["height"].Value));

            Dictionary<string, List<object>> defectPolygons = new Dictionary<string, List<object>>();

            XmlNodeList polygonNodes = imageNode.SelectNodes(".//polygon");
            foreach (XmlNode polygonNode in polygonNodes)
            {
                string label = polygonNode.Attributes["label"].Value;
                string points = polygonNode.Attributes["points"].Value;

                List<string> pointsList = new List<string>(points.Split(';'));

                // float to int
                for (int i = 0; i < pointsList.Count; i++) 
                {
                    string[] pointsStr = pointsList[i].Split(',');
                    // needs to be 2 points
                    double x = double.Parse(pointsStr[0]) / (imageWidth / divider);
                    double y = double.Parse(pointsStr[1]) / (imageHeight / divider);

                    // check list element
                    pointsList[i] = ((int)Math.Round(x)).ToString() + ", " + ((int)Math.Round(y)).ToString();
                }

                if (!defectPolygons.ContainsKey(label))
                {
                    defectPolygons[label] = new List<object>();
                }
                defectPolygons[label].Add(pointsList);
            }

            // output dictionary [SCIIL format]
            List<object> dicts = new List<object>();
            foreach (var defectPolygon in defectPolygons)
            {
                Dictionary<string, object> outputDict = new Dictionary<string, object>
                {
                    { "DefectType",  defectPolygon.Key},
                    { "Polygon", defectPolygon.Value}
                };
                dicts.Add(outputDict);
            }
            string outputAnnotation = Newtonsoft.Json.JsonConvert.SerializeObject(dicts, Newtonsoft.Json.Formatting.Indented);
            string jsonFilePath = annotationDir + Path.GetFileNameWithoutExtension(imageName) + ".json";
            try
            {
                // Write the JSON data to the specified file
                File.WriteAllText(jsonFilePath, outputAnnotation);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            // copy original image to 'Photos/' folder and save image as JPG to 'PhotosJPG/'
            string imagePath = inputDir + "images/" + imageName;
            if (File.Exists(imagePath))
            {
                File.Copy(imagePath, photosDir + imageName, true);
                try
                {
                    using (Image image = Image.FromFile(imagePath))
                    {
                        string jpgOutputPath = photosJPGDir + Path.GetFileNameWithoutExtension(imageName) + ".jpg";
                        image.Save(jpgOutputPath, ImageFormat.Jpeg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while saving image as JPG: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Image {0} does not exists!", imagePath);
            }
        }
    }

    /// <summary>
    /// Check if directory exists and creates if it does not
    /// </summary>
    /// <param name="outputDir">Directory path</param>
    static public void CreateDirectory(string outputDir)
    {
        try
        {
            // Check if the output folder already exists
            if (!Directory.Exists(outputDir))
            {
                // Create the output folder
                Directory.CreateDirectory(outputDir);
                Console.WriteLine("Output folder created successfully!");
            }
            else
            {
                Console.WriteLine("Output folder already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
