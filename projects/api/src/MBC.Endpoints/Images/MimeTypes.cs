using System;
using System.IO;
using System.Threading.Tasks;

namespace MBC.Endpoints.Images;

public static class MimeTypes
{
    private static readonly byte[] JpegHeader = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngHeader = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] WebPHeader = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] WebPSignature = [0x57, 0x45, 0x42, 0x50];

    public static MimeTypeValue? GetMimeType(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 3)
        {
            return null;
        }

        if (buffer.Length >= JpegHeader.Length && MatchesSignature(buffer, JpegHeader))
        {
            return MimeTypeValue.ImageJpeg;
        }

        if (buffer.Length >= PngHeader.Length && MatchesSignature(buffer, PngHeader))
        {
            return MimeTypeValue.ImagePng;
        }

        if (buffer.Length >= 12 && MatchesSignature(buffer, WebPHeader) && MatchesSignature(buffer[8..], WebPSignature))
        {
            return MimeTypeValue.ImageWebP;
        }

        return null;
    }

    public static async Task<MimeTypeValue?> GetMimeType(Stream stream)
    {
        var buffer = new byte[12];
        var bytesRead = await stream.ReadAsync(buffer);
        return GetMimeType(buffer.AsSpan(0, bytesRead));
    }

    private static bool MatchesSignature(ReadOnlySpan<byte> buffer, byte[] signature)
    {
        if (buffer.Length < signature.Length)
        {
            return false;
        }

        for (var i = 0; i < signature.Length; i++)
        {
            if (buffer[i] != signature[i])
            {
                return false;
            }
        }
        return true;
    }
}

