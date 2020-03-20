using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utils.Interfaces;
using Utils.Model;

namespace API.Controllers
{
    [ApiController]
    [Route("analytics")]
    [Authorize(Policy = "ClaimsAuthorizationPolicy")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IDataRepository<ReportedIssue> _dataRepository;
        private readonly IFileRepository<Image> _fileRepository;

        public AnalyticsController(IDataRepository<ReportedIssue> dataRepository, IFileRepository<Image> fileRepository)
        {
            _dataRepository = dataRepository;
            _fileRepository = fileRepository;
        }

        [HttpPut]
        [Route("issue")]
        [AllowAnonymous]
        public async Task<ActionResult> PutIssue([FromBody]ReportedIssue reportedIssue)
        {
            await _dataRepository.AddRow(reportedIssue);
            return Ok();
        }

        [HttpPut]
        [Route("issue/{id}/image")]
        [AllowAnonymous]
        public async Task<ActionResult> PutImage([FromRoute]Guid id, [FromBody]Image image)
        {
            await _fileRepository.SetFileContent(id, image);
            return Ok();
        }

        [HttpGet]
        [Route("issue/{id}/image")]
        public async Task<ActionResult<Image>> GetImage([FromRoute]Guid id)
        {
            return await _fileRepository.GetFileContent(id);
        }

        [HttpDelete]
        [Route("issue/{id}")]
        public async Task<ActionResult> DeleteIssue([FromRoute]Guid id)
        {
            await _dataRepository.DeleteRow(id);
            await _fileRepository.DeleteFile(id);
            return Ok();
        }

        [HttpGet]
        [Route("issues")]
        public async Task<ActionResult<List<ReportedIssue>>> GetIssues()
        {
            return await _dataRepository.GetRows();
        }
    }
}