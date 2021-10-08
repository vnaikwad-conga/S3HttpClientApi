using Microsoft.AspNetCore.Mvc;
using S3Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HttpClientApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class S3Controller : ControllerBase
    {
        private IDataProvider dataProvider;
        private string path = "";
        public S3Controller(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;            
            this.path = Environment.GetEnvironmentVariable("Prefix");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("{key}")]
        public async Task<IActionResult> GetAsync(string key)
        {           
            var sw = Stopwatch.StartNew();
            var documentResponseModel = await dataProvider.GetObjectAsync(new DocumentRequestModel() { Key = key, Path = path });
            sw.Stop();
            Console.WriteLine($"Total Time in MS - {sw.ElapsedMilliseconds}");
            return Ok(new { documentResponseModel, sw.ElapsedMilliseconds});
        }

        [HttpPost]
        public async Task<IActionResult> GetAsync(string[] keys)
        {
            var documentRequestModelList = keys.Select(s => new DocumentRequestModel() { Key = s, Path = path });
            var sw = Stopwatch.StartNew();
            var documentResponseModelList = await dataProvider.GetObjectAsync(documentRequestModelList.ToArray());
            sw.Stop();
            Console.WriteLine($"Total Time in MS - {sw.ElapsedMilliseconds}");
            return Ok(new { documentResponseModelList, sw.ElapsedMilliseconds });
        }
    }
}
