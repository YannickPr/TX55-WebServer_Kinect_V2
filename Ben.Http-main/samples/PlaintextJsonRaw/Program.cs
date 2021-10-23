using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using static System.Console;

using Ben.Http;

//using Microsoft.system;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;






/*
KinectSensor ks = null;

//ColorFrameData colorData = new ColorFrameData();
DepthFrameReader depthFrameReader = null;
//private WriteableBitmap depthBitmap = null;
//byte[] depthPixels = { 0 };
//string statusText = "";


FrameDescription depthFrameDescription = null;

ks = KinectSensor.GetDefault();
depthFrameReader = ks.DepthFrameSource.OpenReader();


depthFrameReader.FrameArrived += Reader_FrameArrived;

depthFrameDescription = ks.DepthFrameSource.FrameDescription;

//depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height];

ks.Open();
*/


var port = 8080;
var server = new HttpServer($"http://localhost:{port}");
var app = new HttpApp();

// Assign routes
app.Get("/plaintext", Plaintext);
app.Get("/json", Json);

Write($"{server} {app}"); // Display listening info

await server.RunAsync(app);


/*

*/
// Route methods
async Task Plaintext(Request request, Response response)
{
    //msfr = ks.OpenMultiSourceFrameReader(FrameSourceTypes.Depth); // | FrameSourceTypes.Color);

    //msfr.MultiSourceFrameArrived += msfr_MultiSourceFrameArrived;


    //byte[] data = null;
    //data = SendColorData();



    var payload = Settings.HelloWorld;

    var headers = response.Headers;

    headers.ContentLength = payload.Length;
    headers[HeaderNames.ContentType] = "text/plain";

    await response.Writer.WriteAsync(payload);



}

static Task Json(Request request, Response response)
{
    var headers = response.Headers;

    headers.ContentLength = 27;
    headers[HeaderNames.ContentType] = "application/json";

    return JsonSerializer.SerializeAsync(
        response.Stream, 
        new JsonMessage { message = "Hello, World!" }, 
        Settings.SerializerOptions);
}

/*
void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
{
    bool depthFrameProcessed = false;

    using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
    {
        if (depthFrame != null)
        {
            // the fastest way to process the body index data is to directly access 
            // the underlying buffer
            using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
            {

                Console.WriteLine("buffer : " + depthBuffer);
                /*
                // verify data and write the color data to the display bitmap
                if (((depthFrameDescription.Width * depthFrameDescription.Height) == (depthBuffer.Size / depthFrameDescription.BytesPerPixel)) &&
                    (depthFrameDescription.Width == depthBitmap.PixelWidth) && (depthFrameDescription.Height == depthBitmap.PixelHeight))
                {
                    // Note: In order to see the full range of depth (including the less reliable far field depth)
                    // we are setting maxDepth to the extreme potential depth threshold
                    ushort maxDepth = ushort.MaxValue;

                    // If you wish to filter by reliable depth distance, uncomment the following line:
                    //// maxDepth = depthFrame.DepthMaxReliableDistance

                    ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                    depthFrameProcessed = true;
                }*//*
            }
        }
    }
    *//*
    if (depthFrameProcessed)
    {
        RenderDepthPixels();
    }*//*
}





static void msfr_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
{
    ushort[] frameData = { 0 };
    if (e.FrameReference == null)
        return;
    var multiFrame = e.FrameReference.AcquireFrame();
    if (multiFrame == null)
        return;
    multiFrame.DepthFrameReference.AcquireFrame().CopyFrameDataToArray(frameData);
    Console.WriteLine("profondeur data : " + frameData);

    *//*bool colorRead = false;

    FrameDescription fd = null;
    if (multiFrame.ColorFrameReference != null)
    {
        using (var cf = multiFrame.ColorFrameReference.AcquireFrame())
        {
            fd = cf.ColorFrameSource.FrameDescription; //parfois une erreur
            cf.CopyConvertedFrameDataToArray(colorData.Data, colorData.Format);
            colorRead = true;
        }
    }
    
    bool bodyRead = false;
    if (multiFrame.BodyFrameReference != null)
    {
        using (var bf = multiFrame.BodyFrameReference.AcquireFrame())
        {
            bf.GetAndRefreshBodyData(bodies);
            bodyRead = true;
        }
    }

    byte[] data = null;
    if (colorRead == true)
    {
        data = SendColorData(colorData, fd);
    }
    string bodyData = null;
    if (bodyRead == true)
    {
        bodyData = SerialiseBodyData();
    }
    *//*
}

static byte[] SendColorData()
{
    byte[] encodedBytes;
    *//*if (data == null)
        return null;
    
    var dpiX = 96.0;
    var dpiY = 96.0;
    var pixelFormat = PixelFormats.Bgra32;
    var bytesPerPixel = (pixelFormat.BitsPerPixel) / 8;
    var stride = bytesPerPixel * fd.Width;

    var bitmap = BitmapSource.Create(fd.Width, fd.Height, dpiX, dpiY,
                                     pixelFormat, null, data.Data, (int)stride);
    var encoder = new JpegBitmapEncoder();
    encoder.Frames.Add(BitmapFrame.Create(bitmap));
    *//*
    using (var ms = new MemoryStream())
    using (var br = new BinaryReader(ms))
    {
        //encoder.Save(ms);
        ms.Flush();
        ms.Position = 0;
        encodedBytes = br.ReadBytes((int)ms.Length);
        Console.WriteLine("encodedBytes : " + encodedBytes);
    }

    return encodedBytes;
}

public class ColorFrameData
{
    public byte[] Data { get; set; }
    public ColorImageFormat Format { get; set; }
}
*/
// Settings and datastructures
struct JsonMessage { public string message { get; set; } }



static class Settings
{
    public static readonly byte[] HelloWorld = Encoding.UTF8.GetBytes("Hello, World!");
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(new JsonSerializerOptions { });
}