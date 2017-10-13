using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Launchpad.NET.Effects
{
    public class PiskelChunk
    {
        public string Base64Png { get; }
        public byte[] Data { get; }
        public JArray Layout { get; }
        
        public PiskelChunk(string data)
        {
            var chunkData = JObject.Parse(data);

            Base64Png = chunkData["base64PNG"].ToString();
            Data = Convert.FromBase64String(Base64Png.Split(',')[1]);
            Layout = chunkData["layout"] as JArray;
        }
    }

    public class PiskelLayer
    {
        public PiskelChunk[] Chunks { get; }
        public int FrameCount { get; }
        public string Name { get; }
        public int Opacity { get; }
        
        public PiskelLayer(string data)
        {
            var layerData = JObject.Parse(data);

            FrameCount = (int)layerData["frameCount"];
            Name = layerData["name"].ToString();
            Opacity = (int)layerData["opacity"];

            var chunksData = layerData["chunks"] as JArray;

            Chunks = new PiskelChunk[chunksData.Count];
            for (var index = 0; index < chunksData.Count; index++)
            {
                Chunks[index] = new PiskelChunk(chunksData[index].ToString());
            }
        }
    }

    public class PiskelFile
    {
        public string Description { get; }
        public int Fps { get; }
        public int Height { get; }
        public PiskelLayer[] Layers { get; }
        public int ModelVersion { get; }
        public string Name { get; }
        public int Width { get; }

        public PiskelFile(string jsonString)
        {
            var jContents = JObject.Parse(jsonString);

            ModelVersion = (int)jContents["modelVersion"];

            var piskelData = jContents["piskel"];

            Description = piskelData["description"].ToString();
            Fps = (int)piskelData["fps"];
            Height = (int)piskelData["height"];
            Name = piskelData["name"].ToString();
            Width = (int)piskelData["width"];

            var layersData = piskelData["layers"] as JArray;

            Layers = new PiskelLayer[layersData.Count];
            for(var index = 0; index < layersData.Count; index++)
            {
                Layers[index] = new PiskelLayer(layersData[index].ToString());
            }
        }
    }

    public class PiskelEffect : ILaunchpadEffect
    {
        List<JObject> layers;
        public string Name => "Piskel";

        public IObservable<int> WhenChangeUpdateFrequency => null;

        public IObservable<Unit> WhenComplete => null;

        public PiskelEffect(string filePath)
        {
            layers = new List<JObject>();
            try
            {
                var img = new BitmapImage();
                Task.Run(async () =>
                {
                    var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(filePath);

                    //var stream = await file.OpenAsync(FileAccessMode.Read);

                    //var a = new Image();
                    //a.
                    //img.SetSource(stream);


                    var fileStream = File.OpenText(filePath);
                    var contents = fileStream.ReadToEnd();
                    var piskelFile = new PiskelFile(contents);
                    


                });
            }
            catch { }
        }

        public void Initiate(Launchpad launchpad)
        {
            
        }

        public void Terminate()
        {
            
        }

        public void Update()
        {
            
        }
    }
}
