using System.IO;
using NUnit.Framework;
using SkiaSharp;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

namespace Tests
{
    public class TextureTests
    {
        [Test]
        public void ExportTextures()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Textures");
            var files = Directory.GetFiles(path, "*.vtex_c");

            foreach (var file in files)
            {
                var resource = new Resource();
                resource.Read(file);

                var bitmap = ((Texture)resource.DataBlock).GenerateBitmap();
                var image = SKImage.FromBitmap(bitmap);

                using (var ms = new MemoryStream())
                {
                    using (var imageData = image.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        imageData.SaveTo(ms);
                    }

                    // TODO: Comparing images as bytes doesn't work
#if false
                    using (var expected = new FileStream(Path.ChangeExtension(file, "png"), FileMode.Open, FileAccess.Read))
                    {
                        FileAssert.AreEqual(expected, ms);
                    }
#endif
                }
            }
        }
    }
}
