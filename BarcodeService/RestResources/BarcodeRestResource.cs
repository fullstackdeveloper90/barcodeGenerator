using Grapevine.Shared;
using Grapevine.Interfaces.Server;
using Grapevine.Server.Attributes;
using ZXing;
using ZXing.Common;
using System;
using System.Collections.Generic;
using Grapevine.Server;
using Metrics;

namespace BarcodeService.RestResources
{
    [RestResource]
    public class BarcodeRestResource
    {
        private readonly Timer timer = Metric.Timer("BarcodeRestApi", Unit.Requests);
        private readonly Counter counter = Metric.Counter("ConcurrentRequests", Unit.Requests);

        public BarcodeRestResource()
        {

        }

        private string GenerateCode(string content, BarcodeFormat format = BarcodeFormat.QR_CODE, int width = 100, int height = 100)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new KeyNotFoundException($"Content for barcode not found.");
            }

            BarcodeWriterSvg writer = new BarcodeWriterSvg();
            writer.Format = format;
            switch (format)
            {
                case BarcodeFormat.QR_CODE:
                    writer.Options = new ZXing.QrCode.QrCodeEncodingOptions()
                    {
                        Height = height,
                        Width = width
                    };

                    content = Uri.UnescapeDataString(content);
                    break;
                case BarcodeFormat.EAN_13:
                case BarcodeFormat.EAN_8:
                    writer.Options = new EncodingOptions()
                    {
                        Height = height,
                    };
                    break;
                default:
                    throw new NotImplementedException($"Format [{format}] not implemented.");
            }

            var encodedContent = writer.Write(content);
            return encodedContent.ToString();
        }

        private IHttpContext HandleRequest(IHttpContext context, BarcodeFormat format, int height = 100)
        {
            var param = context.Request.PathParameters;
            var code = param["code"];

            this.counter.Increment($"{format}");
            using (this.timer.NewContext(code))
            {
                try
                {
                    var generatedCode = GenerateCode(code, format, height: height);
                    context.Response.ContentType = ContentType.SVG;
                    context.Response.SendResponse(HttpStatusCode.Ok, generatedCode);

                }
                catch (Exception ex)
                {
                    context.Response.SendResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            this.counter.Decrement($"{format}");

            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/qr/[code]")]
        public IHttpContext GetQrCode(IHttpContext context)
        {
            return HandleRequest(context, BarcodeFormat.QR_CODE);
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/ean13/[code]")]
        public IHttpContext GetEan13(IHttpContext context)
        {
            return HandleRequest(context, BarcodeFormat.EAN_13, 40);
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/ean8/[code]")]
        public IHttpContext GetEan8(IHttpContext context)
        {
            return HandleRequest(context, BarcodeFormat.EAN_8, 40);
        }
    }
}
