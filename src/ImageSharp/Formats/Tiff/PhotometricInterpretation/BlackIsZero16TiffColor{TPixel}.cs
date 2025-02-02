// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Numerics;
using SixLabors.ImageSharp.Formats.Tiff.Utils;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Formats.Tiff.PhotometricInterpretation;

/// <summary>
/// Implements the 'BlackIsZero' photometric interpretation for 16-bit grayscale images.
/// </summary>
internal class BlackIsZero16TiffColor<TPixel> : TiffBaseColorDecoder<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    private readonly bool isBigEndian;

    private readonly Configuration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlackIsZero16TiffColor{TPixel}" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="isBigEndian">if set to <c>true</c> decodes the pixel data as big endian, otherwise as little endian.</param>
    public BlackIsZero16TiffColor(Configuration configuration, bool isBigEndian)
    {
        this.configuration = configuration;
        this.isBigEndian = isBigEndian;
    }

    /// <inheritdoc/>
    public override void Decode(ReadOnlySpan<byte> data, Buffer2D<TPixel> pixels, int left, int top, int width, int height)
    {
        L16 l16 = TiffUtils.L16Default;
        var color = default(TPixel);
        color.FromScaledVector4(Vector4.Zero);

        int offset = 0;
        for (int y = top; y < top + height; y++)
        {
            Span<TPixel> pixelRow = pixels.DangerousGetRowSpan(y).Slice(left, width);
            if (this.isBigEndian)
            {
                for (int x = 0; x < pixelRow.Length; x++)
                {
                    ushort intensity = TiffUtils.ConvertToUShortBigEndian(data.Slice(offset, 2));
                    offset += 2;

                    pixelRow[x] = TiffUtils.ColorFromL16(l16, intensity, color);
                }
            }
            else
            {
                int byteCount = pixelRow.Length * 2;
                PixelOperations<TPixel>.Instance.FromL16Bytes(
                    this.configuration,
                    data.Slice(offset, byteCount),
                    pixelRow,
                    pixelRow.Length);

                offset += byteCount;
            }
        }
    }
}
