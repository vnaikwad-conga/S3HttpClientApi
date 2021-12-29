using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3PerfTest
{
    public interface IDataProvider
    {
        Task<List<DocumentResponseModel>> GetObjectAsync(params DocumentRequestModel[] documentRequests);
    }
}
