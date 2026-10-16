namespace LostAndFound.Services;

public static class ItemPhotoService
{
    public static async Task<string> SaveAsync(Stream input, string directory)
    {
        const int limit = 5 * 1024 * 1024;
        using var bytes = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            if (bytes.Length + read > limit) throw new InvalidDataException("Choose an image smaller than 5 MB.");
            await bytes.WriteAsync(buffer.AsMemory(0, read));
        }
        var data = bytes.ToArray();
        var png = data.Length >= 8 && data.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var jpeg = data.Length >= 4 && data[0] == 255 && data[1] == 216 && data[^2] == 255 && data[^1] == 217;
        if (!png && !jpeg) throw new InvalidDataException("Choose a PNG or JPEG image.");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, Guid.NewGuid().ToString("N") + (png ? ".png" : ".jpg"));
        await File.WriteAllBytesAsync(path, data);
        return path;
    }
}
