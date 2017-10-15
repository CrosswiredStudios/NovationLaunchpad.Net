using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

            Debug.WriteLine($"Successfully loaded piskel file for {Name}");
        }
    }

    public class PiskelEffect : LaunchpadEffect
    {
        int currentFrameIndex;
        SKBitmap bitmap;
        string filePath;
        List<Color[,]> frames;
        bool isFinished;
        bool isInitiated;
        LaunchpadMk2 launchpad;
        bool loop;
        PiskelFile piskelFile;
        readonly Subject<Unit> whenComplete = new Subject<Unit>();

        public string Name => "Piskel";

        public IObservable<int> WhenChangeUpdateFrequency => null;

        public IObservable<ILaunchpadEffect> WhenComplete => null;

        public PiskelEffect(string filePath, bool loop)
        {
            frames = new List<Color[,]>();
            isFinished = false;
            this.filePath = filePath;
            this.loop = loop;
        }

        void DrawFrame(Color[,] frame)
        {
            var totalFrames = piskelFile.Layers.First().FrameCount;
            var frameWidth = bitmap.Width / totalFrames;

            for (var y = 0; y < frameWidth; y++)
                for(var x=0; x< frameWidth; x++)
                {
                    launchpad.GridBuffer[x, y] = frame[x,y];
                }
            launchpad.FlushGridBuffer();
        }

        public async static Task<BitmapImage> ImageFromBytes(Byte[] bytes)
        {
            try
            {
                var image = new BitmapImage();
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await stream.WriteAsync(bytes.AsBuffer());
                    stream.Seek(0);
                    await image.SetSourceAsync(stream);
                }
                return image;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public void Initiate(Launchpad launchpad)
        {
            this.launchpad = launchpad as LaunchpadMk2;
            try
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(filePath);
                    var fileStream = File.OpenText(filePath);
                    var contents = fileStream.ReadToEnd();
                    piskelFile = new PiskelFile(contents);
                    var pngData = piskelFile.Layers.First().Chunks.First().Data;
                    bitmap = SKBitmap.Decode(pngData);

                    var totalFrames = piskelFile.Layers.First().FrameCount;
                    var frameWidth = bitmap.Width / totalFrames;

                    for (var frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                    {
                        var frame = new Color[frameWidth, bitmap.Height];
                        var xOffset = frameIndex * frameWidth;
                        for (var y = 0; y < bitmap.Height; y++)
                        {
                            for (var x = 0; x < frameWidth; x++)
                            {
                                var pixel = bitmap.Pixels[bitmap.Width * y + (x + xOffset)];
                                frame[x, y] = Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
                            }
                        }
                        frames.Add(frame);
                    }


                    isInitiated = true;
                });

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Terminate()
        {
            
        }

        public void Update()
        {
            if (!isInitiated || isFinished) return;
            currentFrameIndex++;
            if (currentFrameIndex >= piskelFile.Layers.First().FrameCount)
            {
                if (loop)
                    currentFrameIndex = 0;
                else
                {
                    isFinished = true;
                    whenComplete.OnNext(new Unit());
                }
            }

            DrawFrame(frames[currentFrameIndex]);
        }
    }
}
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed