using System.Collections.Generic;
using System.Threading.Tasks;

namespace S3Client
{
    public interface IDataProvider
    {
        Task<List<DocumentResponseModel>> GetObjectAsync(params DocumentRequestModel[] documentRequests);
    }
}