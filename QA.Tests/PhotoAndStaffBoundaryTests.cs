using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LostAndFound.QA;

[TestClass]
[DoNotParallelize]
public sealed class PhotoAndStaffBoundaryTests
{
    [TestMethod]
    public async Task PhotoBytesAreCopiedToUniqueSafeNamesAndPathSurvivesReload()
    {
        var dir = Path.Combine(Path.GetTempPath(), "lf-photo-" + Guid.NewGuid());
        try
        {
            // A real tiny PNG fixture, not just a renamed arbitrary file.
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a5x8AAAAASUVORK5CYII=");
            using var first = new MemoryStream(bytes); using var second = new MemoryStream(bytes);
            var a = await ItemPhotoService.SaveAsync(first, dir); var b = await ItemPhotoService.SaveAsync(second, dir);
            Assert.AreNotEqual(a, b); CollectionAssert.AreEqual(bytes, File.ReadAllBytes(a));
            var store = Path.Combine(dir, "reports.json"); var data = new PrototypeDataService(store);
            data.AddLostReport(new() { Item = new() { ImagePath = a } });
            Assert.AreEqual(a, new PrototypeDataService(store).LostReports[0].Item.ImagePath);
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [TestMethod]
    public async Task InvalidOrOversizedPhotoDoesNotCreateFiles()
    {
        var dir = Path.Combine(Path.GetTempPath(), "lf-photo-invalid-" + Guid.NewGuid());
        try
        {
            using var text = new MemoryStream("not an image"u8.ToArray());
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => ItemPhotoService.SaveAsync(text, dir));
            using var large = new MemoryStream(new byte[5 * 1024 * 1024 + 1]);
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => ItemPhotoService.SaveAsync(large, dir));
            Assert.IsFalse(Directory.Exists(dir));
        }
        finally { if (Directory.Exists(dir)) Directory.Delete(dir, true); }
    }

    [TestMethod]
    public void StaffRegistrationChecksSessionAtServiceBoundary()
    {
        StaffSessionService.SignOut(); var d = new PrototypeDataService();
        Assert.ThrowsExactly<UnauthorizedAccessException>(() => d.TryRegisterFoundReport(new(), out _));
        Assert.IsEmpty(d.FoundReports);
        try
        {
            Assert.IsTrue(StaffSessionService.SignIn("STAFF001", "Campus123!"));
            Assert.IsTrue(d.TryRegisterFoundReport(new() { Name = "Keys", Category = "Keys", Location = "Library", Description = "Three keys", DateFound = DateTime.Today, StorageLocation = "Security" }, out _));
            Assert.HasCount(1, d.FoundReports);
        }
        finally { StaffSessionService.SignOut(); }
    }
}
