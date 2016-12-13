using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace KdSoft.Services
{
    public interface IAttachmentProvider
    {
        string Description { get; }
        string MediaType { get; }
        Task WriteStreamAsync(Stream attStream);
    }

    public class AttachmentProvider: IAttachmentProvider
    {
        const int DefaultFileBufferSize = 65536;

        string file;
        CancellationToken cancelToken;
        int fileBufferSize;

        public AttachmentProvider(string description, string mediaType, string file, CancellationToken cancelToken, int fileBufferSize = DefaultFileBufferSize) {
            this.Description = description;
            this.MediaType = mediaType;
            this.file = file;
            this.cancelToken = cancelToken;
            this.fileBufferSize = fileBufferSize;
        }

        public string Description { get; private set; }

        public string MediaType { get; private set; }

        // will delete the source file after use
        public async Task WriteStreamAsync(Stream target) {
            var options = FileOptions.Asynchronous | FileOptions.DeleteOnClose;
            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None, fileBufferSize, options)) {
                await fileStream.CopyToAsync(target, fileBufferSize, cancelToken);
            }
        }
    }
}
